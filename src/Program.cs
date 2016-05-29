using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using abb.egm;
using System.Diagnostics;
using System.Drawing.Text;
using System.Threading;
using System.Timers;
using System.Threading.Tasks;


namespace ExternalGuidedMotion
{
    public static class Settings
    {
        public enum Mode
        {
            Default,
            Camera,
            Simulate
        };
    }

    public class Program
    {
        // listen on this port for inbound messages
        public static int IpPortNumber = 6510;
        public static int CameraIpPortNumber = 3000;
      
        static void Main(string[] args)
        {
            var mode = Settings.Mode.Default;
            
            switch (args[0])
            {
                case null:
                    Console.WriteLine("args is null");
                    break;

                case "Camera":
                    mode = Settings.Mode.Camera;
                    Console.WriteLine("Camera");
                    break;

                case "Simulate":
                    mode = Settings.Mode.Simulate;
                    Console.WriteLine("Simulate");
                    break;

                default:
                    Console.WriteLine("Leagal options: Camera, Simulate");
                    break;
            }

            Sensor s = new Sensor(mode);
                s.Start();
    
            Console.CancelKeyPress += delegate
            {
                s.Stop();
            };
        }
    }

    public class Sensor
    {
        private Thread _sensorThread;
        private UdpClient _udpServer;
        private uint _seqNumber = 0;
        private Settings.Mode mode;
        private Camera _camera;
        private Predictor _predictor;
        private Position _position;
        private Stopwatch _stopwatch;
        private SimDisc _SimDisc;
        private double _time; 
        private System.Timers.Timer _newPosIntervalTimer;
        private double _newPosInterval = 16;
        private bool _hasDiscStarted = false;
        private ConsoleKeyInfo _start;
        private bool _isSimulate = false;
        private bool _isCamera = false;
        private double _seqNum;
        private double _prevSeqNum;
        private double _prevX;
        private double _prevXSensor;
        private double _prevYSensor;
        private double _prevZSensor;
        private double _timeAhead;

        public bool ExitThread = false;
        public TextWriter Positionfile = new StreamWriter(@"..\...\position.txt", true);

        private double XRobot { get; set; }
        private double YRobot { get; set; }
        private double ZRobot { get; set; }

        private double XSensor;
        private double YSensor;
        private double ZSensor;

        private double X { get; set; }
        private double Y { get; set; }
        private double Z { get; set; }
        
        public Sensor(Settings.Mode mode)
        {
            this.mode = mode;
            switch (mode)
            {
                case Settings.Mode.Camera:
                    _predictor = new Predictor();
                    _camera = new Camera(_predictor);
                    _camera.StartCamera();
                    _stopwatch = new Stopwatch();
                    _isCamera = true; 
                    break;

                case Settings.Mode.Simulate:
                    _position = new Position();
                    _SimDisc = new SimDisc(_position);
                    _SimDisc.StartSimDisc();
                    _stopwatch = new Stopwatch();
                    Task.Factory.StartNew(StartDiscFromConsole);
                    _isSimulate = true;
                    break;

                default:
                    Console.WriteLine("Leagal options: Camera, Simulate");
                    break;        
            }
        }

        public void SimDiscSetPos()
        {
            if (_position.X < 1000)
            {
                X = _position.X;
            }
            else
            {
                X = 1000;
            }

            Y = _position.Y;
            Z = _position.Z;
        }

        public void CameraSetPos()
        {
            X = _predictor.PredictedPosition;
            Y = - _camera.Y;
            Z = 0;
            _seqNum = _camera.Seqnum;
            _timeAhead = _predictor.PredictedTime;
        }



        public void SavePositionToFile()
        {
            _time = _stopwatch.ElapsedMilliseconds;
            Positionfile.WriteLine(_time.ToString("0.00") + " " +
                        Convert.ToInt32(_camera.X).ToString("0.00") + " " +
                        Convert.ToInt32(_camera.Y).ToString("0.00") + " " +
                        Convert.ToInt32(ZSensor).ToString("0.00") + " " +
                        Convert.ToInt32(XRobot).ToString("0.00") + " " +
                        Convert.ToInt32(YRobot).ToString("0.00") + " " +
                        Convert.ToInt32(ZRobot).ToString("0.00") + " " +
                        _timeAhead.ToString("0.00")
                        );
        }

        private void StartDiscFromConsole()
        {
            Console.Write("Type s to start the disc: ");
            _start = Console.ReadKey();
            if (_start.KeyChar == 's' || _start.KeyChar == 'S')
            {
                _SimDisc.StartDisc();
                _hasDiscStarted = true; 
            }
            else if (_start.KeyChar != 's' || _start.KeyChar != 'S')
            {
                Console.WriteLine("Unvalid char, leagal options: s, S");
            }
        }


        public void SensorThread()
        {
            // create an udp client and listen on any address and the port IpPortNumber
            _udpServer = new UdpClient(Program.IpPortNumber);
            var remoteEp = new IPEndPoint(IPAddress.Any, Program.IpPortNumber);

            _stopwatch.Start();
            
            while (ExitThread == false)
            {
                
                //SimDiscSetPos();
                CameraSetPos();

                // Get the message from robot
                var data = _udpServer.Receive(ref remoteEp);

                if (data != null)
                {
                    Console.WriteLine(_predictor.PredictedPosition.ToString());

                    // de-serialize inbound message from robot using Google Protocol Buffer
                    EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();

                    XRobot = robot.FeedBack.Cartesian.Pos.X;
                    YRobot = robot.FeedBack.Cartesian.Pos.Y;
                    ZRobot = robot.FeedBack.Cartesian.Pos.Z;

                    // create a new outbound sensor message
                    EgmSensor.Builder sensor = EgmSensor.CreateBuilder();
                    CreateSensorMessage(sensor);

                    XSensor = sensor.Planned.Cartesian.Pos.X;
                    YSensor = sensor.Planned.Cartesian.Pos.Y;
                    ZSensor = sensor.Planned.Cartesian.Pos.Z;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        EgmSensor sensorMessage = sensor.Build();
                        sensorMessage.WriteTo(memoryStream);

                        // send the udp message to the robot
                        int bytesSent = _udpServer.Send(memoryStream.ToArray(),
                                           (int)memoryStream.Length, remoteEp);
                        if (bytesSent < 0)
                        {
                            Console.WriteLine("Error send to robot");
                        }
                    }

                    SavePositionToFile();

                    _prevXSensor = XSensor;
                    _prevYSensor = YSensor;
                    _prevZSensor = ZSensor;
                }  
            } 
        }

        // Create a sensor message to send to the robot
        void CreateSensorMessage(EgmSensor.Builder sensor)
        {
            // create a header
            EgmHeader.Builder hdr = new EgmHeader.Builder();
            hdr.SetSeqno(_seqNumber++)
               .SetTm((uint)DateTime.Now.Ticks) //  Timestamp in milliseconds (can be used for monitoring delays)
               .SetMtype(EgmHeader.Types.MessageType.MSGTYPE_CORRECTION); // Sent by sensor, MSGTYPE_DATA if sent from robot controller
            sensor.SetHeader(hdr);
            
            // create some sensor data
            EgmPlanned.Builder planned = new EgmPlanned.Builder();
            EgmPose.Builder pos = new EgmPose.Builder();
            EgmQuaternion.Builder pq = new EgmQuaternion.Builder();
            EgmCartesian.Builder pc = new EgmCartesian.Builder();

            pc.SetX(X)
              .SetY(Y)
              .SetZ(Z);

            pq.SetU0(0.0)
              .SetU1(0.0)
              .SetU2(0.0)
              .SetU3(0.0);

            pos.SetPos(pc)
                .SetOrient(pq);

            planned.SetCartesian(pos);  // bind pos object to planned
            sensor.SetPlanned(planned); // bind planned to sensor object
        }

        // Start a thread to listen on inbound messages
        public void Start()
        {
            _seqNum = 0;
            _prevSeqNum = 0;
            _prevX = 0;
            _sensorThread = new Thread(SensorThread);
            _sensorThread.Start();
        }

        // Stop and exit thread
        public void Stop()
        {
            Positionfile.Close();
            ExitThread = true;
            _sensorThread.Abort();
            if (_isSimulate)
            {
                _SimDisc.StopSimDisc();
            }
            else if (_isCamera)
            {
                _camera.StopCamera();
            }
            _stopwatch.Stop();
            _stopwatch.Reset();
            
        }
    }
}



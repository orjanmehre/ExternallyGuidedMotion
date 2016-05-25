using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

/// <summary>
/// This program is acting as a server and gets position data from a Cognex smart camera.
/// X and Y are the position of the disc in relation to the calibration done in In-Sight Explorer.
/// It is impotant that the port is the same port as in In-Sight Explorer.
/// </summary>

namespace ExternalGuidedMotion
{
    class Camera
    {
        private Thread _cameraThread = null;
        private UdpClient _cameraUdpServer = null;
        private Predictor predictor;

        private double _x;
        private double _y;
        private double _timeStamp;
        private double _seqNum;
        private double _prevY;
        private double _prevPrevY;
        private double[] _lastReadingsX;
        private double[] _lastReadingsY;
        private double[] tempArrayX;
        private double _prevX;
        private double _prevPrevX;

        public bool exitThread = false;
        public TextWriter ExecutionTime = new StreamWriter(@"..\...\ExecutionTime.txt", true);
        Stopwatch stopwatch = new Stopwatch();

        public double X { get; set; }
        public double Y { get; set; }
        public double TimeStamp { get; set; }
        public double timeElapsed { get; set; }
        public double Seqnum { get; set; }


        public Camera(Predictor predictor)
        {
            this.predictor = predictor;
            _lastReadingsX = new double[3] { 1, 1, 1};
            _lastReadingsY = new double[3] { 1, 1, 1 };
            tempArrayX = new double[3] { 1, 1, 1 };
            _prevX = 1;
            _prevPrevX = 1;
            _prevY = 1;
            _prevPrevY = 1;
        }


        public void WriteExecutionTimeToFile()
        {
            ExecutionTime.WriteLine(X.ToString());
        }

        public void CameraThread()
        {
            _cameraUdpServer = new UdpClient(Program.CameraIpPortNumber);
            var cameraRemoteEP = new IPEndPoint(IPAddress.Any, Program.CameraIpPortNumber);

            while (exitThread == false)
            {

                var cameraData = _cameraUdpServer.Receive(ref cameraRemoteEP);

                if (cameraData != null)
                {
                    var cameraXYTS = Encoding.Default.GetString(cameraData);

                    string[] XYTS = cameraXYTS.Split(',');

                    for (int i = 0; i < XYTS.Length; i++)
                    {
                        XYTS[i] = XYTS[i].Trim();
                    }

                    string tempX = XYTS[0].Replace('.', ',');
                    string tempY = XYTS[1].Replace('.', ',');
                    string tempT = XYTS[2].Replace('.', ',');
                    string tempS = XYTS[3].Replace('.', ',');

                    Double.TryParse(tempX, out _x);
                    Double.TryParse(tempY, out _y);
                    Double.TryParse(tempT, out _timeStamp);
                    Double.TryParse(tempS, out _seqNum);

                    medianFilter(_x, _y);

                    

                    TimeStamp = _timeStamp;
                    Seqnum = _seqNum;

                    predictor.NewPrediction(TimeStamp,X);

                    timeElapsed = TimeStamp;
                    //WriteExecutionTimeToFile();
                }
                else
                {
                    Console.WriteLine("No data from camera");
                }
            }
        }

        private void medianFilter(double x, double y)
        {

            _lastReadingsX[0] = x;
            _lastReadingsX[1] =_prevX;
            _lastReadingsX[2] = _prevPrevX;
            Array.Sort(_lastReadingsX);
            X = _lastReadingsX[1];
            _prevPrevX = _prevX;
            _prevX = x;

            _lastReadingsY[0] = y;
            _lastReadingsY[1] = _prevY;
            _lastReadingsY[2] = _prevPrevY;
            Array.Sort(_lastReadingsY);
            Y = _lastReadingsY[1];
            _prevPrevY = _prevY;
            _prevY = y;
        }

        public void StartCamera()
        {
            _cameraThread = new Thread(new ThreadStart(CameraThread));
            _cameraThread.Start();
            _prevY = 1;
        }

        public void StopCamera()
        {
            _cameraThread.Abort();
            ExecutionTime.Close();
        }
    }
}

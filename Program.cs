using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using abb.egm;
using System.Diagnostics;
using System.Threading;
using System.Timers;


//////////////////////////////////////////////////////////////////////////
// Sample program using protobuf-csharp-port 
// (http://code.google.com/p/protobuf-csharp-port/wiki/GettingStarted)
//
// 1) Download protobuf-csharp binaries from https://code.google.com/p/protobuf-csharp-port/
// 2) Unpack the zip file
// 3) Copy the egm.proto file to a sub catalogue where protobuf-csharp was un-zipped, e.g. ~\protobuf-csharp\tools\egm
// 4) Generate an egm C# file from the egm.proto file by typing in a windows console: protogen .\egm\egm.proto --proto_SimDisc=.\egm
// 5) Create a C# console application in Visual Studio
// 6) Install Nuget, in Visual Studio, click Tools and then Extension Manager. Goto to Online, find the NuGet Package Manager extension and click Download.
// 7) Install protobuf-csharp via NuGet, select in Visual Studio, Tools Nuget Package Manager and then Package Manager Console and type PM>Install-Package Google.ProtocolBuffers
// 8) Add the generated file egm.cs to the Visual Studio project (add existing item)
// 9) Copy the code below and then compile, link and run.
//
// Copyright (c) 2014, ABB
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that 
// the following conditions are met:
//
//    * Redistributions of source code must retain the 
//      above copyright notice, this list of conditions 
//      and the following disclaimer.
//    * Redistributions in binary form must reproduce the 
//      above copyright notice, this list of conditions 
//      and the following disclaimer in the documentation 
//      and/or other materials provided with the 
//      distribution.
//    * Neither the name of ABB nor the names of its 
//      contributors may be used to endorse or promote 
//      products derived from this software without 
//      specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF 
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

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
            ConsoleKeyInfo start;

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
                    Console.Write("Type s to start the disk: ");
                    Console.ReadKey();
                    start = Console.ReadKey();
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
        private Position _position;
        private Stopwatch _stopwatch;
        private SimDisc _SimDisc;
        private double _time; 
        private bool _isFirstLoop = true;
        private System.Timers.Timer _newPosIntervalTimer;
        private double _newPosInterval = 33;
        private int _z;

        public bool ExitThread = false;
        public TextWriter Positionfile = new StreamWriter(@"..\...\position.txt", true);
        public TextWriter ExecutionTime = new StreamWriter(@"..\...\ExecutionTime.txt", true);
        
        private double _xRobot { get; set; }
        private double _yRobot { get; set; }
        private double _zRobot { get; set; }
        private double _x { get; set; }
        private double _y { get; set; }
        


        public Sensor(Settings.Mode mode)
        {
            this.mode = mode;
            switch (mode)
            {
                case Settings.Mode.Camera:
                    _camera = new Camera();
                    _camera.StartCamera();
                    _stopwatch = new Stopwatch();
                    break;

                case Settings.Mode.Simulate:
                    _position = new Position();
                    _SimDisc = new SimDisc(_position);
                    _SimDisc.StartSimDisc();
                    break;

                default:
                    Console.WriteLine("Leagal options: Camera, Simulate");
                    break;        
            }
        }

        public void SimDiscSetPos(Object source, ElapsedEventArgs e)
        {
            if (_position.X < 1000)
            {
                _x = _position.X;
            }
            else
            {
                _x = 1000;
            }
            _y = -100;
            _z = 0;
        }

        public void CameraSetPos()
        {
            _x = _camera.X;
            _y = _camera.Y;
            _z = 0;
            _time = _stopwatch.ElapsedMilliseconds;
        }

        public void SetTimerInterval()
        {
            _newPosIntervalTimer = new System.Timers.Timer(_newPosInterval);
            _newPosIntervalTimer.Elapsed += SimDiscSetPos;
            _newPosIntervalTimer.Start();
        }


        public void SavePositionToFile()
        {
            _time = _position.time;
            Positionfile.WriteLine(_time.ToString("#.####") + " " +
                        _x.ToString("#.##") + " " +
                        _y.ToString("#.##") + " " +
                        _z.ToString() + " " +
                        _xRobot.ToString("#.##") + " " +
                        _yRobot.ToString("#.##") + " " +
                        _zRobot.ToString("#.##"));
        }


        public void SensorThread()
        {
            // create an udp client and listen on any address and the port IpPortNumber
            _udpServer = new UdpClient(Program.IpPortNumber);
            var remoteEp = new IPEndPoint(IPAddress.Any, Program.IpPortNumber);

            while (ExitThread == false)
            { 
                // Get the message from robot
                var data = _udpServer.Receive(ref remoteEp);

                if (data != null)
                {
                    // de-serialize inbound message from robot using Google Protocol Buffer
                    EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();

                    _xRobot = robot.FeedBack.Cartesian.Pos.X;
                    _yRobot = robot.FeedBack.Cartesian.Pos.Y;
                    _zRobot = robot.FeedBack.Cartesian.Pos.Z;

                    // create a new outbound sensor message
                    EgmSensor.Builder sensor = EgmSensor.CreateBuilder();
                    CreateSensorMessage(sensor);

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
                    if (_isFirstLoop == true)
                    {
                        SetTimerInterval();
                        _SimDisc.StartDisc();
                        _isFirstLoop = false;
                    }

                    // Write the postition to file
                    SavePositionToFile(); 
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

            Debug.WriteLine(_time + " " + _x);
            pc.SetX(_x)
              .SetY(_y)
              .SetZ(_z);

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
            _sensorThread = new Thread(SensorThread);
            _sensorThread.Start();
        }

        // Stop and exit thread
        public void Stop()
        {
            ExecutionTime.Close();
            Positionfile.Close();
            ExitThread = true;
            _sensorThread.Abort();
            _SimDisc.StopSimDisc();
            if (mode == Settings.Mode.Camera)
            {
                _stopwatch.Stop();
                _stopwatch.Reset();
            }
        }
    }
}



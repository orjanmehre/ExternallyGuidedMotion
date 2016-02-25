﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using abb.egm;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
                    


//////////////////////////////////////////////////////////////////////////
// Sample program using protobuf-csharp-port 
// (http://code.google.com/p/protobuf-csharp-port/wiki/GettingStarted)
//
// 1) Download protobuf-csharp binaries from https://code.google.com/p/protobuf-csharp-port/
// 2) Unpack the zip file
// 3) Copy the egm.proto file to a sub catalogue where protobuf-csharp was un-zipped, e.g. ~\protobuf-csharp\tools\egm
// 4) Generate an egm C# file from the egm.proto file by typing in a windows console: protogen .\egm\egm.proto --proto_path=.\egm
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

            switch (args[0])
            {
                case null:
                    Console.WriteLine("args is null");
                    break;

                case "Camera":
                    mode = Settings.Mode.Camera;
                    break;

                case "Simulate":
                    mode = Settings.Mode.Simulate;
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

                Console.ReadLine();   
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
        private Position _position = new Position();
        private bool _isFirstLoop = true;

        public bool ExitThread = false;
        public TextWriter Positionfile = new StreamWriter(@"C:\Users\Isi-Konsulent\Documents\GitHub\ExternalGuidedMotion\position.txt", true);
        public TextWriter ExecutionTime = new StreamWriter(@"C:\Users\Isi-Konsulent\Documents\GitHub\ExternalGuidedMotion\ExecutionTime.txt", true);
        

        public double XRobot { get; set; }
        public double YRobot { get; set; }
        public double ZRobot { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Z;



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
                    _path = new Path(_position);
                    _path.StartPath();
                    _stopwatch = _path.TimeElapsed;
                    break;

                default:
                    break;        
            }
        }

        public void PathSetPos()
        {
            X = _position.X;
            Y = -20;
            Z = 0;
        }

        public void CameraSetPos()
        {
            X = _camera.X;
            Y = _camera.Y;
            Z = 0;
        }


        public void SavePositionToFile()
        {
            Positionfile.WriteLine(_stopwatch.ElapsedMilliseconds.ToString("#.##") + " " +
                        X.ToString("#.##") + " " +
                        Y.ToString("#.##") + " " +
                        Z.ToString() + " " +
                        XRobot.ToString("#.##") + " " +
                        YRobot.ToString("#.##") + " " +
                        ZRobot.ToString("#.##"));
        }


        public void SensorThread()
        {
            // create an udp client and listen on any address and the port IpPortNumber
            _udpServer = new UdpClient(Program.IpPortNumber);
            var remoteEp = new IPEndPoint(IPAddress.Any, Program.IpPortNumber);

            while (ExitThread == false)
            
                // Get the message from robot
                var data = _udpServer.Receive(ref remoteEp);

                if (data != null)
                {
                    if (mode == Settings.mode.Camera)
                    {
                        CameraSetPos();
                    }

                    else if (mode == Settings.mode.Simulate)
                    {
                        PathSetPos();
                    }

                    else
                    {
                        Console.WriteLine("No leagal options!")
                    }

                    // de-serialize inbound message from robot using Google Protocol Buffer
                    EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();

                    // display inbound message
                    DisplayInboundMessage(robot);

                    XRobot = robot.FeedBack.Cartesian.Pos.X;
                    YRobot = robot.FeedBack.Cartesian.Pos.Y;
                    ZRobot = robot.FeedBack.Cartesian.Pos.Z;

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
                        _path.StartDisc();
                        _isFirstLoop = false;
                    }

                    // Write the postition to file
                    SavePositionToFile();
                   
                }
            } 
        }

        // Display message from robot
        void DisplayInboundMessage(EgmRobot robot)
        {
            if (robot.HasHeader && robot.Header.HasSeqno && robot.Header.HasTm)
            {
                Console.WriteLine("Seq={0} tm={1}",
                    robot.Header.Seqno.ToString(), robot.Header.Tm.ToString());
            }
            else
            {
                Console.WriteLine("No header in robot message");
            }
        }

        //////////////////////////////////////////////////////////////////////////
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
            _path.StopPath();
            Stopwatch.Stop();
            Stopwatch.Reset();
        }
    }  
}



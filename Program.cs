using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using abb.egm;
using System.Diagnostics;
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
            CameraWithPlot,
            CameraWithoutPlot,
            SimulatedDiscWithPlot,
            SimulatedDiscWithoutPlot
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

            //Check for null array
            if (args == null)
            {
                Console.WriteLine("args is null"); 
            }

            else if (args[0] == "camerap")
            {
                mode = Settings.Mode.CameraWithPlot;
            }
            else if (args[0] == "camera")
            {
                mode = Settings.Mode.CameraWithoutPlot;
            }
            else if (args[0] == "simulatep")
            {
                mode = Settings.Mode.SimulatedDiscWithPlot;
            }
            else if (args[0] == "simulate")
            {
                mode = Settings.Mode.SimulatedDiscWithoutPlot;
            }

            else
            {
                Console.WriteLine("Legal options: camerap, camera, simulatep, simulate");
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
        public bool ExitThread = false;
        private uint _seqNumber = 0;
        private Settings.Mode mode;
        public Stopwatch Stopwatch = new Stopwatch();
        private Camera camera = new Camera();
        public TextWriter Positionfile = new StreamWriter(@"C:\Users\Robot\Documents\ABB_Masteroppgave EGM Orjan\ExternalGuidedMotion-master\position.txt", true);
        public TextWriter ExecutionTime = new StreamWriter(@"C:\Users\Robot\Documents\ABB_Masteroppgave EGM Orjan\ExternalGuidedMotion-master\ExecutionTime.txt", true);
        public DateTime StartTime = DateTime.Now;

        public double XRobot { get; set; }
        public double YRobot { get; set; }
        public double ZRobot { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
        public int Z;

        public bool IsFirstLoop = true;
        public TimerCallback Tcb;


        public Sensor(Settings.Mode mode)
        {
            this.mode = mode;

            switch (mode)
            {
                case Settings.Mode.CameraWithPlot:
                    camera.StartCamera();
                    break;

                case Settings.Mode.CameraWithoutPlot:
                    camera.StartCamera();
                    break;

                case Settings.Mode.SimulatedDiscWithPlot:
                    Path path = new Path();
                    break;

                case Settings.Mode.SimulatedDiscWithoutPlot:
                    path = new Path();
                    break;
            }
        }


        // Get x and y position from the camera.
        public void CameraXY()
        {
            X = camera.X;
            Y = camera.Y;
            Z = 0;
        }

        // Write the time which the camera use to take and process a new image. 
        public void CameraWithPlot()
        {
            ExecutionTime.WriteLine(camera.timeElapsed);
        }

        // Get x and y position from the simulated disc
        public void PathXY()
        {
            Path path = new Path();
            // Convert the postition data from m to mm
            X = path.Position * 1000;

            // Set a limit for the robot down the ramp, to avoid "Mechanical unit close to joint bound" 
            if (X > 1000)
            {
                X = 1000;
            }

            if (IsFirstLoop)
            {
               Y = - new Random().Next(0, 101);
            }
            
            Z = 0;  
        }

        // Plot the position data of the disc and the robot. 
        public void PathWithPlot()
        {
            Positionfile.WriteLine(Stopwatch.ElapsedMilliseconds.ToString("#.##") + " " +
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
            {
                Stopwatch.Start();

                // Get the position data form the camera
                if(mode == Settings.Mode.CameraWithoutPlot || mode == Settings.Mode.CameraWithPlot)
                {
                    CameraXY();
                    IsFirstLoop = false;
                }
                // Get the position data from the simulated disc
                else if(mode == Settings.Mode.SimulatedDiscWithPlot || mode == Settings.Mode.SimulatedDiscWithoutPlot)
                {
                    PathXY(); 
                }

                else
                {
                    Console.WriteLine("No legal options");
                }

                // Get the message from robot
                var data = _udpServer.Receive(ref remoteEp);

                if (data != null)
                {
                    
                    if (IsFirstLoop)
                    {
                        StartTime = DateTime.Now;
                        IsFirstLoop = false; 
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

                    // Write the elapsed time to file for logging. 
                    if (mode == Settings.Mode.CameraWithPlot)
                    {
                        CameraWithPlot();
                        PathWithPlot();
                    }
                    // Write the position data of the disc and robot to file
                    else if (mode == Settings.Mode.SimulatedDiscWithPlot)
                    {
                        PathWithPlot();
                    }
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
            Stopwatch.Stop();
            Stopwatch.Reset();
        }
    }  
}



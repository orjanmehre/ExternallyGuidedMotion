using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using abb.egm;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media.Media3D;










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
    class Program
    {
        // listen on this port for inbound messages
        public static int _ipPortNumber = 6510;
        public static int _cameraIpPortNumber = 3000;
        private static bool _exit = false;

        static void Main(string[] args)
        {

          

            Camera c = new Camera();
            c.StartCamera();

            /*
            Sensor s = new Sensor();
            s.Start();

            Console.CancelKeyPress += delegate
            {
                s.Stop();
            };
            */
            Console.ReadLine();
        }
    }

    public class Sensor
    {
        private Thread _sensorThread = null;
        private UdpClient _udpServer = null;
        public bool exitThread = false;
        private uint _seqNumber = 0;
        Path path = new Path();

        // Write poisition data to txt file for plotting
        TextWriter positionfile = new StreamWriter
            (@"C:\Users\Isi-Konsulent\Documents\GitHub\ExternalGuidedMotion\position.txt", false);

        public DateTime startTime = DateTime.Now;
        int Y = new Random().Next(0, 301);
        int Z = 0; 
        public double X { get; set; }

        

        public double xRobot { get; set; }
        public double yRobot { get; set; }
        public double zRobot { get; set; }


        public void SensorThread()
        {
            // create an udp client and listen on any address and the port _ipPortNumber
            _udpServer = new UdpClient(Program._ipPortNumber);
            var remoteEP = new IPEndPoint(IPAddress.Any, Program._ipPortNumber);
            TimerCallback tcb = path.Time;
            bool isFirstLoop = true;
            
            while (exitThread == false)
            {
                
                // get the message from robot
                var data = _udpServer.Receive(ref remoteEP);

                if (data != null)
                {
                    if (isFirstLoop == true)
                    {
                        Timer timer = new Timer(tcb, exitThread, 60, 33);
                        startTime = DateTime.Now;
                        isFirstLoop = false; 
                    }

                    // de-serialize inbound message from robot using Google Protocol Buffer
                    EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();

                    // display inbound message
                    DisplayInboundMessage(robot);
                    //Debug.WriteLine(robot.ToString());

                    xRobot = robot.FeedBack.Cartesian.Pos.X;
                    yRobot = robot.FeedBack.Cartesian.Pos.Y;
                    zRobot = robot.FeedBack.Cartesian.Pos.Z;

                    
                    // create a new outbound sensor message
                    EgmSensor.Builder sensor = EgmSensor.CreateBuilder();
                    CreateSensorMessage(sensor);
                    //Debug.WriteLine(sensor.ToString());

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        EgmSensor sensorMessage = sensor.Build();
                        sensorMessage.WriteTo(memoryStream);

                        
                        // send the udp message to the robot
                        int bytesSent = _udpServer.Send(memoryStream.ToArray(),
                                           (int)memoryStream.Length, remoteEP);
                        if (bytesSent < 0)
                        {
                            Console.WriteLine("Error send to robot");
                        }
                    }

                    

                    //Write position data to txt file
                    positionfile.WriteLine( path.time.ToString("#.##") + " " +
                       sensor.Planned.Cartesian.Pos.X.ToString("#.##") + " " + 
                       sensor.Planned.Cartesian.Pos.Y.ToString("#.##") + " " + 
                       sensor.Planned.Cartesian.Pos.Z.ToString()       + " " + 
                       robot.FeedBack.Cartesian.Pos.X.ToString("#.##") + " " + 
                       robot.FeedBack.Cartesian.Pos.Y.ToString("#.##") + " " + 
                       robot.FeedBack.Cartesian.Pos.Z.ToString("#.##"));
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

            // To get the pos in mm
            this.X = path.position * 1000;

            // Set a limit for the robot down the ramp, to avoid "Mechanical unit close to joint bound"
            if (X > 4000)
            {
                X = 4000; 
            }


            pc.SetX(X)
              .SetY(-Y)
              .SetZ(Z);

            pq.SetU0(0.0)
              .SetU1(0.0)
              .SetU2(0.0)
              .SetU3(0.0);

            pos.SetPos(pc)
                .SetOrient(pq);


            planned.SetCartesian(pos);  // bind pos object to planned
            sensor.SetPlanned(planned); // bind planned to sensor object

            return;
        }

        // Start a thread to listen on inbound messages
        public void Start()
        {
            _sensorThread = new Thread(new ThreadStart(SensorThread));
            _sensorThread.Start(); 
        }

        // Stop and exit thread
        public void Stop()
        {
            positionfile.Close();
            exitThread = true;
            _sensorThread.Abort();        
        }
    }  
}



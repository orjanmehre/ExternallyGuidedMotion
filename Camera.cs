﻿using System;
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
        public bool exitThread = false;

        Stopwatch stopwatch = new Stopwatch();

        /*
        public TextWriter executionTime = new StreamWriter
            (@"C:\Users\Isi-Konsulent\Documents\GitHub\ExternalGuidedMotion\executionTime.txt", true);
        */
        public double X { get; set; }
        public double Y { get; set; }

        public void CameraThread()
        {
            _cameraUdpServer = new UdpClient(Program._cameraIpPortNumber);
            var cameraRemoteEP = new IPEndPoint(IPAddress.Any, Program._cameraIpPortNumber);

            while (exitThread == false)
            {
                //Measure the time it takes to get new position data from the camera. 
                stopwatch.Start(); 

                var cameraData = _cameraUdpServer.Receive(ref cameraRemoteEP);

                if(cameraData != null)
                {
                    var cameraXY = Encoding.Default.GetString(cameraData);

                    string[] XY = cameraXY.Split(',');

                    for(int i = 0; i < XY.Length; i++)
                    {
                        XY[i] = XY[i].Trim();
                    }

                    string tempX = XY[0].Replace('.', ',');
                    string tempY = XY[1].Replace('.', ',');

                    X = Convert.ToDouble(tempX);
                    Y = Convert.ToDouble(tempY);

                    /*
                    executionTime.WriteLine(stopwatch.ElapsedMilliseconds);
                    */

                    stopwatch.Stop();
                    stopwatch.Reset();

                    Console.WriteLine(X + " " + Y);   

                }
                else
                {
                    Console.WriteLine("No data from camera");
                }
            }
        }

        public void StartCamera()
        {
            _cameraThread = new Thread(new ThreadStart(CameraThread));
            _cameraThread.Start();
        }

    }
}

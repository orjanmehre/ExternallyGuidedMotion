using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalGuidedMotion
{
    class Camera
    {
        private Thread _cameraThread = null;
        private UdpClient _cameraUdpServer = null;
        public bool exitThread = false;

        public void CameraThread()
        {
            _cameraUdpServer = new UdpClient(Program._cameraIpPortNumber);
            var cameraRemoteEP = new IPEndPoint(IPAddress.Any, Program._cameraIpPortNumber);

            while (exitThread == false)
            {
                var cameraData = _cameraUdpServer.Receive(ref cameraRemoteEP);

                if(cameraData != null)
                {
                 
                    string dataFromCamera = Encoding.ASCII.GetString(cameraData);
                    Console.WriteLine(dataFromCamera);
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

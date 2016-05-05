using System;
using System.Diagnostics;
using System.Threading;


/// <summary>
/// This class generate simulated position data for a object friction less sliding down a ramp.
/// The data that is generated is in one dimmension.
/// This data is used in simulation and testing. 
/// </summary>



namespace ExternalGuidedMotion
{
    public class SimDisc
    {
        private Stopwatch _stopwatch;
        private Thread _SimDiscThread;
        private Position _updatePos;
        private bool _hasDiscStarted; 

        public const double ANGLE = 5;
        public bool ExitThread = false;

        public double Position { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double TimeElapsed { get; set; }

        public SimDisc(Position _updatePos)
        {
            _stopwatch = new Stopwatch();
            this._updatePos = _updatePos;
            _updatePos.SetPosition(0, 0, 0, 0);
        }

        public void SimDiscThread()
        {
           
            while (ExitThread == false)
            {
                if (_hasDiscStarted)
                {
                    Y = -0.1;
                    Z = 0;
                }
                else
                {
                    Y = 0;
                    Z = 0; 
                }
                
                TimeElapsed = _stopwatch.ElapsedMilliseconds / 1000d;
                double speed = CalculateSpeed(TimeElapsed);
                Position = CalculatePosition(speed, TimeElapsed);
                if (Position > 1)
                {
                    Position = 1;
                }
                _updatePos.SetPosition(Position, Y, Z, TimeElapsed);
            }
        }

        public double CalculateSpeed(double time)
        {
            double acceleration = 9.81 * Math.Sin(ANGLE * Math.PI/180);
            double speed = acceleration * time; 
            return speed;
        }

        public double CalculatePosition(double speed, double time)
        {
            double half = (double)1 / 2;
            double position = half * speed * time;
            return position;
        }

        public void StartDisc()
        {
            _stopwatch.Start();
            _hasDiscStarted = true;
        }

        public void StartSimDisc()
        {
            _SimDiscThread = new Thread(SimDiscThread);
            _SimDiscThread.Start();
        }

        public void StopSimDisc()
        {
            ExitThread = true;
            _SimDiscThread.Abort();
        }
    }
}

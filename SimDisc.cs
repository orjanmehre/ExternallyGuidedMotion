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

        public const double ANGLE = 10;
        public bool ExitThread = false;

        public double Position { get; set; }
        public double TimeElapsed { get; set; }

        public SimDisc(Position _updatePos)
        {
            _stopwatch = new Stopwatch();
            this._updatePos = _updatePos;
        }

        public void SimDiscThread()
        {
            TimeElapsed = 0;
            Position = 0;

            while (ExitThread == false)
            {
                TimeElapsed = _stopwatch.ElapsedMilliseconds / 1000d;
                double speed = CalculateSpeed(TimeElapsed);
                Position = CalculatePosition(speed, TimeElapsed);
                if (Position > 1)
                {
                    Position = 1;
                }
                _updatePos.SetPosition(Position,TimeElapsed);
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

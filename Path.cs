using System;


/// <summary>
/// This class generate simulated position data for a object friction less sliding down a ramp.
/// The data that is generated is in one dimmension.
/// This data is used in simulation and testing. 
/// </summary>



namespace ExternalGuidedMotion
{
    public class Path
    {
        public const double ANGLE = 45;
        private int _i = 1;
        public double Position { get; set; }
        public double Time { get; set; }
        
        public void StartPath()
        {
            Time = (double)(33 * _i) / 1000;
            double speed = CalculateSpeed(Time);
            Position = CalculatePosition(speed, Time);    
            _i++;
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
    }
}

using System;
using System.Windows.Forms;



/// This class generate simulated position data for a object sliding down a ramp.
/// This data is used in simulation and testing. 

namespace ExternalGuidedMotion
{
    public class Path
    {
        public const double WEIGHT = 0.01;
        public const double ANGLE = 1;
        public int i = 1;


        public void Time()
        {
            Timer timer = new Timer();
            timer.Interval = 30;
            timer.Start();
            timer.Tick += TimerCallback_Handler; 
        }

        public void TimerCallback_Handler(object sender, EventArgs args)
        {
            double time = 30 * i;
            double speed = CalculateSpeed(time); 
            double position = CalculatePosition(speed, time);
            Console.WriteLine(speed.ToString() + " " + position.ToString());
            i++; 
        }

        public double CalculateSpeed(double time)
        {
            double forceAccel = WEIGHT * 9.81 * Math.Sin(ANGLE);
            double acceleration = WEIGHT / forceAccel;
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

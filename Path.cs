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


        public void Time()
        {
            Timer timer = new Timer();
            timer.Tick += TimerCallback_Handler;
            timer.Interval = 1000;
            timer.Start();
        }

        public static void TimerCallback_Handler(object sender, EventArgs args)
        {
            Console.WriteLine("Tick");
        }

        public double CalculateSpeed()
        {
            double forceAccel = WEIGHT * 9.81 * Math.Sin(ANGLE);
            double acceleration = WEIGHT / forceAccel;
            double speed = acceleration * stopwatch.Elapsed.TotalMilliseconds;
            CalculatePosition(speed);
            return speed;
        }

        public double CalculatePosition(double speed)
        {
            double position = (1/2) * (speed) * stopwatch.Elapsed.TotalMilliseconds;
            return position;
        }
    }
}

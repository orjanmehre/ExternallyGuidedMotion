using System;
using System.Windows.Forms;
using System.Diagnostics;



/// This class generate simulated position data for a object sliding down a ramp.
/// The data that is generated is in one dimmension.
/// This data is used in simulation and testing. 

namespace ExternalGuidedMotion
{
    public class Path
    {
        public const double ANGLE = 30;
        public int i = 1;


        public void Time()
        {
            Timer timer = new Timer();
            // 30fps.
            timer.Interval = 33; 
            timer.Start();
            timer.Tick += TimerCallback_Handler; 
        }

        public void TimerCallback_Handler(object sender, EventArgs args)
        {
            // In sec.
            double time = (double)(30 * i)/1000; 
            double speed = CalculateSpeed(time); 
            double position = CalculatePosition(speed, time);
            Console.WriteLine(speed.ToString() + " " + position.ToString());
            Debug.WriteLine("Speed: " + String.Format("{0:0.0000}" ,speed)+ " | Time: " + String.Format("{0:0.0000}", time) + " | Pos: " + String.Format("{0:0.0000}", position));
            i++; 
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

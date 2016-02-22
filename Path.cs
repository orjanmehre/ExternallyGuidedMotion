﻿using System;


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
        public int i = 1;
        public double position { get; set; }
        public double time { get; set; }
        
        public void Time(Object stateInfo)
        {
            time = (double)(33 * i) / 1000;
            double speed = CalculateSpeed(time);
            position = CalculatePosition(speed, time);    
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

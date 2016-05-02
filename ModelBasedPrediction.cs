using System;

/// <summary>
/// Finds the 1D distace of a object sliding down a ramp. 
/// Input parameters are angle of the ramp, gravity, time and the coefficient of kinetic friction. 
/// Output parameter is the distance in 1D
/// </summary>

namespace ExternalGuidedMotion
{
    public class ModelBasedPrediction
    {
        private double _g;
        private double _angel;
        private double _time;
        private double _friction;
        private double _accel;
        private double _vel;

        public double Dist
        {
            get
            {
                return (0.5)*(Velocity()*_time);
            }
        }


        public ModelBasedPrediction(double _angel, double _g, double _time, double _friction)
        {
            this._angel = _angel * (Math.PI / 180);
            this._g = _g;
            this._time = _time;
            this._friction = _friction;
        }

        public double Acceleration()
        {
            _accel = _g*Math.Sin(_angel) - (_friction*_g*Math.Cos(_angel));

            return _accel;
        }

        public double Velocity()
        {
            _vel = Acceleration()*_time;

            return _vel;
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ExternalGuidedMotion
{
    public class Predictor
    {
        private double _frames;
        private double _prevPos;
        private double _currentPosition; 
        private double _currentVelocity;
        private double _currentTime;
        private double _a;
        private double _g;
        private double _fricKoeff;
        private double _angle;


        public double PredictedPosition { get; set; }
        public double PredictedTime { get; set; }

        public Predictor()
        {
            _g = 0.00981;
            _fricKoeff = 0.5;
            _angle = 30 * (Math.PI / 180);
            _prevPos = 1;
            _frames = 30;
            
        }

        public void NewPrediction(double time, double position)
        {
            _currentTime = time;
            _currentPosition = position;
            checkCurrentPosition();
            velocity();
            _a = acceleration();
            predictedPosition();
            predictedTime();
            _prevPos = _currentPosition;
        }

        private void checkCurrentPosition()
        {
            if(_currentPosition > (_prevPos + 200))
            {
                _currentPosition = _prevPos;
            }
            else
            {
                _prevPos = _currentPosition;
            }
        }

        private double velocity()
        {
            _currentVelocity = Math.Abs((_currentPosition - _prevPos)/(_currentTime));
 
            return _currentVelocity; 
        }

        private double acceleration()
        {
            _a = -(_g * Math.Sin(_angle)) - (_fricKoeff * _g * Math.Cos(_angle));
            return _a;
        }

        private void predictedPosition()
        {
            PredictedPosition = _currentPosition + _currentVelocity * _currentTime*_frames + (1/2)*(_a*Math.Pow(_currentTime*_frames,2));

            if (PredictedPosition > 1150)
            {
                PredictedPosition = 1150;
            }
        }

        private void predictedTime()
        {
            PredictedTime = _currentTime* _frames;
        }
    }
}

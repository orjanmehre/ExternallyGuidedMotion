using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalGuidedMotion
{
    public class Predictor
    {

        private double _prevPosition;
        private double _currentPosition; 
        private double _prevVelocity;
        private double _currentVelocity;
        public double _a { get; set; }
        private double _prevTime;
        private double _currentTime;
        public double _error { get; set; }

        public double PredictedPosition { get; set; }
       
        public Predictor()
        {
            _prevPosition = 0;
            _prevTime = 0;
            _prevVelocity = 0;
            _error = 0;
        }

        public void NewPrediction(double time, double position)
        {
            this._currentTime = time;
            this._currentPosition = position;
            setError();
            predictedPosition();
            _a = acceleration();
            setPrevPosition();
            setPrevVelocity();
        }


        private void setPrevPosition()
        {
            _prevPosition = _currentPosition;
        }

        private void setPrevVelocity()
        {
            _prevVelocity = _currentVelocity;
        }


        private double velocity()
        {
            _currentVelocity = (double) (_currentPosition - _prevPosition)/_currentTime;
 
            return _currentVelocity; 
        }

        private double acceleration()
        {
            _a = (double) (_currentVelocity - _prevVelocity)/
                 (_currentTime + _prevTime);
            return _a;
        }

        private void predictedPosition()
        {
            PredictedPosition = _currentPosition + velocity()*_currentTime + _error;
            
        }

        private void setError()
        {
            _error = _currentPosition - PredictedPosition;

            if (_error < 0 || _error > 100)
            {
                _error = 0;
                Console.WriteLine("Error has an unnatural value");
            }
        }
    }
}

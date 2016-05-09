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
        private double _a;
        private double _prevTime;
        private double _currentTime;

        public double PredictedPosition { get; set; }
       
        public Predictor()
        {
            _prevPosition = 313;
            _prevTime = 4;
            _prevVelocity = 1.75;
        }

        public void NewPrediction(double time, double position)
        {
            this._currentTime = time;
            this._currentPosition = position; 
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
            PredictedPosition = _currentPosition + velocity()*_currentTime;
        }
    }
}

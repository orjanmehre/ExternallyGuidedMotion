using System;
using System.Diagnostics;
using System.IO;

namespace ExternalGuidedMotion
{
    public class Predictor
    {

        private double _prevPosition;
        private double _currentPosition; 
        private double _prevVelocity;
        private double _currentVelocity;
        private double _prevPrevVelocity;
        private double _prevPrevPrevVelocity;
        private double _prevPrevPrevPrevVelocity;
        public double _a { get; set; }
        private double _prevTime;
        private double _currentTime;
        public double _error { get; set; }
        public TextWriter ExecutionTime = new StreamWriter(@"..\...\Test.txt", true);

        private double _regCoeff;

        public double PredictedPosition { get; set; }
       
        public Predictor()
        {
            _prevPosition = 0;
            _prevTime = 0;
            _prevVelocity = 1;
            _error = 0;
            _regCoeff = 1;
            _prevPrevVelocity = 1;
            _prevPrevPrevVelocity = 1;
            _prevPrevPrevPrevVelocity = 1;

        }

        public void NewPrediction(double time, double position)
        {
            this._currentTime = time;
            this._currentPosition = position;
            setError();
            regressionCoeffisient();
            predictedPosition();
            _a = acceleration();
            setPrevPosition();
            setPrevVelocitys();
            WriteExecutionTimeToFile();
        }

        public void WriteExecutionTimeToFile()
        {
            ExecutionTime.WriteLine(_currentPosition.ToString() + " " + PredictedPosition.ToString() + " " + _currentVelocity.ToString() +   " " + _regCoeff.ToString());
        }

        private void setPrevPosition()
        {
            _prevPosition = _currentPosition;
        }

        private void setPrevVelocitys()
        {
            _prevPrevPrevPrevVelocity = _prevPrevPrevVelocity;
            _prevPrevPrevVelocity = _prevPrevVelocity;
            _prevPrevVelocity = _prevVelocity;
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

        private void setError()
        {
            _error = _currentPosition - PredictedPosition;

            if (_error < 0 || _error > 100)
            {
                _error = 0;
                //Console.WriteLine("Error has an unnatural value");
            }
        }

        private void regressionCoeffisient()
        {
            _regCoeff = Math.Abs(_currentVelocity / _prevPrevPrevPrevVelocity);
            
            if(_regCoeff < 0.5 || _regCoeff > 5 || _regCoeff == double.NaN)
            {
                _regCoeff = 1;
            }                     
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ExternalGuidedMotion
{
    public class Predictor
    {

        private double[] _prevPositions;
        private double[] _prevTimes;
        private double[] _prevVelocities;
        private double[] _predictedPositions;
        private double _prevPos;

        private double _currentPosition; 
        private double _currentVelocity;
        private double _currentTime;
        private double _a;
        private double _error;
        private double _regCoeff;
        private double _g;
        private double _fricKoeff;
        private double _angle;
        private int _pk; 


        public TextWriter ExecutionTime = new StreamWriter(@"..\...\Test.txt", true);

        public double PredictedPosition { get; set; }
       
        public Predictor()
        {
            _pk = 6;
            _prevTimes = new double[6] { 1, 1, 1, 1, 1, 1 };
            _prevPositions = new double[6] { 1, 1, 1, 1, 1, 1 };
            _prevVelocities = new double[6] { 1, 1, 1, 1, 1, 1 };
            _predictedPositions = new double[6] { 1, 1, 1, 1, 1, 1 };
            _error = 0;
            _regCoeff = 1;
            _g = 9.81;
            _fricKoeff = 0.5;
            _angle = 20 * (Math.PI / 180);
            _prevPos = 1;
            
        }

        public void NewPrediction(double time, double position)
        {
            _currentTime = time;
            _currentPosition = position;
            setError();
            velocity();
            regressionCoeffisient();
            _a = acceleration();
            predictedPosition();
            setPrevPositions();
            setPrevVelocitys();
            setPrevTimes();
            _prevPos = _currentPosition;
            WriteExecutionTimeToFile();
        }

        public void WriteExecutionTimeToFile()
        {
            ExecutionTime.WriteLine("Accel " + _a.ToString() + " Vel " + _currentVelocity.ToString() );
        }
        
        private void setPrevTimes()
        {
            for(int i = 1; i < _pk; i++)
            {
                _prevTimes[i - 1] = _prevTimes[i];
            }

            _prevTimes[0] = _currentTime;
        }

        private void setPrevPositions()
        {
            for(int i = 1; i < _pk; i++)
            {
                _prevPositions[i - 1] = _prevPositions[i];
            }

            _prevPositions[0] = _currentPosition;

        }

        private void setPrevVelocitys()
        {
            for(int i = 1; i < _pk; i++)
            {
                _prevVelocities[i - 1] = _prevVelocities[i];
            }

            _prevVelocities[0] = _currentVelocity;
        }


        private double velocity()
        {
            _currentVelocity = Math.Abs((_currentPosition - _prevPos)/(_currentTime));
 
            return _currentVelocity; 
        }

        private double acceleration()
        {
            _a = Math.Abs((_g * Math.Sin(_angle)) - (_fricKoeff * _g * Math.Cos(_angle))) * 0.001;
            return _a;
        }

        private void predictedPosition()
        {
            PredictedPosition = _currentPosition + _currentVelocity * _currentTime*7 + (1/2)*(_a*Math.Pow(_currentTime*7,2));
        }

        private void setError()
        {
            _error = _currentPosition - _predictedPositions[5];

            if (_error < 0)
            {
                _error = 0;
            }
        }

        private void regressionCoeffisient()
        {
            _regCoeff = Math.Abs(_currentVelocity / _prevVelocities[5]);
                             
        }
    }
}

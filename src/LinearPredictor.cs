using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Linear predictor
/// </summary>

namespace ExternalGuidedMotion
{
    public class LinearPredictor
    {
        public double[] _a { get; set; }
        private double[] _predictorCoefficients;
        public double[] _x { get; set; }
        private double[] _prevReadings;

        private double _ai;
        public double _error { get; set; }
        public int _arrayPointer { get; set; }
        private int _i;
        private int _k;
        private double _currentPosition;
        public double _newCoeff { get; set; }

        public double EstimatedPosition
        {
            get
            {
                return _a.ElementAt(4)*_x.ElementAt(4) +
                       _a.ElementAt(3)*_x.ElementAt(3) +
                       _a.ElementAt(2)*_x.ElementAt(2) +
                       _a.ElementAt(1)*_x.ElementAt(1);
            }
        }

        public LinearPredictor()
        {
            _arrayPointer = 0;
            _a = new double[5] { 0, 0, 0, 0, 0 };
            _x = new double[5] { 0, 0, 0, 0, 0 };
            _predictorCoefficients = new double[30000];
            Array.Clear(_predictorCoefficients, 0, 30000);
            _prevReadings = new double[30000];
            Array.Clear(_prevReadings, 0, 30000);
        }

        public void UpdateEstimate(double currentPosition)
        {
            this._currentPosition = currentPosition;
            UpdateError(EstimatedPosition, currentPosition);
            PrevReadings(currentPosition);
            _newCoeff = CalculatePredictorCoefficients();
            PredictorCoefficients(_newCoeff);
            UpdateArrayPointer();
        }

        private void PredictorCoefficients(double a)
        {
            _predictorCoefficients[_arrayPointer] = a;
            _k = 0;

            for (_i = _arrayPointer; _i > _arrayPointer - 4 && _i > 0; _i--)
            {
                _a[_k] = _predictorCoefficients[_i];
                _k++;

            }
        }

        private void PrevReadings(double currentPosition)
        {
            _prevReadings[_arrayPointer] = currentPosition;
            _k = 0; 

            for (_i = _arrayPointer; _i > _arrayPointer - 4 && _i > 0; _i--)
            {
                _x[_k] = _prevReadings[_i];
                _k++;
            }
        }
    
        private double CalculatePredictorCoefficients()
        {
            _ai = -(((_x.ElementAt(4) * _a.ElementAt(4)) + (_x.ElementAt(3) * _a.ElementAt(3)) + 
                (_x.ElementAt(2) * _a.ElementAt(2)) + (_x.ElementAt(1) * _a.ElementAt(1))) /_x.ElementAt(0))  - _error;
            return _ai;
        }

        private void UpdateArrayPointer()
        {
            _arrayPointer = _arrayPointer + 1; 
        }

        private void UpdateError(double prevEstimate, double currentPosition)
        {
            _error = currentPosition - prevEstimate;
        }
    }
}

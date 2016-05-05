using System;
using System.Collections.Generic;
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
        private double[] _a;
        private double[] _predictorCoefficients;
        private double[] _x;
        private double[] _prevReadings;

        private double _ai;
        private double _error;
        private int _arrayPointer;
        private int _i;
        private int _k;
        private double _currentPosition;
        private double _newCoeff;

        public double EstimatedPosition
        {
            get
            {
                return _a.ElementAt(4) * _x.ElementAt(4) +
                       _a.ElementAt(3) * _x.ElementAt(3) +
                       _a.ElementAt(2) * _x.ElementAt(2) +
                       _a.ElementAt(1) * _x.ElementAt(1) +
                       _a.ElementAt(0) * _x.ElementAt(0);
            }
        }

        public LinearPredictor()
        {
            _arrayPointer = 0;
            _a = new double[4] { 0, 0, 0, 0 };
            _x = new double[4] { 0, 0, 0, 0 };
            _predictorCoefficients = new double[] { 0 };
            _prevReadings = new double[] { 0 };
        }

        public void UpdateEstimate(double currentPosition)
        {
            this._currentPosition = currentPosition;
            UpdateError(EstimatedPosition, currentPosition);
            _newCoeff = CalculatePredictorCoefficients(_a);
            PredictorCoefficients(_newCoeff);
            PrevReadings(currentPosition);
            UpdateArrayPointer();
        }

        private void PredictorCoefficients(double a)
        {
            _predictorCoefficients[_arrayPointer] = a;
            _k = 0;

            for (_i = _arrayPointer; _i > _arrayPointer - 4 && _i >= 0; _i--)
            {
                   _a[_k] = _predictorCoefficients[_i]; 

            }
        }

        private void PrevReadings(double currentPosition)
        {
            _prevReadings[_arrayPointer] = currentPosition;
            _k = 0; 

            for (_i = _arrayPointer; _i > _arrayPointer - 4 && _i >= 0; _i--)
            {
                _x[_k] = _prevReadings[_i];
            }
        }
    
        private double CalculatePredictorCoefficients(double[] a)
        {
             _a = a;
            _ai = -((_x.ElementAt(4) *_a.ElementAt(4)) 
                +  ( _x.ElementAt(3) *_a.ElementAt(3)) + (_x.ElementAt(2)*_a.ElementAt(2))
                +  ( _x.ElementAt(1) * _a.ElementAt(1))) / (_x.ElementAt(0)) - _error;
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

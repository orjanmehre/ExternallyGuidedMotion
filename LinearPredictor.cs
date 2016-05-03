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
    class LinearPredictor
    {
        private double[] _a = new double[4];
        private double[] _predictorCoefficients = new double[] { 0 };
        private double[] _x = new double[4];
        private double[] _prevReadings = new double[] { 0 };

        private double _ai;
        private double _error;
        private int _arrayIndex;
        private int _i;
        private double _currentPosition;

        public double EstimatedPosition
        {
            get
            {
                return _a.ElementAt(1) * _x.ElementAt(1) +
                       _a.ElementAt(2) * _x.ElementAt(2) +
                       _a.ElementAt(3) * _x.ElementAt(3) +
                       _a.ElementAt(4) * _x.ElementAt(4);
            }
        }

        public LinearPredictor(double currentPosition)
        {
            this._currentPosition = currentPosition; 

        }

        private void PredictorCoefficients(double a)
        {
            _predictorCoefficients[_arrayIndex] = a;

            for (_i = _arrayIndex;  _i < _arrayIndex + 4; _i++)
            {
                _a[_i - _arrayIndex] = a;
            }

        }

        private void PrevReadings(double currentPosition)
        {
            _prevReadings[_arrayIndex] = currentPosition;

            for (_i = _arrayIndex; _i < _arrayIndex + 4; _i++)
            {
                _x[_i - _arrayIndex] = currentPosition;
            }
        }
    


        private double CalculatePredictorCoefficients(double[] a)
        {
            _a = a;
            _ai = -((_x.ElementAt(1)*_a.ElementAt(1)) + (_x.ElementAt(2)*_a.ElementAt(2)) + (_x.ElementAt(3)*_a.ElementAt(3)) +
                  (_x.ElementAt(4)*_a.ElementAt(4)))/(_x.ElementAt(0)) - _error;

            return _ai;
        }

        private void UpdateArrayIndex()
        {
            _arrayIndex = _arrayIndex + 1; 
        }
  


    }
}

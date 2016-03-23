using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

/// <summary>
/// Kalmanfilter used to filter the position data from the camera. 
/// 
/// </summary>

namespace ExternalGuidedMotion
{
    class KalmanFilter
    {
        private Camera _camera;
        private double _pos;
        private double _vel;
        private double _prevPos;
        private double _time;
        private double _prevTime;
        Matrix<double> stateMatrix;
        Matrix<double> transMatrix;


        public KalmanFilter(Camera _camera)
        {
            this._camera = _camera;
        }

        public Matrix<double> StateMatrix()
        {
            stateMatrix = DenseMatrix.OfArray(new double[,]
            {
                {_pos},
                {_vel}
            });
            return stateMatrix;
        }

        public Matrix<double> TrasMatrix()
        {

            transMatrix = DenseMatrix.OfArray(new double[,]
            {
                {_camera.X, 0},
                {(double)(_pos - _prevPos) / (_time - _prevTime), 0}
            });
            return transMatrix;
        }



    }
}

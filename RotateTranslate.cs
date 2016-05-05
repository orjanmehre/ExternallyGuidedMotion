using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


/// <summary>
/// This class is used to rotate and tanslate a coordinate system to another coordinate system
/// 
/// </summary>

namespace ExternalGuidedMotion
{
    public class RotateTranslate
    {
        private Camera _camera;
        private double _transX = 91;
        private double _transY = -732;
        private double _transZ = 666;
        private double _theta = -30;
        private double _gamma = 150;
        private double _tau = 0;
        public Vector<double> newXYCord = Vector<double>.Build.Dense(4);

        public double X;
        public double Y;
        public double Z;


        public RotateTranslate(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z; 
        }

        private Matrix<double> RotateZ()
        {
            Matrix<double> _rotZ = DenseMatrix.OfArray(new double[,] 
            {
            {Math.Cos(_theta), -Math.Sin(_theta), 0, 0},
            {Math.Sin(_theta), Math.Cos(_theta), 0, 0},
            {0, 0, 1, 0},
            {0, 0, 0, 1} });
            return _rotZ;
        }

        private Matrix<double> RotateY()
        {
            Matrix<double> _rotY = DenseMatrix.OfArray(new double[,]
            {
            {Math.Cos(_gamma), 0, Math.Sin(_gamma), 0},
            {0, 1, 0, 0},
            {-Math.Sin(_gamma), 0, Math.Cos(_gamma), 0},
            {0, 0, 0, 1} });
            return _rotY;
        }

        private Matrix<double> RotateX()
        {
            Matrix<double> _rotX = DenseMatrix.OfArray(new double[,]
            {
            {1, 0, 0, 0},
            {0, Math.Cos(_tau), -Math.Sin(_tau), 0},
            {0, Math.Sin(_tau), Math.Cos(_tau), 0},
            {0, 0, 0, 1} });
            return _rotX;
        }

        private Matrix<double> TranslationMatrix()
        {
            Matrix<double> _trans = DenseMatrix.OfArray(new double[,]
            {
            {1, 0, 0, _transX},
            {0, 1, 0, _transY},
            {0, 0, 1, _transZ},
            {0, 0, 0, 1} });
            return _trans;
        }

        private Matrix<double> RotateZYX()
        {
            Matrix<double> _rotZYX = (RotateZ())*(-RotateY())*(RotateX());
            return _rotZYX;
        }

        private Matrix<double> TransformationMatrix()
        {
            Matrix<double> _transform = (TranslationMatrix() * RotateZYX());
            return _transform;
        }

        public Vector<double> RotatedTranslatedCord()
        {
            var _cameraXY = Vector<double>.Build.Dense(new[] {X, Y, 0, 1 });

            newXYCord = TransformationMatrix()*_cameraXY;
            return newXYCord;
        }

    }
}

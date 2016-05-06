using System;
using System.CodeDom;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExternalGuidedMotion;
using MathNet.Numerics.LinearAlgebra;


namespace EgmTest
{
    [TestClass]
    public class RotateTranslateTest
    {
        public Vector<double> newCord = Vector<double>.Build.Dense(4);
        [TestMethod]
        public void IsRotateTranslateCorrect()
        {
            double X = 0;
            double Y = 0;
            double Z = 0;
            double coordinateAndreas = 9001;
            bool translateNotCorrect = true;

            RotateTranslate FromWobjToBase = new RotateTranslate(X,Y,Z);
            newCord = FromWobjToBase.RotatedTranslatedCord();
            Debug.WriteLine(newCord.ToString());

        }
    }
}

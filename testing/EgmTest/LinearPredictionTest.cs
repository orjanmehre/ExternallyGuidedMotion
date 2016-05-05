using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExternalGuidedMotion;

namespace EgmTest
{
    [TestClass]
    public class LinearPredictionTest
    {


        [TestMethod]
        public void TestLinearPrediction()
        {
            double p1 = 320.00;
            double p2 = 327.00;
            double p3 = 334.00;
            double p4 = 334.00;
            double p5 = 349.00;
            double p6 = 356.00;

            LinearPredictor linearPredictor = new LinearPredictor();

            linearPredictor.UpdateEstimate(p1);
            Debug.WriteLine("EstimatedPos. " + linearPredictor.EstimatedPosition.ToString());
            Debug.WriteLine("Error " + linearPredictor._error.ToString());
            Debug.WriteLine("newCoef : " + linearPredictor._newCoeff.ToString());
            Debug.WriteLine("ArryPointer: " + linearPredictor._arrayPointer.ToString()); 
            Debug.WriteLine("PredCoeff: " + "[{0}]", string.Join(", ", linearPredictor._a));
            Debug.WriteLine("PrevRead : " + "[{0}]", string.Join(", ", linearPredictor._x));
            Debug.WriteLine("");
            

            linearPredictor.UpdateEstimate(p2);
            Debug.WriteLine("EstimatedPos. " + linearPredictor.EstimatedPosition.ToString());
            Debug.WriteLine("Error " + linearPredictor._error.ToString());
            Debug.WriteLine("newCoef : " + linearPredictor._newCoeff.ToString());
            Debug.WriteLine("ArrayPointer: " + linearPredictor._arrayPointer.ToString());
            Debug.WriteLine("PredCoeff: " + "[{0}]", string.Join(", ", linearPredictor._a));
            Debug.WriteLine("PrevRead : " + "[{0}]", string.Join(", ", linearPredictor._x));
            Debug.WriteLine("");

            linearPredictor.UpdateEstimate(p3);
            Debug.WriteLine("EstimatedPos. " + linearPredictor.EstimatedPosition.ToString());
            Debug.WriteLine("Error " + linearPredictor._error.ToString());
            Debug.WriteLine("newCoef : " +  linearPredictor._newCoeff.ToString());
            Debug.WriteLine("ArrayPointer: " + linearPredictor._arrayPointer.ToString());
            Debug.WriteLine("PredCoeff: " + "[{0}]", string.Join(", ", linearPredictor._a));
            Debug.WriteLine("PrevRead : " + "[{0}]", string.Join(", ", linearPredictor._x));
            Debug.WriteLine("");

            linearPredictor.UpdateEstimate(p4);
            Debug.WriteLine("EstimatedPos. " + linearPredictor.EstimatedPosition.ToString());
            Debug.WriteLine("Error " + linearPredictor._error.ToString());
            Debug.WriteLine("newCoef : " + linearPredictor._newCoeff.ToString());
            Debug.WriteLine("ArrayPointer: " + linearPredictor._arrayPointer.ToString());
            Debug.WriteLine("PredCoeff: " + "[{0}]", string.Join(", ", linearPredictor._a));
            Debug.WriteLine("PrevRead : " + "[{0}]", string.Join(", ", linearPredictor._x));
            Debug.WriteLine("");

            linearPredictor.UpdateEstimate(p5);
            Debug.WriteLine("EstimatedPos. " + linearPredictor.EstimatedPosition.ToString());
            Debug.WriteLine("Error " + linearPredictor._error.ToString());
            Debug.WriteLine("newCoef : " + linearPredictor._newCoeff.ToString());
            Debug.WriteLine("ArrayPointer: " + linearPredictor._arrayPointer.ToString());
            Debug.WriteLine("PredCoeff: " + "[{0}]", string.Join(", ", linearPredictor._a));
            Debug.WriteLine("PrevRead : " + "[{0}]", string.Join(", ", linearPredictor._x));
            Debug.WriteLine("");

            linearPredictor.UpdateEstimate(p6);
            Debug.WriteLine("EstimatedPos. " + linearPredictor.EstimatedPosition.ToString());
            Debug.WriteLine("Error " + linearPredictor._error.ToString());
            Debug.WriteLine("newCoef : " + linearPredictor._newCoeff.ToString());
            Debug.WriteLine("ArrayPointer: " + linearPredictor._arrayPointer.ToString());
            Debug.WriteLine("PredCoeff: " + "[{0}]", string.Join(", ", linearPredictor._a));
            Debug.WriteLine("PrevRead : " + "[{0}]", string.Join(", ", linearPredictor._x));
        }
    }
}
using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExternalGuidedMotion;

namespace EgmTest
{
    [TestClass]
    public class EgmUnitTest
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
            Debug.WriteLine(linearPredictor.EstimatedPosition.ToString());

            linearPredictor.UpdateEstimate(p2);
            Debug.WriteLine(linearPredictor.EstimatedPosition.ToString());

            linearPredictor.UpdateEstimate(p3);
            Debug.WriteLine(linearPredictor.EstimatedPosition.ToString());

            linearPredictor.UpdateEstimate(p4);
            Debug.WriteLine(linearPredictor.EstimatedPosition.ToString());

            linearPredictor.UpdateEstimate(p5);
            Debug.WriteLine(linearPredictor.EstimatedPosition.ToString());

            linearPredictor.UpdateEstimate(p6);
            Debug.WriteLine(linearPredictor.EstimatedPosition.ToString());


            // assert
            double actual = 364;
            Assert.AreEqual(linearPredictor.EstimatedPosition, actual, 10,
                "The prediction is off by: " +
                (actual - linearPredictor.EstimatedPosition));
        }
    }
}

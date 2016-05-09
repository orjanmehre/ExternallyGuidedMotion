using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExternalGuidedMotion;

namespace EgmTest
{
    [TestClass]
    public class PredictorTest
    {
        [TestMethod]
        public void TestPredictor()
        {

            int i;


            double[] postion = new double[5] {320, 327, 334, 341, 349};
            double[] time = new double[5] {4, 4, 4, 5, 3};


            Predictor predictor = new Predictor();

            for (i = 0; i < 5; i++)
            {
                predictor.NewPrediction(time[i], postion[i]);
                Debug.WriteLine(predictor.PredictedPosition.ToString());
            }
            

        }
    }
}

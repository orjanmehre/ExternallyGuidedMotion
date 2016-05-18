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


            double[] postion = new double[19] {180, 186, 197, 206, 209, 225, 232, 241, 252, 260, 269, 278, 289, 297, 310, 319, 329, 339, 351};
            double[] time = new double[19] {8, 8, 8, 8, 12, 8, 8, 8, 8, 13, 7, 8, 9, 8, 8, 8, 8, 8, 8};


            Predictor predictor = new Predictor();

            for (i = 0; i < 19; i++)
            {
                predictor.NewPrediction(time[i], postion[i]);
                Debug.WriteLine("Pos: "  + predictor.PredictedPosition.ToString());
            }
            

        }
    }
}

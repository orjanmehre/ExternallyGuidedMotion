using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExternalGuidedMotion;

namespace EgmTest
{
    [TestClass]
    public class TestOfModel
    {
        private double _position;
        private double _accel;
        private double _vel;

        [TestMethod]
        public void ModelTest()
        {
            double angel = 30;
            double g = 9.81;
            double time = 0.8;
            double friction = 0.5;

            ModelBasedPrediction testModel = new ModelBasedPrediction(angel, g, time, friction);

            _vel = testModel.Velocity();
            _accel = testModel.Acceleration();
            _position = testModel.Dist;
            Debug.WriteLine("Postition " + _position.ToString() + " Vel " +  _vel.ToString() + " Accel " + _accel.ToString());
        }
    }
}

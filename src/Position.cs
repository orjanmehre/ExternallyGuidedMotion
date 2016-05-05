
/// <summary>
/// Binding class between Program and Path.
/// Is made to avoid problems when executing two different thread.
/// </summary>


namespace ExternalGuidedMotion
{
   public class Position
   {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Time { get; set; }

        public void SetPosition(double position, double y, double z, double time)
        {
            Time = time;
            Y = y * 1000;
            Z = z * 1000;
            X = position * 1000;
            
            if (X > 1000)
            {
                X = 1000;
            }
        }
    }
}

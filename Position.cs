
/// <summary>
/// Binding class between Program and Path.
/// Is made to avoid problems when executing two different thread.
/// </summary>

namespace ExternalGuidedMotion
{
   public class Position
   {
       public double X { get; set; }

       public void SetPosition(double position)
        {
            X = position * 1000;

            if (X > 3000)
            {
                X = 3000;
            }
        }
    }
}

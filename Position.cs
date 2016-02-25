using System.Diagnostics;

/// <summary>
/// Binding class between Program and Path.
/// Is made to avoid problems when executing two different thread.
/// </summary>

namespace ExternalGuidedMotion
{
   public class Position
   {
       public double X { get; set; }

       public void SetPosition(double Position)
        {
            X = Position * 1000;

            if (X > 1000)
            {
                X = 1000;
            }
        }
    }
}

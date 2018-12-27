
using System.Drawing;

namespace LandscapeGenCore
{
    public class Simple3d : IRender
    {
        #region IRender Members

        public void Free ()
        {

        }

        public Bitmap Render (float [,] ResultGrid)
        {
            Bitmap result = new Bitmap (ResultGrid.GetLength (0), ResultGrid.GetLength (1));

            result.SetPixel (3, 3, Color.Green);


            return result;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace LandscapeGenCore
{
    public class Simple3d : IRender
    {
        #region IRender Members

        public void Free()
        {

        }

        public System.Drawing.Bitmap Render(float[,] ResultGrid)
        {
            Bitmap result = new Bitmap(ResultGrid.GetLength(0), ResultGrid.GetLength(1));

            result.SetPixel(3, 3, Color.Green);


            return result;
        }

        #endregion
    }
}

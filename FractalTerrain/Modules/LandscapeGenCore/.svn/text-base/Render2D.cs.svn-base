using System;
using System.Drawing;

namespace LandscapeGenCore
{
	public class TerranValues {
		public float waterLevel = 0.5F;			// Water upto %
		public float beachLevel = 0.52F;			// Beaches upto %
		public float grassLevel = 0.75F;			// Grass upto %
		public float moutainLevel = 0.95F;			// Moutains upto %
		public float snowLevel = 1.0F;				// Snow upto %, this should always be 1 (100%)

		public Color waterColor = Color.Blue;			// Water
		public Color beachColor = Color.Yellow;			// Beaches
		public Color grassColor =  Color.Green;			// Grass
		public Color moutainColor = Color.Brown;			// Moutains
		public Color snowColor = Color.White;				// Snow
	}
	public class Render2D: IRender
	{
		private TerranValues _settings;
		private float _boundsMin = float.MinValue;
		private float _boundsMax = float.MaxValue;

		public float BoundsMax {
			get {return _boundsMax; }
			set { _boundsMax = value; }
		}

		public float BoundsMin {
			get {return _boundsMin; }
			set { _boundsMin = value; }
		}

		public Render2D() {
			_settings = new TerranValues();
		}

		public void Free() {
		}

		private Color ScaleGreyscale(float val) {
			int grey;
			float newVal;
			float range = _boundsMax - _boundsMin;

			newVal = val;
			newVal = 255/range*newVal;

			grey = (int)Math.Round(newVal);
			return Color.FromArgb(grey, grey, grey);
		}


        // TODO: This isn't correct :( It's not taking _boundsMin into account and it can't cope with negitive values
		private Color ScaleTerran(float val, TerranValues vals) {
            float range = _boundsMax - _boundsMin;
            
			if (val <= _boundsMax * vals.waterLevel)
            {
				return vals.waterColor;
            }
            else if (val <= _boundsMax * vals.beachLevel)
            {
				return vals.beachColor;
            }
            else if (val <= _boundsMax * vals.grassLevel)
            {
				return vals.grassColor;
            }
            else if (val <= _boundsMax * vals.moutainLevel)
            {
				return vals.moutainColor;
            }
            else if (val <= _boundsMax * vals.snowLevel)
            {
				return vals.snowColor;
			}
            else 
            {
				return Color.Red;
			}
		}


		private Color ScaleThreeColor(float val, Color color1, Color color2, Color color3) {
			int red;
			int green;
			int blue;
            float bias;
            float range = _boundsMax - _boundsMin;
            float midPoint = range / 2F;

            /* _boundsMin: 100% color1
             * Inbetween: color1 -> color2
             * float midPoint: 100% color2
             * Inbetween: color2 -> color3
             * _boundsMax: = 100% color3
             * */

            if (val == _boundsMin)
            {
				// 100% Color 1
				red = color1.R;
				green = color1.G;
				blue = color1.B;

			}
            else if (val == midPoint)
            {
				// 100% Color 2
				red = color2.R;
				green = color2.G;
				blue = color2.B;

            }
            else if (val == _boundsMax)
            {
					// 100% Color 3
					red = color3.R;
					green = color3.G;
					blue = color3.B;

			}
            else if (val < midPoint) 
            {
				// Blend of Color 1 and 2
				bias = val / midPoint;

				// Blend between the two colours
				red = (int)Math.Round(Common.Linear_Interpolate(color1.R, color2.R, bias));
				green = (int)Math.Round(Common.Linear_Interpolate(color1.G, color2.G, bias));
				blue = (int)Math.Round(Common.Linear_Interpolate(color1.B, color2.B, bias));

            }
            else if (val < _boundsMax)
            {
				// Blend of Color 2 and 3
                bias = (val - midPoint) / (_boundsMax - midPoint);

				// Blend between the two colours
				red = (int)Math.Round(Common.Linear_Interpolate(color2.R, color3.R, bias));
				green = (int)Math.Round(Common.Linear_Interpolate(color2.G, color3.G, bias));
				blue = (int)Math.Round(Common.Linear_Interpolate(color2.B, color3.B, bias));

			} 
            else
            {
				throw new Exception("Out of range :(");
			}
			return Color.FromArgb(red, green, blue);
		}


		private Color ScaleTwoColor(float val, Color color1, Color color2) {
			float bios;
			bios = (float)val / (_boundsMax - _boundsMin);

			int red;
			int green;
			int blue;

			// Blend between the two colours
			red = (int)Math.Round(Common.Linear_Interpolate(color1.R, color2.R, bios));
			green = (int)Math.Round(Common.Linear_Interpolate(color1.G, color2.G, bios));
			blue = (int)Math.Round(Common.Linear_Interpolate(color1.B, color2.B, bios));

			return Color.FromArgb(red, green, blue);
		}


		public Bitmap RenderRainbow(float[,] ResultGrid)
		{
			Bitmap img = new Bitmap(ResultGrid.GetLength(0), ResultGrid.GetLength(1));

			for (int y=0; y<ResultGrid.GetLength(1); y++) {
				for (int x=0; x<ResultGrid.GetLength(0); x++) {
					img.SetPixel(x,y, ScaleThreeColor(ResultGrid[x,y], Color.Red, Color.Lime, Color.Blue));
				}
			}
			return img;
		}


		public Bitmap RenderGreyscale(float[,] ResultGrid)
		{
			Bitmap img = new Bitmap(ResultGrid.GetLength(0), ResultGrid.GetLength(1));

			for (int x=0; x<ResultGrid.GetLength(0); x++) {
				for (int y=0; y<ResultGrid.GetLength(1); y++) {
					img.SetPixel(x,y, ScaleGreyscale(ResultGrid[x,y]));
				}
			}
			return img;
		}


		public Bitmap RenderClouds(float[,] ResultGrid) {
			Bitmap img = new Bitmap(ResultGrid.GetLength(0), ResultGrid.GetLength(1));

			for (int y=0; y<ResultGrid.GetLength(1); y++) {
				for (int x=0; x<ResultGrid.GetLength(0); x++) {
					img.SetPixel(x,y, ScaleTwoColor(ResultGrid[x,y], Color.Blue, Color.White));
				}
			}
			return img;
		}

		public Bitmap RenderFire(float[,] ResultGrid) {
			Bitmap img = new Bitmap(ResultGrid.GetLength(0), ResultGrid.GetLength(1));

			for (int y=0; y<ResultGrid.GetLength(1); y++) {
				for (int x=0; x<ResultGrid.GetLength(0); x++) {
					img.SetPixel(x,y, ScaleThreeColor(ResultGrid[x,y], Color.Orange, Color.Red, Color.Black));
				}
			}
			return img;
		}

		public Bitmap RenderFire2(float[,] ResultGrid)		{
			Bitmap img = new Bitmap(ResultGrid.GetLength(0), ResultGrid.GetLength(1));

			for (int y=0; y<ResultGrid.GetLength(1); y++) {
				for (int x=0; x<ResultGrid.GetLength(0); x++) {
					img.SetPixel(x,y, ScaleThreeColor(ResultGrid[x,y], Color.Red, Color.Orange, Color.Black));
				}
			}
			return img;
		}

		public Bitmap RenderTerran(float[,] ResultGrid) {
			Bitmap img = new Bitmap(ResultGrid.GetLength(0), ResultGrid.GetLength(1));

			for (int x=0; x<ResultGrid.GetLength(0); x++) {
				for (int y=0; y<ResultGrid.GetLength(1); y++) {
					img.SetPixel(x,y, ScaleTerran(ResultGrid[x,y], _settings));
				}
			}
			return img;
		}


		public Bitmap Render(float[,] ResultGrid) {
			return RenderGreyscale(ResultGrid);
		}


	}
}

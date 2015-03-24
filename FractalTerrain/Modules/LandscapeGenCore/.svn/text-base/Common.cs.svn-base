using System;

namespace LandscapeGenCore
{
	/// <summary>
	/// Summary description for Common.
	/// </summary>
	public class Common
	{
		public static double Linear_Interpolate(double a, double b, double x) {
			return  a*(1-x) + b*x;
		}

		public static double Cosine_Interpolate(double a, double b, double x) {
			double ft;
			double f;
			ft = x * 3.1415927f;
			f = (double)(1 - Math.Cos((double)ft)) * 0.5f;
			return  a*(1-f) + b*f;
		}

		public static double Cubic_Interpolate(double v0,double v1, double v2, double v3, double x) {
			double P;
			double Q;
			double R;
			double S;
			P = (v3 - v2) - (v0 - v1);
			Q = (v0 - v1) - P;
			R = v2 - v0;
			S = v1;
			return (double)((P * Math.Pow(x,3)) + (Q * Math.Pow(x,2)) + (R * x) + S);
		}



		public static float[,] ConvertDoubleArrayToFloat( double[,] arr) {
			float[,] res;
			res = new float[arr.GetLongLength(0), arr.GetLongLength(1)];

			for (int x=0; x< arr.GetLongLength(0); x++) {
				for (int y=0; y< arr.GetLongLength(0); y++) {
					if (arr[x,y] >= float.MaxValue) {
						res[x,y] = float.MaxValue;

					} else if ((arr[x,y] <= float.MinValue)) {
						res[x,y] = float.MinValue;

					} else {
						res[x,y] = (float)arr[x,y];
					}
				}
			}


			return res;
		}

	}
}

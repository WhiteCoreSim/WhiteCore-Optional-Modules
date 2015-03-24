/*
 * LandscapeCoreGen
 * Copyright (c) 2006, Bevan Coleman
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the WhiteCore-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

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
			f = (double)(1 - Math.Cos(ft)) * 0.5f;
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
			return ((P * Math.Pow(x,3)) + (Q * Math.Pow(x,2)) + (R * x) + S);
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

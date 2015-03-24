using System;

namespace LandscapeGenCore
{
	/// <summary>
	/// Summary description for Normalize.
	/// </summary>
	public class Normalize : IPostProcessor
	{
		private float m_UpperLimit;
		private float m_LowerLimit;

		#region PostProcessor Members

		public Normalize():this(10000, -10000) {}

		public Normalize(float upperLimit, float lowerLimit) {
			m_UpperLimit = upperLimit;
			m_LowerLimit = lowerLimit;
		}

		public void Free() {
			// TODO:  Add Normalize.Free implementation
		}

		public float UpperLimit {
			get{ return m_UpperLimit; }
			set{ m_UpperLimit = value; }
		}

		public float LowerLimit {
			get{ return m_LowerLimit; }
			set{ m_LowerLimit = value; }
		}

		public float[,] Process(float[,] input) {
			float[,] ret = new float[input.GetLength(0), input.GetLength(1)];

			float minValue = float.MaxValue;
			float maxValue = float.MinValue;
			float scale = 1;
			float range;
			double newVal;		// Needs to be double as the values involved could be double that of a float

			// Find Min and max
			for (int i=0; i<input.GetLength(0); i++) {
				for (int j=0; j<input.GetLength(1); j++) {
					if (input[i,j] > maxValue) { maxValue = input[i,j]; }
					if (input[i,j] < minValue) { minValue = input[i,j]; }
				}
			}
			// Calc Range
			range = maxValue -  minValue;

			//Calculate scale
			scale = (m_UpperLimit-m_LowerLimit) / range;

			//Normalize values
			for (int i=0; i<input.GetLength(0); i++) {
				for (int j=0; j<input.GetLength(1); j++) {
					newVal = input[i,j] - minValue;	// Offset values to zero
					newVal *= scale;		// Scale to max, note that teh min value has been taken into account
					newVal += m_LowerLimit;	// Reset from base of zero to lower scale
					// Rounding errors can cause the values to be slightly outside of required limits
					if (newVal > m_UpperLimit) newVal = m_UpperLimit;
					if (newVal < m_LowerLimit) newVal = m_LowerLimit;

					ret[i,j] = (float)newVal;		// Reset from base of zero to lower scale
				}
			}

			
			// Find NEW Min and max
			minValue = float.MaxValue;
			maxValue = float.MinValue;
			for (int i=0; i<ret.GetLength(0); i++) {
				for (int j=0; j<ret.GetLength(1); j++) {
					if (ret[i,j] > maxValue) { maxValue = ret[i,j]; }
					if (ret[i,j] < minValue) { minValue = ret[i,j]; }
				}
			}

			return ret;
		}

		#endregion
	}
}

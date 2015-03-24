using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Text;

namespace LandscapeGenCore {

	public class PerlinNoiseSettings: INoiseSettings {
		private int _numberOfOctaves = 5;
		private double _persistence = 0.7d;
		private int _resultX = 200;
		private int _resultY = 200;
		private int _corsenessX = 10;
		private int _corsenessY = 10;
		private bool _flatEdges = false;
		private int _randomSeed = 0;

		[CategoryAttribute("General"), DescriptionAttribute("Edges will be zero or less.")]
		public bool FlatEdges {
			get { return _flatEdges; }
			set { _flatEdges = value; }
		}

		[CategoryAttribute("General"), DescriptionAttribute("Inital seed for random number generator. Zero means it is taken from the clock.")]
		public int RandomSeed {
			get { return _randomSeed; }
			set { _randomSeed = value; }
		}

		[CategoryAttribute("General"), DescriptionAttribute("Number of interations that will be run. More generates a finer noise.")]
		public int Octaves {
			get { return _numberOfOctaves; }
			set { _numberOfOctaves = value; }
		}

		[CategoryAttribute("General"), DescriptionAttribute("Persistence.")]
		public double Persistence {
			get { return _persistence; }
			set { _persistence = value; }
		}

		[CategoryAttribute("Result Size"), DescriptionAttribute("Width of resulting data.")]
		public int ResultX {
			get { return _resultX; }
			set { _resultX = value; }
		}

		[CategoryAttribute("Result Size"), DescriptionAttribute("Height of resulting data.")]
		public int ResultY {
			get { return _resultY; }
			set { _resultY = value; }
		}

		[CategoryAttribute("Corseness"), DescriptionAttribute("Corseness on X.")]
		public int CorsenessX {
			get { return _corsenessX; }
			set {
				_corsenessX = value;
				// Validate the value entered
				if (_corsenessX <= 1) {
					_corsenessX = 2;
					throw new Exception("Corseness X must be 2 or greater.");
				}
			}
		}

		[CategoryAttribute("Corseness"), DescriptionAttribute("Corseness on Y.")]
		public int CorsenessY {
			get { return _corsenessY; }
			set {
				
				_corsenessY = value;
				if (_corsenessY <= 1) {
					_corsenessY = 2;
					throw new Exception("Corseness Y must be 2 or greater.");
				}
			}
		}

	}


	public class PerlinNoise: INoiseGenerator {
		private double[,] _ResultGrid;
		private int _seedOffset;

		private PerlinNoiseSettings _settings;

		private int[] _Primes = {10719073,10719077,10719089,10719109,10719113,10719119,10719139,10719143,
									10719151,10719157,10719161,10719187,10719193,10719199,10719217,10719227,
									10719239,10719251,10719259,10719301,10719329,10719343,10719353,10719377,
									10719419,10719421,10719451,10719463,10719487,10719497,10719517,10719523,
									10719557,10719601,10719607,10719613,10719649,10719659,10719671,10719691,
									10719697,10719701,10719721,10719733,10719749,10719767,10719791,10719811,
									10719827,10719829,10719851,10719853,10719881,10719883,10719889,10719913,
									10719967,10719997,10720007,10720019,10720027,10720033,10720043,10720069,
									10720091,10720153,10720159,10720163,10720181,10720187,10720189,10720211,
									10720247,10720249,10720253,10720261,10720267,10720271,10720289,10720309,
									10720321,10720337,10720357,10720399,10720429,10720433,10720453,10720471,
									10720483,10720487,10720511,10720519,10720531,10720573,10720583,10720601,
									10720627,10720663,10720667,10720679,10720709,10720711,10720733,10720751,
									10720753,10720771,10720777,10720781,10720793,10720799,10720837,10720859,
									10720867,10720873,10720877,10720891,10720907,10720939,10720943,10720951,
									10720973,10720979,10720991,10721003,10721041,10721047,10721057,10721077,
									10721089,10721099,10721107,10721141,10721143,10721153,10721159,10721219};

		/// <summary>
		/// Retreives a Prime number. Effectivly turns an array of primes into a loop.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		private int GetPrime(int index) {
			int maxIdx = _Primes.Length;

			while (index>=maxIdx)
				index -= maxIdx;

			return _Primes[index];
		}

		public INoiseSettings Settings {
			get {
				return _settings;
			}
			set {
				_settings = (PerlinNoiseSettings)value;
			}
		}

		public string Name() {
			return "Perlin";
		}

		public PerlinNoise() {
			_settings = new PerlinNoiseSettings();
		}


		public void Free() {
			_ResultGrid = null;
		}
		
		float[,] LandscapeGenCore.INoiseGenerator.Generate() {
			// Using the Gradiant grid above, generate ResultGrid
			_ResultGrid = new double[_settings.ResultX, _settings.ResultY];
			
			_seedOffset = _settings.RandomSeed;
			while (_seedOffset>=_Primes.Length)
				_seedOffset -= _Primes.Length;
			
			double noise;
			for (int y=0; y<_settings.ResultY; y++) {
				for (int x=0; x<_settings.ResultX; x++) {
					noise = PerlinNoise_2D((double)x/_settings.CorsenessX, (double)y/_settings.CorsenessY, _settings.Octaves, _settings.Persistence);
					_ResultGrid[x, y] = noise;
				}
			}

			return Common.ConvertDoubleArrayToFloat(_ResultGrid);
		}


		private int ScaleNoise(double noise) {
			int ret;
			// While the value should be outside of these bounds, I'm playing it safe and forcing the matter.
			if (noise > 1) { noise = 1; }
			if (noise < -1) { noise = -1; }

			// Scale double (-1 to 1) to 32bit int
			if (noise < 0) {
				ret = (int)Math.Round(Math.Abs(noise) * int.MinValue);
			} else {
				ret = (int)Math.Round(noise * int.MaxValue);
			}
			
			return ret;
		}

		private double Noise_2D(int octave, int x, int y) {
			//http://www.devx.com/Intel/Article/20182/0/page/2

			int offset = _seedOffset + (octave * 3);
			int p1 = GetPrime(offset);
			int p2 = GetPrime(offset+1);
			int p3 = GetPrime(offset+2);
			int noise;

			int n = x + y * 59;
			n = (n<<13)^n;

			/* This code will overflow in some situations!!! 
			 * So keep overflow checking turned off if you want it to finish.*/
			unchecked {
				noise = (p1 * (n*n*n)) + (p2 * n) + (p3);
			}
			double n2 = (double)noise /int.MaxValue ;

			if (_settings.FlatEdges) {
				// If noise point on or off edge of map
				if (  (x <= 0) 
					| (y <= 0) 
					| (x >= _settings.ResultX/_settings.CorsenessX)
					| (y >= _settings.ResultY/_settings.CorsenessY)  ) {

					if (n2 >0) {
						return 0;
					}
				}
			}
			return n2;
		}

		
		private double SmoothNoise_2D(int octave, int x, int y) {
			double corners;
			double sides;
			double center;

			corners = ( Noise_2D(octave, x-1, y-1)+Noise_2D(octave, x+1, y-1)+Noise_2D(octave, x-1, y+1)+Noise_2D(octave, x+1, y+1) ) / 16f;
			sides   = ( Noise_2D(octave, x-1, y)  +Noise_2D(octave, x+1, y)  +Noise_2D(octave, x, y-1)  +Noise_2D(octave, x, y+1) ) /  8f;
			center  =  Noise_2D(octave, x, y) / 4;

			return corners + sides + center;
		}

		private double InterpolatedNoise_2D(int octave, double x, double y) {
			int integer_X    = (int)Math.Floor(x);
			double fractional_X = x - integer_X;
			int integer_Y    = (int)Math.Floor(y);
			double fractional_Y = y - integer_Y;

			double v1;
			double v2;
			double v3;
			double v4;
			double i1;
			double i2;
			double i3;

			v1 = SmoothNoise_2D(octave, integer_X,     integer_Y);
			v2 = SmoothNoise_2D(octave, integer_X + 1, integer_Y);
			v3 = SmoothNoise_2D(octave, integer_X,     integer_Y + 1);
			v4 = SmoothNoise_2D(octave, integer_X + 1, integer_Y + 1);

			
			//Linear
			i1 = Common.Linear_Interpolate(v1 , v2 , fractional_X);
			i2 = Common.Linear_Interpolate(v3 , v4 , fractional_X);
			i3 = Common.Linear_Interpolate(i1 , i2 , fractional_Y);
						

			/*
			//Cosine
			i1 = Common.Cosine_Interpolate(v1 , v2 , fractional_X);
			i2 = Common.Cosine_Interpolate(v3 , v4 , fractional_X);
			i3 = Common.Cosine_Interpolate(i1 , i2 , fractional_Y);
			*/

			
			//Cubic
			/*i1 = Common.Cubic_Interpolate(v1 , v2 , fractional_X);
			i2 = Common.Cubic_Interpolate(v3 , v4 , fractional_X);
			i3 = Common.Cubic_Interpolate(i1 , i2 , fractional_Y);*/
			

			return i3;
		}


		private double PerlinNoise_2D(double x, double y, int Number_Of_Octaves, double persistence) {
			// See http://freespace.virgin.net/hugo.elias/models/m_perlin.htm
			double total;
			double p;
			double n;
			double frequency;
			double amplitude;

			total = 0;
			p = persistence;
			n = Number_Of_Octaves - 1;
			for (int i=0; i < n; i++) {
				frequency = Math.Pow(2, i);
				amplitude = Math.Pow(p, i);
				total = total + InterpolatedNoise_2D(i, x * frequency, y * frequency) * amplitude;
			}
			return total;
		}
	}
}

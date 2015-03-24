using System;
using Laan.Risk.Terrain.Generator;
using System.Drawing;

namespace LandscapeGenCore
{
	public class LaanInterfaceRegister
	{
		public static IGenerator[] RegisterGenerator() {

			int seed = new Random().Next();
			int size = 129;

			return new IGenerator[] {
						new PerlinGenerator(seed, size),
						new KochSurfaceGenerator(seed, size)
			};
		}
	}


	public class PerlinGenerator : IGenerator {
		private INoiseSettings _settings;

		public PerlinGenerator(int seed, int size) {
			PerlinNoiseSettings objSettings = new PerlinNoiseSettings();

			objSettings.RandomSeed = seed;
			objSettings.ResultX = size;
			objSettings.ResultY = size;

			_settings = (INoiseSettings)objSettings;
		}

		#region IGenerator Members
		public System.Drawing.Bitmap Execute() {
			float[,] result;
			Bitmap img;
			
			// Create
			INoiseGenerator objGen = new PerlinNoise();
			objGen.Settings = _settings;

			result = objGen.Generate();

			objGen.Free();
			objGen = null;

			//Normalise and convert to ushort
			IPostProcessor objPostProc = new Normalize();

			result = objPostProc.Process(result);

			objPostProc.Free();
			objPostProc = null;
			

			// Render
			Render2D objRender = new Render2D();

			img = objRender.RenderGreyscale(result);

			objRender.Free();
			objRender = null;

			return img;
		}

		public string DisplayName() {
			return "Bevan - Perlin";
		}

		public object Properties() {
			return _settings;
		}
		#endregion

	}

	
	public class KochSurfaceGenerator : IGenerator {
		private INoiseSettings _settings;

		public KochSurfaceGenerator(int seed, int size) {
			KochLikeNoiseSettings objSettings = new KochLikeNoiseSettings();

			objSettings.RandomSeed = seed;
			objSettings.ResultX = size;
			objSettings.ResultY = size;

			_settings = (INoiseSettings)objSettings;
		}

		#region IGenerator Members
		public System.Drawing.Bitmap Execute() {
			float[,] result;
			Bitmap img;
			
			// Create
			INoiseGenerator objGen = new KochLikeNoise();
			objGen.Settings = _settings;

			result = objGen.Generate();

			objGen.Free();
			objGen = null;

			//Normalise and convert to ushort
			IPostProcessor objPostProc = new Normalize();

			result = objPostProc.Process(result);

			objPostProc.Free();
			objPostProc = null;
			

			// Render
			Render2D objRender = new Render2D();

			img = objRender.RenderGreyscale(result);

			objRender.Free();
			objRender = null;

			return img;
		}

		public string DisplayName() {
			return "Bevan - Koch Surface";
		}

		public object Properties() {
			return _settings;
		}
		#endregion

	}
}

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
		INoiseSettings _settings;

		public PerlinGenerator(int seed, int size) {
			PerlinNoiseSettings objSettings = new PerlinNoiseSettings();

			objSettings.RandomSeed = seed;
			objSettings.ResultX = size;
			objSettings.ResultY = size;

			_settings = objSettings;
		}

		#region IGenerator Members
		public Bitmap Execute() {
			float[,] result;
			Bitmap img;
			
			// Create
			INoiseGenerator objGen = new PerlinNoise();
			objGen.Settings = _settings;

			result = objGen.Generate();

			objGen.Free();

			//Normalise and convert to ushort
			IPostProcessor objPostProc = new Normalize();

			result = objPostProc.Process(result);

			objPostProc.Free();
			

			// Render
			Render2D objRender = new Render2D();

			img = objRender.RenderGreyscale(result);

			objRender.Free();

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
		INoiseSettings _settings;

		public KochSurfaceGenerator(int seed, int size) {
			KochLikeNoiseSettings objSettings = new KochLikeNoiseSettings();

			objSettings.RandomSeed = seed;
			objSettings.ResultX = size;
			objSettings.ResultY = size;

			_settings = objSettings;
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

			//Normalise and convert to ushort
			IPostProcessor objPostProc = new Normalize();

			result = objPostProc.Process(result);

			objPostProc.Free();
			

			// Render
			Render2D objRender = new Render2D();

			img = objRender.RenderGreyscale(result);

			objRender.Free();

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

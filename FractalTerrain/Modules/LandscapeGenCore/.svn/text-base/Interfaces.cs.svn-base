using System;
using System.Drawing;

namespace LandscapeGenCore
{
	interface IRender {
		void Free();
		Bitmap Render(float[,] ResultGrid);
	}

	public interface INoiseGenerator {
		void Free();
		float[,] Generate();
		string Name();
		INoiseSettings Settings {get; set; }
	}

	public interface INoiseSettings {
		int ResultX {get; set; }
		int ResultY {get; set; }
		int RandomSeed {get; set; }
	}

	interface IPostProcessor {
		void Free();
		float[,] Process(float[,] input);
	}
}

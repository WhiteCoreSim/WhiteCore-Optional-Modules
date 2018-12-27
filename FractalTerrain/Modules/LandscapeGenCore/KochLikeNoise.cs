using System;
using System.ComponentModel;

namespace LandscapeGenCore
{

    public class KochLikeNoiseSettings : INoiseSettings
    {
        int _resultX = 200;
        int _resultY = 200;

        int _initalGridX = 2;
        int _initalGridY = 2;

        int _randomSeed = 0;
        int _rndMin = -1;
        int _rndMax = 1;
        double _h = 1;
        double _scale = 1;

        [Category ("Settings"), Description ("H controls roughness. 1-2 is good for landscapes, 3 or more is practicaly a gradiant, less then 0.5 is very rough.")]
        public double H {
            get { return _h; }
            set { _h = value; }
        }

        [Category ("Settings"), Description ("Scale controls height. Not much use when normalising.")]
        public double Scale {
            get { return _scale; }
            set { _scale = value; }
        }

        [Category ("Settings"), Description ("Inital Grid X.")]
        public int InitalGridX {
            get { return _initalGridX; }
            set { _initalGridX = value; }
        }

        [Category ("Settings"), Description ("Inital Grid Y.")]
        public int InitalGridY {
            get { return _initalGridY; }
            set { _initalGridY = value; }
        }

        [Category ("Random"), Description ("Inital seed for random number generator. Zero means it is taken from the clock.")]
        public int RandomSeed {
            get { return _randomSeed; }
            set { _randomSeed = value; }
        }
        [Category ("Random"), Description ("Minimum value for random.")]
        public int RandomMin {
            get { return _rndMin; }
            set { _rndMin = value; }
        }
        [Category ("Random"), Description ("Maximum value for random.")]
        public int RandomMax {
            get { return _rndMax; }
            set { _rndMax = value; }
        }

        [Category ("Result Size"), Description ("Width of resulting data.")]
        public int ResultX {
            get { return _resultX; }
            set { _resultX = value; }
        }

        [Category ("Result Size"), Description ("Height of resulting data.")]
        public int ResultY {
            get { return _resultY; }
            set { _resultY = value; }
        }
    }


    public class KochLikeNoise : INoiseGenerator
    {
        Random _rnd;

        KochLikeNoiseSettings _settings;

        public INoiseSettings Settings {
            get {
                return _settings;
            }
            set {
                _settings = (KochLikeNoiseSettings)value;
            }
        }

        public KochLikeNoise ()
        {
            _settings = new KochLikeNoiseSettings ();
        }

        public string Name ()
        {
            return "Koch Surface (square)";
        }


        public void Free ()
        {

        }

        double GetRand ()
        {
            return (_rnd.NextDouble () * (_settings.RandomMax - _settings.RandomMin)) + _settings.RandomMin;
        }


        float [,] INoiseGenerator.Generate ()
        {
            if (_settings.RandomSeed != 0) {
                _rnd = new Random (_settings.RandomSeed);
            } else {
                _rnd = new Random ();
            }

            // Note that internally this generator works with doubles!
            double [,] prevGrid;
            double [,] grid;
            double n;
            int sizeX = _settings.InitalGridX;
            int sizeY = _settings.InitalGridY;

            grid = new double [sizeX, sizeY];
            n = 0;
            while ((sizeX <= Settings.ResultX) && (sizeY <= Settings.ResultY)) {
                prevGrid = grid;
                grid = new double [sizeX, sizeY];

                FillGrid (ref grid, prevGrid, _settings.Scale, n, _settings.H);

                sizeX = sizeX * 2 - 1;
                sizeY = sizeY * 2 - 1;
                n++;
            }
            return Common.ConvertDoubleArrayToFloat (grid);
        }

        void FillGrid (ref double [,] grid, double [,] prevGrid, double scale, double n, double H)
        {
            /*
			 * n = iteration
			 * H = roughness, higher is more smooth
			 * */

            double avgH;
            double pointScale;

            if (Math.Abs (n) < GlobalVar.EPSILON) {
                // First iteration, just fill the grid with random*scale
                for (int x = 0; x < grid.GetLength (0); x++) {
                    for (int y = 0; y < grid.GetLength (0); y++) {
                        grid [x, y] = GetRand () * scale;
                        //grid[x,y] = 0.5;
                    }//for y
                }//for x
            } else {
                // All other iterations, little more complicated!

                /* First copy all existing points over, you just multiple 
				 * the possition in the orig grid x2 to get the new possitions
				 * NOTE: this is using the coords of the Original grid!
				 * */
                for (int x = 0; x < prevGrid.GetLength (0); x++) {
                    for (int y = 0; y < prevGrid.GetLength (0); y++) {
                        grid [x * 2, y * 2] = prevGrid [x, y];
                    }//for y
                }//for x

                /* Now get the scale */
                pointScale = Math.Pow (0.5F, n * H / 2F) * scale;

                /* Then calculate the fill in points
				 * NOTE: this is using the coords of the New grid!
				 * */
                for (int x = 0; x < grid.GetLength (0); x++) {
                    for (int y = 0; y < grid.GetLength (0); y++) {

                        if ((x % 2 == 0) && (y % 2 == 0)) {
                            // Ignore, already copied

                        } else {
                            // Number crunching time!

                            /* First get average Height, for an edge point (x or y equals 0) it's two points.
							 * For all others it's all 4 surrounding points.
							 * Note you can NOT get x==0 AND y==0 as thats caught but the even/odd check above. */
                            if ((x % 2 == 0)) {
                                // 2 points up and down
                                avgH = (grid [x, y - 1] + grid [x, y + 1]) / 2F;

                            } else if ((y % 2 == 0)) {
                                // 2 points left and right
                                avgH = (grid [x - 1, y] + grid [x + 1, y]) / 2F;

                            } else {
                                // four points
                                avgH = (grid [x - 1, y - 1] + grid [x + 1, y - 1] + grid [x - 1, y + 1] + grid [x + 1, y + 1]) / 4F;
                            }

                            /* Finaly we take the average height, and add a random number multiplied by the scale */
                            grid [x, y] = avgH + (GetRand () * pointScale);


                        }
                    }//for y
                }//for x
            }
        }
    }
}

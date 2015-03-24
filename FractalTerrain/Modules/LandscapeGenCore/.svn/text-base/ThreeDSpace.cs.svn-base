using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;

namespace LandscapeGenCore
{
    public class ThreeDSpace
    {
        public struct Point3D
        {
            public Point3D(float x, float y, float z, Color c)
            {
                X = x;
                Y = y;
                Z = z;
                color = c;
            }

            public float X;
            public float Y;
            public float Z;
            public Color color;
        }

        public struct Lines3D
        {
            public Lines3D(Point3D start, Point3D end)
            {
                Start = start;
                End = end;
            }
            public Point3D Start;
            public Point3D End;
        }

        public struct Matrix
        {
        }

        public interface IPointObject
        {
            Point3D[] Points
            {
                get;
                set;
            }

            Lines3D[] Lines
            {
                get;
                set;
            }
        }

        public class RectangleObject : IPointObject
        {
            private Point3D[] _points = new Point3D[8];
            private Lines3D[] _lines = new Lines3D[12];

            public RectangleObject(float width, float height, float depth, Color c)
            {
                Setup(width, height, depth, c);
            }

            public void Setup(float width, float height, float depth, Color c)
            {
                // Points - Front face
                _points[0] = new Point3D(0, 0, 0, c); //F-TL
                _points[1] = new Point3D(width, 0, 0, c); //F-TR
                _points[2] = new Point3D(0, height, 0, c); //F-BL
                _points[3] = new Point3D(width, height, 0, c); //F-BR

                // Points - Back face
                _points[4] = new Point3D(0, 0, depth, c); //B-TL
                _points[5] = new Point3D(width, 0, depth, c); //B-TR
                _points[6] = new Point3D(0, height, depth, c); //B-BL
                _points[7] = new Point3D(width, height, depth, c); //B-BR

                // Lines - Front face
                _lines[0] = new Lines3D(_points[0], _points[1]);    // F-T
                _lines[1] = new Lines3D(_points[2], _points[3]);    // F-B
                _lines[2] = new Lines3D(_points[0], _points[2]);    // F-L
                _lines[3] = new Lines3D(_points[1], _points[3]);    // F-R

                // Lines - back face
                _lines[4] = new Lines3D(_points[4], _points[5]);    // B-T
                _lines[5] = new Lines3D(_points[6], _points[7]);    // B-B
                _lines[6] = new Lines3D(_points[4], _points[6]);    // B-L
                _lines[7] = new Lines3D(_points[5], _points[7]);    // B-R

                // Lines - center
                _lines[8] = new Lines3D(_points[0], _points[4]);    // C-TL
                _lines[9] = new Lines3D(_points[1], _points[5]);    // B-TR
                _lines[10] = new Lines3D(_points[2], _points[6]);    // B-BL
                _lines[11] = new Lines3D(_points[3], _points[7]);    // B-BR

            }

            #region IPointObject Members

            Point3D[] IPointObject.Points
            {
                get
                {
                    return _points;
                }
                set
                {
                    _points = value;
                }
            }

            Lines3D[] IPointObject.Lines
            {
                get
                {
                    return _lines;
                }
                set
                {
                    _lines = value;
                }
            }

            #endregion
        }

        public class CubeObject : RectangleObject
        {
            public CubeObject(float width, Color c): base(width,width,width,c) {
            }

            public void Setup(float width, Color c) {
                base.Setup(width,width, width, c);
            }
        }

        public class LineObject : IPointObject
        {
            private Point3D _from;
            private Point3D _to;

            public LineObject(Point3D from, Point3D to)
            {
                _from = from;
                _to = to;
            }

            private Point3D[] GetPoints()
            {
                Point3D[] ret = new Point3D[2];
                ret[0] = _from;
                ret[1] = _to;
                return ret;
            }

            private Lines3D[] GetLines()
            {
                Lines3D[] ret = new Lines3D[1];
                ret[0] = new Lines3D(_from, _to);
                return ret;
            }


            #region IPointObject Members

            Point3D[] IPointObject.Points
            {
                get
                {
                    return GetPoints();
                }
                set
                {
                    throw new Exception("The method or operation is not implemented.");
                }
            }

            Lines3D[] IPointObject.Lines
            {
                get
                {
                    return GetLines();
                }
                set
                {
                    throw new Exception("The method or operation is not implemented.");
                }
            }

            #endregion
        }

        public class MeshObject : IPointObject
        {
            private float[,] _mesh;
            private Color _meshColor;
            private float _scale = 1;

            public float Scale
            {
                get { return _scale; }
                set { _scale = value; }
            }
                

            public MeshObject(int meshX, int meshY, Color c)
            {
                Setup(meshX, meshY, c);
            }

            public void Setup(int meshX, int meshY, Color c)
            {
                _meshColor = c;
                _mesh = new float[meshX, meshY];


            }

            private Point3D[] GetPoints() {
                int x = _mesh.GetLength(0);
                int y = _mesh.GetLength(1);
                Point3D[] p = new Point3D[x * y];

                int c = 0;
                float spaceX = 1f / (x-1);
                float spaceY = 1f / (y-1);
                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        p[c] = new Point3D(
                                spaceX * i  * _scale,
                                spaceY * j  * _scale,
                                _mesh[i, j] * _scale, 
                                _meshColor);
                        c++;
                    }
                }

                return p;
            }

            private Lines3D[] GetLines() {
                int x = _mesh.GetLength(0);
                int y = _mesh.GetLength(1);

                // Lines is based of the points
                Point3D[] p = GetPoints();

                // Create line object
                // TODO: this can be simplifed
                Lines3D[] l = new Lines3D[((x-1) * y) + (x* (y-1))];

                // Populate values in line objects, the lines will look somthing like so
                /*   *-*-*-*
                 *   | | | |
                 *   *-*-*-*
                 *   | | | |
                 *   *-*-*-*
                 *   | | | |
                 *   *-*-*-*
                 * */
                int linePos = 0;
                for (int idxY=0; idxY<y; idxY++)
                {

                    // Horz Line  (count = x-1)
                    for (int idxX = 0; idxX < x - 1; idxX++)
                    {
                        l[linePos].Start = p[(idxY*x) +idxX];
                        l[linePos].End = p[(idxY*x) +idxX +1];  // Point to right
                        linePos++;
                    }

                    // Only add Vert lines if not on last row
                    if (idxY < y-1)
                    {
                        // Vert Line  (count = x)
                        for (int idxX = 0; idxX < x; idxX++)
                        {
                            l[linePos].Start = p[(idxY * x) + idxX];    // Same point as Horz line
                            l[linePos].End = p[((idxY + 1) * x) + idxX];  // Point down
                            linePos++;
                        }
                    }
                }

                return l;
            }

            #region IPointObject Members

            Point3D[] IPointObject.Points
            {
                get
                {
                    return GetPoints();
                }
                set { }
            }

            Lines3D[] IPointObject.Lines
            {
                get
                {
                    return GetLines();
                }
                set { }
            }

            #endregion
        }

        public class PointObjects: ArrayList
        {
            public int Add(IPointObject value)
            {
                return base.Add(value);
            }
            public new IPointObject this[int index]
            {
                get
                {
                    return (IPointObject)base[index];
                }
                set
                {
                    base[index] = value;
                }
            }

        }
        public PointObjects pointObjects = new PointObjects();


        public Point3D viewpoint;
        public Point3D screen;

        public PointF Render(Point3D p)
        {
            // TODO: This logic can not cope with points behind the camera!! It can also not look backwards
            // Note, I'm not sure about the formular used :( Need to check this cause damn thing doesn't work that well
            //http://www.codeproject.com/cpp/3demo.asp
            PointF r = new Point();
            Point3D e = viewpoint;
            Point3D s = screen;

            /*r.Y = ((p.Y - e.Y) * (s.Z - e.Z) / (p.Z - e.Z)) + e.Y;
            r.X = ((p.X - e.X) * (s.Z - e.Z) / (p.Z - e.Z)) + e.X;*/

            r.Y = ((p.Y - e.Y) * s.Z / (p.Z - e.Z));
            r.X = ((p.X - e.X) * s.Z / (p.Z - e.Z));

            return r;
        }
    }
}

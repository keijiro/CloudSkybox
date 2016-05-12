using UnityEngine;

namespace NoiseTools
{
    public class PerlinNoise : NoiseGeneratorBase
    {
        #region Constructor

        public PerlinNoise(int frequency, int repeat, int seed = 0)
        : base(frequency, repeat, seed)
        {
        }

        #endregion

        #region Private members

        static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        static float Lerp(float t, float a, float b)
        {
            return a + t * (b - a);
        }

        static float Grad(int hash, float x, float y)
        {
            return ((hash & 1) == 0 ? x : -x) + ((hash & 2) == 0 ? y : -y);
        }

        static float Grad(int hash, float x, float y, float z)
        {
            var h = hash & 15;
            var u = h < 8 ? x : y;
            var v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        #endregion

        #region 2D noise

        protected override float Calculate2D(Vector2 point)
        {
            var x = point.x * Frequency;
            var y = point.y * Frequency;

            var cx = Mathf.FloorToInt(x);
            var cy = Mathf.FloorToInt(y);

            x -= cx;
            y -= cy;

            var u = Fade(x);
            var v = Fade(y);

            var h00 = Hash(CellID(cx    , cy    ));
            var h01 = Hash(CellID(cx + 1, cy    ));
            var h10 = Hash(CellID(cx    , cy + 1));
            var h11 = Hash(CellID(cx + 1, cy + 1));

            var n = Lerp(v, Lerp(u, Grad(h00, x, y  ), Grad(h01, x-1, y  )),
                            Lerp(u, Grad(h10, x, y-1), Grad(h11, x-1, y-1)));

            return n * 0.5f + 0.5f;
        }

        #endregion

        #region 3D noise

        protected override float Calculate3D(Vector3 point)
        {
            var x = point.x * Frequency;
            var y = point.y * Frequency;
            var z = point.z * Frequency;

            var cx = Mathf.FloorToInt(x);
            var cy = Mathf.FloorToInt(y);
            var cz = Mathf.FloorToInt(z);

            x -= cx;
            y -= cy;
            z -= cz;

            var u = Fade(x);
            var v = Fade(y);
            var w = Fade(z);

            var h000 = Hash(CellID(cx    , cy    , cz    ));
            var h001 = Hash(CellID(cx + 1, cy    , cz    ));
            var h010 = Hash(CellID(cx    , cy + 1, cz    ));
            var h011 = Hash(CellID(cx + 1, cy + 1, cz    ));
            var h100 = Hash(CellID(cx    , cy    , cz + 1));
            var h101 = Hash(CellID(cx + 1, cy    , cz + 1));
            var h110 = Hash(CellID(cx    , cy + 1, cz + 1));
            var h111 = Hash(CellID(cx + 1, cy + 1, cz + 1));

            var n = Lerp(w, Lerp(v, Lerp(u, Grad(h000, x, y  , z  ), Grad(h001, x-1, y  , z  )),
                                    Lerp(u, Grad(h010, x, y-1, z  ), Grad(h011, x-1, y-1, z  ))),
                            Lerp(v, Lerp(u, Grad(h100, x, y  , z-1), Grad(h101, x-1, y  , z-1)),
                                    Lerp(u, Grad(h110, x, y-1, z-1), Grad(h111, x-1, y-1, z-1))));
            return n * 0.5f + 0.5f;
        }

        #endregion
    }
}

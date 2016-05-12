using UnityEngine;

namespace NoiseTools
{
    public class WorleyNoise : NoiseGeneratorBase
    {
        #region Constructor

        public WorleyNoise(int frequency, int repeat, int seed = 0)
        : base(frequency, repeat, seed)
        {
        }

        #endregion

        #region 2D noise

        const int kIDOffs1 = 100000;
        const int kIDOffs2 = 200000;

        Vector2 Feature(int cx, int cy)
        {
            var id = CellID(cx, cy);
            return new Vector2(
                Hash01(id           ) + cx,
                Hash01(id + kIDOffs1) + cy
            );
        }

        float DistanceToFeature(Vector2 p, int cx, int cy)
        {
            return Vector2.Distance(p, Feature(cx, cy));
        }

        protected override float Calculate2D(Vector2 point)
        {
            point *= Frequency;

            var cx = Mathf.FloorToInt(point.x);
            var cy = Mathf.FloorToInt(point.y);

            var d = DistanceToFeature(point, cx, cy);

            d = Mathf.Min(d, DistanceToFeature(point, cx - 1, cy - 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx    , cy - 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx + 1, cy - 1));

            d = Mathf.Min(d, DistanceToFeature(point, cx - 1, cy    ));
            d = Mathf.Min(d, DistanceToFeature(point, cx + 1, cy    ));

            d = Mathf.Min(d, DistanceToFeature(point, cx - 1, cy + 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx    , cy + 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx + 1, cy + 1));

            return d;
        }

        #endregion

        #region 3D noise

        Vector3 Feature(int cx, int cy, int cz)
        {
            var id = CellID(cx, cy, cz);
            return new Vector3(
                Hash01(id           ) + cx,
                Hash01(id + kIDOffs1) + cy,
                Hash01(id + kIDOffs2) + cz
            );
        }

        float DistanceToFeature(Vector3 p, int cx, int cy, int cz)
        {
            return Vector3.Distance(p, Feature(cx, cy, cz));
        }

        protected override float Calculate3D(Vector3 point)
        {
            point *= Frequency;

            var cx = Mathf.FloorToInt(point.x);
            var cy = Mathf.FloorToInt(point.y);
            var cz = Mathf.FloorToInt(point.z);

            var d = DistanceToFeature(point, cx, cy, cz);

            d = Mathf.Min(d, DistanceToFeature(point, cx - 1, cy - 1, cz - 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx    , cy - 1, cz - 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx + 1, cy - 1, cz - 1));

            d = Mathf.Min(d, DistanceToFeature(point, cx - 1, cy    , cz - 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx    , cy    , cz - 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx + 1, cy    , cz - 1));

            d = Mathf.Min(d, DistanceToFeature(point, cx - 1, cy + 1, cz - 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx    , cy + 1, cz - 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx + 1, cy + 1, cz - 1));

            d = Mathf.Min(d, DistanceToFeature(point, cx - 1, cy - 1, cz));
            d = Mathf.Min(d, DistanceToFeature(point, cx    , cy - 1, cz));
            d = Mathf.Min(d, DistanceToFeature(point, cx + 1, cy - 1, cz));

            d = Mathf.Min(d, DistanceToFeature(point, cx - 1, cy    , cz));
            d = Mathf.Min(d, DistanceToFeature(point, cx + 1, cy    , cz));

            d = Mathf.Min(d, DistanceToFeature(point, cx - 1, cy + 1, cz));
            d = Mathf.Min(d, DistanceToFeature(point, cx    , cy + 1, cz));
            d = Mathf.Min(d, DistanceToFeature(point, cx + 1, cy + 1, cz));

            d = Mathf.Min(d, DistanceToFeature(point, cx - 1, cy - 1, cz + 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx    , cy - 1, cz + 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx + 1, cy - 1, cz + 1));

            d = Mathf.Min(d, DistanceToFeature(point, cx - 1, cy    , cz + 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx    , cy    , cz + 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx + 1, cy    , cz + 1));

            d = Mathf.Min(d, DistanceToFeature(point, cx - 1, cy + 1, cz + 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx    , cy + 1, cz + 1));
            d = Mathf.Min(d, DistanceToFeature(point, cx + 1, cy + 1, cz + 1));

            return d;
        }

        #endregion
    }
}

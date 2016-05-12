using UnityEngine;

namespace NoiseTools
{
    public abstract class NoiseGeneratorBase
    {
        #region Public members

        public NoiseGeneratorBase(int frequency, int repeat, int seed = 0)
        {
            _freq = frequency;
            _repeat = repeat * frequency;
            _seed = seed;
        }

        public float GetAt(float x, float y)
        {
            return Calculate2D(new Vector2(x, y));
        }

        public float GetAt(Vector2 point)
        {
            return Calculate2D(point);
        }

        public float GetAt(float x, float y, float z)
        {
            return Calculate3D(new Vector3(x, y, z));
        }

        public float GetAt(Vector3 point)
        {
            return Calculate3D(point);
        }

        public float GetFractal(float x, float y, int level)
        {
            return Calculate2DFractal(new Vector2(x, y), level);
        }

        public float GetFractal(Vector2 point, int level)
        {
            return Calculate2DFractal(point, level);
        }

        public float GetFractal(float x, float y, float z, int level)
        {
            return Calculate3DFractal(new Vector3(x, y, z), level);
        }

        public float GetFractal(Vector3 point, int level)
        {
            return Calculate3DFractal(point, level);
        }

        #endregion

        #region Protected members

        int _freq;
        int _repeat;
        int _seed;

        protected float Frequency { get { return _freq; } }

        protected int Repeat(int i)
        {
            i %= _repeat;
            if (i < 0) i += _repeat;
            return i;
        }

        protected int Hash(int id)
        {
            return (int)XXHash.GetHash(id, _seed);
        }

        protected float Hash01(int id)
        {
            return XXHash.GetHash(id, _seed) / (float)uint.MaxValue;
        }

        protected int CellID(int cx, int cy)
        {
            return Repeat(cy) * _repeat + Repeat(cx);
        }

        protected int CellID(int cx, int cy, int cz)
        {
            return (Repeat(cz) * _repeat + Repeat(cy)) * _repeat + Repeat(cx);
        }

        #endregion

        #region Noise functions

        protected abstract float Calculate2D(Vector2 point);
        protected abstract float Calculate3D(Vector3 point);

        float Calculate2DFractal(Vector2 point, int level)
        {
            var originalFreq = _freq;
            var originalRepeat = _repeat;

            var sum = 0.0f;
            var w = 0.5f;

            for (var i = 0; i < level; i++)
            {
                sum += Calculate2D(point) * w;
                _freq *= 2;
                _repeat *= 2;
                w *= 0.5f;
            }

            _freq = originalFreq;
            _repeat = originalRepeat;

            return sum;
        }

        float Calculate3DFractal(Vector3 point, int level)
        {
            var originalFreq = _freq;
            var originalRepeat = _repeat;

            var sum = 0.0f;
            var w = 0.5f;

            for (var i = 0; i < level; i++)
            {
                sum += Calculate3D(point) * w;
                _freq *= 2;
                _repeat *= 2;
                w *= 0.5f;
            }

            _freq = originalFreq;
            _repeat = originalRepeat;

            return sum;
        }

        #endregion

    }
}

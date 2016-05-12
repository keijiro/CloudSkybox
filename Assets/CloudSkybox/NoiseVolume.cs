using UnityEngine;

namespace CloudSkybox
{
    public class NoiseVolume : ScriptableObject
    {
        #region Asset creation

        enum NoiseType { Perlin, Worley }

        [SerializeField]
        NoiseType _noiseType = NoiseType.Perlin;

        [SerializeField]
        int _frequency = 1;

        [SerializeField]
        int _fractalLevel = 0;

        [SerializeField]
        int _seed;

        [SerializeField, HideInInspector]
        Texture3D _texture;

        const int kDefaultResolution = 32;

        public Texture3D texture {
            get { return _texture; }
        }

        void OnEnable()
        {
            if (_texture == null)
            {
                _texture = new Texture3D(
                    kDefaultResolution,
                    kDefaultResolution,
                    kDefaultResolution,
                    TextureFormat.Alpha8, false
                );
                _texture.name = "Texture3D";
            }
        }

        public void ChangeResolution(int newResolution)
        {
            DestroyImmediate(_texture);

            _texture = new Texture3D(
                newResolution,
                newResolution,
                newResolution,
                TextureFormat.Alpha8, false
            );
            _texture.name = "Texture3D";
        }

        public void RebuildTexture()
        {
            if (_texture == null)
            {
                Debug.LogError("Texture3D asset is missing.");
                return;
            }

            var size = _texture.width;
            var scale = 1.0f / size;

            NoiseTools.NoiseGeneratorBase noise;
            if (_noiseType == NoiseType.Perlin)
                noise = new NoiseTools.PerlinNoise(_frequency, 1, _seed);
            else
                noise = new NoiseTools.WorleyNoise(_frequency, 1, _seed);

            var buffer = new Color[size * size * size];
            var index = 0;

            for (var ix = 0; ix < size; ix++)
            {
                var x = scale * ix;
                for (var iy = 0; iy < size; iy++)
                {
                    var y = scale * iy;
                    for (var iz = 0; iz < size; iz++)
                    {
                        var z = scale * iz;
                        var c = _fractalLevel > 1 ?
                            noise.GetFractal(x, y, z, _fractalLevel) :
                            noise.GetAt(x, y, z);
                        buffer[index++] = new Color(c, c, c, c);
                    }
                }
            }

            _texture.SetPixels(buffer);
            _texture.Apply();
        }

        #endregion
    }
}

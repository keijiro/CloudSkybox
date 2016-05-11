using UnityEngine;

namespace CloudSkybox
{
    public class NoiseVolume : ScriptableObject
    {
        #region Asset creation

        // This value is const because we can't resize Texture3D.
        const int kResolution = 128;

        public int resolution {
            get { return kResolution; }
        }

        const int kOctaves = 10;

        [SerializeField]
        int _frequency = 1;

        [SerializeField]
        int _seed;

        [SerializeField, HideInInspector]
        Texture3D _texture;

        public Texture3D texture {
            get { return _texture; }
        }

        void OnEnable()
        {
            if (_texture == null)
            {
                _texture = new Texture3D(
                    kResolution, kResolution, kResolution,
                    TextureFormat.Alpha8, false
                );
                _texture.name = "Texture3D";
            }
        }

        public void RebuildTexture()
        {
            if (_texture == null)
            {
                Debug.LogError("Texture3D asset is missing.");
                return;
            }

            var buffer = new Color[kResolution * kResolution * kResolution];

            var index = 0;
            for (var x = 0; x < kResolution; x++)
                for (var y = 0; y < kResolution; y++)
                    for (var z = 0; z < kResolution; z++)
                        buffer[index++] = Noise(x, y, z);

            _texture.SetPixels(buffer);
            _texture.Apply();
        }

        #endregion

        #region Periodic noise generator

        Color Noise(int x, int y, int z)
        {
            var s = (float)_frequency / kResolution;
            var n = Fbm(s * x, s * y, s * z, _frequency);
            n = n * (0.5f / 0.75f) + 0.5f;
            return new Color(n, n, n, n);
        }

        float Fbm(float x, float y, float z, int rep)
        {
            var f = 0.0f;
            var w = 0.5f;
            for (var i = 0; i < kOctaves; i++) {
                f += w * Noise(x, y, z, rep);
                x *= 2;
                y *= 2;
                z *= 2;
                rep *= 2;
                w *= 0.5f;
            }
            return f;
        }

        float Noise(float x, float y, float z, int rep)
        {
            var X = Mathf.FloorToInt(x);
            var Y = Mathf.FloorToInt(y);
            var Z = Mathf.FloorToInt(z);
            x -= Mathf.Floor(x);
            y -= Mathf.Floor(y);
            z -= Mathf.Floor(z);
            var u = Fade(x);
            var v = Fade(y);
            var w = Fade(z);
            var A  = Perm(X  , rep) + Y;
            var B  = Perm(X+1, rep) + Y;
            var AA = Perm(A  , rep) + Z;
            var BA = Perm(B  , rep) + Z;
            var AB = Perm(A+1, rep) + Z;
            var BB = Perm(B+1, rep) + Z;
            return Lerp(w, Lerp(v, Lerp(u, Grad(Perm(AA  , rep), x, y  , z  ), Grad(Perm(BA  , rep), x-1, y  , z  )),
                                   Lerp(u, Grad(Perm(AB  , rep), x, y-1, z  ), Grad(Perm(BB  , rep), x-1, y-1, z  ))),
                           Lerp(v, Lerp(u, Grad(Perm(AA+1, rep), x, y  , z-1), Grad(Perm(BA+1, rep), x-1, y  , z-1)),
                                   Lerp(u, Grad(Perm(AB+1, rep), x, y-1, z-1), Grad(Perm(BB+1, rep), x-1, y-1, z-1))));
        }

        static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        static float Lerp(float t, float a, float b)
        {
            return a + t * (b - a);
        }

        static float Grad(int hash, float x, float y, float z)
        {
            var h = hash & 15;
            var u = h < 8 ? x : y;
            var v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        int Perm(int x, int rep)
        {
            return perm[(x % rep + _seed) & 0xff];
        }

        static int[] perm = {
            151,160,137,91,90,15,
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

        #endregion
    }
}

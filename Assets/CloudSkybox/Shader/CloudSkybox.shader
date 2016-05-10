Shader "CloudSkybox"
{
    Properties
    {
        _NoiseTex("Noise Volume", 3D) = ""{}

        [Space]
        _Altitude0("Altitude (bottom)", Float) = 5000
        _Altitude1("Altitude (top)", Float) = 6000

        [Space]
        _Scatter("Scattering Coeff", Float) = 0.008
        _Extinct("Extinction Coeff", Float) = 0.01

        [Space]
        _NoiseFreq("Noise Frequency", Float) = 1.1
        _NoiseOffset("Noise Offset", Float) = 0
        _NoiseAmplitude("Noise Amplitude", Float) = 1

        [Space]
        _SunSize ("Sun Size", Range(0,1)) = 0.04
        
        _AtmosphereThickness ("Atmoshpere Thickness", Range(0,5)) = 1.0
        _SkyTint ("Sky Tint", Color) = (.5, .5, .5, 1)
        _GroundColor ("Ground", Color) = (.369, .349, .341, 1)

        _Exposure("Exposure", Range(0, 8)) = 1.3
    }

    CGINCLUDE

    static const int kLightSampleCount = 50;
    static const int kCloudSampleCount = 50;

    struct appdata_t
    {
        float4 vertex : POSITION;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float3 rayDir : TEXCOORD0;
        float3 groundColor : TEXCOORD1;
        float3 skyColor : TEXCOORD2;
        float3 sunColor : TEXCOORD3;
    };

    #include "ProceduralSky.cginc"

    sampler3D _NoiseTex;

    float _Altitude0;
    float _Altitude1;

    float _Scatter;
    float _Extinct;

    float _NoiseFreq;
    float _NoiseOffset;
    float _NoiseAmplitude;

    float SampleNoise(float3 uvw)
    {
        float d = exp(-0.0001 * length(uvw.xz));
        uvw *= _NoiseFreq * 1e-5;
        float n = tex3Dlod(_NoiseTex, float4(uvw, 0)).a * 2 - 1;
        return max(0.0, n * _NoiseAmplitude + _NoiseOffset) * d;
    }

    float MarchLight(float3 pos)
    {
        float3 light = _WorldSpaceLightPos0.xyz;

        float stride = (_Altitude1 / light.y - pos.y / light.y) / kLightSampleCount + 1;
        float acc = 1;

        while (pos.y < _Altitude1)
        {
            float n = SampleNoise(pos);
            acc *= exp(-_Extinct * n * stride);
            pos += light * stride;
        }

        return acc;
    }

    v2f vert(appdata_t v)
    {
        v2f o;
        o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
        vert_sky(v.vertex.xyz, o);
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {
        float3 v = -i.rayDir;
        if (v.y <= 0.01) return 0;

        float d0 = _Altitude0 / v.y;
        float d1 = _Altitude1 / v.y;

        float stride = (d1 - d0) / kCloudSampleCount;
        float3 acc = frag_sky(i);
        float3 pos = _WorldSpaceCameraPos + v * d0;

        [loop]
        for (float d = d0; d < d1; d += stride)
        {
            float n = SampleNoise(pos);
            acc *= exp(-_Extinct * n * stride);
            acc += n * stride * _Scatter * MarchLight(pos);
            pos += v * stride;
        }

        return half4(acc, 1);
    }

    ENDCG

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}

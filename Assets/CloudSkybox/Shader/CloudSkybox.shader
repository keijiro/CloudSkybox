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
        _HGCoeff("Henyey-Greenstein", Float) = 0.5
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

    static const int kLightSampleCount = 10;
    static const int kCloudSampleCount = 40;

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
    float _HGCoeff;
    float _Extinct;

    float _NoiseFreq;
    float _NoiseOffset;
    float _NoiseAmplitude;

    float SampleNoise(float3 uvw)
    {
        float d = exp(-0.0001 * length(uvw.xz));
        uvw *= _NoiseFreq * 1e-5;
        float n = tex3Dlod(_NoiseTex, float4(uvw, 0)).a * 2 - 1;
        n += (tex3Dlod(_NoiseTex, float4(uvw*11.13, 0)).a * 2 - 1) * 0.2;
        return max(0.0, n * _NoiseAmplitude + _NoiseOffset) * d;
    }

    float MarchTowardLight(float3 pos)
    {
        float3 light = _WorldSpaceLightPos0.xyz;

        float p0 = pos.y      / light.y;
        float p1 = _Altitude1 / light.y;
        float stride = (p1 - p0) / kLightSampleCount;
        
        float acc = 1;
        float d = 0;

        for (int i = 0; i < kLightSampleCount; i++)
        {
            float n = SampleNoise(pos);

            if (n > 0.0)
            {
                d += n * stride;
            }
            else if (d > 0)
            {
        //        acc *= exp(-_Extinct * d);
//                acc *= 1 - exp(-2 * _Extinct * d);
         //       d = 0;
            }

            pos += light * stride;
        }

        if (d > 0)
            acc *= exp(-_Extinct * d)
         * (1 - exp(-2 * _Extinct * d));

        return acc;
    }

    v2f vert(appdata_t v)
    {
        v2f o;
        o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
        vert_sky(v.vertex.xyz, o);
        return o;
    }

    float HG(float cs)
    {
        const float g = _HGCoeff;
        return 0.5 * (1 - g * g) / pow(1 + g * g - 2 * g * cs, 1.5);
    }

    fixed4 frag(v2f i) : SV_Target
    {
        float3 v = -i.rayDir;

        float p0 = _Altitude0 / v.y;
        float p1 = _Altitude1 / v.y;
        float stride = (p1 - p0) / kCloudSampleCount;

        float3 acc = 0;
        float3 pos = _WorldSpaceCameraPos + v * p0;

        float cs = dot(v, _WorldSpaceLightPos0.xyz);
        float hg = HG(cs);

        float d = 0;

        if (v.y > 0.01)
        [loop]
        for (int idx = 0; idx < kCloudSampleCount; idx++)
        {
            float n = SampleNoise(pos);
            if (n > 0.0)
            {
                d += n * stride;
                acc += n * stride * _Scatter * hg * MarchTowardLight(pos) * exp(-_Extinct * d)
         ;//* (1 - exp(-2 * _Extinct * d));
            }
            else if (d > 0.01)
            {
                //acc *= exp(-_Extinct * d);
//                acc *= 1 - exp(-2 * _Extinct * d);
                //d = 0;
            }
            /*
            acc *= exp(-_Extinct * n * stride);// * (1.0 - exp(-2.0 * _Extinct * n * stride));
            acc += n * stride * _Scatter * hg * MarchTowardLight(pos);
            */
            pos += v * stride;
        }

        //if (d > 0)
            //acc *= exp(-_Extinct * d);

        acc += frag_sky(i) * exp(-_Extinct * d);

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

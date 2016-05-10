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
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    static const int kLightSampleCount = 30;
    static const int kCloudSampleCount = 30;

    struct appdata_t
    {
        float4 vertex : POSITION;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float3 texcoord : TEXCOORD0;
    };

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
        float stride = (_Altitude1 - pos.y) / kLightSampleCount + 1;
        float acc = 1;

        while (pos.y < _Altitude1)
        {
            float n = SampleNoise(pos);
            acc *= exp(-_Extinct * n * stride);
            pos.y += stride;
        }

        return acc;
    }

    v2f vert(appdata_t v)
    {
        v2f o;
        o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
        o.texcoord = v.vertex.xyz;
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {
        float3 v = i.texcoord;
        if (v.y <= 0.01) return 0;

        float d0 = _Altitude0 / v.y;
        float d1 = _Altitude1 / v.y;

        float stride = (d1 - d0) / kCloudSampleCount;
        float3 acc = float3(0.2, 0.5, 0.6);
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

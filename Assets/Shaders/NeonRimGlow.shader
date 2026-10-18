Shader "ZeroGStacker/NeonRimGlow"
{
    Properties
    {
        [HDR] _BaseColor("Base Color", Color) = (0.0, 0.94, 1.0, 1.0)
        [HDR] _RimColor("Rim Glow Color", Color) = (1.5, 1.5, 2.0, 1.0)
        _RimPower("Rim Power", Range(0.1, 8.0)) = 2.5
        _InnerAlpha("Inner Alpha", Range(0.0, 1.0)) = 0.85
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "NeonRimPass"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _RimColor;
                half _RimPower;
                half _InnerAlpha;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Procedural 2D distance from center for rim calculation
                float2 centerUV = input.uv - float2(0.5, 0.5);
                float dist = length(centerUV) * 2.0;
                dist = saturate(dist);

                float rim = pow(dist, _RimPower);
                half4 col = lerp(_BaseColor * _InnerAlpha, _RimColor, rim);
                col.a = saturate(lerp(_InnerAlpha, 1.0, rim));
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Sprites/Default"
}

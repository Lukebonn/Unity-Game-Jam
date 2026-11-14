// The lucky-star glow around the player (PlayerStar): a see-through mirror ball. The sphere is cut
// into facets that each shine their own colour and flash, spinning slowly. Additive, so it only
// ever adds light, and brightest at the edges so the player stays visible inside it.
Shader "Remus/Disco Aura"
{
    Properties
    {
        _Intensity ("Brightness", Range(0, 4)) = 1.6
        _Facets ("Mirror Facets", Float) = 12
        _Speed ("Spin Speed", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "DiscoAura"
            Blend One One
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _Intensity;
                float _Facets;
                float _Speed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float3 directionOS : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(positions.positionWS);
                output.directionOS = normalize(input.positionOS.xyz);
                return output;
            }

            float3 Rainbow(float hue)
            {
                return saturate(abs(frac(hue + float3(0.0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0) - 1.0);
            }

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float t = _Time.y * _Speed;

                // Which mirror facet this point is on (the ball turns slowly)
                float around = atan2(input.directionOS.z, input.directionOS.x) / 6.2831853 + 0.5 + t * 0.08;
                float up = input.directionOS.y * 0.5 + 0.5;
                float2 facet = floor(float2(around * _Facets * 2.0, up * _Facets));
                float seed = Hash(facet);

                float3 colour = Rainbow(seed + t * 0.5);
                float flash = pow(saturate(sin(seed * 6.2831853 + t * 5.0)), 8.0);

                // Faint in the middle so the player shows through, bright at the edges
                float facing = saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS)));
                float rim = pow(1.0 - facing, 2.0);
                float glow = rim * 0.9 + flash * 0.45 + 0.05;
                return half4(colour * glow * _Intensity, 1.0);
            }
            ENDHLSL
        }
    }
}

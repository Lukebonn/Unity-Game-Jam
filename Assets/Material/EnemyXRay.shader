// Draws a glowing outline of enemies where a pillar hides them.
// Used by the "Enemy Outline Through Pillars" renderer feature on PC_Renderer and Mobile_Renderer.
// That feature only draws where a pillar was drawn (stencil 1) and where the enemy is behind it (depth Greater).
Shader "Remus/Enemy X-Ray Outline"
{
    Properties
    {
        _Color ("Outline Colour", Color) = (1, 0.35, 0.2, 1)
        _Power ("Edge Sharpness", Range(0.5, 8)) = 2.5
        _Fill ("Inside Fill", Range(0, 1)) = 0.08
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "XRayOutline"
            Blend SrcAlpha One
            ZWrite Off
            ZTest Greater
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Power;
                half _Fill;
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
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(positions.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Bright at the edges (where the surface turns away from the camera), faint in the middle
                half facing = saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS)));
                half edge = pow(1.0h - facing, _Power);
                return half4(_Color.rgb, saturate(edge + _Fill) * _Color.a);
            }
            ENDHLSL
        }
    }
}

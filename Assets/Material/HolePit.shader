// The inside of a hole in the floor: dark walls that fade to black as they go down.
// It draws a box's inside faces (Cull Front), so a box under a hole shows its walls
// and bottom through the hole, and nothing on top. Used by Material/HolePit on the Trench prefab.
Shader "Remus/Hole Pit"
{
    Properties
    {
        _TopColor ("Top Colour", Color) = (0.2, 0.17, 0.15, 1)
        _BottomColor ("Bottom Colour", Color) = (0, 0, 0, 1)
        _FadeDepth ("Fade Depth (metres)", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "HolePit"
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _BottomColor;
                float _FadeDepth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float depth : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                // Metres below the top of the box: the box is 1 unit tall (-0.5 to 0.5), scaled by its height
                float height = length(float3(UNITY_MATRIX_M._m01, UNITY_MATRIX_M._m11, UNITY_MATRIX_M._m21));
                output.depth = (0.5 - input.positionOS.y) * height;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half t = saturate(input.depth / _FadeDepth);
                return lerp(_TopColor, _BottomColor, t);
            }
            ENDHLSL
        }
    }
}

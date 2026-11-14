// The red attack warning on the ground (GroundTelegraph). Red in the middle, orange at the edges,
// see-through. _Shape 0 is a strip (fills from the attacker outwards along its length),
// _Shape 1 is a circle or wedge (fills from the centre outwards). _Fill is the charge, 0 to 1.
// The per-warning values come from a MaterialPropertyBlock set by GroundTelegraph.cs.
Shader "Remus/Ground Telegraph"
{
    Properties
    {
        _CenterColor ("Centre Colour", Color) = (1, 0.08, 0.04, 1)
        _EdgeColor ("Edge Colour", Color) = (1, 0.55, 0.08, 1)
        _Alpha ("Opacity", Range(0, 1)) = 0.42
        _Shape ("Shape (0 strip, 1 arc)", Float) = 0
        _Arc ("Arc Half Angle (radians)", Float) = 3.1416
        _Fill ("Charge (0 to 1)", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-50" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "GroundTelegraph"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _CenterColor;
                half4 _EdgeColor;
                half _Alpha;
                float _Shape;
                float _Arc;
                float _Fill;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float edge;      // 0 in the middle of the shape, 1 at its edge
                float progress;  // how far along the charge has to reach this point
                float sides = 1; // fades the straight sides of a wedge

                if (_Shape < 0.5)
                {
                    edge = abs(input.uv.x - 0.5) * 2;
                    progress = input.uv.y;
                }
                else
                {
                    float2 p = input.uv * 2 - 1;        // -1..1, +y points forward
                    float r = length(p);
                    float angle = abs(atan2(p.x, p.y));
                    if (r > 1 || angle > _Arc) discard;
                    edge = r;
                    progress = r;
                    if (_Arc < 3.1) sides = saturate((_Arc - angle) / 0.12);
                }

                half3 colour = lerp(_CenterColor.rgb, _EdgeColor.rgb, saturate(edge));
                half alpha = _Alpha * (1 - smoothstep(0.82, 1, edge) * 0.6) * sides;
                if (progress < _Fill) alpha = saturate(alpha + 0.22);   // the charge creeping outwards
                return half4(colour, alpha);
            }
            ENDHLSL
        }
    }
}

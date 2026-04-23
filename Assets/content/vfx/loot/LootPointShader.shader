Shader "Custom/LootPointShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 0.85, 0.3, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _CoreSharpness("Core Sharpness",        Range(20, 300)) = 100
        _RayLength("Primary Ray Length",         Range(5,  100)) = 30
        _RayWidth("Primary Ray Width",           Range(50, 800)) = 350
        _SecondaryLength("Secondary Ray Length", Range(5,  100)) = 22
        _SecondaryWidth("Secondary Ray Width",   Range(50, 800)) = 560
        _SecondaryStrength("Secondary Strength", Range(0,  1  )) = 0.35
        _SecondaryAngle("Secondary Angle (deg)", Range(0,  360)) = 45
        _GlowRadius("Glow Radius",               Range(1,  40 )) = 12
        _PulseSpeed("Pulse Speed",               Range(0,  10 )) = 3.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend One One
        ZWrite Off
       // ZTest Always
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4   _BaseColor;
                float4  _BaseMap_ST;
                float   _CoreSharpness;
                float   _RayLength;
                float   _RayWidth;
                float   _SecondaryLength;
                float   _SecondaryWidth;
                float   _SecondaryStrength;
                float   _SecondaryAngle;
                float   _GlowRadius;
                float   _PulseSpeed;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv - 0.5;
                float  t  = _Time.y;

                // ── Animation ──────────────────────────────────────────
                float pulse   = lerp(0.80, 1.20, sin(t * _PulseSpeed)        * 0.5 + 0.5);
                float twinkle = lerp(0.90, 1.10, sin(t * _PulseSpeed * 2.71) * 0.5 + 0.5);

                float r2 = dot(uv, uv);

                // ── Core ───────────────────────────────────────────────
                float core = exp(-r2 * _CoreSharpness * pulse);

                // ── Soft halo ──────────────────────────────────────────
                float halo = exp(-r2 * _GlowRadius * pulse) * 0.45;

                // ── Primary rays (axis-aligned, 4-point) ───────────────
                float rW = _RayWidth  * pulse;
                float rL = _RayLength * pulse;

                float rayH = exp(-uv.y * uv.y * rW) * exp(-uv.x * uv.x * rL);
                float rayV = exp(-uv.x * uv.x * rW) * exp(-uv.y * uv.y * rL);

                // ── Secondary rays (rotatable, 4-point) ────────────────
                // Convert angle to radians and build a 2D rotation matrix
                float  rad  = _SecondaryAngle * (3.14159265 / 180.0);
                float  cosA = cos(rad);
                float  sinA = sin(rad);
                // Rotate UV into the secondary rays' local space
                float2 uvS  = float2(cosA * uv.x + sinA * uv.y,
                                    -sinA * uv.x + cosA * uv.y);

                float sW = _SecondaryWidth  * pulse;
                float sL = _SecondaryLength * pulse;

                float rayS1 = exp(-uvS.y * uvS.y * sW) * exp(-uvS.x * uvS.x * sL);
                float rayS2 = exp(-uvS.x * uvS.x * sW) * exp(-uvS.y * uvS.y * sL);
                float raysS = (rayS1 + rayS2) * _SecondaryStrength;

                // ── Combine ────────────────────────────────────────────
                float rays  = (rayH + rayV + raysS) * twinkle;
                float total = core + halo + rays;

                // ── Colour ─────────────────────────────────────────────
                half3 col  = lerp(_BaseColor.rgb, half3(1, 1, 1),
                                  saturate(core * 4.0 + halo * 0.5));
                col       += _BaseColor.rgb * rays;

                return half4(col * total, total);
            }
            ENDHLSL
        }
    }
}
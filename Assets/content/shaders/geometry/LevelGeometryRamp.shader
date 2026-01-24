Shader "LevelGeometryRamp"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (0.2,0.2,0.2,1)
        _HighlightColor ("Highlight Color", Color) = (1,1,1,1)
        
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.3
        _HighlightThreshold ("Highlight Threshold", Range(0, 1)) = 0.7
        _Smoothness ("Blend Smoothness", Range(0.01, 1)) = 0.1
        
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _FadeDistance ("Fade Distance", Float) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 color : COLOR;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _ShadowColor;
                float4 _HighlightColor;
                float _ShadowThreshold;
                float _HighlightThreshold;
                float _Smoothness;
                
                float4 _MainTex_ST;
                float _Glossiness;
                float _Metallic;
                float _FadeDistance;
            CBUFFER_END
            
            float4 _OcclusionSphereCenter;
            float _OcclusionSphereRadius;
            float _OcclusionCircleOffset;
            
            static const float DitherPattern[16] = 
            {
                0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
                12.0/16.0, 4.0/16.0, 14.0/16.0,  6.0/16.0,
                3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
                15.0/16.0, 7.0/16.0, 13.0/16.0,  5.0/16.0
            };
            
            float GetDitherValue(float4 screenPos)
            {
                int2 ditherCoord = int2(screenPos.xy+(_OcclusionSphereCenter.xz*100)%4) % 4;
                return DitherPattern[ditherCoord.y * 4 + ditherCoord.x];
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Occlusion Logic (Copied from original)
                float3 cameraPos = _WorldSpaceCameraPos;
                float3 toCenter = _OcclusionSphereCenter.xyz - cameraPos;
                float centerDist = length(toCenter);
                float3 viewDir = toCenter / centerDist;
                
                float3 circleCenter = _OcclusionSphereCenter.xyz - viewDir * _OcclusionCircleOffset;
                float circleDist = centerDist - _OcclusionCircleOffset;
                
                float3 toFragment = input.positionWS - cameraPos;
                float fragmentProjection = dot(toFragment, viewDir);
                
                if (fragmentProjection > 0 && fragmentProjection < circleDist)
                {
                    float3 fragmentOnPlane = cameraPos + viewDir * circleDist;
                    float3 toFragmentFromPlane = input.positionWS - fragmentOnPlane;
                    float3 projectedOffset = toFragmentFromPlane - viewDir * dot(toFragmentFromPlane, viewDir);
                    float distFromCenter = length(projectedOffset);
                    
                    if (distFromCenter < _OcclusionSphereRadius)
                    {
                        float fadeStart = circleDist - _FadeDistance;
                        float distanceFade = saturate((fragmentProjection - fadeStart) / _FadeDistance);
                        float radialFade = distFromCenter / _OcclusionSphereRadius;
                        float ditherThreshold = GetDitherValue(input.positionCS);
                        radialFade *= radialFade * radialFade * radialFade;
                        float mask = ditherThreshold * radialFade;
                        
                        clip(mask - 0.1);
                    }
                }
                
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                float3 normalWS = normalize(input.normalWS);
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                
                Light mainLight = GetMainLight(shadowCoord);
                
                // Lighting Intensity Calculation
                half NdotL = dot(normalWS, mainLight.direction);
                half lightIntensity = NdotL * mainLight.shadowAttenuation;
                // Remap to 0-1 range for blending
                lightIntensity = saturate(lightIntensity * 0.5 + 0.5); 

                // 3-Color Blend logic
                half shadowFactor = smoothstep(_ShadowThreshold - _Smoothness, _ShadowThreshold + _Smoothness, lightIntensity);
                half highlightFactor = smoothstep(_HighlightThreshold - _Smoothness, _HighlightThreshold + _Smoothness, lightIntensity);
                
                half3 blendedColor = lerp(_ShadowColor.rgb, _BaseColor.rgb, shadowFactor);
                blendedColor = lerp(blendedColor, _HighlightColor.rgb, highlightFactor);
                
                half3 ambient = SampleSH(normalWS);
                
                // Combine with texture and vertex color
                half3 finalColor = blendedColor * texColor.rgb * input.color.rgb;
                
                // Add ambient light (usually tinted by base/shadow)
                finalColor += ambient * _ShadowColor.rgb * texColor.rgb;
                
                return half4(finalColor, 1);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            float3 _LightDirection;
            
            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                return output;
            }
            
            half4 ShadowFrag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            ZWrite On
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings DepthVert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            half4 DepthFrag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}

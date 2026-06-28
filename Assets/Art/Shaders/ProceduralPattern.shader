Shader "CapsuleWars/ProceduralPattern"
{
    // Procedural coat patterns for body parts. The grayscale base map supplies sculpted detail; the
    // pattern (stripes/spots) is generated in OBJECT SPACE so it wraps the mesh without needing UV-aligned
    // masks. Final albedo = lerp(primary, secondary, pattern) * grayscale, with simple URP main-light shading.
    // Driven per-renderer via MaterialPropertyBlock (_BaseMap, _Primary, _Secondary, _Pattern, _Freq).
    Properties
    {
        _BaseMap   ("Grayscale", 2D) = "white" {}
        _Primary   ("Primary",   Color) = (0.8, 0.6, 0.4, 1)
        _Secondary ("Secondary", Color) = (0.1, 0.08, 0.06, 1)
        _Pattern   ("Pattern (0 solid,1 stripes,2 spots)", Float) = 0
        _Freq      ("Frequency", Float) = 9
        _EyeOn     ("Eyes On", Float) = 0
        _EyeColor  ("Eye Color", Color) = (0.85, 0.72, 0.20, 1)
        _EyeOffset ("Eye Offset (x=spread,y=height,z=depth)", Vector) = (0.12, 0.06, 0.20, 0)
        _EyeRadius ("Eye Radius", Float) = 0.08
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _Primary;
                float4 _Secondary;
                float  _Pattern;
                float  _Freq;
                float  _EyeOn;
                float4 _EyeColor;
                float4 _EyeOffset;
                float  _EyeRadius;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float3 normalWS : TEXCOORD0; float2 uv : TEXCOORD1; float3 positionOS : TEXCOORD2; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.positionOS  = IN.positionOS.xyz;
                return OUT;
            }

            float hash21(float2 p) { p = frac(p * float2(123.34, 345.45)); p += dot(p, p + 34.345); return frac(p.x * p.y); }

            float patternValue(float3 p)
            {
                if (_Pattern < 0.5) return 0.0; // solid
                float ang = atan2(p.z, p.x) / 6.2831853 + 0.5; // 0..1 around the body's vertical axis
                if (_Pattern < 1.5)
                {
                    // stripes: HORIZONTAL bands (rings) running up the body
                    float s = frac(p.y * _Freq * 0.7);
                    return smoothstep(0.40, 0.48, abs(s - 0.5) * 2.0);
                }
                // spots: jittered cells in (angle, height) (leopard / cheetah)
                float2 q = float2(ang * _Freq, p.y * _Freq * 0.5);
                float2 cell = floor(q);
                float2 f = frac(q);
                float2 c = float2(hash21(cell), hash21(cell + 7.3));
                float d = length(f - c);
                return 1.0 - smoothstep(0.16, 0.30, d);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float gray = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).r;
                float pat = saturate(patternValue(IN.positionOS));
                // Eyes (per-race placement): keep the pattern off the eye spheres and tint them with the eye color.
                float eyeMask = 0.0;
                if (_EyeOn > 0.5)
                {
                    float3 eL = float3( _EyeOffset.x, _EyeOffset.y, _EyeOffset.z);
                    float3 eR = float3(-_EyeOffset.x, _EyeOffset.y, _EyeOffset.z);
                    float d = min(distance(IN.positionOS, eL), distance(IN.positionOS, eR));
                    eyeMask = 1.0 - smoothstep(_EyeRadius * 0.7, _EyeRadius, d);
                    pat *= (1.0 - eyeMask);
                }
                float3 baseCol = lerp(_Primary.rgb, _Secondary.rgb, pat) * gray;
                // eye region reads vivid (only lightly shaded by the sculpt) so the iris colour shows
                baseCol = lerp(baseCol, _EyeColor.rgb * (gray * 0.35 + 0.65), eyeMask);
                float3 lightDir = normalize(_MainLightPosition.xyz);
                float ndotl = saturate(dot(normalize(IN.normalWS), lightDir));
                float3 lit = baseCol * (_MainLightColor.rgb * ndotl * 0.8 + 0.4);
                return half4(lit, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}

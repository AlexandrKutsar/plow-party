Shader "Plow Party/Snow Surface"
{
    Properties
    {
        [NoScaleOffset] _HeightMap ("Height Map", 2D) = "black" {}
        _HeightScale ("Height Scale (m)", Float) = 1
        _SnowLine ("Full Snow Height (0-1 of scale)", Range(0.01, 1)) = 0.12
        _CellSize ("Cell Size (m)", Float) = 0.5
        _GroundColor ("Ground", Color) = (0.36, 0.38, 0.41, 1)
        _SnowColor ("Snow", Color) = (0.94, 0.96, 1, 1)
        _PileColor ("Pile", Color) = (0.86, 0.92, 1, 1)
        _ShadeColor ("Shade Tint", Color) = (0.62, 0.72, 0.9, 1)
        _Wrap ("Light Wrap", Range(0, 1)) = 0.35
        _Rim ("Rim Brightness", Range(0, 1)) = 0.12
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _HeightMap_TexelSize;
            float _HeightScale;
            float _SnowLine;
            float _CellSize;
            half4 _GroundColor;
            half4 _SnowColor;
            half4 _PileColor;
            half4 _ShadeColor;
            half _Wrap;
            half _Rim;
        CBUFFER_END

        TEXTURE2D(_HeightMap);
        SAMPLER(sampler_HeightMap);

        float SampleHeightBSpline(float2 uv)
        {
            float2 texel = _HeightMap_TexelSize.xy;
            float2 coord = uv * _HeightMap_TexelSize.zw - 0.5;
            float2 index = floor(coord);
            float2 f = coord - index;
            float2 f2 = f * f;
            float2 f3 = f2 * f;
            float2 w0 = (1.0 - 3.0 * f + 3.0 * f2 - f3) / 6.0;
            float2 w1 = (4.0 - 6.0 * f2 + 3.0 * f3) / 6.0;
            float2 w2 = (1.0 + 3.0 * f + 3.0 * f2 - 3.0 * f3) / 6.0;
            float2 w3 = f3 / 6.0;
            float2 g0 = w0 + w1;
            float2 g1 = w2 + w3;
            float2 h0 = (index - 0.5 + w1 / g0) * texel;
            float2 h1 = (index + 1.5 + w3 / g1) * texel;
            float a = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, float2(h0.x, h0.y), 0).r;
            float b = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, float2(h1.x, h0.y), 0).r;
            float c = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, float2(h0.x, h1.y), 0).r;
            float d = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, float2(h1.x, h1.y), 0).r;
            return g0.y * (g0.x * a + g1.x * b) + g1.y * (g0.x * c + g1.x * d);
        }

        float3 DisplacedObjectPosition(float3 positionOS, float2 uv, out float height)
        {
            height = SampleHeightBSpline(uv);
            return float3(positionOS.x, positionOS.y + height * _HeightScale, positionOS.z);
        }

        float3 HeightNormalWS(float2 uv)
        {
            float2 texel = _HeightMap_TexelSize.xy;
            float left = SampleHeightBSpline(uv - float2(texel.x, 0));
            float right = SampleHeightBSpline(uv + float2(texel.x, 0));
            float down = SampleHeightBSpline(uv - float2(0, texel.y));
            float up = SampleHeightBSpline(uv + float2(0, texel.y));
            float slopeX = (right - left) * _HeightScale / (2.0 * _CellSize);
            float slopeZ = (up - down) * _HeightScale / (2.0 * _CellSize);
            return normalize(float3(-slopeX, 1.0, -slopeZ));
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float height : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float height;
                float3 positionOS = DisplacedObjectPosition(input.positionOS.xyz, input.uv, height);
                VertexPositionInputs positions = GetVertexPositionInputs(positionOS);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = HeightNormalWS(input.uv);
                output.height = height;
                output.fogFactor = ComputeFogFactor(positions.positionCS.z);
                return output;
            }

            half3 Albedo(float height)
            {
                half snow = smoothstep(0.0, _SnowLine, height);
                half pile = saturate((height - _SnowLine) / max(1.0 - _SnowLine, 1e-3));
                half3 cover = lerp(_SnowColor.rgb, _PileColor.rgb, pile);
                return lerp(_GroundColor.rgb, cover, snow);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half facing = dot(normalWS, mainLight.direction);
                half wrapped = saturate((facing + _Wrap) / (1.0 + _Wrap));
                half lit = wrapped * mainLight.shadowAttenuation;
                half3 albedo = Albedo(input.height);
                half3 ambient = SampleSH(normalWS) * lerp(_ShadeColor.rgb, half3(1, 1, 1), lit);
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half rim = pow(1.0 - saturate(dot(normalWS, viewDirWS)), 4.0) * _Rim;
                half3 colour = albedo * (ambient + mainLight.color * lit) + rim;
                colour = MixFog(colour, input.fogFactor);
                return half4(colour, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings DepthVert(Attributes input)
            {
                Varyings output;
                float height;
                output.positionCS = TransformObjectToHClip(DisplacedObjectPosition(input.positionOS.xyz, input.uv, height));
                return output;
            }

            half DepthFrag(Varyings input) : SV_Target
            {
                return input.positionCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthNormalsVert
            #pragma fragment DepthNormalsFrag
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings DepthNormalsVert(Attributes input)
            {
                Varyings output;
                float height;
                output.positionCS = TransformObjectToHClip(DisplacedObjectPosition(input.positionOS.xyz, input.uv, height));
                output.normalWS = HeightNormalWS(input.uv);
                return output;
            }

            half4 DepthNormalsFrag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                #if defined(_GBUFFER_NORMALS_OCT)
                float2 packed = saturate(PackNormalOctQuadEncode(normalWS) * 0.5 + 0.5);
                return half4(PackFloat2To888(packed), 0.0);
                #else
                return half4(normalWS, 0.0);
                #endif
            }
            ENDHLSL
        }
    }

    FallBack Off
}

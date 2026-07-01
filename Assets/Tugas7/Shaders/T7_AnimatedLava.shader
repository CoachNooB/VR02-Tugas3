Shader "Tugas7/Animated Lava"
{
    Properties
    {
        _BaseMap("Base Color", 2D) = "white" {}
        [HDR]_EmissionMap("Emission", 2D) = "white" {}
        [Normal]_NormalMap("Normal", 2D) = "bump" {}
        _HeightMap("Height", 2D) = "gray" {}
        _RoughnessMap("Roughness", 2D) = "white" {}
        _AOMap("Ambient Occlusion", 2D) = "white" {}
        _FlowSpeedA("Flow Speed A", Vector) = (0.025, 0.01, 0, 0)
        _FlowSpeedB("Flow Speed B", Vector) = (-0.012, 0.02, 0, 0)
        _FlowOffsetA("Flow Offset A", Vector) = (0, 0, 0, 0)
        _FlowOffsetB("Flow Offset B", Vector) = (0, 0, 0, 0)
        _Tiling("Tiling", Float) = 1
        _EmissionIntensity("Emission Intensity", Float) = 4
        _NormalStrength("Normal Strength", Range(0, 2)) = 1
        _DistortionStrength("Distortion Strength", Range(0, 0.2)) = 0.035
        _DisplacementAmplitude("Displacement Amplitude", Range(0, 0.1)) = 0.02
        _CrustColor("Crust Color", Color) = (0.12, 0.018, 0.008, 1)
        [HDR]_HotColor("Hot Color", Color) = (5, 0.65, 0.02, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_HeightMap); SAMPLER(sampler_HeightMap);
            TEXTURE2D(_RoughnessMap); SAMPLER(sampler_RoughnessMap);
            TEXTURE2D(_AOMap); SAMPLER(sampler_AOMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _FlowSpeedA, _FlowSpeedB, _FlowOffsetA, _FlowOffsetB;
                float4 _CrustColor, _HotColor;
                float _Tiling, _EmissionIntensity, _NormalStrength;
                float _DistortionStrength, _DisplacementAmplitude;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float4 tangentOS : TANGENT; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1; float3 tangentWS : TEXCOORD2; float3 bitangentWS : TEXCOORD3; float2 uv : TEXCOORD4; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float2 uv = input.uv * _Tiling + _FlowOffsetA.xy + _Time.y * _FlowSpeedA.xy;
                float height = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, uv, 0).r - 0.5;
                input.positionOS.xyz += input.normalOS * height * _DisplacementAmplitude;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normals = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = normals.normalWS;
                output.tangentWS = normals.tangentWS;
                output.bitangentWS = normals.bitangentWS;
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uvA = input.uv * _Tiling + _FlowOffsetA.xy + _Time.y * _FlowSpeedA.xy;
                float2 uvB = input.uv * (_Tiling * 1.73) + _FlowOffsetB.xy + _Time.y * _FlowSpeedB.xy;
                float2 distortion = (SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvB).rg * 2 - 1) * _DistortionStrength;
                float heightA = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uvA + distortion).r;
                float heightB = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uvB - distortion).r;
                float hotMask = saturate(1.25 - (heightA + heightB) * 0.7);
                half3 baseTexture = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvA).rgb;
                half3 crust = baseTexture * _CrustColor.rgb;
                half3 emissionTexture = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uvB).rgb;
                half ao = SAMPLE_TEXTURE2D(_AOMap, sampler_AOMap, uvA).r;
                half3 normalA = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvA), _NormalStrength);
                half3 normalB = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvB), _NormalStrength * 0.65);
                half3 normalTS = normalize(half3(normalA.xy + normalB.xy, normalA.z * normalB.z));
                half3 normalWS = normalize(normalTS.x * input.tangentWS + normalTS.y * input.bitangentWS + normalTS.z * input.normalWS);
                Light mainLight = GetMainLight();
                half ndl = saturate(dot(normalWS, mainLight.direction));
                half3 lit = crust * ao * (0.24 + ndl * mainLight.color);
                half3 emission = emissionTexture * _HotColor.rgb * hotMask * _EmissionIntensity;
                return half4(lit + emission, 1);
            }
            ENDHLSL
        }
    }
}

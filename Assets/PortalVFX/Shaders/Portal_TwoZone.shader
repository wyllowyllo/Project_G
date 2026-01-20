Shader "Custom/Portal_TwoZone_URP"
{
    Properties
    {
        [Header(Texture Masks)]
        [Space(10)]
        _DistortionTexture("Distortion Texture", 2D) = "white" {}
        _DistortionAmount("Distortion Amount", Float) = 0.25
        _DistortionSpeed("Distortion Speed", Float) = 5
        [Space(10)]
        _AdditionalNoiseTexture("Additional Noise Texture", 2D) = "white" {}
        _NoiseSpeed("Noise Speed", Float) = 5
        [Space(10)]
        [Header(Additional Parameters)]
        [Space(10)]
        _Brightness("Brightness", Float) = 1
        _Contrast("Contrast", Float) = 1
        [Space]
        _RadialBrightness("Radial brightness", Float) = 1
        _RadialContrast("Radial contrast", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Portal"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float fogCoord : TEXCOORD2;
                float3 positionOS : TEXCOORD3;
                float3 normalOS : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_DistortionTexture);
            SAMPLER(sampler_DistortionTexture);
            float4 _DistortionTexture_ST;

            TEXTURE2D(_AdditionalNoiseTexture);
            SAMPLER(sampler_AdditionalNoiseTexture);
            float4 _AdditionalNoiseTexture_ST;

            CBUFFER_START(UnityPerMaterial)
                half _Brightness;
                half _Contrast;
                half _RadialBrightness;
                half _RadialContrast;
                half _DistortionAmount;
                half _DistortionSpeed;
                half _NoiseSpeed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                output.positionOS = input.positionOS.xyz;
                output.normalOS = input.normalOS;

                return output;
            }

            // Triplanar 텍스처 샘플링 (UV seam 제거)
            half SampleTriplanar(TEXTURE2D_PARAM(tex, samp), float3 pos, float3 normal, float4 st, float timeOffset)
            {
                float3 blend = abs(normal);
                // 더 부드러운 블렌딩을 위해 pow 적용
                blend = pow(blend, 4);
                blend = blend / (blend.x + blend.y + blend.z + 0.001);

                float2 uvX = pos.yz * st.xy + st.zw - timeOffset;
                float2 uvY = pos.xz * st.xy + st.zw - timeOffset;
                float2 uvZ = pos.xy * st.xy + st.zw - timeOffset;

                half texX = SAMPLE_TEXTURE2D(tex, samp, uvX).r;
                half texY = SAMPLE_TEXTURE2D(tex, samp, uvY).r;
                half texZ = SAMPLE_TEXTURE2D(tex, samp, uvZ).r;

                return texX * blend.x + texY * blend.y + texZ * blend.z;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 normal = normalize(input.normalOS);

                // Triplanar 샘플링
                half distortion = SampleTriplanar(
                    TEXTURE2D_ARGS(_DistortionTexture, sampler_DistortionTexture),
                    input.positionOS, normal, _DistortionTexture_ST, _Time.x * _DistortionSpeed);
                distortion = pow(distortion * max(_Brightness, 0), _Contrast);

                // 오브젝트 공간 좌표 기반 radial (UV seam 없음)
                half radialDist = saturate(length(input.positionOS) * 2);

                // 중심 = Noise, 가장자리 = Distortion
                half uMask = radialDist * max(_RadialBrightness, 0);
                half uMaskInv = 1 - radialDist;

                // Triplanar 샘플링
                half additionalNoise = SampleTriplanar(
                    TEXTURE2D_ARGS(_AdditionalNoiseTexture, sampler_AdditionalNoiseTexture),
                    input.positionOS, normal, _AdditionalNoiseTexture_ST, _Time.x * _NoiseSpeed * 0.25);
                additionalNoise *= uMaskInv * uMaskInv;
                additionalNoise *= 3;

                // Sample opaque texture with distortion
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 distortedUV = screenUV + distortion * _DistortionAmount * 0.1;
                half3 bgcolor = SampleSceneColor(distortedUV);
                half4 color = half4(bgcolor, 1.0);

                // 오른쪽 절반: Distortion 효과
                half distortionEffect = distortion * distortion * uMask;
                distortionEffect += uMask;

                // 왼쪽 절반: Noise 효과
                half noiseEffect = additionalNoise * uMaskInv * 3;
                noiseEffect += uMaskInv;

                // 두 효과 합치기
                half finalEffect = distortionEffect + noiseEffect;

                color *= input.color;
                color.rgb += additionalNoise * (additionalNoise * additionalNoise + additionalNoise) * 0.1 * uMaskInv;

                color.rgb = MixFog(color.rgb, input.fogCoord);

                return half4(color.rgb * finalEffect, color.a * finalEffect);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}

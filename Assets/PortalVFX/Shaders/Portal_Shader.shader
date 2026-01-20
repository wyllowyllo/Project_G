Shader "Custom/Portal_URP"
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
        _CenterGlow("Center Glow", Float) = 0.25
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
                half _CenterGlow;
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

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 distortionUV = TRANSFORM_TEX(input.uv, _DistortionTexture) - _Time.x * _DistortionSpeed;
                half distortion = SAMPLE_TEXTURE2D(_DistortionTexture, sampler_DistortionTexture, distortionUV).r;
                distortion = pow(distortion * max(_Brightness, 0), _Contrast);

                half uMaskOriginal = input.uv.x;
                half uMask = input.uv.x * max(_RadialBrightness, 0);
                half uMaskInv = 1 - uMask;

                float2 noiseUV = TRANSFORM_TEX(input.uv, _AdditionalNoiseTexture) - _Time.x * _NoiseSpeed * 0.25;
                half additionalNoise = SAMPLE_TEXTURE2D(_AdditionalNoiseTexture, sampler_AdditionalNoiseTexture, noiseUV).r;
                additionalNoise *= uMaskInv * uMaskInv;
                additionalNoise *= 3;

                // Sample opaque texture with distortion
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 distortedUV = screenUV + distortion * _DistortionAmount * 0.1;
                half3 bgcolor = SampleSceneColor(distortedUV);
                half4 color = half4(bgcolor, 1.0);

                distortion *= distortion * uMask;
                distortion += additionalNoise * 10;
                distortion += uMask;
                distortion *= uMask;
                distortion += uMask;

                color *= input.color;
                color.rgb += uMaskOriginal * (uMaskOriginal * uMaskOriginal * uMaskOriginal + uMaskOriginal) * _CenterGlow;
                color.rgb += additionalNoise * (additionalNoise * additionalNoise + additionalNoise) * 0.01;

                color.rgb = MixFog(color.rgb, input.fogCoord);

                return half4(color.rgb * distortion, color.a * distortion);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}

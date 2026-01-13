Shader "UI/LevelUpGradient"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Gradient Settings)]
        _GradientProgress ("Gradient Progress", Range(0, 1)) = 1
        _GradientWidth ("Gradient Width", Range(0, 1)) = 0.2
        _GradientPower ("Gradient Power", Range(0.1, 5)) = 2
        
        [Header(Unity UI)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _GradientProgress;
            float _GradientWidth;
            float _GradientPower;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 텍스처 샘플링
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                // UV 좌표를 중심(0.5)을 기준으로 계산
                float distFromCenter = abs(IN.texcoord.x - 0.5) * 2.0; // 0~1 범위

                // 그라데이션 진행도에 따라 알파 계산
                float gradientEdge = _GradientProgress;
                float gradientStart = gradientEdge - _GradientWidth;

                // 부드러운 그라데이션 적용
                float alpha = 1.0 - smoothstep(gradientStart, gradientEdge, distFromCenter);
                alpha = pow(alpha, _GradientPower);

                // 최종 알파 적용
                color.a *= alpha;

                // UI 클리핑
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                // 알파가 0에 가까우면 완전히 제거
                clip(color.a - 0.001);

                return color;
            }
            ENDCG
        }
    }
}

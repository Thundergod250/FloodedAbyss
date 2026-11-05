
Shader "Custom/OutlineGlow"
{
    Properties
    {
        [HDR] _OutlineColor ("Outline Color (HDR)", Color) = (1, 1, 0, 1)
        _OutlineWidth ("Outline Width (Pixels)", Range(0, 20)) = 3.0
        _GlowIntensity ("Glow Intensity", Range(0, 25)) = 5.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        LOD 100

        Pass
        {
            Cull Front
            ZWrite Off
            ZTest LEqual

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float4 _OutlineColor;
            float _OutlineWidth;
            float _GlowIntensity;

            v2f vert(appdata v)
            {
                v2f o;

                // Original vertex position in clip space
                o.pos = UnityObjectToClipPos(v.vertex);

                // Transform the normal into view space
                float3 normalVS = normalize(
                    mul((float3x3)UNITY_MATRIX_IT_MV, v.normal)
                );

                // Project the normal into screen space
                float2 projectedNormal =
                    TransformViewToProjection(normalVS.xy);

                // Normalize the projected direction
                float normalLength = length(projectedNormal);

                if (normalLength > 0.00001)
                {
                    projectedNormal /= normalLength;
                }
                else
                {
                    projectedNormal = float2(0, 0);
                }

                // Expand in actual screen pixels.
                // Multiplying by clip W keeps the width consistent
                // regardless of camera distance.
                float2 pixelOffset =
                    projectedNormal *
                    (2.0 * _OutlineWidth / _ScreenParams.xy);

                o.pos.xy += pixelOffset * o.pos.w;

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Increase RGB brightness without changing opacity.
                float3 finalColor =
                    _OutlineColor.rgb * _GlowIntensity;

                return float4(
                    finalColor,
                    saturate(_OutlineColor.a)
                );
            }

            ENDHLSL
        }
    }
}

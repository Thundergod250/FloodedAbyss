
Shader "Custom/OutlineGlow"
{
    Properties
    {
        [HDR] _OutlineColor ("Outline Color (HDR)", Color) = (1, 1, 0, 1)

        [Enum(World Space, 0, Screen Pixels, 1)]
        _OutlineMode ("Outline Mode", Float) = 0

        _WorldOutlineWidth ("World Space Width", Range(0, 0.5)) = 0.025
        _PixelOutlineWidth ("Pixel Width", Range(0, 20)) = 3.0

        _GlowIntensity ("Glow Intensity", Range(1, 25)) = 5.0
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
            Offset -1, -1

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
            float _OutlineMode;
            float _WorldOutlineWidth;
            float _PixelOutlineWidth;
            float _GlowIntensity;

            v2f vert(appdata v)
            {
                v2f o;

                // MODE 0: WORLD SPACE
                if (_OutlineMode < 0.5)
                {
                    float3 worldPos =
                        mul(unity_ObjectToWorld, v.vertex).xyz;

                    float3 worldNormal =
                        normalize(UnityObjectToWorldNormal(v.normal));

                    // Expand using actual world units.
                    worldPos += worldNormal * _WorldOutlineWidth;

                    o.pos = mul(
                        UNITY_MATRIX_VP,
                        float4(worldPos, 1.0)
                    );
                }
                // MODE 1: SCREEN PIXELS
                else
                {
                    o.pos = UnityObjectToClipPos(v.vertex);

                    float3 normalVS = normalize(
                        mul((float3x3)UNITY_MATRIX_IT_MV, v.normal)
                    );

                    float2 projectedNormal =
                        mul((float2x2)UNITY_MATRIX_P, normalVS.xy);

                    float2 pixelOffset =
                        projectedNormal *
                        (2.0 * _PixelOutlineWidth / _ScreenParams.xy);

                    o.pos.xy += pixelOffset * o.pos.w;
                }

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
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

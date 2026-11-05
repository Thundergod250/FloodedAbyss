Shader "Custom/OutlineGlow"
{
    Properties
    {
        // [HDR] allows the color to go above 1.0 brightness (which triggers Bloom)
        [HDR] _OutlineColor ("Outline Color (HDR)", Color) = (1, 1, 0, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 10.0)) = 2.0
        _GlowIntensity ("Glow Intensity", Range(1.0, 10.0)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Cull Front
            ZWrite Off
            ZTest LEqual
            
            // THIS FIXES THE FAINTNESS: 
            // It pushes the outline slightly forward so it doesn't fight with the object's pixels
            Offset -1, -1 

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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

            float _OutlineWidth;
            float4 _OutlineColor;
            float _GlowIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                
                // Get the vertex position in clip space
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // Get the normal in view space
                float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                
                // Project the normal into 2D screen space
                float2 offset = TransformViewToProjection(norm.xy);
                
                // Offset the vertex based on screen aspect ratio and width
                o.pos.xy += offset * o.pos.z * _OutlineWidth * 0.001; 
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Multiply the color by the intensity to push it into HDR range
                return _OutlineColor * _GlowIntensity;
            }
            ENDHLSL
        }
    }
}
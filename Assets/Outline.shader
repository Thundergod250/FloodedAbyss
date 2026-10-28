Shader "Custom/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1, 1, 0, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 10.0)) = 2.0 // Note: Higher numbers = thicker
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            // Cull front faces so we only see the "inside" of the extruded mesh
            Cull Front
            ZWrite On
            ZTest LEqual

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

            v2f vert (appdata v)
            {
                v2f o;
                
                // 1. Get the vertex position in clip space
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // 2. Get the normal in view space
                float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                
                // 3. Project the normal into 2D screen space
                float2 offset = TransformViewToProjection(norm.xy);
                
                // 4. Offset the vertex based on screen aspect ratio and width
                o.pos.xy += offset * o.pos.z * _OutlineWidth * 0.001; 
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
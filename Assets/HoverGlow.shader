Shader "Custom/HoverGlow"
{
    Properties
    {
        _Color ("Glow Color", Color) = (1, 1, 0, 1)
        _GlowPower ("Glow Power", Range(0.5, 5.0)) = 3.0
        _Thickness ("Thickness (Make Bigger)", Range(0.0, 1.0)) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        
        Pass
        {
            // This Offset command fixes the Z-Fighting flickering by pushing the glow slightly towards the camera
            Offset -1, -1 
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

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
                float3 normal : NORMAL;
                float3 viewDir : TEXCOORD0;
            };

            float4 _Color;
            float _GlowPower;
            float _Thickness;

            v2f vert (appdata v)
            {
                v2f o;
                
                // Push the vertex outward along its normal to make the glow physically bigger
                v.vertex.xyz += v.normal * _Thickness;
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float fresnel = dot(i.normal, i.viewDir);
                fresnel = saturate(1.0 - fresnel);
                fresnel = pow(fresnel, _GlowPower);
                
                return half4(_Color.rgb, fresnel * _Color.a);
            }
            ENDHLSL
        }
    }
}
Shader "Custom/SoftLightBeam"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,0.6,1)
        _Intensity ("Intensity", Float) = 4.0
        _RadiusSoftness ("Edge Softness", Float) = 2.0
        _HeightFade ("Height Fade", Float) = 1.5
        _CoreSharpness ("Core Sharpness", Float) = 6.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
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
                float3 localPos : TEXCOORD0;
            };

            float4 _Color;
            float _Intensity;
            float _RadiusSoftness;
            float _HeightFade;
            float _CoreSharpness;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex.xyz;   // object-space position
                return o;
            }

            float remap(float value, float inMin, float inMax, float outMin, float outMax)
            {
                return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // ----- RADIAL DISTANCE (x,z) -----
                float dist = length(i.localPos.xz);  // distance from cylinder center
                float edgeFade = exp(-dist * _RadiusSoftness);

                // ----- VERTICAL FADE -----
                float height = i.localPos.y;
                float verticalFade = saturate(exp(-abs(height) * _HeightFade));

                // ----- GLOWING CORE -----
                float core = 1.0 - smoothstep(0.0, 1.0, dist * _CoreSharpness);

                // Combined brightness
                float brightness = (edgeFade * verticalFade) + core * 0.5;
                brightness *= _Intensity;

                float4 col = _Color * brightness;
                col.a = brightness;       // let bloom handle most of it

                return col;
            }
            ENDCG
        }
    }
}

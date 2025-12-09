Shader "Custom/StylizedLit"
{
    Properties
    {
        _Color ("Base Color", Color) = (1, 1, 1, 1)
        _RampTex ("Light Ramp", 2D) = "white" {}
        _AmbientStrength ("Ambient Strength", Range(0, 2)) = 0.5
        _MaxBrightness ("Max Brightness", Range(0.1, 2.0)) = 0.9

        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
        _RimStrength ("Rim Strength", Range(0, 3)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf ToonRamp fullforwardshadows
        #include "UnityCG.cginc"

        sampler2D _RampTex;
        fixed4 _Color;
        half _AmbientStrength;
        half _MaxBrightness;

        fixed4 _RimColor;
        half _RimPower;
        half _RimStrength;

        struct Input {
            float3 viewDir;
        };

        // Custom lighting: toon ramp + ambient, clamped brightness
        inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
        {
            half NdotL = saturate(dot(s.Normal, lightDir));      // 0..1
            half rampSample = tex2D(_RampTex, float2(NdotL, 0.5)).r;

            half3 diffuse = s.Albedo * _LightColor0.rgb * rampSample * atten;
            half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * _AmbientStrength * s.Albedo;

            half3 col = diffuse + ambient;

            // Clamp so it never blows out
            col = min(col, _MaxBrightness);

            half4 c;
            c.rgb = col;
            c.a = s.Alpha;
            return c;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
            o.Alpha  = _Color.a;

            // Rim light for that cartoony edge highlight
            float3 n = normalize(o.Normal);
            float3 v = normalize(IN.viewDir);
            float rim = 1.0 - saturate(dot(v, n));
            rim = pow(rim, _RimPower);
            o.Emission = _RimColor.rgb * rim * _RimStrength;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

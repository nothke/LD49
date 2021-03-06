Shader "Custom/ClipBelowWaterRGB"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RGBCustom" = "TRUE"}
        LOD 200
        Cull Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        #include "Waves.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            bool isFacing : SV_IsFrontFace;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed3 _ColorR;
        fixed3 _ColorG;
        fixed3 _ColorB;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {

            if (IN.worldPos.y < WaterHeight(IN.worldPos) )
                discard;

            fixed3 inputColors = _ColorR + _ColorG + _ColorR;

            fixed4 c;
            if (inputColors.r + inputColors.g + inputColors.b <= 0)
            {
                c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c;
            }
            else {

                if (IN.isFacing)
                    c = tex2D(_MainTex, IN.uv_MainTex);
                else
                {
                    fixed2 uv = IN.uv_MainTex;
                    uv.x = 1 - uv.x;
                    c = tex2D(_MainTex, uv);
                }

                float total = c.r + c.g + c.b;

                fixed3 colorMix = _ColorR * c.r / total + _ColorG * c.g / total + _ColorB * c.b / total;
                colorMix = lerp(fixed3(0, 0, 0), colorMix, min(1, total));
                o.Albedo = colorMix;
            }

            //if (IN.isFacing)
                //o.Albedo = float4(0, 1, 0, 1);

            o.Normal *= IN.isFacing ? 1 : -1;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
}

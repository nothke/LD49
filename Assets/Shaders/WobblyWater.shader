Shader "Custom/WobblyWater"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}


		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#include "Waves.cginc"
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows vertex:vert

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;

			struct Input
			{
				float2 uv_MainTex;
				float3 worldPos;
			};

			void vert(inout appdata_full v) {
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

				// Add custom water formula here:
				worldPos.y = WaterHeight(worldPos.xyz);

				float d = 0.2;
				float h0 = WaterHeight(worldPos.xyz + float3(-d, 0, 0));
				float h1 = WaterHeight(worldPos.xyz + float3(+d, 0, 0));
				float h2 = WaterHeight(worldPos.xyz + float3(0, 0, -d));
				float h3 = WaterHeight(worldPos.xyz + float3(0, 0, +d));

				const float normalIntensity = 0.5;
				v.normal.x = (h1 - h0) * normalIntensity;
				v.normal.z = (h3 - h2) * normalIntensity;

				//v.normal.xyz += cos(_Time.y + worldPos.x * 0.2) * 0.4; // derivative?

				v.vertex.xyz = mul(unity_WorldToObject, worldPos);
			}

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D(_MainTex, IN.worldPos.xz * 0.05) + _Color;
				o.Albedo = c.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}

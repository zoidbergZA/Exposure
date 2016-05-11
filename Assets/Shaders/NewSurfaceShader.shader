Shader "Custom/NewSurfaceShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ScanTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_ScanFactor ("Scan Factor", Range(0.1,1.0)) = 0.5
		_Radius ("Radius", Range(0.1,1.0)) = 0.5
		_CenterCoords ("Center Coords", Vector) = (0,0,0,0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _ScanTex;
		half _Glossiness;
		half _Metallic;
		fixed _ScanFactor;
		fixed4 _Color;
		fixed _Radius;
		fixed4 _CenterCoords;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c;
			c = ((tex2D (_MainTex, IN.uv_MainTex) * _ScanFactor) + (tex2D(_ScanTex, IN.uv_MainTex) * (1-_ScanFactor))) * _Color;

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

Shader "Custom/NewSurfaceShader"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ScanTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_ScanFactor ("Scan Factor", Range(0.1,1.0)) = 1.0
		_Radius ("Radius", Float) = 2.0
		_Duration ("Duration", Float) = 3.0
		_DurationBack ("Duration Back", Float) = 3.0
		_setDuration ("Set Duration", Float) = 3.0
		_CenterCoords ("Center Coords", Vector) = (0,0,0,0)
	}

	SubShader
	{
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
		fixed _Duration;
		fixed _DurationBack;
		fixed _setDuration;
		fixed4 _CenterCoords;

		struct Input {
			float2 uv_MainTex;
			float2 uv_ScanTex;
			float3 worldPos;
		};

		fixed getRadius(fixed backScan)
		{ 
			if(backScan == 0)	return _Radius / _Duration;
			else				return _Radius * _DurationBack;
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			fixed dist = distance(IN.worldPos, _CenterCoords);
			fixed scan;

			if(dist > 0)
			{
				if(_Duration > 0) scan = dist / getRadius(0);
				if(_Duration <= 0 && _DurationBack > 0) scan = dist / getRadius(1);
			}

			if(scan <= 1.0) _ScanFactor = 1-scan; //should be scan or 1 - scan
			else _ScanFactor = 1;

			fixed4 c = ((tex2D (_MainTex, IN.uv_MainTex) * _ScanFactor) + (tex2D(_ScanTex, IN.uv_ScanTex) * (1-_ScanFactor))) * _Color;
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

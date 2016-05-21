Shader "Custom/ScanShader"
{
	Properties
	{
		[MaterialToggle] 
		_DebugMode("_DebugMode", Float) = 0
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ScanTex ("Albedo (RGB)", 2D) = "white" {}
		
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		
		_Radius ("Radius", Float) = 2.0
		_CenterPoint ("Center Point", Vector) = (0,0,0,0)
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
		fixed4 _Color;
		fixed _Radius;
		fixed _DebugMode;
		fixed4 _CenterPoint;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_ScanTex;
			float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			fixed dist = distance(IN.worldPos, _CenterPoint);
			// fixed scan;

			// if(dist > 0)
			// {
			// 	if(_Duration > 0) scan = dist / getRadius(0);
			// 	if(_Duration <= 0 && _DurationBack > 0) scan = dist / getRadius(1);
			// }

			// if(scan <= 1.0) _ScanFactor = 1-scan; //should be scan or 1 - scan
			// else _ScanFactor = 1;

			fixed4 mainSample = (tex2D (_MainTex, IN.uv_MainTex)) * _Color;
			fixed4 scanSample = (tex2D (_ScanTex, IN.uv_ScanTex));

			if (_DebugMode)
			{
				o.Albedo = scanSample.rgb;
			}
			else
			{
				if (dist < _Radius)
				{
					o.Albedo = scanSample.rgb;
				}
				else
				{
					o.Albedo = mainSample.rgb;
				}				
			}
			// fixed4 c = ((tex2D (_MainTex, IN.uv_MainTex) * _ScanFactor) + (tex2D(_ScanTex, IN.uv_ScanTex) * (1-_ScanFactor))) * _Color;
			// o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = mainSample.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

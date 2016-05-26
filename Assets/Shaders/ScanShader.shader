Shader "Custom/ScanShader"
{
	Properties
	{
		[MaterialToggle] 
		_DebugMode("_DebugMode", Float) = 0
		_Color ("Color", Color) = (1,1,1,1)
		_EmissionMap ("Emission (RGB)", 2D) = "white" {}
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ScanTex ("Build Map (RGB)", 2D) = "white" {}
		_GroundTex ("Ground Map (RGB)", 2D) = "white" {}

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		
		_Radius ("Radius", Float) = 2.0
		_Fade ("Fade", Float) = 10.0
		_CenterPoint ("Center Point", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		// #pragma vertex vert
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		#include "UnityCG.cginc"

		sampler2D _MainTex;		
		sampler2D _ScanTex;
		sampler2D _GroundTex;

		sampler2D _EmissionMap;
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed _Radius;
		fixed _Fade;
		fixed _DebugMode;
		fixed4 _CenterPoint;

		// // vertex shader inputs
  //       struct appdata
  //       {
  //           float4 vertex : POSITION; // vertex position
  //           float2 uv : TEXCOORD0; // texture coordinate
  //       };

  //       // vertex shader outputs ("vertex to fragment")
  //       struct v2f
  //       {
  //           float2 uv : TEXCOORD0; // texture coordinate
  //           float4 vertex : SV_POSITION; // clip space position
  //       };

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_ScanTex;
			float2 uv_GroundTex;
			float2 uv_EmissionMap;
			float3 worldPos;
		};

		// // Vertex modifier
	 //    void vert (inout appdata_full v) 
	 //    {
	 //    	fixed d = distance(v.vertex, _CenterPoint);

	 //        if (d < 1)
	 //        {
	 //        	v.vertex.z = 0;
	 //        }
	 //        else
	 //        {
	 //        	// v.vertex.z -= 2;
	 //        }
	 //    }

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
			fixed4 groundSample = (tex2D (_GroundTex, IN.uv_GroundTex));
			fixed4 emmisionSample = (tex2D (_EmissionMap, IN.uv_EmissionMap));

			// if (scanSample.r >= 0.9)
			// {
			// 	scanSample.g = 0;
			// }

			if (_DebugMode)
			{
				o.Albedo = scanSample.rgb;
			}
			else
			{
				if (dist < _Radius)
				{
					if (dist > _Radius - _Fade)
					{
						fixed frac = (_Radius - dist) / _Fade;
						o.Albedo = frac * groundSample.rgb + (1 - frac) * mainSample.rgb;
						o.Metallic = (1 - frac) * _Metallic;
						o.Smoothness = (1 - frac) * _Glossiness;
						o.Alpha = 1;
					}
					else
					{
						o.Albedo = groundSample.rgb;
						o.Metallic = 0;
						o.Smoothness = 0;
						o.Alpha = 1;
					}					
				}
				else
				{
					o.Albedo = mainSample.rgb;
					o.Metallic = _Metallic;
					o.Smoothness = _Glossiness;
					o.Alpha = mainSample.a;
					// o.Emission = emmisionSample.rgb * 0.2;
				}				
			}
			// fixed4 c = ((tex2D (_MainTex, IN.uv_MainTex) * _ScanFactor) + (tex2D(_ScanTex, IN.uv_ScanTex) * (1-_ScanFactor))) * _Color;
			// o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			//
		}
		ENDCG
	}
	FallBack "Diffuse"
}

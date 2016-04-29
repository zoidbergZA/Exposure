Shader "Custom/ScanShader" 
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ScanTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_SpecColor ("Specular Color", Color) = (1,1,1,1)
		_Shininess ("Shininess", Float) = 10

	}

	SubShader 
	{
		Pass
		{
			Tags {"LightMode" = "ForwardBase"}
			CGPROGRAM

			#pragma vertex vert Standard fullforwardshadows // Physically based Standard lighting model, and enable shadows on all light types
			#pragma fragment frag Standard fullforwardshadows
			#pragma target 3.0 // Use shader model 3.0 target, to get nicer looking lighting

			uniform sampler2D _MainTex;
			uniform sampler2D _ScanTex;
			uniform float4 _MainTex_ST;
			uniform float4 _SpecColor;
			uniform half _Glossiness;
			uniform half _Metallic;
			uniform fixed4 _Color;
			uniform float _Shininess;


			struct vertexInput
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float2 tex : TEXCOORD0;
				float3 normalDir : TEXCOORD2;
				float4 posWorld : TEXCOORD1;
			};

			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;

				o.posWorld = mul(_Object2World, v.vertex);
				o.normalDir = normalize(mul(float4(o.normal, 0), _World2Object).xyz);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.tex = v.texcoord;

				return o;
			}

			float4 frag(vertexOutput i) : COLOR
			{
				// Albedo comes from a texture tinted by color
				//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
				//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * tex2D (_ScanTex, IN.uv_MainTex);
				//o.Albedo = c.rgb;
				// Metallic and smoothness come from slider variables
				//o.Metallic = _Metallic;
				//o.Smoothness = _Glossiness;
				//o.Alpha = c.a;

				return tex2D (_MainTex, i.tex) * tex2D (_ScanTex, i.tex);
			}

			ENDCG
		}
	}
	//fallback commented out during development
	//FallBack "Diffuse"
}

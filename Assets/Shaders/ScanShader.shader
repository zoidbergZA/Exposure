Shader "Custom/ScanShader" 
{
	Properties
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ScanTex ("Albedo (RGB)", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (1,1,1,1)
		_RimColor ("Rim Color", Color) = (1,1,1,1)
		_RimPower ("Rim Power", Range(0.1, 10.0)) = 3.0

		_Shininess ("Shininess", Float) = 10
		//_Glossiness ("Smoothness", Range(0,1)) = 0.5
		//_Metallic ("Metallic", Range(0,1)) = 0.0
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
			uniform fixed4 _Color;
			uniform float4 _SpecColor;
			uniform float4 _RimColor;
			uniform float _RimPower;
			uniform float _Shininess;

			//uniform half _Glossiness;
			//uniform half _Metallic;

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
				o.normalDir = normalize(mul(float4(o.normalDir, 0), _World2Object).xyz);
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

				float3 normalDirection = i.normalDir;
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float3 lightDirection;
				float atten;

				if(_WorldSpaceLightPos0.w == 0.0) //directional light
				{
					atten = 1.0;
					lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				} else
				{
					float3 fragmantToLightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
					float distance = length(fragmantToLightSource);
					atten = 1.0/distance;
					lightDirection = normalize(fragmantToLightSource);
				}

				//lighting
				float3 diffuseReflection = atten * _LightColor0.xyz * saturate( dot( normalDirection, lightDirection ) );
				float3 specularReflection = diffuseReflection * _SpecColor.xyz * pow( saturate( dot( reflect( -lightDirection, normalDirection ), viewDirection ) ), _Shinimess );

				//rim lighting
				float rim = 1 - saturate( dot( viewDirection, normalDirection ) );
				float3 rimLighting = saturate( dot( normalDirection, lightDirection ) * _RimColor.xyz * _LightColor0.xyz * pow( rim, _RimPower ));

				float3 lightFinal = UNITY_LIGHTMODEL_AMBIENT.xyz + diffuseReflection + specularReflection + rimLighting;

				float4 tex = tex2D ( _MainTex, i.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw );

				return float4( tex.xyz * _Color * lightFinal.xyz, 1.0 );
			}

			ENDCG
		}
	}
	//fallback commented out during development
	//FallBack "Diffuse"
}

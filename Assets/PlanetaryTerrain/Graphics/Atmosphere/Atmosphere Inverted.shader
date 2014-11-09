Shader "PlanetaryTerrain/Atmosphere"
{
	Properties 
	{
		_MainColor("_MainColor", Color) = (1,1,1,1)
		_SecondaryColor("_SecondaryColor", Color) = (1,1,1,1)
		_EdgeDensity("_EdgeDensity", Range(0.1,3) ) = 1.725506
		_Ramp("_Ramp", Range(0.1,3) ) = 1
		_MainTex("_MainTex", 2D) = "white" {}
	}
	
	SubShader 
	{
		Tags
		{
			"Queue"="Transparent+100"
			"IgnoreProjector"="False"
			"RenderType"="Transparent"
		}

		Cull Back
		ZWrite On
		ZTest LEqual
		ColorMask RGBA
		Blend SrcAlpha OneMinusSrcAlpha
		Fog{ Mode Off }


		CGPROGRAM
		#pragma surface surf BlinnPhongEditor  vertex:vert
		#pragma target 2.0

		float4 _MainColor;
		float4 _SecondaryColor;
		float _EdgeDensity;
		float _Ramp;
		sampler2D _MainTex;

		struct EditorSurfaceOutput {
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half3 Gloss;
			half Specular;
			half Alpha;
			half4 Custom;
		};
			
		inline half4 LightingBlinnPhongEditor_PrePass (EditorSurfaceOutput s, half4 light)
		{
			float4 SmoothStep0= _MainColor * Luminance( light.xyz ) * _Ramp;
			float4 Multiply0=SmoothStep0 * float4( s.Albedo.x, s.Albedo.y, s.Albedo.z, s.Alpha );
			return Multiply0;
		}

		inline half4 LightingBlinnPhongEditor (EditorSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			half3 h = normalize (lightDir + viewDir);
			
			half diff = max (0, dot ( lightDir, s.Normal ));
			
			half nh = max (0, dot (s.Normal, h));
			half spec = pow (nh, s.Specular*128.0);
			
			half4 res;
			res.rgb = _LightColor0.rgb * diff;
			res.a = spec * Luminance (_LightColor0.rgb);
			res *= atten * 2.0;
			res = half4(1,1,1,1) - res;

			return LightingBlinnPhongEditor_PrePass( s, res );
		}
		
		struct Input {
			float2 uv_MainTex;
			float3 viewDir;
		};

		void vert (inout appdata_full v, out Input o) {
		}
		

		void surf (Input IN, inout EditorSurfaceOutput o) {
			o.Normal = float3(0.0,0.0,1.0);
			o.Alpha = 1.0;
			o.Albedo = 0.0;
			o.Emission = 0.0;
			o.Gloss = 0.0;
			o.Specular = 0.0;
			o.Custom = 0.0;
			
			float4 Tex2D0=tex2D(_MainTex,(IN.uv_MainTex.xyxy).xy);
			float4 Fresnel0_1_NoInput = float4(0,0,1,1);
			float4 Fresnel0=(1.0 - dot(normalize( float4( IN.viewDir.x, IN.viewDir.y,IN.viewDir.z,1.0 ).xyz), normalize( Fresnel0_1_NoInput.xyz ) )).xxxx;
			float4 Invert0= float4(1.0, 1.0, 1.0, 1.0) - Fresnel0;
			float4 Pow0=pow(Invert0,_EdgeDensity.xxxx);
			float4 Master0_1_NoInput = float4(0,0,1,1);
			float4 Master0_2_NoInput = float4(0,0,0,0);
			float4 Master0_3_NoInput = float4(0,0,0,0);
			float4 Master0_4_NoInput = float4(0,0,0,0);
			float4 Master0_7_NoInput = float4(0,0,0,0);
			float4 Master0_6_NoInput = float4(1,1,1,1);
			o.Albedo = Tex2D0;
			o.Alpha = Pow0;

			o.Normal = normalize(o.Normal);
		}
		ENDCG
	}
	Fallback "Diffuse"
}
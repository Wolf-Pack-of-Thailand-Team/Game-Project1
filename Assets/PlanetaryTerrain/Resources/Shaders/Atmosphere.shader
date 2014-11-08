Shader "Atmosphere"
{
	Properties 
	{
_DiffuseColor("_DiffuseColor", Color) = (1,1,1,1)
_RimColor("_RimColor", Color) = (0,0.1188812,1,1)
_RimPower("_RimPower", Range(0.1,3) ) = 1.707772

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Transparent"
"IgnoreProjector"="False"
"RenderType"="Transparent"

		}

		
Cull Back
ZWrite On
ZTest LEqual
ColorMask RGBA
Blend SrcAlpha OneMinusSrcAlpha
Fog{
}


		CGPROGRAM
#pragma surface surf BlinnPhongEditor  vertex:vert
#pragma target 2.0


float4 _DiffuseColor;
float4 _RimColor;
float _RimPower;

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
float4 Invert0= float4(1.0, 1.0, 1.0, 1.0) - float4( s.Albedo.x, s.Albedo.y, s.Albedo.z, 1.0 );
float4 Multiply0=Invert0 * light;
float4 Invert2= float4(1.0, 1.0, 1.0, 1.0) - float4( s.Gloss.x, s.Gloss.y, s.Gloss.z, 1.0 );
float4 Splat0=light.w;
float4 Multiply2=Invert2 * Splat0;
float4 Multiply1=light * Multiply2;
float4 Add0=Multiply0 + Multiply1;
float4 Mask0=float4(Add0.x,Add0.y,Add0.z,0.0);
float4 Luminance0= Luminance( Multiply2.xyz ).xxxx;
float4 Add1=Luminance0 + Invert0;
float4 Mask1=float4(0.0,0.0,0.0,Add1.w);
float4 Add2=Mask0 + Mask1;
return Add2;

			}

			inline half4 LightingBlinnPhongEditor (EditorSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
			{
				half3 h = normalize (lightDir + viewDir);
				
				half diff = max (0, dot ( lightDir, s.Normal ));
				
				float nh = max (0, dot (s.Normal, h));
				float spec = pow (nh, s.Specular*128.0);
				
				half4 res;
				res.rgb = _LightColor0.rgb * diff;
				res.w = spec * Luminance (_LightColor0.rgb);
				res *= atten * 2.0;

				return LightingBlinnPhongEditor_PrePass( s, res );
			}
			
			struct Input {
				float4 color : COLOR;

			};

			void vert (inout appdata_full v, out Input o) {
float4 VertexOutputMaster0_0_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_1_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_2_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_3_NoInput = float4(0,0,0,0);


			}
			

			void surf (Input IN, inout EditorSurfaceOutput o) {
				o.Normal = float3(0.0,0.0,1.0);
				o.Alpha = 1.0;
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Gloss = 0.0;
				o.Specular = 0.0;
				o.Custom = 0.0;
				
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_2_NoInput = float4(0,0,0,0);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_5_NoInput = float4(1,1,1,1);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);
o.Albedo = _DiffuseColor;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Diffuse"
}
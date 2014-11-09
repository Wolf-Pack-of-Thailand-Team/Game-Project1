Shader "ColoredPlanetUnpacked"
{
	Properties 
	{
_MainTex("_MainTex", 2D) = "gray" {}
_Normal("_Normal", 2D) = "bump" {}
_NormalPower("_NormalPower", Range(1,-1) ) = 0
_Color1("_Color1", Color) = (1,1,1,1)
_Color2("_Color2", Color) = (1,1,1,1)
_Color3("_Color3", Color) = (1,1,1,1)
_Color4("_Color4", Color) = (1,1,1,1)
_Value1("_Value1", Range(0,1) ) = 0.33
_Value2("_Value2", Range(0,1) ) = 0.66
_PolarColor("_PolarColor", Color) = (1,1,1,1)
_Polarity("_Polarity", Float) = 0

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Geometry"
"IgnoreProjector"="False"
"RenderType"="Opaque"

		}

		
Cull Back
ZWrite On
ZTest LEqual
ColorMask RGBA
Fog{
}


		CGPROGRAM
#pragma surface surf BlinnPhongEditor  vertex:vert
#pragma target 2.0


sampler2D _MainTex;
sampler2D _Normal;
float _NormalPower;
float4 _Color1;
float4 _Color2;
float4 _Color3;
float4 _Color4;
float _Value1;
float _Value2;
float4 _PolarColor;
float _Polarity;

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
half3 spec = light.a * s.Gloss;
half4 c;
c.rgb = (s.Albedo * light.rgb + light.rgb * spec);
c.a = s.Alpha;
return c;

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
float2 uv_Normal;

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
				
float4 Split1=IN.color;
float4 Divide0=float4( Split1.x, Split1.x, Split1.x, Split1.x) / _Value1.xxxx;
float4 Saturate0=saturate(Divide0);
float4 Lerp0=lerp(_Color1,_Color2,Saturate0);
float4 Subtract0=float4( Split1.x, Split1.x, Split1.x, Split1.x) - _Value1.xxxx;
float4 Subtract1=_Value2.xxxx - _Value1.xxxx;
float4 Divide1=Subtract0 / Subtract1;
float4 Saturate1=saturate(Divide1);
float4 Lerp2=lerp(Lerp0,_Color3,Saturate1);
float4 Subtract2=float4( Split1.x, Split1.x, Split1.x, Split1.x) - _Value2.xxxx;
float4 Subtract3=float4( 1.0, 1.0, 1.0, 1.0 ) - _Value2.xxxx;
float4 Divide2=Subtract2 / Subtract3;
float4 Saturate2=saturate(Divide2);
float4 Lerp1=lerp(Lerp2,_Color4,Saturate2);
float4 Multiply1=float4( Split1.y, Split1.y, Split1.y, Split1.y) * _Polarity.xxxx;
float4 Multiply0=Multiply1 * _PolarColor;
float4 Add1=Lerp1 + Multiply0;
float4 Tex2D0=tex2D(_Normal,(IN.uv_Normal.xyxy).xy);
float4 Add0=Tex2D0 + _NormalPower.xxxx;
float4 Normalize0=normalize(Add0);
float4 Master0_2_NoInput = float4(0,0,0,0);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_5_NoInput = float4(1,1,1,1);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);
o.Albedo = Add1;
o.Normal = Normalize0;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "BumpedSpecular"
}
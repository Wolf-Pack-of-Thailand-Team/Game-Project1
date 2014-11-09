Shader "PlanetaryTerrain/BumpedPlanetColorMap"
{
	Properties 
	{
_ColorMap("_ColorMap", 2D) = "black" {}
_ColorMapBlend("_ColorMapBlend", Range(0,1) ) = 0.5
_ColorMapDistance("_ColorMapDistance", Float) = 10
_CameraPos("_CameraPos", Vector) = (0,0,0,0)

_SlopeTexture("_SlopeTexture", 2D) = "white" {}
_SlopePower("_SlopePower", Float ) = 1

_Color1("_Color1", Color) = (1,1,1,1)
_Color2("_Color2", Color) = (1,1,1,1)
_Color3("_Color3", Color) = (1,1,1,1)
_Color4("_Color4", Color) = (1,1,1,1)
_Value1("_Value1", Range(0,1) ) = 0.33
_Value2("_Value2", Range(0,1) ) = 0.66
_Polarity("_Polarity", Float) = 0
_PolarColor("_PolarColor", Color) = (1,1,1,1)
_Texture1("_Texture1", 2D) = "white" {}
_Texture2("_Texture2", 2D) = "white" {}
_Texture3("_Texture3", 2D) = "white" {}
_Texture4("_Texture4", 2D) = "white" {}
_PolarStrenght("_PolarStrenght", Float) = 0
_Brightness("_Brightness", Float) = 1
_Equator("_Equator", Vector) = (0,0,0,0)
_EquatorColor("_EquatorColor", Color) = (1,1,1,1)
_Normal1("_Normal1", 2D) = "black" {}
_Normal2("_Normal2", 2D) = "black" {}
_Normal3("_Normal3", 2D) = "black" {}
_Normal4("_Normal4", 2D) = "black" {}

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
#pragma surface surf BlinnPhongEditor  fullforwardshadows vertex:vert

#pragma target 3.0

sampler2D _ColorMap;
float _ColorMapBlend;
float _ColorMapDistance;
float4 _CameraPos;

sampler2D _SlopeTexture;
float _SlopePower;

float4 _Color1;
float4 _Color2;
float4 _Color3;
float4 _Color4;
float _Value1;
float _Value2;
float _Polarity;
float4 _PolarColor;
sampler2D _Texture1;
sampler2D _Texture2;
sampler2D _Texture3;
sampler2D _Texture4;
float _PolarStrenght;
float _Brightness;
float4 _Equator;
float4 _EquatorColor;
sampler2D _Normal1;
sampler2D _Normal2;
sampler2D _Normal3;
sampler2D _Normal4;

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
				float2 uv_Texture1;
float2 uv_Texture2;
float4 color : COLOR;
float2 uv_Texture3;
float2 uv_Texture4;
float2 uv_SlopeTexture;
float2 uv2_ColorMap;
float3 worldPos;

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
				
float4 Tex2D0=tex2D(_Texture1,IN.uv_Texture1.xy);
float4 Multiply2=Tex2D0 * _Color1;
float4 Tex2D1=tex2D(_Texture2,IN.uv_Texture2.xy);
float4 Multiply3=Tex2D1 * _Color2;
float4 Split0=IN.color;
float4 Divide0=float4( Split0.x, Split0.x, Split0.x, Split0.x) / _Value1.xxxx;
float4 Saturate0=saturate(Divide0);
float4 Lerp0=lerp(Multiply2,Multiply3,Saturate0);
float4 Tex2D2=tex2D(_Texture3,IN.uv_Texture3.xy);
float4 Multiply4=Tex2D2 * _Color3;
float4 Subtract0=float4( Split0.x, Split0.x, Split0.x, Split0.x) - _Value1.xxxx;
float4 Subtract1=_Value2.xxxx - _Value1.xxxx;
float4 Divide1=Subtract0 / Subtract1;
float4 Saturate1=saturate(Divide1);
float4 Lerp2=lerp(Lerp0,Multiply4,Saturate1);
float4 Tex2D3=tex2D(_Texture4,IN.uv_Texture4.xy);
float4 Multiply5=Tex2D3 * _Color4;
float4 Subtract2=float4( Split0.x, Split0.x, Split0.x, Split0.x) - _Value2.xxxx;
float4 Subtract3=float4( 1.0, 1.0, 1.0, 1.0 ) - _Value2.xxxx;
float4 Divide2=Subtract2 / Subtract3;
float4 Saturate2=saturate(Divide2);
float4 Lerp1=lerp(Lerp2,Multiply5,Saturate2);
float4 Multiply10=_Brightness.xxxx * Lerp1;
float4 Subtract4=float4( Split0.y, Split0.y, Split0.y, Split0.y) - _Polarity.xxxx;
float4 Max0=max(Subtract4,float4( 0.0, 0.0, 0.0, 0.0 ));
float4 Multiply1=_PolarColor * _PolarStrenght.xxxx;
float4 Multiply0=Max0 * Multiply1;
float4 Split2=_Equator;
float4 Subtract5=float4( Split2.x, Split2.x, Split2.x, Split2.x) - float4( Split0.y, Split0.y, Split0.y, Split0.y);
float4 Max1=max(Subtract5,float4( 0.0, 0.0, 0.0, 0.0 ));
float4 Multiply12=_EquatorColor * float4( Split2.y, Split2.y, Split2.y, Split2.y);
float4 Multiply11=Max1 * Multiply12;
float4 Subtract6=float4( Split0.x, Split0.x, Split0.x, Split0.x) - float4( Split2.z, Split2.z, Split2.z, Split2.z);
float4 Max2=max(Subtract6,float4( 0.0, 0.0, 0.0, 0.0 ));
float4 Multiply13=Multiply11 * Max2;
float4 Add1=Multiply0 + Multiply13;
float4 Add0=Multiply10 + Add1;
float4 Tex2D4=tex2D(_Normal1,IN.uv_Texture1.xy);
float4 Tex2D5=tex2D(_Normal2,IN.uv_Texture2.xy);
float4 Lerp3=lerp(Tex2D4,Tex2D5,Saturate0);
float4 Tex2D6=tex2D(_Normal3,IN.uv_Texture3.xy);
float4 Lerp4=lerp(Lerp3,Tex2D6,Saturate1);
float4 Tex2D7=tex2D(_Normal4,IN.uv_Texture4.xy);
float4 Lerp5=lerp(Lerp4,Tex2D7,Saturate2);
float4 UnpackNormal0=float4(UnpackNormal(Lerp5).xyz, 1.0);
float4 Master0_2_NoInput = float4(0,0,0,0);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_5_NoInput = float4(1,1,1,1);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);

// slope texture
float4 SlopeTex = tex2D(_SlopeTexture, IN.uv_SlopeTexture.xy);
float slope = IN.color.z;
float slopePower = slope * _SlopePower;
Add0 = lerp(Add0, SlopeTex, slopePower);

// colormap
float4 ColorMap = tex2D(_ColorMap, IN.uv2_ColorMap.xy);

float4 Distance0 = distance(_CameraPos, float4( IN.worldPos.x, IN.worldPos.y,IN.worldPos.z,1.0 ));
float distanceBlend = (saturate(length(Distance0) / _ColorMapDistance)) * (1.0 - _ColorMapBlend);
float colorMapStrength = _ColorMapBlend + distanceBlend;

o.Albedo = Add0 * (float4(1,1,1,1) * (1.0 - colorMapStrength) + ColorMap * colorMapStrength);
o.Normal = UnpackNormal0;
o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "BumpedSpecular"
}
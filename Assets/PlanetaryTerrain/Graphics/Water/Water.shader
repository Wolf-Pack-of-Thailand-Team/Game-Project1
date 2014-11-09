Shader "Water"
{
	Properties 
	{
_DiffuseColor("_DiffuseColor", Color) = (0.3330363,0.6378568,0.858209,1)
_RimColor("_RimColor", Color) = (0.171419,0.3054835,0.4029851,1)
_RimPower("_RimPower", Range(0.1,3) ) = 1.707772
_Glossiness("_Glossiness", Range(0.1,1) ) = 0.4300518
_SpecularColor("_SpecularColor", Color) = (0.4850746,0.4850746,0.4850746,1)
_CameraPos("_CameraPos", Vector) = (0,0,0,0)
_FogStart("_FogStart", Float) = 0
_FogEnd("_FogEnd", Float) = 100
_Depth("_Depth", Float) = 0
_MinAlpha("_MinAlpha", Float) = 0.5
_MainTex("_MainTex", 2D) = "white" {}
_WaveTime("_WaveTime", Float) = 0
_BumpMap("Normalmap", 2D) = "bump" {}

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Transparent+100"
"IgnoreProjector"="False"
"RenderType"="Transparent"

		}

		
Cull Off
ZWrite On
ZTest LEqual
ColorMask RGBA
Blend SrcAlpha OneMinusSrcAlpha
Fog{
}


		CGPROGRAM
#pragma surface surf BlinnPhong vertex:vert
#pragma target 3.0


float4 _DiffuseColor;
float4 _RimColor;
float _RimPower;
float _Glossiness;
float4 _SpecularColor;
float4 _CameraPos;
float _FogStart;
float _FogEnd;
float _Depth;
float _MinAlpha;
sampler2D _MainTex;
float _WaveTime;
sampler2D _BumpMap;
sampler2D _CameraDepthTexture;
			
			struct Input {
				float2 uv_MainTex;
				float2 uv_BumpMap;
				float4 screenPos;
				float3 worldPos;

			};

			void vert (inout appdata_full v, out Input o) {
				v.tangent = float4(0,0,0,0);
			}
			

			void surf (Input IN, inout SurfaceOutput o) {
				o.Normal = float3(0.0,0.0,1.0);
				o.Alpha = 1.0;
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Gloss = 0.0;
				o.Specular = 0.0;
				
float4 Multiply5=_WaveTime.xxxx * float4( 0.5,0.5,0.5,0.5 );
float4 Invert0= float4(1.0, 1.0, 1.0, 1.0) - Multiply5;
float4 UV_Pan1=float4((IN.uv_MainTex.xyxy).x + Invert0.x,(IN.uv_MainTex.xyxy).y + Invert0.x,(IN.uv_MainTex.xyxy).z,(IN.uv_MainTex.xyxy).w);
float4 Tex2D1=tex2D(_MainTex,UV_Pan1.xy);
float4 Add1=(IN.uv_MainTex.xyxy) + float4( 0.5,0.5,0.5,0.5 );
float4 Invert2= float4(1.0, 1.0, 1.0, 1.0) - Add1;
float4 UV_Pan0=float4(Invert2.x + Invert0.x,Invert2.y + Invert0.x,Invert2.z,Invert2.w);
float4 Tex2D0=tex2D(_MainTex,UV_Pan0.xy);
float4 Multiply1=Tex2D1 * Tex2D0;
float4 Multiply3=Multiply1 * float4( 2,2,2,2 );
float4 Multiply2=Multiply3 * _DiffuseColor;
float4 UV_Pan3=float4((IN.uv_BumpMap.xyxy).x,(IN.uv_BumpMap.xyxy).y + _WaveTime.xxxx.x,(IN.uv_BumpMap.xyxy).z,(IN.uv_BumpMap.xyxy).w);
float4 Tex2D3=tex2D(_BumpMap,UV_Pan3.xy);
float4 Add2=(IN.uv_BumpMap.xyxy) + float4( 0.5,0.5,0.5,0.5 );
float4 Invert1= float4(1.0, 1.0, 1.0, 1.0) - _WaveTime.xxxx;
float4 UV_Pan2=float4(Add2.x + Invert1.x,Add2.y,Add2.z,Add2.w);
float4 Tex2D2=tex2D(_BumpMap,UV_Pan2.xy);
float4 Multiply0=Tex2D3 * Tex2D2;
float4 Multiply4=Multiply0 * float4( 2,2,2,2 );
float4 UnpackNormal0=float4(UnpackNormal(Multiply4).xyz, 1.0);
float4 ScreenDepth0= LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD( IN.screenPos)).r);
float4 Divide0=ScreenDepth0 / _FogEnd.xxxx;
float4 Saturate0=saturate(Divide0);
float4 Distance0=distance(_CameraPos,float4( IN.worldPos.x, IN.worldPos.y,IN.worldPos.z,1.0 ));
float4 Subtract0=Distance0 - _FogStart.xxxx;
float4 Divide2=Subtract0 / _Depth.xxxx;
float4 Saturate1=saturate(Divide2);
float4 Max1=max(Saturate0,Saturate1);
float4 Max0=max(Max1,_MinAlpha.xxxx);
float4 Master0_2_NoInput = float4(0,0,0,0);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);
o.Albedo = Multiply2;
o.Normal = UnpackNormal0;
o.Specular = _Glossiness.xxxx;
o.Gloss = _SpecularColor;
o.Alpha = Max0;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Transparent"
}
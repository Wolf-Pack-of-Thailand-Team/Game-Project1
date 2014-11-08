Shader "Voxeland/Highlight" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Diffuse(RGB). Cutout(A)", 2D) = "gray" {}
		_Offset ("Offset", Float) = 0.25
	} 


Category 
{
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	//Blend One One
	
	AlphaTest Greater .01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }

	
	SubShader
	{
		
		Pass 
		{
			//BlendOp Sub
			Blend DstColor OneMinusSrcAlpha
			
			//surface shaders do not work... do not know why
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _Color;
			float _Offset;
			
			struct appdata_t 
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				v.vertex.xyz += normalize(ObjSpaceViewDir(v.vertex)) * _Offset;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				return 2.0f * _Color * tex2D(_MainTex, i.texcoord);
			}
			
			ENDCG 
		}
		
		

		Pass 
		{
			//BlendOp Sub
			Blend SrcAlpha One
			
			//surface shaders do not work... do not know why
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _Color;
			float _Offset;
			
			struct appdata_t 
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				v.vertex.xyz += normalize(ObjSpaceViewDir(v.vertex)) * _Offset;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				return 2.0f * _Color * tex2D(_MainTex, i.texcoord);
			}
			
			ENDCG 
		}
		
	} 	

}
}
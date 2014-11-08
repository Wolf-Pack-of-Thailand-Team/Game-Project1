Shader "Voxeland/Grass" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Diffuse(RGB). Cutout(A)", 2D) = "gray" {}
		_Cutoff ("Alpha Ref", Range(0,1)) = 0.33
		_Ambient ("Additional Ambient", Color) = (1,1,1,1)
//		_GrassAnimState ("Animation State", Range(0,6.283185307179586476925286766559)) = 0
		_AnimStrength ("Animation Strength", Range(0,1)) = 0.33
	} 


	SubShader 
	{
		Cull Off
		
		CGPROGRAM
		
		#pragma surface surf Lambert alphatest:_Cutoff vertex:vert
		//#include "Assets/Shaders/WShaders.cginc"
		
		struct Input 
		{
			float2 uv_MainTex;        
			float3 worldPos;
			float3 screenPos;
			float4 color : Color;
		};
		
		sampler2D _MainTex;
		half3 _Ambient;
		fixed4 _Color;
		float _GrassAnimState;
		float _AnimStrength;
		
		void vert (inout appdata_full v) 
		{
			//float shiftedState = v.color.g + _GrassAnimState;
			//if (shiftedState > 1) shiftedState = 2 - shiftedState;
			//if (shiftedState < 0) shiftedState = - shiftedState;
			//shiftedState -= 0.5;
			//v.vertex.xyz += float3(v.color.r, 0, v.color.b) * shiftedState;
			
			//v.vertex.y += v.color.g;
			
			float linearState = cos(_GrassAnimState + v.color.g*6.283185307179586476925286766559);
			v.vertex.xyz += float3(1-v.color.r*2, 0, 1-v.color.b*2) * linearState * _AnimStrength;
			
			//v.vertex.xyz += float3(0, v.color.g*10, 0);
		}
		
		void surf (Input IN, inout SurfaceOutput o) 
		{
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * _Color; 
			o.Alpha = c.a;

			o.Emission = o.Albedo * _Ambient * IN.color.a; 
		}
     
		ENDCG
    }
    
    Fallback "Transparent/Cutout/VertexLit"
}
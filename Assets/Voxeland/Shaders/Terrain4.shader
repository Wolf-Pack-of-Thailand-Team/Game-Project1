Shader "Voxeland/Terrain4"  
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Diffuse1(RGB)", 2D) = "gray" {}
		_MainTex2 ("Diffuse2(RGB)", 2D) = "gray" {}
		_MainTex3 ("Diffuse3(RGB)", 2D) = "gray" {}
		_MainTex4 ("Diffuse4(RGB)", 2D) = "gray" {}
		_Ambient ("Additional Ambient", Color) = (1,1,1,1)
	} 


	SubShader 
	{
		CGPROGRAM
		
		#pragma surface surf BlinnPhong

		struct Input 
		{
			float2 uv_MainTex;        
			float3 worldPos;
			float3 screenPos;
			float4 color : Color;
		};
		
		sampler2D _MainTex;
		sampler2D _MainTex2; 
		sampler2D _MainTex3;
		sampler2D _MainTex4;
		fixed4 _Color;
		half3 _Ambient;
		
		void surf (Input IN, inout SurfaceOutput o) 
		{
			//getting last color
			half4 color4 = 1-IN.color.r-IN.color.b-IN.color.g;
			
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex)*IN.color.r 
				+ tex2D(_MainTex2, IN.uv_MainTex)*IN.color.g 
				+ tex2D(_MainTex3, IN.uv_MainTex)*IN.color.b 
				+ tex2D(_MainTex4, IN.uv_MainTex)*color4;
			
			o.Albedo = tex.rgb * _Color.rgb;
			o.Alpha = tex.a * _Color.a;
			
			o.Emission = o.Albedo * IN.color.a * _Ambient;
		}
     
		ENDCG
    }
    
    Fallback "VertexLit"
}
Shader "Voxeland/TerrainAmbientOnly"  
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
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
		
		fixed4 _Color;
		half3 _Ambient;
		
		void surf (Input IN, inout SurfaceOutput o) 
		{
			//getting last color
			half4 color4 = 1-IN.color.r-IN.color.b-IN.color.g;
			
			o.Albedo = 0;//IN.color.a; //IN.color.rgb;
			//o.Alpha = IN.color.a;
			
			o.Emission = IN.color.a;
		}
     
		ENDCG
    }
    
    Fallback "VertexLit"
}
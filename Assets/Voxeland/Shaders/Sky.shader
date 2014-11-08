Shader "Voxeland/Demo/Sky" 
{
	Properties 
	{
		_MainTex ("Diffuse(RGB)", 2D) = "gray" {}
		_SkyColor ("Sky Color", Color) = (1,1,1,1)
		_CloudsColor ("Clouds Color", Color) = (1,1,1,1)
	} 

	SubShader 
	{
		Tags { "Queue"="Background" "RenderType"="Background" }
		Cull Off 
		ZWrite Off
		Fog { Mode off }
		
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert
		#pragma target 3.0

		struct Input 
		{
			float2 uv_MainTex;        
			float3 worldPos;
			float3 screenPos;
		};
		
		sampler2D _MainTex;
		float _Height;
		float _Size;
		float4 _SkyColor; 
		float4 _CloudsColor;
		
		void vert (inout appdata_full v)
		{
			float4 worldVertPos = float4(_WorldSpaceCameraPos, 0) + v.vertex;
			v.vertex = mul(_World2Object,  worldVertPos);
		}
		

		void surf (Input IN, inout SurfaceOutput o) 
		{
			float4 skyTex = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = (_SkyColor * (1-skyTex.a)) + (skyTex.rgb * skyTex.a * _CloudsColor);

			o.Emission = o.Albedo;

		}
     
		ENDCG
    }
    //Fallback "VertexLit"
}


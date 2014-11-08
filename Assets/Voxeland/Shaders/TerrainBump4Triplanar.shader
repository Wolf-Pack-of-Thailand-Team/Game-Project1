Shader "Voxeland/TerrainBump4Triplanar"  
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Diffuse1(RGB)", 2D) = "gray" {}
		_BumpMap ("Bump1(CA)", 2D) = "bump" {}
		_MainTex2 ("Diffuse2(RGB)", 2D) = "gray" {}
		_BumpMap2 ("Bump2(CA)", 2D) = "bump" {}
		_MainTex3 ("Diffuse3(RGB)", 2D) = "gray" {}
		_BumpMap3 ("Bump3(CA)", 2D) = "bump" {}
		_MainTex4 ("Diffuse4(RGB)", 2D) = "gray" {}
		_BumpMap4 ("Bump4(CA)", 2D) = "bump" {}
		_Ambient ("Additional Ambient", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_BakeAmbient ("Shininess", Range (0, 20)) = 0
		//_Tiling ("Offset", Float) = 0.25
		_Tile ("Tile", Float) = 0.25
	} 


	SubShader
	{
		CGPROGRAM
		
		#pragma surface surf BlinnPhong vertex:vert
		#pragma target 3.0

		struct Input 
		{
			float2 uv_MainTex;        
			float3 worldPos;
			float4 color : Color;
			float3 plane;
		};
		
		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _MainTex2;
		sampler2D _BumpMap2; 
		sampler2D _MainTex3;
		sampler2D _BumpMap3;
		sampler2D _MainTex4;
		sampler2D _BumpMap4;
		half3 _Ambient;
		fixed4 _Color;
		half _Shininess;
		half _BakeAmbient;
		half _Tile;
		
		void vert (inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.plane = (0,0,0);
			
			float absNormX = abs(v.normal.x);
			float absNormY = abs(v.normal.y);
			float absNormZ = abs(v.normal.z);
			
			if (v.normal.y > 0.65) { o.plane.y = 1; v.tangent = float4(0,0,1,-1); }
			else if (v.normal.y < -0.75) { o.plane.y = 1; v.tangent = float4(0,0,-1,1); }
			
			else if (v.normal.x > 0.65) { o.plane.x = 1; v.tangent = float4(0,1,0,-1); }
			else if (v.normal.x < -0.65) { o.plane.x = 1; v.tangent = float4(0,-1,0,1); }
			
			else if (v.normal.z > 0) { o.plane.z = 1; v.tangent = float4(0,-1,0,1); }
			else { o.plane.z = 1; v.tangent = float4(0,1,0,-1); }

			v.tangent.xyz = cross(v.normal, v.tangent.xyz);
		}
		
		fixed4 GetTriplanarColorOld (sampler2D tex, float3 worldPos, float3 plane)
		{
			float2 uv = float2(0,0);
			
			uv.x = worldPos.x*0.2;
			uv.y = worldPos.z*0.2;	
			fixed4 texy = tex2D(tex, uv);

			uv.x = worldPos.z*0.2;
			uv.y = worldPos.y*0.2;	
			fixed4 texx = tex2D(tex, uv);

			uv.x = worldPos.x*0.2;
			uv.y = worldPos.y*0.2;	
			fixed4 texz = tex2D(tex, uv);
			
			return texy*plane.y + texx*plane.x + texz*plane.z;
		}
		
		fixed4 GetColor (sampler2D tex1, sampler2D tex2, sampler2D tex3, sampler2D tex4, float2 uv, half4 type)
		{
			if (type.r > 0.99) return tex2D(tex1, uv)*type.r;
			else if (type.g > 0.99) return tex2D(tex1, uv)*type.g;
			else if (type.b > 0.99) return tex2D(tex1, uv)*type.b;
			else if (type.a > 0.99) return tex2D(tex1, uv)*type.a;
			
			else return tex2D(tex1, uv)*type.r 
				+ tex2D(tex2, uv)*type.g 
				+ tex2D(tex3, uv)*type.b 
				+ tex2D(tex4, uv)*type.a;
		}
		
		fixed4 GetTriplanarColor (sampler2D tex1, sampler2D tex2, sampler2D tex3, sampler2D tex4, 	float3 worldPos, half4 type, float3 plane)
		{
			float2 uv = float2(0,0);

			uv.x = worldPos.x*0.2;
			uv.y = worldPos.z*0.2;	
			fixed4 texy = fixed4(0,0,0,0);
			//if (plane.y > 0.99) 
			texy = tex2D(tex1, uv)*type.r 
				+ tex2D(tex2, uv)*type.g 
				+ tex2D(tex3, uv)*type.b 
				+ tex2D(tex4, uv)*type.a;

			uv.x = worldPos.z*0.2;
			uv.y = worldPos.y*0.2;	
			fixed4 texx = fixed4(0,0,0,0);
			//if (plane.x > 0.99) 
			texx = tex2D(tex1, uv)*type.r 
				+ tex2D(tex2, uv)*type.g 
				+ tex2D(tex3, uv)*type.b 
				+ tex2D(tex4, uv)*type.a;

			uv.x = worldPos.x*0.2;
			uv.y = worldPos.y*0.2;	
			fixed4 texz = fixed4(0,0,0,0);
			//if (plane.z > 0.99) 
			texz = tex2D(tex1, uv)*type.r 
				+ tex2D(tex2, uv)*type.g 
				+ tex2D(tex3, uv)*type.b 
				+ tex2D(tex4, uv)*type.a;
			
			return texy*plane.y + texx*plane.x + texz*plane.z; 
 
			//return fixed4(0,0,0,0);
		}
		
		void surf (Input IN, inout SurfaceOutput o) 
		{
			half4 type = half4(IN.color.r, IN.color.g, IN.color.b, 1-IN.color.r-IN.color.b-IN.color.g);
			
			fixed4 tex = GetTriplanarColor (_MainTex, _MainTex2, _MainTex3, _MainTex4, 	IN.worldPos*_Tile, type, IN.plane);
			fixed4 norm = GetTriplanarColor (_BumpMap, _BumpMap2, _BumpMap3, _BumpMap4, IN.worldPos*_Tile, type, IN.plane);
			
			//fixed4 tex = GetColor (_MainTex, _MainTex2, _MainTex3, _MainTex4, 	 IN.uv_MainTex, type);
			//fixed4 norm = GetColor (_BumpMap, _BumpMap2, _BumpMap3, _BumpMap4, 	 IN.uv_MainTex, type);
			
			o.Normal = UnpackNormal(norm);

			o.Albedo = tex.rgb * _Color.rgb;
			o.Gloss = tex.a;
			o.Alpha = tex.a * _Color.a;
			o.Specular = _Shininess;
			
			o.Emission = o.Albedo * IN.color.a * _Ambient;
			o.Albedo += o.Emission*_BakeAmbient;
		}
     
		ENDCG
    }
    
    Fallback "VertexLit"
}
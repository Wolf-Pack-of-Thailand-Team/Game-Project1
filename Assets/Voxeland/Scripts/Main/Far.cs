using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Voxeland
{
	
	[ExecuteInEditMode]
	public class Far : MonoBehaviour 
	{
		public VoxelandTerrain land;
		public MeshFilter filter;
		
		public int subdiv = 2;
		public int chunks = 10;
		public int centerX = 0;
		public int centerZ = 0;
		
		static public string[] shaderMainTexNames = new string[] {"_MainTex", "_MainTex2", "_MainTex3", "_MainTex4"};
		static public string[] shaderBumpTexNames = new string[] {"_BumpMap", "_BumpMap2", "_BumpMap3", "_BumpMap4"};
		
		public static Far Create (VoxelandTerrain land)
		{
			GameObject obj = new GameObject("Far");
			obj.transform.parent = land.transform;
			obj.transform.localPosition = Vector3.zero;
			
			Far far = obj.AddComponent<Far>();
			far.land = land;
			
			obj.AddComponent<MeshRenderer>();
			far.filter = obj.AddComponent<MeshFilter>();
			
			far.renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));
			if (land.hideChunks) far.transform.hideFlags = HideFlags.HideInHierarchy;
			
			//if (land == null) land = (Voxeland)FindObjectOfType(typeof(Voxeland));
			//if (filter == null) filter = GetComponent<MeshFilter>();
			//if (filter.sharedMesh == null) filter.sharedMesh = new Mesh();
			
			#region Assigning Material
			if (land.landShader!=null) obj.renderer.sharedMaterial = new Material(land.landShader);
			else obj.renderer.sharedMaterial = new Material(Shader.Find("VertexLit"));
			
			obj.renderer.sharedMaterial.SetColor("_Ambient", land.landAmbient);
			obj.renderer.sharedMaterial.SetColor("_SpecColor", land.landSpecular);
			obj.renderer.sharedMaterial.SetFloat("_Shininess", land.landShininess);
			obj.renderer.sharedMaterial.SetFloat("_BakeAmbient", land.landBakeAmbient);
			
			for (int t=1; t<Mathf.Min(land.types.Length,5); t++) 
			{
				if (!land.types[t].differentTop)
				{
					if (land.types[t].texture!=null) obj.renderer.sharedMaterial.SetTexture(shaderMainTexNames[t-1], land.types[t].texture);
					if (land.types[t].bumpTexture!=null) obj.renderer.sharedMaterial.SetTexture(shaderBumpTexNames[t-1], land.types[t].bumpTexture);
				}
				else
				{
					if (land.types[t].topTexture!=null) obj.renderer.sharedMaterial.SetTexture(shaderMainTexNames[t-1], land.types[t].topTexture);
					if (land.types[t].topBumpTexture!=null) obj.renderer.sharedMaterial.SetTexture(shaderBumpTexNames[t-1], land.types[t].topBumpTexture);				
				}
			}
			#endregion
			
			return far;
		}
		
		public void Build (int x, int z)
		{
			centerX = x; centerZ = z;
			Build();
		}
		
		public void Build () 
		{
			if (filter.sharedMesh == null) filter.sharedMesh = new Mesh();
			filter.sharedMesh.Clear();
			
			int chunkVisSize = land.chunkSize - land.overlap*2;
			float polySize = (1.0f + chunkVisSize) / subdiv;
			float halfSize = chunks*chunkVisSize/2;
			int i = 0;
			
			#region Get active chunks min and max
			if (land.profile) Profiler.BeginSample ("Get Min Max");
			
			int minX = 2147483000;
			int maxX = -2147483000;
			int minZ = 2147483000;
			int maxZ = -2147483000;
			
			for (i=0; i<land.activeChunks.Count; i++)
			{
				Chunk chunk = land.activeChunks[i];
				
				if (chunk.offsetX+land.overlap < minX) minX = chunk.offsetX+land.overlap;
				if (chunk.offsetZ+land.overlap < minZ) minZ = chunk.offsetZ+land.overlap;
				
				if (chunk.offsetX+land.overlap + chunkVisSize > maxX) maxX = chunk.offsetX+land.overlap + chunkVisSize;
				if (chunk.offsetZ+land.overlap + chunkVisSize > maxZ) maxZ = chunk.offsetZ+land.overlap + chunkVisSize;
			}
			
			if (land.profile) Profiler.EndSample();
			#endregion
	
			#region Build Verts
			Vector3[] verts = new Vector3[chunks*subdiv * chunks*subdiv];
			Vector2[] uvs = new Vector2[chunks*subdiv * chunks*subdiv];
			Color[] colors = new Color[chunks*subdiv * chunks*subdiv];
			
			i = 0;
			for (int x=0; x<subdiv*chunks; x++)
				for (int z=0; z<subdiv*chunks; z++)
			{
				//getting coords
				float coordX = x*polySize - halfSize + centerX;
				float coordZ = z*polySize - halfSize + centerZ;
				
				//get height
				int height = land.data.GetTopPoint((int)coordX, (int)coordZ, (int)coordX, (int)coordZ);

				//adding verts
				verts[i] = new Vector3(coordX, height, coordZ); 
				uvs[i] = new Vector2(0,0);
				
				//lowering borders
				//if (Contains(minX, minZ, maxX, maxZ, coordX, coordZ)) verts[i] = new Vector3(coordX, height-5, coordZ);
				
				//getting type
				byte type = land.data.GetTopType((int)coordX, (int)coordZ);
				if (type==4) colors[i] = new Color(0,0,0,1);
				else if (type==3) colors[i] = new Color(0,0,1,1);
				else if (type==2) colors[i] = new Color(0,1,0,1);
				else colors[i] = new Color(1,0,0,1);
				
				i++;
			}
			#endregion
			
			#region Calculate Normals
			Vector3[] normals = new Vector3[chunks*subdiv * chunks*subdiv];
			i = 0;
			for (int x=0; x<subdiv*chunks; x++)
				for (int z=0; z<subdiv*chunks; z++)
			{
				Vector3 tangent;
				if (x>0 && x<subdiv*chunks-1) tangent = (verts[i+subdiv*chunks]-verts[i]).normalized*0.5f + (verts[i]-verts[i-subdiv*chunks]).normalized*0.5f;
				else if (x>0) tangent = (verts[i] - verts[i-subdiv*chunks]).normalized;
				else tangent = (verts[i+subdiv*chunks] - verts[i]).normalized;
				
				Vector3 binormal;
				if (z>0 && z<subdiv*chunks-1) binormal = (verts[i+1]-verts[i]).normalized*0.5f + (verts[i]-verts[i-1]).normalized*0.5f; 
				else if (z>0) binormal = (verts[i] - verts[i-1]).normalized;
				else binormal = (verts[i+1] - verts[i]).normalized;
				
				normals[i] = Vector3.Cross(binormal, tangent);
				
				i++;
			}
			#endregion
			
			#region Lower borders
			for (int v=0; v<verts.Length; v++)
				if (Contains(minX, minZ, maxX, maxZ, verts[v].x, verts[v].z)) 
					verts[v] = new Vector3(verts[v].x, verts[v].y-5, verts[v].z);
			#endregion
			
			#region Get Number of faces
			if (land.profile) Profiler.BeginSample ("Face Num");
			
			int numFaces = 0;
			
			for (int x=1; x<subdiv*chunks; x++)
				for (int z=1; z<subdiv*chunks; z++)
					if (!Contains(minX, minZ, maxX, maxZ, 
							(x-1)*polySize - halfSize + centerX, 
							(z-1)*polySize - halfSize + centerZ, 
							x*polySize - halfSize + centerX, 
							z*polySize - halfSize + centerZ) &&
					 	(verts[x*subdiv*chunks + z].y >= 1 ||
					    	verts[(x-1)*subdiv*chunks + z].y >= 1 ||
					    	verts[x*subdiv*chunks + z-1].y >= 1 ||
					    	verts[(x-1)*subdiv*chunks + z-1].y >= 1))
								numFaces++;
			
			if (land.profile) Profiler.EndSample();
			#endregion
			
			#region Create Tris
			if (land.profile) Profiler.BeginSample ("Create Tris");
			
			i = 0;
			int[] tris = new int[numFaces*6];
			
			for (int x=1; x<subdiv*chunks; x++)
				for (int z=1; z<subdiv*chunks; z++)
			{
				int cur = x*subdiv*chunks + z;
				int prev = cur - subdiv*chunks;
				
				if (!Contains(minX, minZ, maxX, maxZ, 
				        (x-1)*polySize - halfSize + centerX, 
				        (z-1)*polySize - halfSize + centerZ, 
				        x*polySize - halfSize + centerX, 
				       	z*polySize - halfSize + centerZ) &&
				    (verts[x*subdiv*chunks + z].y >= 1 ||
						 verts[(x-1)*subdiv*chunks + z].y >= 1 ||
						 verts[x*subdiv*chunks + z-1].y >= 1 ||
						 verts[(x-1)*subdiv*chunks + z-1].y >= 1))
				{
					tris[i*6] = prev-1;
					tris[i*6+1] = prev;
					tris[i*6+2] = cur-1;
					tris[i*6+3] = prev; 
					tris[i*6+4] = cur;
					tris[i*6+5] = cur-1;
					i++;
				}
			}
			if (land.profile) Profiler.EndSample();
			#endregion
			
			filter.sharedMesh.vertices = verts;
			filter.sharedMesh.normals = normals;
			filter.sharedMesh.uv = uvs;
			filter.sharedMesh.colors = colors;
			filter.sharedMesh.triangles = tris;
			
			filter.sharedMesh.RecalculateBounds();
			//filter.sharedMesh.RecalculateNormals();
			
			//hiding wireframe
			#if UNITY_EDITOR
			if (land.hideWire) UnityEditor.EditorUtility.SetSelectedWireframeHidden(filter.renderer, true);
			#endif
			
			//stopwatch.Stop();
			//Debug.Log("Created In: " + (0.001f * stopwatch.ElapsedMilliseconds));
		}
		
		/*
		public void BuildVerts ()
		{
			int chunkVisSize = land.chunkSize - land.overlap*2;
	
			List<Vector3> verts = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();
			List<Color> colors = new List<Color>();
			zeroLevel.Clear();
	
			float polySize = (1.0f + chunkVisSize) / subdiv;
			float halfSize = chunks*chunkVisSize/2;
			
			for (int x=0; x<subdiv*chunks; x++)
				for (int z=0; z<subdiv*chunks; z++)
			{
				//getting coords
				float coordX = x*polySize - halfSize + centerX;
				float coordZ = z*polySize - halfSize + centerZ;
				
				//get height
				int height = land.data.GetTopPoint((int)coordX, (int)coordZ, (int)coordX, (int)coordZ);
				
				//setting exist bool
				if (height <= 0.1) zeroLevel.Add(true);
				else zeroLevel.Add(false);
				
				//adding verts
				verts.Add(new Vector3(coordX, height, coordZ)); 
				uvs.Add(new Vector2(0,0));
				
				//getting type
				byte type = land.data.GetTopType((int)coordX, (int)coordZ);
				if (type==4) colors.Add(new Color(0,0,0,1));
				else if (type==3) colors.Add(new Color(0,0,1,1));
				else if (type==2) colors.Add(new Color(0,1,0,1));
				else colors.Add(new Color(1,0,0,1));
			}
	
			if (filter.sharedMesh == null) filter.sharedMesh = new Mesh();
			filter.sharedMesh.Clear();
			filter.sharedMesh.vertices = verts.ToArray();
			filter.sharedMesh.uv = uvs.ToArray();
			filter.sharedMesh.colors = colors.ToArray();
		}
		
		
		public void BuildTris () 
		{
			int chunkVisSize = land.chunkSize - land.overlap*2;
			float polySize = (1.0f + chunkVisSize) / subdiv;
			float halfSize = chunks*chunkVisSize/2;
			
			#region Get Number of faces
			if (land.profile) Profiler.BeginSample ("Face Num");
			
			int numFaces = 0;
			
			for (int x=1; x<subdiv*chunks; x++)
				for (int z=1; z<subdiv*chunks; z++)
					if (!Contains(minX, minZ, maxX, maxZ, 
		              (x-1)*polySize - halfSize + centerX, 
		              (z-1)*polySize - halfSize + centerZ, 
		              x*polySize - halfSize + centerX, 
		              z*polySize - halfSize + centerZ))
					      numFaces++;

			if (land.profile) Profiler.EndSample();
			#endregion
			

			
			if (land.profile) Profiler.BeginSample ("Set Mesh");
			filter.sharedMesh.triangles = tris;
			if (land.profile) Profiler.EndSample();
			
			//filter.sharedMesh.RecalculateBounds();
			//filter.sharedMesh.RecalculateNormals();
		}
		*/
		
		bool Contains (float rangeMinX, float rangeMinZ, float rangeMaxX, float rangeMaxZ, float pointX, float pointZ)
		{
			return pointX >= rangeMinX && pointX <= rangeMaxX && pointZ >= rangeMinZ && pointZ <= rangeMaxZ;
		}
		
		bool Contains (float rangeMinX, float rangeMinZ, float rangeMaxX, float rangeMaxZ, float minX, float minZ, float maxX, float maxZ)
		{
			return  Contains(rangeMinX, rangeMinZ, rangeMaxX, rangeMaxZ, minX, minZ) &&
					Contains(rangeMinX, rangeMinZ, rangeMaxX, rangeMaxZ, minX, maxZ) &&
					Contains(rangeMinX, rangeMinZ, rangeMaxX, rangeMaxZ, maxX, minZ) &&
					Contains(rangeMinX, rangeMinZ, rangeMaxX, rangeMaxZ, maxX, maxZ);
		}
	}
	
}//namespace

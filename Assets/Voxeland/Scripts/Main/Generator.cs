using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
//[ExecuteInEditMode]
public class Generator
{
	public int seed = 1;
	
	public int mapSizeX = 512;
	public int mapSizeZ = 512;
	[System.NonSerialized] public float[] heightMap = new float[0];
	[System.NonSerialized] public float[] erodedMap = new float[0];
	[System.NonSerialized] public bool[] grassMap = new bool[0];
	
	public float level = 100;
	public void Level () { for (int i=0; i<heightMap.Length; i++) heightMap[i] = level; }
	
	public int offsetX = 0;
	public int offsetZ = 0;

	#region Noise
	public bool noise = true;
	public byte noiseType = 1;
	public int fractals = 10; 
	public float fractalMin = 5f;
	public float fractalMax = 150f;  
	public float valueMin = 6f;   
	public float valueMax = 100f;

	public void Noise ()
	{
		Random.seed = seed;
		
		float fractalStep = (fractalMax-fractalMin) / fractals;
		float valueStep = (valueMax-valueMin) / fractals;
		
		for (int i = fractals-1; i>=0; i--)
		{
			float curFractal = fractalMin + fractalStep*i;
			float curValue = valueMin + valueStep*i;

			for (int x=0; x<mapSizeX; x++)
				for (int z=0; z<mapSizeZ; z++)
			{
				//heightMap[z*mapSizeX+x] = (offsetX+x-1152+30)/3;
				
				
				heightMap[z*mapSizeX+x] += Mathf.PerlinNoise(
					(x+offsetX+10000)/curFractal, 
					(z+offsetZ+10000)/curFractal
						) * curValue - curValue/2;
			}
		}
	}
	
	public void NoiseIteration (float[] height, float[] mask, int sizeX, int sizeZ, float fractal, float strength, float offset)
	{
		for (int x=0; x<mapSizeX; x++)
			for (int z=0; z<mapSizeZ; z++)
			{
				float val = Mathf.PerlinNoise(x/fractal + offset, z/fractal + offset) * strength - strength/2;
				
				if (mask == null) height[z*mapSizeX+x] += val;
				else height[z*mapSizeX+x] += val*mask[z*mapSizeX+x];
			}
	}
	#endregion
	
	#region Terrace	
	public bool terrace = true;
	public float minTerrace = 10f;
	public float maxTerrace = 20f;
	public float terraceIncline = 5;
	public float terraceInclineLength = 4;
	public float terraceOpacity = 0.5f;
	
	public void Terrace (float[] map)
	{
		int terraceCount = 512;
		int i = 0; int j=0;
		
		float[] terraceStart = new float[terraceCount]; //vertical array of terrace levels
		for (i=1; i<terraceCount; i++) terraceStart[i] = terraceStart[i-1] + Random.Range(minTerrace, maxTerrace);
		
		
		float[] terrace = new float[mapSizeX*mapSizeZ];
		for (int x=0; x<mapSizeX; x++)
			for (int z=0; z<mapSizeZ; z++)
		{
			for (j=1; j<terraceCount; j++) 
				if (terraceStart[j] > map[z*mapSizeX+x]) 
					{ terrace[z*mapSizeX + x] = terraceStart[j-1]; break; } 
		}
		
		//creating array of safe coords
		bool[] safe = new bool[mapSizeX*mapSizeZ];
		for (int x=1; x<mapSizeX-1; x++)
			for (int z=1; z<mapSizeZ-1; z++)
				safe[z*mapSizeX+x] = true;
		
		//spreading terrace
		for (int c=0; c<terraceInclineLength; c++) 
		{
			for (i=terrace.Length-1; i>=0; i--) { if (!safe[i]) continue; SpreadStep(i, -1, terrace); }
			for (i=0; i<terrace.Length; i++) 	{ if (!safe[i]) continue; SpreadStep(i, 1, terrace); }
			for (i=terrace.Length-1; i>=0; i--) { if (!safe[i]) continue; SpreadStep(i, -mapSizeX, terrace); }
			for (i=0; i<terrace.Length; i++) 	{ if (!safe[i]) continue; SpreadStep(i, mapSizeX, terrace); }
		}
		
		//adding terrace
		for (i=1; i<map.Length; i++) map[i] = terrace[i]*terraceOpacity + map[i]*(1-terraceOpacity);
	}
	
	public void SpreadStep (int coord, int step, float[] map)
	{
		//float halfsum = (map[coord] + map[coord+step])/2;
		//map[coord] = Mathf.Max(map[coord], halfsum);
		map[coord] = Mathf.Max(map[coord+step], map[coord]-terraceIncline);
	}
	
	#endregion
	
	#region Valley
	public bool valley = true;
	public byte valleyType = 1;
	public float valleyLevel = 50f;
	public float valleySize = 100f;
	public float valleyOpacity = 0.5f;
	
	public void Valley (float[] map)
	{
		for (int x=0; x<mapSizeX; x++)
			for (int z=0; z<mapSizeZ; z++)
		{
			int i = z*mapSizeX+x;
			float valley = 0;
			
			//calculating
			if (map[i] > valleyLevel) valley = Mathf.Max(valleyLevel, map[i]-valleySize); 
			else valley = map[i]; 
			
			//blending
			map[i] = valley*valleyOpacity + map[i]*(1-valleyOpacity);
		}

	}
	#endregion
	
	#region Plateau
	public bool plateau = true;
	public float plateauLevel = 75f;
	public float plateauOpacity = 0.5f;
		
	public void Plateau (float[] map)
	{
		for (int x=0; x<mapSizeX; x++)
			for (int z=0; z<mapSizeZ; z++)
		{
			int i = z*mapSizeX+x;
			map[i] = (Mathf.Min(plateauLevel, map[i]))*plateauOpacity + map[i]*(1-plateauOpacity);
		}
		
	}
	#endregion
	
	
	#region Erosion
	public bool erosion = true;
	public byte erosionType = 1;
	public float erosionAmount = 0.03f;
	public float sedimentAmount = 0.03f;
	public int erosionIterations = 10;
	public float sedimentLower = 0;
	public float erosionBlur = 2.5f;
	public float erosionTorrentMax = 20f;
	public float erosionNoise = 0.2f;
	//public bool erode = false;
	
	//public float mudAmount, float blurValue, int deblurIterations
	
	int HeightCompare(int a, int b)
	{
		return (int)((heightMap[b] - heightMap[a])*100);
	}
	
	public void ErosionComplex ()
	{
		Erosion(heightMap, erosionIterations, mapSizeX, mapSizeZ);
	}
	
	public float[] ReduceMap (float[] map, int sizeX, int sizeZ)
	{
		int x=0; int z=0;
		float[] result = new float[(sizeX/2)*(sizeZ/2)];
		//reference = new float[(sizeX/2)*(sizeZ/2)];
		
		for (x=0; x<sizeX/2; x++)
			for (z=0; z<sizeZ/2; z++)
		{
			//int bx = x*2; int bz = z*2;
			
			int i = z*2*sizeX + x*2;
			result[z*sizeX/2+x] = map[i] / 2; //lowering height
			//reference 
		}
		
		return result;
	}
	
	public float[] EnlargeMap (float[] map, int sizeX, int sizeZ)
	{
		int sx=0; int sz=0;
		float[] result = new float[sizeX*2 * sizeZ*2];
		
		for (sx=0; sx<sizeX; sx++)
			for (sz=0; sz<sizeZ; sz++)
		{
			//int bx = x*2; int bz = z*2;
			
			//int bi = sz*sizeX*2+sx;
			//int si = sz*sizeX+sx;
			
			//result[bi] = map[si];
			
			/*
			result[i] = map[z*sizeX+x];
			result[i+1] = map[z*sizeX+x]/2 + map[z*sizeX+x+1]/2;
			result[i+sizeX*2] = map[z*sizeX+x]/2 + map[z*sizeX+x+sizeX]/2;
			result[i+sizeX*2+1] = map[z*sizeX+x]/2 + map[z*sizeX+x+sizeX+1]/2;
			*/
		}
		
		return result;
	}
	
	public void Erosion (float[] height, int iterations, int sizeX, int sizeZ)
	{
		int x=0; int z=0; int i=0; int j=0; int c=0; //float amount=0;
		
		erodedMap = new float[sizeX*sizeZ];

		float[] erosion = new float[sizeX*sizeZ];
		
		float heightXp=0; float heightXn=0; float heightZp=0; float heightZn=0; //float minHeight=0; float avgHeight=0;
		//float avgheight=0; float minheight=0;
		float torrentSum=0;
		
		//System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
		//stopwatch.Start();
		
		//creating array of safe coords
		bool[] safe = new bool[sizeX*sizeZ];
		for (x=1; x<sizeX-1; x++)
			for (z=1; z<sizeZ-1; z++)
				safe[z*sizeX+x] = true;
		
		//creating ordered list. Sorting it in iteration
		List<int> order = new List<int>(height.Length);
		for (x=1; x<sizeX-1; x++)
			for (z=1; z<sizeZ-1; z++)
				order.Add(z*sizeX+x);
				
		
		//iterating
		for (int iteration=0; iteration<iterations; iteration++)
		{
			//UnityEditor.EditorUtility.DisplayProgressBar("Generating", "Erosion", 1);
			#if UNITY_EDITOR
			if (iterations!=1) UnityEditor.EditorUtility.DisplayProgressBar("Generating", "Erosion", 1f*iteration/iterations);
			#endif
			
			//casting rain
			for (i=0; i<height.Length; i++) erosion[i] = 1;
			
			//torrents flow
			order.Sort(HeightCompare);
			for (j=0; j<order.Count; j++)
			{
				i = order[j];
				
				heightXp = height[i-1];
				heightXn = height[i+1];
				heightZp = height[i-sizeX];
				heightZn = height[i+sizeX];
				
				//calculating torrent direction
				heightXp = Mathf.Max(0, height[i]-heightXp); heightXn = Mathf.Max(0, height[i]-heightXn);
				heightZp = Mathf.Max(0, height[i]-heightZp); heightZn = Mathf.Max(0, height[i]-heightZn);
				
				torrentSum = heightXp + heightXn + heightZp + heightZn;
				
				//sending water down
				if (torrentSum > 0.1)
				{
					heightXp /= torrentSum; heightXn /= torrentSum; heightZp /= torrentSum; heightZn /= torrentSum; 
					
					erosion[i-1] += heightXp*erosion[i];
					erosion[i+1] += heightXn*erosion[i];
					erosion[i-sizeX] += heightZp*erosion[i];
					erosion[i+sizeX] += heightZn*erosion[i];
				}
			}
			
			//clamping erosion
			for (i=0; i<erosion.Length; i++) erosion[i] = Mathf.Min(erosionTorrentMax, erosion[i]);
			
			//spreading erosion
			for (c=0; c<3; c++) 
			{
				for (i=erosion.Length-1; i>=0; i--) { if (!safe[i]) continue; Spread(i, -1, erosion, erosionBlur);  }
				for (i=0; i<erosion.Length; i++) { if (!safe[i]) continue; Spread(i, 1, erosion, erosionBlur);  }
				for (i=erosion.Length-1; i>=0; i--) { if (!safe[i]) continue; Spread(i, -sizeX, erosion, erosionBlur);  }
				for (i=0; i<erosion.Length; i++) { if (!safe[i]) continue; Spread(i, sizeX, erosion, erosionBlur);  }
			}
			
			
			//raising mud
			for (i=0; i<height.Length; i++) height[i] -= erosion[i] * erosionAmount; 
			
			//sediment
			//for (i=0; i<height.Length; i++) erosion[i] = 1;
			for (i=0; i<erosion.Length; i++) erosion[i] *= sedimentAmount;
			for (c=0; c<15; c++)
			{
				for (i=height.Length-1; i>=0; i--)  { if (!safe[i]) continue; SpreadSediment(i, -1, height,erosion); SpreadSediment(i, -sizeX, height,erosion); }
				for (i=0; i<height.Length; i++) 	{ if (!safe[i]) continue; SpreadSediment(i, 1, height,erosion);  SpreadSediment(i, sizeX, height,erosion); } 	
			}
		
			//backing height change and sediment
			for (i=0; i<height.Length; i++) 
			{
				float heightChange = Mathf.Max(0,erosion[i]-sedimentLower);
				height[i] += heightChange;
				erodedMap[i] += heightChange;
			}
		}

		//adding some noise
		for (x=0; x<sizeX; x++)
			for (z=0; z<sizeZ; z++)
		{
			i = z*mapSizeX+x;
			float val = Mathf.PerlinNoise(x*0.5f, z*0.5f) * erosionNoise*2 + Mathf.PerlinNoise(x*1.1f, z*1.1f) * erosionNoise;
			
			height[i] -= val; 
		}
		
		//baking eroded map
		for (i=0; i<height.Length; i++) erodedMap[i] = heightMap[i] - erodedMap[i];
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.ClearProgressBar();
		#endif
		
		//stopwatch.Stop();
		//Debug.Log("Erosion: " + (0.001f * stopwatch.ElapsedMilliseconds));
	}
	

	public void Spread (int coord, int step, float[] map, float value)
	{
		float halfsum = (map[coord] + map[coord+step])/value;
		map[coord] = Mathf.Max(map[coord], halfsum);
		map[coord+step] = Mathf.Max(map[coord+step], halfsum);
	}
	
	
	public void SpreadSediment (int coord, int step, float[] height, float[] sediment)
	{
		//1. copy all to old cell
		sediment[coord] += sediment[coord+step];
		sediment[coord+step] = 0;
		
		//2. copy the value to raze to new cell
		float valueToRaze = height[coord] - height[coord+step];
		valueToRaze = Mathf.Clamp(valueToRaze, 0, sediment[coord]);
		sediment[coord] -= valueToRaze;
		sediment[coord+step] += valueToRaze;
		
		//3. copy the half of oldvalue-Max() to newvalue
		float valueToHalf = sediment[coord]+height[coord] - Mathf.Max(height[coord], height[coord+step]);
		valueToHalf = Mathf.Clamp(valueToHalf, 0, sediment[coord]);
		sediment[coord] -= valueToHalf / 2;
		sediment[coord+step] += valueToHalf / 2;
	}
	
	#endregion
	
	#region Border
	public int borderMargin = 2;
	public void Border (float[] map, int margin)
	{
		int size = (int)Mathf.Sqrt(map.Length);
		margin = Mathf.Min(margin, size-1);
		
		for (int z=0; z<size; z++)
		{
			float val = map[z*size+(margin-2)];
			for (int x=margin-1; x>=0; x--) map[z*size+x] = val;
			
			val = map[z*size+(size-margin-1)];
			for (int x=size-margin; x<size; x++) map[z*size+x] = val;
		}
		
		for (int x=0; x<size; x++)
		{
			float val = map[(margin-2)*size+x];
			for (int z=margin-1; z>=0; z--) map[z*size+x] = val;
			
			val = map[(size-margin-1)*size+x];
			for (int z=size-margin; z<size; z++) map[z*size+x] = val;
		}
		
	}
	#endregion
	
	#region Blend
	public bool blend = true;
	public int blendDist = 4;
	public byte blendType = 1;
	public float[] Blend (int[] originalMap, int[] changedMap)
	{
		int size = (int)Mathf.Sqrt(originalMap.Length);
		float step = 1.0f / 10; //blendDist;
		
		//creating mask
		float[] mask = new float[originalMap.Length];
		for (int i=0; i<mask.Length; i++)
		{
			if (originalMap[i] == 0) mask[i] = 1;
			else mask[i] = 0;
		}
		
		//creating array of safe coords
		bool[] safe = new bool[originalMap.Length];
		for (int x=1; x<size-1; x++)
			for (int z=1; z<size-1; z++)
				safe[z*size+x] = true;
		
		//spreading original map
		for (int j=0; j<2; j++)
		{
			for (int i=mask.Length-1; i>=0; i--) 
			{ 
				if (!safe[i]) continue; 
				if (originalMap[i]!=0) continue;
				originalMap[i] = originalMap[i+1];
			}
			for (int i=0; i<mask.Length; i++) 	
			{ 
				if (!safe[i]) continue;
				if (originalMap[i]!=0) continue;
				originalMap[i] = originalMap[i-1];
			}
			
			for (int i=mask.Length-1; i>=0; i--) 
			{ 
				if (!safe[i]) continue; 
				if (originalMap[i]!=0) continue;
				originalMap[i] = originalMap[i+size];
			}
			for (int i=0; i<mask.Length; i++) 	
			{ 
				if (!safe[i]) continue;
				if (originalMap[i]!=0) continue;
				originalMap[i] = originalMap[i-size];
			}
		}
		
		
		//spreading mask
		for (int i=mask.Length-1; i>=0; i--) 
		{ 
			if (!safe[i]) continue; 
			mask[i] = Mathf.Min(mask[i+1]+step, mask[i]);
			mask[i] = Mathf.Min(mask[i+size]+step, mask[i]);
		}
		for (int i=0; i<mask.Length; i++) 	
		{ 
			if (!safe[i]) continue;
			mask[i] = Mathf.Min(mask[i-1]+step, mask[i]);
			mask[i] = Mathf.Min(mask[i-size]+step, mask[i]); 
		}
		
		
		//blending
		float[] result = new float[originalMap.Length];
		for (int i=0; i<mask.Length; i++)
			result[i] = originalMap[i]*(1-mask[i]) + changedMap[i]*mask[i];
			
		return result;
	}
	#endregion
	
	#region Grass
	public bool grass = true;
	public byte grassType = 1;
	public float grassNoiseSize = 15f;
	public float grassDensity = 0.3f;  
	
	public void Grass ()
	{
		Random.seed = seed;

		for (int x=0; x<mapSizeX; x++)
			for (int z=0; z<mapSizeZ; z++)
		{
			float noise = Mathf.PerlinNoise(
				(x+offsetX)/grassNoiseSize, 
				(z+offsetZ)/grassNoiseSize);
			
			if (noise*Random.value*2 < grassDensity) grassMap[z*mapSizeX+x] = true;
			else grassMap[z*mapSizeX+x] = false;
		}
	}
	#endregion

	#region Test on plane (commented out)
	// Use this for initialization
	/*
	void OnEnable () 
	{
		Random.seed = seed;
		heightMap = new float[mapSizeX*mapSizeZ];
		color1Map = new float[mapSizeX*mapSizeZ];
		color2Map = new float[mapSizeX*mapSizeZ];
		color3Map = new float[mapSizeX*mapSizeZ];
		
		Level();
		if (noise) Noise();
		if (terrace) Terrace(heightMap);
		if (erosion) ErosionComplex();
		
		CompileMeshes();
	}
	
	public Material displayMaterial = null;
	public float displayBlend = 0;
	public float displayAdd = 0;
	public float displayFactor = 1;
	
	void Update ()
	{
		if (erode) { Erosion(heightMap, 1, mapSizeX, mapSizeZ); CompileMeshes(); erode = false; }
		
		if (displayMaterial == null) return;
		
		displayBlend = Mathf.Clamp01(displayBlend);
		displayAdd = Mathf.Clamp01(displayAdd);
		
		displayMaterial.SetFloat("_Display", displayBlend);
		displayMaterial.SetFloat("_Additive", displayAdd);
		displayMaterial.SetFloat("_Factor", displayFactor);
	} 
	
	
	void CompileMeshes ()
	{
		foreach (Transform sub in transform)
		{
			int x=0; int z=0; float c1=0; float c2=0; float c3=0;
			MeshFilter filter = (MeshFilter)sub.GetComponent(typeof(MeshFilter));
			
			Vector3[] verts = filter.sharedMesh.vertices;
			Color[] colors = new Color[verts.Length];
			
			for (int v=0; v<verts.Length; v++) 
			{
				x = (int)(-verts[v].x*mapSizeX);
				z = (int)(verts[v].y*mapSizeZ);
				
				x = Mathf.Clamp(x, 0, mapSizeX-1);
				z = Mathf.Clamp(z, 0, mapSizeZ-1);
				
				verts[v] = new Vector3(
					verts[v].x, 
					verts[v].y, 
					heightMap[ z*mapSizeX + x ]);
				
				c1 = color1Map[ z*mapSizeX + x ];
				c2 = color2Map[ z*mapSizeX + x ];
				c3 = color3Map[ z*mapSizeX + x ];
				colors[v] = new Color(c1*0.01f,c2*0.01f,c3,1);
			}
			
			filter.sharedMesh.vertices = verts;
			filter.sharedMesh.colors = colors;
			
			filter.sharedMesh.RecalculateBounds();
			filter.sharedMesh.RecalculateNormals();
			
			UnityEditor.EditorUtility.SetSelectedWireframeHidden(sub.renderer, true);
		}
	}
	*/
	#endregion
}

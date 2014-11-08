using UnityEngine;
using System.Collections;
using System.Collections.Generic;



[System.Serializable]
public class IntervalTree
{
	//classes there are pure data holders.
	[System.Serializable]
	public class ListWrapper 
	{ 
		public List<byte> list; 
		
		#region fns
		public ListWrapper Copy ()
		{
			ListWrapper result = new ListWrapper();
			result.list = new List<byte>();
			result.list.AddRange(list);
			return result;
		}
		
		public void CopyFrom (ListWrapper source)
		{
			list = new List<byte>();
			list.AddRange(source.list);
		}
		
		public void LoadFromData (int x, int z, VoxelandData data)
		{
			//generating byte column
			for (int y=0; y<data.sizeY; y++)
				byteColumn[y] = (byte)(data.GetNode(x,y,z).type);

			//baking it
			BakeByteColumn();
		}
		
		public void WriteByteColumn ()
		{
			if (list.Count==0) { byteColumnLength=0; return; }
			
			int typemax = list[1];
			int num = 0;
			byte curType = list[0];
			
			for (int y=0; y<byteColumn.Length; y++)
			{
				if (y >= typemax) 
				{
					num+=2;
					
					if (num>=list.Count) { byteColumnLength=y; break; }
					
					curType = list[num];
					typemax += list[num+1];
				}
				
				byteColumn[y] = curType;
			}
		}
		
		public void BakeByteColumn ()
		{		
			list.Clear();
			
			byte curType = 255;
			for (int y=0; y<byteColumnLength; y++)
			{
				if (byteColumn[y] != curType || list[list.Count-1] >= 250)
				{
					curType = byteColumn[y];
					
					list.Add(curType); //type
					list.Add(0);	//depth
				}
				
				list[list.Count-1]++; //depth
			}
			
			//removing top level
			while (list.Count != 0 && list[list.Count-2]==0)
				list.RemoveRange(list.Count-2, 2);
		}
		
		public byte GetBlock (int y) 
		{ 
			int layer = 0;
			for (int num=0; num<list.Count; num+=2)
			{
				layer += list[num+1]; //depth
				if (layer > y) return list[num]; //type
			}
			return 0;
		}
		
		public void  SetBlock (int y, byte t)
		{
			WriteByteColumn();

			byteColumn[y] = t;
			
			//adding zeros if y is larger than byte list length
			if (y>=byteColumnLength)
			{
				for (int i=byteColumnLength; i<y; i++) byteColumn[i] = 0;
				byteColumnLength = Mathf.Max(byteColumnLength, y+1);
			}

			BakeByteColumn();
		}
		
		public int GetTopPoint () 
		{ 
			int layer = 0;
			for (int num=1; num<list.Count; num+=2) //depth
			{
				layer += list[num]; //depth
			}
			return layer;
		}
		
		public int GetBottomPoint (bool[] exist) 
		{ 
			int layer = 0;
			for (int num=0; num<list.Count; num+=2) //type
			{
				if (!exist[ list[num] ]) return layer; //type
				layer += list[num+1]; //depth
			}
			return layer;
		}
		
		public byte GetTopType ()
		{
			if (list.Count==0) return 0;
			return list[list.Count-2];
		}
		
		public void SetHeight (int height, byte type)
		{
			WriteByteColumn();
			
			for (int i=byteColumnLength; i<height; i++) byteColumn[i] = type;
			
			byteColumnLength = height;
			byteColumnLength = Mathf.Max(1, byteColumnLength);
			
			BakeByteColumn();
		}
		
		#endregion
	}
	
	[System.Serializable]
	public class Area 
	{ 
		public ListWrapper[] columns;
		public ListWrapper[] grass;
		public bool serializable = false;
		public bool initialized = false;
		
		public int size;
		
		public void Initialize (int areaSize)
		{
			columns = new ListWrapper[areaSize*areaSize];
			for (int c=0; c<columns.Length; c++) 
			{
				columns[c] = new ListWrapper(); 
				columns[c].list = new List<byte>(); 
			}
			
			grass = new ListWrapper[areaSize];
			for (int c=0; c<grass.Length; c++) 
			{
				grass[c] = new ListWrapper(); 
				grass[c].list = new List<byte>(); 
			}
			
			initialized = true;
			size = areaSize;
		}
	}
	
	static public byte[] byteColumn = new byte[4096];
	static public int byteColumnLength = 0;
	
	public ListWrapper[] cols;
	//public int sizeX = 0;
	//public int sizeY = 0;
	//public int sizeZ = 0;

	public Area[] areas = new Area[100*100];

	public bool[] exist = new bool[256];
	
	private  ListWrapper emptyColumn = new ListWrapper();
	
	public class UndoColumns
	{
		public ListWrapper[] columns;
		public int x;
		public int z;
		public int range;
		
		public UndoColumns (int sx, int sz, int sr) { x=sx; z=sz; range=sr; columns = new ListWrapper[(range*2+1) * (range*2+1)]; }
		
		public void PerformUndo (IntervalTree data)
		{
			int minX = x-range; int minZ = z-range;
			int maxX = x+range; int maxZ = z+range;
			
			for (int xi = 0; xi<=maxX-minX; xi++)
				for (int zi = 0; zi<=maxZ-minZ; zi++)
					data.GetColumn(xi+minX, zi+minZ).CopyFrom( columns[xi*(range*2+1) + zi] );
		}
	}
	private List<UndoColumns> undos = new List<UndoColumns>();


	public int areaSize = 512;
	
	public IntervalTree ()
	{
		exist = new bool[256];
		areas = new Area[100*100];
		for (int i=0; i<areas.Length; i++) areas[i] = new Area();
	}
	
	/*
	public IntervalTree (int x, int y, int z)
	{
		cols = new ListWrapper[areaSize*areaSize];
		
		for (int i=0; i<cols.Length; i++) { cols[i] = new ListWrapper(); cols[i].list = new List<byte>(); }
		
		exist[1] = true;
		
		areas = new Area[100*100];
	}
	*/
	
	public IntervalTree (VoxelandData data)
	{
		byteColumnLength = data.sizeY;
		
		areas = new Area[100*100];
		for (int i=0; i<areas.Length; i++) areas[i] = new Area();
		
		areas[5050].Initialize(areaSize);
		
		//loading from data
		for (int x=0; x<data.sizeX; x++)
			for (int z=0; z<data.sizeZ; z++)
				GetColumn(x,z).LoadFromData(x,z,data);
		
		//filling exist array
		for (int x=0; x<data.sizeX; x++)
			for (int z=0; z<data.sizeZ; z++)
				for (int y=0; y<data.sizeY; y++)
				{
					VoxelandOctNode node = data.GetNode(x,y,z);
					exist[node.type] = node.exists;
				}
	}
	
	public ListWrapper GetColumn (int x, int z)
	{
		int shiftX = x + areaSize*50; int shiftZ = z + areaSize*50;
		
		int areaX = shiftX/areaSize; int areaZ = shiftZ/areaSize;
		
		Area area = areas[ areaZ*100 + areaX ];
		//area = areas[5050]; //ground zero

		//creating area
		if (area == null || !area.initialized)
		{
			area = new Area();
			area.Initialize(areaSize);
			areas[ areaZ*100 + areaX ] = area;
			
			/*
			if (emptyColumn==null || emptyColumn.list==null) { emptyColumn = new ListWrapper(); emptyColumn.list = new List<byte>(); }
			if (emptyColumn.list.Count==0) { emptyColumn.list.Add(1); emptyColumn.list.Add(10); }
			return emptyColumn;
			*/
		}
		
		return area.columns[(shiftZ-areaZ*areaSize)*areaSize + (shiftX-areaX*areaSize)];
	}
	
	public ListWrapper GetGrassColumn (int x, int z)
	{
		int shiftX = x + areaSize*50; int shiftZ = z + areaSize*50;
		int areaX = shiftX/areaSize; int areaZ = shiftZ/areaSize;
		Area area = areas[ areaZ*100 + areaX ];

		if (!area.initialized) return emptyColumn;

		return area.columns[x];
	}


	//public void WriteByteColumn (int x, int z) { WriteByteColumn(GetColumn(x,z)); }
	
	//public void BakeByteColumn (int x, int z) { BakeByteColumn(GetColumn(x,z)); }
	
	public byte GetBlock (int x, int y, int z) { return GetColumn(x,z).GetBlock(y); }
	
	public bool GetExist (int x, int y, int z) { return exist[ GetBlock(x,y,z) ]; }
	
	public void SetBlock (int x, int y, int z, byte t, bool filled)
	{
		exist[t] = filled;
		GetColumn(x,z).SetBlock(y, t);
	}
	
	public byte GetGrass (int x, int z) 
	{ 
		int areaStartX=0; int areaStartZ=0; int areaSize=0;
		//return GetArea(x,z, out areaStartX, out areaStartZ, out areaSize).columns[x-areaStartX].GetBlock(z-areaStartZ);
		
		Area area = GetArea(x,z, out areaStartX, out areaStartZ, out areaSize);
		ListWrapper column = area.grass[x-areaStartX];
		byte block = column.GetBlock(z-areaStartZ);
		
		return block;
	}
	
	public void SetGrass (int x, int z, byte t) 
	{ 
		int areaStartX=0; int areaStartZ=0; int areaSize=0;
		//GetArea(x,z, out areaStartX, out areaStartZ, out areaSize).columns[x-areaStartX].SetBlock(z-areaStartZ, t);
		
		Area area = GetArea(x,z, out areaStartX, out areaStartZ, out areaSize);
		ListWrapper column = area.grass[x-areaStartX];
		column.SetBlock(z-areaStartZ, t);
	}
	
	public void RegisterUndo (int x, int z, int extend) 
	{ 
		UndoColumns undo = new UndoColumns(x,z,extend);
		
		int minX = x-extend; int minZ = z-extend;
		int maxX = x+extend; int maxZ = z+extend;
		
		for (int xi = 0; xi<=maxX-minX; xi++)
			for (int zi = 0; zi<=maxZ-minZ; zi++)
				undo.columns[xi*(extend*2+1) + zi] = GetColumn(xi+minX, zi+minZ).Copy();
		
		if (undos.Count > 16) undos.RemoveAt(0);
		undos.Add(undo); 
	}
	public void PerformUndo ()
	{
		if (undos.Count==0) return;
		
		undos[undos.Count-1].PerformUndo(this);
		undos.RemoveAt(undos.Count-1);
	}
	
	public int GetTopPoint (int x, int z) { return GetColumn(x,z).GetTopPoint(); }
	public int GetTopPoint (int sx, int sz, int ex, int ez)
	{
		int result = 0;

		for (int x=sx; x<=ex; x++)
			for (int z=sz; z<=ez; z++)
				result = Mathf.Max( GetColumn(x,z).GetTopPoint(), result);
		
		return result;
	}
	
	public int GetBottomPoint (int x, int z) { return GetColumn(x,z).GetBottomPoint(exist); }
	public int GetBottomPoint (int sx, int sz, int ex, int ez)
	{
		int result = 2147483646;

		for (int x=sx; x<=ex; x++)
		{
			for (int z=sz; z<=ez; z++) 
			{
				result = Mathf.Min( GetColumn(x,z).GetBottomPoint(exist), result);
			}
		}
		
		return result;
	}
	
	public byte GetTopType (int x, int z) { return GetColumn(x,z).GetTopType(); }
	
	public void GetExistMatrix (bool[] matrix, int sx, int sy, int sz, int ex, int ey, int ez)
	{
		int matrixSizeX = ex-sx;
		int matrixSizeY = ey-sy;
		int matrixSizeZ = ez-sz;
		
		for (int x = 0; x<matrixSizeX; x++)
			for (int z = 0; z<matrixSizeZ; z++)
		{
			List<byte> column = GetColumn(x+sx,z+sz).list;
			int coord = z*matrixSizeX*matrixSizeY + x;
			
			//resetting matrix if column empty
			if (column.Count==0) 
			{ 
				for (int y=0; y<matrixSizeY; y++) 
					matrix[coord + y*matrixSizeX] = false;
				continue;
			}
				
			//writing byte column directly to matrix
			int typemax = column[1];
			int num = 0;
			byte curType = column[0];
				
			for (int y=0; y<ey; y++)
			{
				//getting cur type
				if (y >= typemax) 
				{
					num+=2;
					
					if (num>=column.Count) curType = 0;
					else
					{
						curType = column[num];
						typemax += column[num+1];
					}
				}
				
				//filling matrix
				if (y>=sy) 
				{
					matrix[coord + (y-sy)*matrixSizeX] = exist[curType];
				}
			}			
		}
	}
	
	public void GetMatrix (byte[] matrix, int sx, int sy, int sz, int ex, int ey, int ez)
	{
		int matrixSizeX = ex-sx;
		int matrixSizeY = ey-sy;
		int matrixSizeZ = ez-sz;
		
		for (int x = 0; x<matrixSizeX; x++) 
		{
			//if (x+sx<0 || x+sx>=100) continue;
			for (int z = 0; z<matrixSizeZ; z++)
			{
				//if (z+sz<0 || z+sz>=100) continue;

				for (int y = 0; y<matrixSizeY; y++)
				{
					matrix[z*matrixSizeX*matrixSizeY + y*matrixSizeX + x] = GetBlock(x+sx, y+sy, z+sz);
				}
			}
		}
	}
	
	public Area GetArea (int x, int z, out int offsetX, out int offsetZ, out int size)
	{
		int shiftX = (int)(x + areaSize*50); 
		int shiftZ = (int)(z + areaSize*50);
		
		int areaX = shiftX/areaSize; int areaZ = shiftZ/areaSize;
		
		offsetX = areaX*areaSize - areaSize*50;
		offsetZ = areaZ*areaSize - areaSize*50;
		size = areaSize;
		
		return areas[ areaZ*100 + areaX ];
	}
	
	public void Generate (Generator generator, int offsetX, int offsetZ, int range, bool overwrite)
	{
		//if no generator - using new (default)
		if (generator==null) generator = new Generator();
		
		generator.mapSizeX = range*2;
		generator.mapSizeZ = range*2;
		generator.heightMap = new float[range*2 * range*2];
		generator.offsetX = offsetX-range;
		generator.offsetZ = offsetZ-range;
		
		generator.Level();
		
		if (overwrite) Clear(offsetX, offsetZ, range);
		
		int[] originalMap = GetHeightmap(offsetX, offsetZ, range);
		bool[] originalMask = new bool[originalMap.Length];
		for (int i=0; i<originalMap.Length; i++) 
		{
			if (originalMap[i] < 1) originalMask[i] = true;
		}
		
		if (generator.noise) 
		{ 
			generator.Noise();
			SetHeightmap(generator.heightMap, originalMask, offsetX, offsetZ, range, generator.noiseType);
		}
		
		if (generator.valley) 
		{ 
			generator.Valley(generator.heightMap); 
			SetHeightmap(generator.heightMap, originalMask, offsetX, offsetZ, range, generator.valleyType); 
		}
		
		if (generator.terrace) 
		{ 
			generator.Terrace(generator.heightMap); 
			SetHeightmap(generator.heightMap, originalMask, offsetX, offsetZ, range, generator.noiseType); 
		}
		
		if (generator.plateau) 
		{ 
			generator.Plateau(generator.heightMap); 
			SetHeightmap(generator.heightMap, originalMask, offsetX, offsetZ, range, 1); 
		}
		
		if (generator.erosion) 
		{ 
			generator.ErosionComplex();
			generator.Border(generator.erodedMap, 4);
			generator.Border(generator.heightMap, 4);
			
			//extracting
			SetHeightmap(generator.erodedMap, originalMask, offsetX, offsetZ, range, generator.erosionType);
			
			//adding
			SetHeightmap(generator.heightMap, originalMask, offsetX, offsetZ, range, generator.erosionType);
		}
		
		if (generator.grass)
		{
			generator.grassMap = new bool[range*2 * range*2];
			generator.Grass();
			SetGrassmap(generator.grassMap, originalMask, offsetX, offsetZ, range, generator.grassType);
		}
		
		else if (generator.blend)
		{
			int[] generatedMap = GetHeightmap(offsetX, offsetZ, range);
			float[] blendedMap = generator.Blend (originalMap, generatedMap);
			SetHeightmap(blendedMap, originalMask, offsetX, offsetZ, range, generator.blendType);
		}
	}
	
	public void SetHeightmap (float[] heightMap, bool[] mask, int offsetX, int offsetZ, int range, byte type)
	{
		int minX = offsetX-range;
		int minZ = offsetZ-range;
		
		for (int x = 0; x<range*2; x++)
			for (int z = 0; z<range*2; z++)
		{
			if (!mask[z*range*2 + x]) continue;
			int height = (int)heightMap[z*range*2 + x];
			GetColumn(x+minX, z+minZ).SetHeight(height, type);
		}
	}
	
	public void SetLevel (int level, int offsetX, int offsetZ, int range, byte type)
	{
		int minX = offsetX-range;
		int minZ = offsetZ-range;
		
		for (int x = 0; x<range*2; x++)
			for (int z = 0; z<range*2; z++)
		{
			GetColumn(x+minX, z+minZ).SetHeight(level, type);
		}
	}
	
	public int[] GetHeightmap (int offsetX, int offsetZ, int range)
	{
		int[] heightMap = new int[range*2 * range*2];
		
		int minX = offsetX-range;
		int minZ = offsetZ-range;
		
		for (int x = 0; x<range*2; x++)
			for (int z = 0; z<range*2; z++)
				heightMap[z*range*2 + x] = GetTopPoint(x+minX, z+minZ);
	
		return heightMap;
	}
	
	public void SetGrassmap (bool[] grassMap, bool[] mask, int offsetX, int offsetZ, int range, byte type)
	{
		int minX = offsetX-range;
		int minZ = offsetZ-range;
		
		for (int x = 0; x<range*2; x++)
			for (int z = 0; z<range*2; z++)
		{
			if (!mask[z*range*2 + x]) continue;
			
			if (grassMap[z*range*2 + x]) SetGrass(minX+x,minZ+z,type);
			else SetGrass(minX+x,minZ+z,0);
		}
	}
	
	public void Clear (int offsetX, int offsetZ, int range)
	{
		int minX = offsetX-range;
		int minZ = offsetZ-range;
		
		for (int x = 0; x<range*2; x++)
			for (int z = 0; z<range*2; z++)
				GetColumn(x+minX, z+minZ).list.Clear();
	}
	
	
	public List<byte> SaveToByteList ()
	{
		//254 - uninitialized area, then byte count of un-initialized areas
		//253 - initialized area
		//252 - empty column, then number of empty columns
		//251 - ordinary column
		
		List<byte> byteList = new List<byte>();
		
		for (int a=0; a<areas.Length; a++)
		{
			if (!areas[a].initialized) 
			{
				//adding uninitialized mark
				if (byteList.Count<=0 || byteList[byteList.Count-2]!=254 || byteList[byteList.Count-1]>=249)
				{
					byteList.Add(254);
					byteList.Add(1);
				}
				else byteList[byteList.Count-1]++;
			}
			else
			{
				byteList.Add(253);
				
				for (int c=0; c<areas[a].columns.Length; c++)
				{
					if (areas[a].columns[c].list.Count == 0) 
					{
						if (byteList.Count<=1 || byteList[byteList.Count-2]!=252 || byteList[byteList.Count-1]>=250)
						{
							byteList.Add(252);
							byteList.Add(1);
						}
						else byteList[byteList.Count-1]++;
					}
					else
					{
						byteList.Add(251);
						byteList.AddRange( areas[a].columns[c].list );
					}
				}
				
				
				for (int c=0; c<areas[a].grass.Length; c++)
				{
					if (areas[a].grass[c].list.Count == 0) 
					{
						if (byteList.Count<=1 || byteList[byteList.Count-2]!=252 || byteList[byteList.Count-1]>=250)
						{
							byteList.Add(252);
							byteList.Add(1);
						}
						else byteList[byteList.Count-1]++;
					}
					else
					{
						byteList.Add(251);
						byteList.AddRange( areas[a].grass[c].list );
					}
				}
			}
		}
		
		return byteList;
	}
	
	public void LoadFromByteList (List<byte> byteList)
	{
		int a = -1;
		int c = -1;
		for (int i=0; i<byteList.Count; i++)
		{
			byte b = byteList[i];
			
			//non-initialized
			if (b==254) 
			{ 
				i++;
				for (int a2=0; a2<byteList[i]; a2++)
				{
					a++;
					areas[a]=new Area(); 
				}
			}
			
			//initialized
			else if (b==253) 
			{ 
				a++; 
				areas[a]=new Area(); 
				areas[a].Initialize(512); 
				c=-1; 
			}

			//empty column
			else if (b==252) 
			{ 
				i++;
				for (int c2=0; c2<byteList[i]; c2++)
				{
					c++;
					
					if (c<areas[a].columns.Length) 
					{
						areas[a].columns[c]=new ListWrapper();
						areas[a].columns[c].list = new List<byte>(); 
					}
					else 
					{
						areas[a].columns[c-areas[a].columns.Length]=new ListWrapper();
						areas[a].columns[c-areas[a].columns.Length].list = new List<byte>(); 
					}
				}
			}
			
			//ordinary column
			else if (b==251) 
			{ 
				c++;
				
				if (c<areas[a].columns.Length) 
				{
					areas[a].columns[c]=new ListWrapper();
					areas[a].columns[c].list = new List<byte>(); 
				}
				else 
				{
					areas[a].grass[c-areas[a].columns.Length]=new ListWrapper();
					areas[a].grass[c-areas[a].columns.Length].list = new List<byte>(); 
				}
			}
			
			//any other
			else 
			{ 
				if (c<areas[a].columns.Length) areas[a].columns[c].list.Add(b); 
				else areas[a].grass[c-areas[a].columns.Length].list.Add(b);
			}
		}
	}
	
	public string SaveToString ()
	{
		System.IO.StringWriter str = new System.IO.StringWriter();
		
		//saving blocks
		List<byte> byteList = SaveToByteList();
		for (int b=0; b<byteList.Count; b++) str.Write( System.Convert.ToChar(byteList[b]) );
		
		//saving 
		
		return str.ToString();
	}
	
	public void LoadFromString (string s)
	{
		System.IO.StringReader str = new System.IO.StringReader(s);
		List<byte> byteList = new List<byte>();
		
		while (str.Peek() >= 0) 
			byteList.Add((byte)str.Read());
		
		LoadFromByteList(byteList);
	}
}

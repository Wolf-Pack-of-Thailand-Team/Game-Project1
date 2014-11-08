
using UnityEngine;
using System.Collections;


[System.Serializable]
public class VoxelandOctree
{
	//int[ matrix,,];
	//float[ ambientMatrix,,];
	public enum VoxelandDataGenerateBlend {additive=0, replace=1}; 
	
	static public VoxelandOctNode[] nullArray = new VoxelandOctNode[0];
	
	[HideInInspector] public  int[] blocks;
	[HideInInspector] public float[] ambient;
	[HideInInspector] public bool[] exists;
	
	//[HideInInspector] 
	public VoxelandOctNode[] octree;
	public int biggestNode = 16;
	public int nodesX;
	public int nodesY;
	public int nodesZ;
	
	public int sizeX;
	public int sizeY;
	public int sizeZ;
	//int chunkSize = 10; 
	
	public int newSizeX;
	public int newSizeY;
	public int newSizeZ;
	public int newBiggestNode;
	public int offsetX;
	public int offsetY;
	public int offsetZ;
	
	public VoxelandOctree (VoxelandData data)
	{
		biggestNode = data.biggestNode;
		nodesX = data.nodesX;
		nodesY = data.nodesY;
		nodesZ = data.nodesZ;
		
		sizeX = data.sizeX;
		sizeY = data.sizeY;
		sizeZ = data.sizeZ;
		
		octree = new VoxelandOctNode[data.octree.Length];
		for (int i=0; i<octree.Length; i++) octree[i] = data.octree[i];
	}
	
	public VoxelandOctree (int sx, int sy, int sz)
	{
		nodesX = Mathf.CeilToInt(1f*sx/biggestNode);
		nodesY = Mathf.CeilToInt(1f*sy/biggestNode);
		nodesZ = Mathf.CeilToInt(1f*sz/biggestNode);
		
		sizeX = nodesX*biggestNode;
		sizeY = nodesY*biggestNode;
		sizeZ = nodesZ*biggestNode;
		
		octree = new VoxelandOctNode[nodesX*nodesY*nodesZ];
		for (int i=0; i<octree.Length; i++) octree[i] = new VoxelandOctNode();
		
		//filling
		for (int nodeY = 0; nodeY<=nodesY/3; nodeY++)
			for (int nodeX = 0; nodeX<nodesX; nodeX++)
				for (int nodeZ = 0; nodeZ<nodesZ; nodeZ++)
				{
					octree[nodeZ*nodesY*nodesX + nodeY*nodesX + nodeX].exists = true;
					octree[nodeZ*nodesY*nodesX + nodeY*nodesX + nodeX].type = 1;
				}
	}


	public VoxelandOctNode GetBigNode ( int x ,   int y ,   int z  )  //returns the most parent node
	{
		int nX = x/biggestNode;
		int nY = y/biggestNode;
		int nZ = z/biggestNode;
		
		return octree[nZ*nodesY*nodesX + nY*nodesX + nX];
	}
	
	public VoxelandOctNode GetNode ( int x ,   int y ,   int z  )  //returns the particular child node
	{
		//if (octree==null || octree.Length==0) Upgrade();
		//y = Mathf.Clamp(y,0,sizeY-1);
		
		int nX = x/biggestNode;
		int nY = y/biggestNode;
		int nZ = z/biggestNode;
		
		return octree[nZ*nodesY*nodesX + nY*nodesX + nX].GetNode(x-nX*biggestNode, y-nY*biggestNode, z-nZ*biggestNode, biggestNode/2);
	}
	
	public VoxelandOctNode GetClosestNode ( int x ,   int y ,   int z  )  //returns child node with safe frame
	{
		int sX = Mathf.Min(sizeX-1, Mathf.Max(0,x));
		int sY = Mathf.Min(sizeY-1, Mathf.Max(0,y));
		int sZ = Mathf.Min(sizeZ-1, Mathf.Max(0,z));
		
		return GetNode(sX, sY, sZ);
		//return octree[0];
		//return GetNode(x, y, z);
	}
	
	public bool CheckBounds ( int x ,   int y ,   int z  ){
		return (x>=0 && x<sizeX &&
		        y>=0 && y<sizeY &&
		        z>=0 && z<sizeZ);
	} 
	
	public int GetBlock ( int x ,   int y ,   int z  ){
		return GetClosestNode(x,y,z).type;
		//return blocks[ z*sizeY*sizeX + y*sizeX + x ];
		
		
	}
	public void  SetBlock ( int x ,   int y ,   int z ,   int type ,   bool filled  )
	{ 
		//if (octree==null || octree.Length==0) Upgrade();
		
		//getting top node
		int nX = x/biggestNode;
		int nY = y/biggestNode;
		int nZ = z/biggestNode;
		
		VoxelandOctNode topNode = octree[nZ*nodesY*nodesX + nY*nodesX + nX];
		
		#if UNITY_EDITOR
		//AddUndoNode(nZ*nodesY*nodesX + nY*nodesX + nX);
		#endif
		
		//dividing nodes recursive
		VoxelandOctNode node = topNode.SetNode(x-nX*biggestNode, y-nY*biggestNode, z-nZ*biggestNode, biggestNode/2);
		
		node.exists = filled;
		node.type = type;
		
		//merging nodes if possible
		topNode.Collapse();
		
		//int pos = z*sizeY*sizeX + y*sizeX + x;
		//blocks[pos] = type; 
		//exists[pos] = filled;
		
		#if UNITY_EDITOR
		//UnityEditor.EditorUtility.SetDirty(this);
		#endif
	}
	
	public bool GetExist ( int x ,   int y ,   int z  ){ 
		return GetNode(x,y,z).exists;
		//return exists[ z*sizeY*sizeX + y*sizeX + x ];
	}
	
	public bool GetSafeExist ( int x ,   int y ,   int z  ){ 
		return GetNode(x,y,z).exists;
		//return exists[ z*sizeY*sizeX + y*sizeX + x ];
	}
	
	public int GetTopPoint ( int x ,   int z  )
	{
		int nX = x/biggestNode;
		int nZ = z/biggestNode;
		
		for (int nY = nodesY-1; nY>=0; nY--)
		{
			int result= octree[nZ*nodesY*nodesX + nY*nodesX + nX].GetHeight(x-nX*biggestNode, z-nZ*biggestNode, biggestNode/2);
			if (result >= 0) return result + nY*biggestNode;
		}
		return 0;
	}
	
	public int GetTopPoint ( int sx ,   int sz ,   int ex ,   int ez  )
	{
		return sizeY;
		
		/*
		int startNodeX = Mathf.Clamp(sx/biggestNode,0,nodesX-1);
		int startNodeZ = Mathf.Clamp(sz/biggestNode,0,nodesZ-1);
		int endNodeX = Mathf.Clamp(ex/biggestNode,0,nodesX-1);
		int endNodeZ = Mathf.Clamp(ez/biggestNode,0,nodesZ-1);
		
		int result = -1;
		
		for (int nodeY = nodesY-1; nodeY>=0; nodeY--)
		{
			for (int nodeX = startNodeX; nodeX<=endNodeX; nodeX++)
				for (int nodeZ = startNodeZ; nodeZ<=endNodeZ; nodeZ++)
			{
				//int nodeNum = nodeZ*nodesY*nodesX + nodeY*nodesX + nodeX;
				
				result = Mathf.Max(result, octree[nodeZ*nodesY*nodesX + nodeY*nodesX + nodeX].GetTopPoint(
					sx-nodeX*biggestNode, 
					sz-nodeZ*biggestNode, 
					ex-nodeX*biggestNode, 
					ez-nodeZ*biggestNode, 
					biggestNode/2));
			}
			if (result >= 0) return result + nodeY*biggestNode;
		}
		return 0;
		*/
	}
	
	public int GetBottomPoint (int sx,   int sz ,   int ex ,   int ez  )
	{
		return 0;
		
		/*
		int startNodeX = Mathf.Clamp(sx/biggestNode,0,nodesX-1);
		int startNodeZ = Mathf.Clamp(sz/biggestNode,0,nodesZ-1);
		int endNodeX = Mathf.Clamp(ex/biggestNode,0,nodesX-1);
		int endNodeZ = Mathf.Clamp(ez/biggestNode,0,nodesZ-1);
		
		int result = 1000000;
		
		for (int nodeY = 0; nodeY<nodesY; nodeY++)
		{
			for (int nodeX = startNodeX; nodeX<=endNodeX; nodeX++)
				for (int nodeZ = startNodeZ; nodeZ<=endNodeZ; nodeZ++)
			{
				//int nodeNum = nodeZ*nodesY*nodesX + nodeY*nodesX + nodeX;
				
				result = Mathf.Min(result, octree[nodeZ*nodesY*nodesX + nodeY*nodesX + nodeX].GetBottomPoint(
					sx-nodeX*biggestNode, 
					sz-nodeZ*biggestNode, 
					ex-nodeX*biggestNode, 
					ez-nodeZ*biggestNode, 
					(int)(biggestNode/2) ));
			}
			if (result < 1000000) return result + nodeY*biggestNode;
		}
		return 0;
		*/
	}
	
	public void GetExistMatrix (bool[] matrix, int sx, int sy, int sz, int ex, int ey, int ez)
	{
		//getting op nodes
		int startNodeX = Mathf.Clamp(sx/biggestNode,0,nodesX-1);
		int startNodeY = Mathf.Clamp(sy/biggestNode,0,nodesY-1);
		int startNodeZ = Mathf.Clamp(sz/biggestNode,0,nodesZ-1);
		int endNodeX = Mathf.Clamp(ex/biggestNode,0,nodesX-1);
		int endNodeY = Mathf.Clamp(ey/biggestNode,0,nodesY-1);
		int endNodeZ = Mathf.Clamp(ez/biggestNode,0,nodesZ-1);
		
		//iterating in op nodes
		for (int nodeY = startNodeY; nodeY<=endNodeY; nodeY++)
			for (int nodeX = startNodeX; nodeX<=endNodeX; nodeX++)
				for (int nodeZ = startNodeZ; nodeZ<=endNodeZ; nodeZ++)
			{
				octree[nodeZ*nodesY*nodesX + nodeY*nodesX + nodeX].GetExistMatrix(
					matrix, ex-sx, ey-sy, ez-sz, 
					sx-nodeX*biggestNode,
					sy-nodeY*biggestNode, 
					sz-nodeZ*biggestNode, 
					ex-nodeX*biggestNode,
					ey-nodeY*biggestNode, 
					ez-nodeZ*biggestNode, 
					biggestNode/2);
			}
	}
	
	public bool[]  GetExistMatrix (int sx, int sy, int sz, int ex, int ey, int ez)
	{
		//creating matrix
		int matrixSizeX= ex-sx;
		int matrixSizeY= ey-sy;
		//int matrixSizeZ= ez-sz; //never used
		
		bool[] matrix = new bool[matrixSizeX*matrixSizeY*matrixSizeX];
		
		GetExistMatrix(matrix, sx, sy, sz, ex, ey, ez);
		
		return matrix;
		
		/*
		for (int x = 0; x<matrixSizeX; x++)
			for (int y = 0; y<matrixSizeY; y++)
				for (int z = 0; z<matrixSizeZ; z++)
					matrix[ z*matrixSizeY*matrixSizeX + y*matrixSizeX + x ] = Exists(x+sx, y+sy, z+sz);
		*/
	}
	
	
	public bool SafeExists ( int x ,   int y ,   int z  ){ 
		if (x>sizeX-3 || z>sizeZ-3) return false;
		if (CheckBounds(x,y,z)) return exists[ z*sizeY*sizeX + y*sizeX + x ];
		else return false;
	}
	public bool SafeExists ( int pos  ){  return exists[pos]; }
	
	public void  Visualize ( bool nodes ,    bool fill  ){
		if (octree==null || octree.Length==0) return; 
		
		for (int x=0; x<nodesX; x++) 
			for (int y=0; y<nodesY; y++)
				for (int z=0; z<nodesZ; z++) octree[z*nodesY*nodesX + y*nodesX + x].Visualize(x*biggestNode, y*biggestNode, z*biggestNode, biggestNode/2, nodes, fill);
	}
	
	
	public bool  HasBlockAbove ( int x ,   int y ,   int z  ){
		for (int y2=sizeY-1; y2>=y; y2--)
			if (SafeExists(x,y2,z)) return true;
		return false;
	}
	
	public bool ValidateSize ( int size ,   int chunkSize  ){  return Mathf.RoundToInt((size*1.0f)/(chunkSize*1.0f))*chunkSize == size; }
	
	
	public void  Clear ( int newX ,   int newY ,   int newZ ,   int newNode  ){ Clear(newX, newY, newZ, newNode, 2); }
	public void  Clear ( int newX ,   int newY ,   int newZ ,   int newNode ,   int filledLevel  ){
		float[,] planarMap= new float[newX, newZ];		
		//GeneratePlanar(planarMap, filledLevel);
		
		SetHeightmap(planarMap, 1, newY, newNode);
		
		//setting new size
		sizeX = newX;
		sizeY = newY;
		sizeZ = newZ;
		biggestNode = newNode;
	}
	
	public void  Copy ( VoxelandData data  ){
		sizeX = data.sizeX;
		sizeY = data.sizeY;
		sizeZ = data.sizeZ;
		biggestNode = data.biggestNode;
		nodesX = data.nodesX;
		nodesY = data.nodesY;
		nodesZ = data.nodesZ;
		blocks = new int[0];
		
		octree = new VoxelandOctNode[ nodesX * nodesY * nodesZ ];
		
		for (int x=0; x<nodesX; x++) 
			for (int y=0; y<nodesY; y++)
				for (int z=0; z<nodesZ; z++) 
			{
				int i = z*nodesY*nodesX + y*nodesX + x;
				octree[i] = data.octree[i].Copy();			
			}
	}

	public float[,] GetHeighmap ()
	{
		float[,] map = new float[sizeX, sizeZ];
		for (int x=0; x<sizeX; x++)
			for (int z=0; z<sizeZ; z++)
				for (int y=0; y<sizeY; y++)
					if (GetExist(x,y,z)) map[x,z] = y;
		return map;
	}
	
	public void  SetHeightmap ( float[,] map1, float[,] map2,  float[,] map3,  float[,] map4, 
	                           int type1,   int type2,   int type3,   int type4,
	                           int height,   int nodeSize)
	{
		nodesX = Mathf.CeilToInt(1.0f*map1.GetLength(0)/nodeSize);
		nodesY = Mathf.CeilToInt(1.0f*height/nodeSize);
		nodesZ = Mathf.CeilToInt(1.0f*map1.GetLength(1)/nodeSize);
		
		octree = new VoxelandOctNode[ nodesX * nodesY * nodesZ ];
		
		for (int x=0; x<nodesX; x++) 
			for (int y=0; y<nodesY; y++)
				for (int z=0; z<nodesZ; z++) 
			{
				int i = z*nodesY*nodesX + y*nodesX + x;
				
				octree[i] = new VoxelandOctNode();
				//octree[i].GenerateFromHeightmap( map1, map2, map3, map4, type1, type2, type3, type4,
				//                                x*nodeSize, y*nodeSize, z*nodeSize, nodeSize/2); //offsets
			}
		
		//#if UNITY_EDITOR
		//UnityEditor.EditorUtility.SetDirty(this);
		//#endif
	}
	
	public void  SetHeightmap ( float[,] map,  int type ,   int height ,   int nodeSize  )
	{
		float[,] emptyMap= new float[map.GetLength(0), map.GetLength(1)];
		SetHeightmap(map, emptyMap, emptyMap, emptyMap, type, type, type, type, height, nodeSize);
	}
	
	public void  HeightmapToTexture ( float[,] map,  float factor  ){
		int mapSizeX = map.GetLength(0);
		int mapSizeZ = map.GetLength(1);
		
		Texture2D texture= new Texture2D(mapSizeX, mapSizeZ);
		//tempMat.mainTexture = texture;
		//tempMat.SetTexture("_ShadowTex", texture);
		
		for (int x=0; x<mapSizeX; x++)
			for (int z=0; z<mapSizeZ; z++)
		{
			texture.SetPixel (x, z, new Color(map[x,z]*factor-2,map[x,z]*factor-1,map[x,z]*factor));
		}		
		
		// Apply all SetPixel calls
		texture.Apply();
	}
	
	public void  SubstractHeightmap ( float[,] map1,  float[,] map2,  float[,] map3,  float[,] map4,  float[,] substract,  float factor  )
	{
		int mapSizeX = map1.GetLength(0);
		int mapSizeZ = map1.GetLength(1);
		
		for (int x=0; x<mapSizeX; x++)
			for (int z=0; z<mapSizeZ; z++)
		{
			float remain = substract[x,z]*factor;
			float temp;
			
			temp = map4[x,z];
			map4[x,z] = Mathf.Max(0, temp-remain);
			remain = Mathf.Max(0, remain-temp);
			
			temp = map3[x,z];
			map3[x,z] = Mathf.Max(0, temp-remain);
			remain = Mathf.Max(0, remain-temp);
			
			temp = map2[x,z];
			map2[x,z] = Mathf.Max(0, temp-remain);
			remain = Mathf.Max(0, remain-temp);
			
			temp = map1[x,z];
			map1[x,z] = Mathf.Max(0, temp-remain);
			remain = Mathf.Max(0, remain-temp);
			
			substract[x,z] -= remain;
		}
	}
	
	public float[,]  SumHeightmaps ( float[  , ] map1,  float[  , ] map2,  float[  , ] map3,  float[  , ] map4)
	{
		int mapSizeX = map1.GetLength(0);
		int mapSizeZ = map1.GetLength(1);
		
		float[ ,] map = new float[mapSizeX, mapSizeZ];
		
		for (int x=0; x<mapSizeX; x++)
			for (int z=0; z<mapSizeZ; z++)
				map[x,z] = Mathf.Clamp(map1[x,z] + map2[x,z] + map3[x,z] + map4[x,z], 0, sizeY-1);
		
		return map;
	}
	
	
	
}
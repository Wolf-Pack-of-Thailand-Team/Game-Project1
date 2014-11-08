
using UnityEngine;
using System.Collections;

[System.Serializable]
public class VoxelandOctNode
{
	//byte halfsize;
	
	public bool  exists;
	public int type;
	//byte ambient;
	
	public VoxelandOctNode[] nodes;
	
	//TestStruct temp;
	
	
	public VoxelandOctNode GetNode (int x, int y, int z, int halfsize) //returns the node at given coordinates
	{
		if (nodes==null || nodes.Length==0) return this;
		else
		{
			//if (z>halfsize) z-=halfsize;
			//if (x>halfsize) x-=halfsize;
			
			if (x<halfsize)
			{
				if (y<halfsize) 
				{
					if (z<halfsize) return nodes[0].GetNode(x,y,z,halfsize/2);
					else return nodes[1].GetNode(x,y,z-halfsize,halfsize/2);
				}
				else 
				{
					if (z<halfsize) return nodes[2].GetNode(x,y-halfsize,z,halfsize/2);
					else return nodes[3].GetNode(x,y-halfsize,z-halfsize,halfsize/2);
				}
			}
			
			else
			{
				if (y<halfsize) 
				{
					if (z<halfsize) return nodes[4].GetNode(x-halfsize,y,z,halfsize/2);
					else return nodes[5].GetNode(x-halfsize,y,z-halfsize,halfsize/2);
				}
				else 
				{
					if (z<halfsize) return nodes[6].GetNode(x-halfsize,y-halfsize,z,halfsize/2);
					else return nodes[7].GetNode(x-halfsize,y-halfsize,z-halfsize,halfsize/2);
				}
			}
			
			/*
			int ix=0; if (x>halfsize) ix=1;
			int iy=0; if (y>halfsize) iy=1;
			int iz=0; if (z>halfsize) iz=1;

			return nodes[iz*4 + iy*2 + ix].GetNode(x+halfsize*ix,y+halfsize*iy,z+halfsize*iz);
			*/
		}
	}
	
	public VoxelandOctNode SetNode (int x, int y , int z, int halfsize) //returns the deepest node at given coords
	{
		//returning the deepest node
		if (halfsize == 0) return this;
		
		//dividing nodes if no nodes exist
		if (nodes==null || nodes.Length==0) 
		{
			nodes = new VoxelandOctNode[8];
			
			for (int i=0;i<8;i++)
			{
				nodes[i] = new VoxelandOctNode();
				nodes[i].exists = exists;
				nodes[i].type = type;
			}
		}
		
		//setting node recursively
		if (x<halfsize)
		{
			if (y<halfsize) 
			{
				if (z<halfsize) return nodes[0].SetNode(x,y,z,halfsize/2);
				else return nodes[1].SetNode(x,y,z-halfsize,halfsize/2);
			}
			else 
			{
				if (z<halfsize) return nodes[2].SetNode(x,y-halfsize,z,halfsize/2);
				else return nodes[3].SetNode(x,y-halfsize,z-halfsize,halfsize/2);
			}
		}
		else
		{
			if (y<halfsize) 
			{
				if (z<halfsize) return nodes[4].SetNode(x-halfsize,y,z,halfsize/2);
				else return nodes[5].SetNode(x-halfsize,y,z-halfsize,halfsize/2);
			}
			else 
			{
				if (z<halfsize) return nodes[6].SetNode(x-halfsize,y-halfsize,z,halfsize/2);
				else return nodes[7].SetNode(x-halfsize,y-halfsize,z-halfsize,halfsize/2);
			}
		}
	}
	
	public int GetHeight (int x, int z, int size) //returns at which level inside the node toppoint is
	{
		//if solid node
		if (nodes==null || nodes.Length == 0) 
		{
			if (exists) return size;
			else return -1;
		}
		
		//getting 2-nodes column
		int topNodeNum = 0;
		int bottomNodeNum = 0;
		int halfsize = size/2;
		
		if (x<halfsize)
		{
			if (z<halfsize)  {bottomNodeNum=0; topNodeNum=2; }
			else { bottomNodeNum=1; topNodeNum=3; z-=halfsize; }
		}
		else
		{
			if (z<halfsize)  {bottomNodeNum=4; topNodeNum=6; x-=halfsize; }
			else { bottomNodeNum=5; topNodeNum=7; x-=halfsize; z-=halfsize; }
		}
		
		//getting top height
		int result = nodes[topNodeNum].GetHeight(x,z,halfsize);
		if (result >= 0) return result + halfsize;
		
		//getting bottom height - if top not found
		else 
		{
			result = nodes[bottomNodeNum].GetHeight(x,z,halfsize);
			if (result >= 0) return result;
		}
		
		return -1;
		
		/*
		if (nodes==null || nodes.Length == 0) //if solid node
		{
			if (exists) return 0;
			else return -1;
		}
		
		int topNodeNum = 0;
		int bottomNodeNum = 0;
		
		if (x<halfsize)
		{
			if (z<halfsize)  {bottomNodeNum=0; topNodeNum=2; }
			else { bottomNodeNum=1; topNodeNum=3; z-=halfsize; }
		}
		else
		{
			if (z<halfsize)  {bottomNodeNum=4; topNodeNum=6; x-=halfsize; }
			else { bottomNodeNum=5; topNodeNum=7; x-=halfsize; z-=halfsize; }
		}
		
		int result = nodes[topNodeNum].GetHeight(x,z,halfsize/2);
		if (result >= 0) return result + halfsize;
		else 
		{
			result = nodes[bottomNodeNum].GetHeight(x,z,halfsize/2);
			if (result >= 0) return result;
		}
		
		return -1;
		*/
	}
	/*
	public int WriteHeightmap (int x, int z, int halfsize, 
		int[] map, int mapSize,
		int level)
	{
		if (nodes==null || nodes.Length == 0) //if solid node
		{
			if (exists)
			{
				for (int xi=0; xi<halfsize*2; xi++)
					for (int zi=0; zi<halfsize*2; zi++)
						if (map[(z+zi)*mapSize + x+xi] < level) map[(z+zi)*mapSize + x+xi] = level;
			}
			return;
		}
		
		nodes[2].GetTopPoint(x,z,halfsize/2, map,mapSize, level);
		nodes[3].GetTopPoint(x,z-halfsize,halfsize/2, map,mapSize, level);
		nodes[6].GetTopPoint(x-halfsize,z,halfsize/2, map,mapSize, level);
		nodes[7].GetTopPoint(x-halfsize,z-halfsize,halfsize/2, map,mapSize, level);
		
		if (result>=0) result+=halfsize;	
		
		//looking at bottom layer
		else //if result -1
		{
			result = Mathf.Max(result, nodes[0].GetTopPoint(sx,sz,ex,ez,halfsize/2));
			result = Mathf.Max(result, nodes[1].GetTopPoint(sx,sz-halfsize,ex,ez-halfsize,halfsize/2));
			result = Mathf.Max(result, nodes[4].GetTopPoint(sx-halfsize,sz,ex-halfsize,ez,halfsize/2));
			result = Mathf.Max(result, nodes[5].GetTopPoint(sx-halfsize,sz-halfsize,ex-halfsize,ez-halfsize,halfsize/2));
		}
		
		return result;
	}
	*/
	
	public int GetTopPoint ( int sx ,   int sz ,   int ex ,   int ez ,   int halfsize  ) //returns top point in the scope
	{
		//checking if the node is in bounds:
		if (ex<0 || sx>=halfsize*2) return -1;
		if (ez<0 || sz>=halfsize*2) return -1;
		
		//if solid node
		if (nodes==null || nodes.Length == 0) 
		{
			if (exists) return 0;
			else return -1;
		}
		
		//looking at top layer
		int result = -1;
		
		result = Mathf.Max(result, nodes[2].GetTopPoint(sx,sz,ex,ez,halfsize/2));
		result = Mathf.Max(result, nodes[3].GetTopPoint(sx,sz-halfsize,ex,ez-halfsize,halfsize/2));
		result = Mathf.Max(result, nodes[6].GetTopPoint(sx-halfsize,sz,ex-halfsize,ez,halfsize/2));
		result = Mathf.Max(result, nodes[7].GetTopPoint(sx-halfsize,sz-halfsize,ex-halfsize,ez-halfsize,halfsize/2));
		
		if (result>=0) result+=halfsize;	
		
		//looking at bottom layer
		else //if result -1
		{
			result = Mathf.Max(result, nodes[0].GetTopPoint(sx,sz,ex,ez,halfsize/2));
			result = Mathf.Max(result, nodes[1].GetTopPoint(sx,sz-halfsize,ex,ez-halfsize,halfsize/2));
			result = Mathf.Max(result, nodes[4].GetTopPoint(sx-halfsize,sz,ex-halfsize,ez,halfsize/2));
			result = Mathf.Max(result, nodes[5].GetTopPoint(sx-halfsize,sz-halfsize,ex-halfsize,ez-halfsize,halfsize/2));
		}
		
		return result;
	}
	
	public int GetBottomPoint ( int sx ,   int sz ,   int ex ,   int ez ,   int halfsize  ) //returns top point in the scope
	{
		//checking if the node is in bounds:
		if (ex<0 || sx>=halfsize*2) return 1000000;
		if (ez<0 || sz>=halfsize*2) return 1000000;
		
		//if solid node
		if (nodes==null || nodes.Length == 0) 
		{
			if (!exists) return 0;
			else return 1000000;
		}
		
		//looking at bottom layer
		int result = 1000000;
		
		result = Mathf.Min(result, nodes[0].GetBottomPoint(sx,sz,ex,ez,halfsize/2));
		result = Mathf.Min(result, nodes[1].GetBottomPoint(sx,sz-halfsize,ex,ez-halfsize,halfsize/2));
		result = Mathf.Min(result, nodes[4].GetBottomPoint(sx-halfsize,sz,ex-halfsize,ez,halfsize/2));
		result = Mathf.Min(result, nodes[5].GetBottomPoint(sx-halfsize,sz-halfsize,ex-halfsize,ez-halfsize,halfsize/2));
		
		//looking at bottom layer
		if (result == 1000000)
		{
			result = Mathf.Min(result, nodes[2].GetBottomPoint(sx,sz,ex,ez,halfsize/2));
			result = Mathf.Min(result, nodes[3].GetBottomPoint(sx,sz-halfsize,ex,ez-halfsize,halfsize/2));
			result = Mathf.Min(result, nodes[6].GetBottomPoint(sx-halfsize,sz,ex-halfsize,ez,halfsize/2));
			result = Mathf.Min(result, nodes[7].GetBottomPoint(sx-halfsize,sz-halfsize,ex-halfsize,ez-halfsize,halfsize/2));
			
			if (result<1000000) result+=halfsize;
		}
		
		return result;
	}
	
	public void GetExistMatrix (bool[] matrix,  int matrixSizeX ,   int matrixSizeY ,   int matrixSizeZ , 
	                             int sx, int sy, int sz, int ex, int ey, int ez, int halfsize)
	{
		//checking if the node is in bounds:
		if (ex<0 || sx>=halfsize*2) return;
		if (ey<0 || sy>=halfsize*2) return;
		if (ez<0 || sz>=halfsize*2) return;
		
		//if solid node
		if (nodes==null || nodes.Length == 0) 
		{
			for (int x=-sx; x<=halfsize*2-sx; x++)
				for (int y=-sy; y<=halfsize*2-sy; y++)
					for (int z=-sz; z<=halfsize*2-sz; z++)
				{
					if (x<0 || y<0 || z<0 || 
					    x>=matrixSizeX || y>=matrixSizeY || z>=matrixSizeZ) continue;
					
					matrix[z*matrixSizeY*matrixSizeX + y*matrixSizeX + x] = exists;
				}
		}
		
		else
		{
			nodes[0].GetExistMatrix(matrix,matrixSizeX,matrixSizeY,matrixSizeZ, sx,sy,sz,ex,ey,ez, halfsize/2);
			nodes[1].GetExistMatrix(matrix,matrixSizeX,matrixSizeY,matrixSizeZ, sx,sy,sz-halfsize,ex,ey,ez-halfsize, halfsize/2);
			nodes[2].GetExistMatrix(matrix,matrixSizeX,matrixSizeY,matrixSizeZ, sx,sy-halfsize,sz,ex,ey-halfsize,ez, halfsize/2);
			nodes[3].GetExistMatrix(matrix,matrixSizeX,matrixSizeY,matrixSizeZ, sx,sy-halfsize,sz-halfsize,ex,ey-halfsize,ez-halfsize, halfsize/2);
			nodes[4].GetExistMatrix(matrix,matrixSizeX,matrixSizeY,matrixSizeZ, sx-halfsize,sy,sz,ex-halfsize,ey,ez, halfsize/2);
			nodes[5].GetExistMatrix(matrix,matrixSizeX,matrixSizeY,matrixSizeZ, sx-halfsize,sy,sz-halfsize,ex-halfsize,ey,ez-halfsize, halfsize/2);
			nodes[6].GetExistMatrix(matrix,matrixSizeX,matrixSizeY,matrixSizeZ, sx-halfsize,sy-halfsize,sz,ex-halfsize,ey-halfsize,ez, halfsize/2);
			nodes[7].GetExistMatrix(matrix,matrixSizeX,matrixSizeY,matrixSizeZ, sx-halfsize,sy-halfsize,sz-halfsize,ex-halfsize,ey-halfsize,ez-halfsize, halfsize/2);
		}
	}
	
	
	public void Collapse () //if nodes are the same - closing 
	{
		if (nodes==null || nodes.Length == 0) return;
		
		//recursively collapsing children
		for (int i=0; i<8; i++) nodes[i].Collapse();
		
		//determining if all nodes are the same
		bool  allSame = true;
		
		for (int i=0; i<8; i++)
		{
			if (nodes[i].nodes!=null && nodes[i].nodes.Length != 0) { allSame = false; break; }
			if (nodes[i].exists != nodes[0].exists || nodes[i].type != nodes[0].type) { allSame = false; break; }
		}
		
		
		//collapsing this one if all nodes are same
		if (allSame)
		{
			exists = nodes[0].exists;
			type = nodes[0].type;
			nodes = VoxelandData.nullArray;
		}
	}
	
	
	public void  Visualize ( int offsetX ,   int offsetY ,   int offsetZ ,   int halfsize ,   bool tree ,    bool fill  ){
		if (nodes!=null && nodes.Length!=0)
		{
			nodes[0].Visualize(offsetX, offsetY, offsetZ, halfsize/2, tree, fill);
			nodes[1].Visualize(offsetX, offsetY, offsetZ+halfsize, halfsize/2, tree, fill);
			nodes[2].Visualize(offsetX, offsetY+halfsize, offsetZ, halfsize/2, tree, fill);
			nodes[3].Visualize(offsetX, offsetY+halfsize, offsetZ+halfsize, halfsize/2, tree, fill);
			
			nodes[4].Visualize(offsetX+halfsize, offsetY, offsetZ, halfsize/2, tree, fill);
			nodes[5].Visualize(offsetX+halfsize, offsetY, offsetZ+halfsize, halfsize/2, tree, fill);
			nodes[6].Visualize(offsetX+halfsize, offsetY+halfsize, offsetZ, halfsize/2, tree, fill);
			nodes[7].Visualize(offsetX+halfsize, offsetY+halfsize, offsetZ+halfsize, halfsize/2, tree, fill);
		}
		else
		{
			//setting color
			float val = Mathf.Sqrt(halfsize*2)/4.0f;
			Vector2 color = new Vector2(1-val, val);
			color /= color.sqrMagnitude;
			Gizmos.color = new Color(color.x, color.y, 0, val*0.75f + 0.25f);
			
			//setting size and drawing
			int size = halfsize*2;
			float offset = halfsize;
			if (halfsize==0) { size = 1; offset = 0.5f; }
			
			if ((fill && exists && (nodes==null || nodes.Length==0)) || tree) Gizmos.DrawWireCube(new Vector3(offsetX+offset, offsetY+offset, offsetZ+offset), new Vector3(size,size,size));
		}
	}
	
	/*
	public void  GenerateFromHeightmap (float[,] map1,  float[,] map2,  float[,] map3,  float[,] map4, 
	                                    int type1 ,   int type2 ,   int type3 ,   int type4 , 
	                                    int offsetX ,   int offsetY ,   int offsetZ ,   int halfsize  ){
		int mapsizeX = map1.GetLength(0);
		int mapsizeZ = map1.GetLength(1);
		
		//finding reference type at this coords
		int refType = 0; bool  refExists = false;
		
		if (offsetX>=mapsizeX || offsetZ>=mapsizeZ) { refType=0; refExists=false; }
		else
		{
			refType = type1; refExists=true;
			if (offsetY>map1[offsetX, offsetZ]) { refType = type2; refExists=true; }
			if (offsetY>map1[offsetX, offsetZ]+map2[offsetX, offsetZ]) { refType = type3; refExists=true; }
			if (offsetY>map1[offsetX, offsetZ]+map2[offsetX, offsetZ]+map3[offsetX, offsetZ]) { refType = type4; refExists=true; }
			if (offsetY>map1[offsetX, offsetZ]+map2[offsetX, offsetZ]+map3[offsetX, offsetZ]+map4[offsetX, offsetZ]) { refType = 0; refExists=false; }
		}
		
		//finding if all the blocks in scope of node are the same
		bool  allSame = true;
		
		int curType = 0; bool  curExists = false;
		if (halfsize != 0)
			for (int x=offsetX; x<offsetX+halfsize*2; x++) 
				for (int y=offsetY; y<offsetY+halfsize*2; y++)
					for (int z=offsetZ; z<offsetZ+halfsize*2; z++)
				{
					if (x>=mapsizeX || z>=mapsizeZ) { curType=0; curExists=false; }
					else
					{
						curType = type1; curExists=true;
						if (y>map1[x,z]) { curType = type2; curExists=true; }
						if (y>map1[x,z]+map2[x,z]) { curType = type3; curExists=true; }
						if (y>map1[x,z]+map2[x,z]+map3[x,z]) { curType = type4; curExists=true; }
						if (y>map1[x,z]+map2[x,z]+map3[x,z]+map4[x,z]) { curType = 0; curExists=false; }
					}
					
					if (curType != refType) 
					{
						allSame = false;
						break;
					}
				}
		
		//if all blocks are same - setting node
		if (allSame)
		{
			exists = refExists;
			type = refType;
			nodes = VoxelandData.nullArray;
		}
		
		//if not the same - iterating in child nodes
		else
		{
			nodes = new VoxelandOctNode[8];
			
			for (int i=0;i<8;i++) nodes[i] = new VoxelandOctNode();
			
			nodes[0].GenerateFromHeightmap( map1, map2, map3, map4, type1, type2, type3, type4, offsetX, offsetY, offsetZ, halfsize/2);
			nodes[1].GenerateFromHeightmap( map1, map2, map3, map4, type1, type2, type3, type4, offsetX, offsetY, offsetZ+halfsize, halfsize/2);
			nodes[2].GenerateFromHeightmap( map1, map2, map3, map4, type1, type2, type3, type4, offsetX, offsetY+halfsize, offsetZ, halfsize/2);
			nodes[3].GenerateFromHeightmap( map1, map2, map3, map4, type1, type2, type3, type4, offsetX, offsetY+halfsize, offsetZ+halfsize, halfsize/2);
			
			nodes[4].GenerateFromHeightmap( map1, map2, map3, map4, type1, type2, type3, type4, offsetX+halfsize, offsetY, offsetZ, halfsize/2);
			nodes[5].GenerateFromHeightmap( map1, map2, map3, map4, type1, type2, type3, type4, offsetX+halfsize, offsetY, offsetZ+halfsize, halfsize/2);
			nodes[6].GenerateFromHeightmap( map1, map2, map3, map4, type1, type2, type3, type4, offsetX+halfsize, offsetY+halfsize, offsetZ, halfsize/2);
			nodes[7].GenerateFromHeightmap( map1, map2, map3, map4, type1, type2, type3, type4, offsetX+halfsize, offsetY+halfsize, offsetZ+halfsize, halfsize/2);
		}
	}
	
	
	public void  GenerateFromOctree ( VoxelandData data ,   int offsetX ,   int offsetY ,   int offsetZ ,   int halfsize , 
	                                 int dataOffsetX ,   int dataOffsetY ,   int dataOffsetZ  ){
		//finding reference type at this coords
		VoxelandOctNode refNode;
		int refType = 0; bool  refExists = false;
		
		refNode = data.GetClosestNode(offsetX+dataOffsetX, offsetY+dataOffsetY, offsetZ+dataOffsetZ); 
		refType=refNode.type; 
		refExists=refNode.exists;
		
		
		//finding if all the blocks in scope of node are the same
		bool  allSame = true;
		
		int curType = 0; bool  curExists = false;
		VoxelandOctNode curNode;
		if (halfsize != 0)
			for (int x=offsetX; x<offsetX+halfsize*2; x++) 
				for (int y=offsetY; y<offsetY+halfsize*2; y++)
					for (int z=offsetZ; z<offsetZ+halfsize*2; z++)
				{
					curNode = data.GetClosestNode(x+dataOffsetX, y+dataOffsetY, z+dataOffsetZ); 
					curType=curNode.type; 
					curExists=curNode.exists;
					
					if (curType != refType) 
					{
						allSame = false;
						break;
					}
				}
		
		//if all blocks are same - setting node
		if (allSame)
		{
			exists = refExists;
			type = refType;
			nodes = VoxelandData.nullArray;
		}
		
		//if not the same - iterating in child nodes
		else
		{
			nodes = new VoxelandOctNode[8];
			
			for (int i=0;i<8;i++) nodes[i] = new VoxelandOctNode();
			
			nodes[0].GenerateFromOctree(data, offsetX, offsetY, offsetZ, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			nodes[1].GenerateFromOctree(data, offsetX, offsetY, offsetZ+halfsize, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			nodes[2].GenerateFromOctree(data, offsetX, offsetY+halfsize, offsetZ, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			nodes[3].GenerateFromOctree(data, offsetX, offsetY+halfsize, offsetZ+halfsize, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			
			nodes[4].GenerateFromOctree(data, offsetX+halfsize, offsetY, offsetZ, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			nodes[5].GenerateFromOctree(data, offsetX+halfsize, offsetY, offsetZ+halfsize, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			nodes[6].GenerateFromOctree(data, offsetX+halfsize, offsetY+halfsize, offsetZ, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			nodes[7].GenerateFromOctree(data, offsetX+halfsize, offsetY+halfsize, offsetZ+halfsize, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
		}
	}
	
	public void  GenerateFromTwoOctrees ( VoxelandData data ,   VoxelandData backgroundData ,   int offsetX ,   int offsetY ,   int offsetZ ,   int halfsize , 
	                                     int dataOffsetX ,   int dataOffsetY ,   int dataOffsetZ  ){
		//finding reference type at this coords
		VoxelandOctNode refNode;
		int refType = 0; bool  refExists = false;
		
		if (offsetX+dataOffsetX<0 || offsetX+dataOffsetX>=data.sizeX-1 ||
		    offsetY+dataOffsetY<0 || offsetY+dataOffsetY>=data.sizeY-1 ||
		    offsetZ+dataOffsetZ<0 || offsetZ+dataOffsetZ>=data.sizeZ-1) refNode = backgroundData.GetClosestNode(offsetX, offsetY, offsetZ);
		else refNode = data.GetNode(offsetX+dataOffsetX, offsetY+dataOffsetY, offsetZ+dataOffsetZ);
		refType=refNode.type; 
		refExists=refNode.exists;
		
		
		//finding if all the blocks in scope of node are the same
		bool  allSame = true;
		
		int curType = 0; bool  curExists = false;
		VoxelandOctNode curNode;
		if (halfsize != 0)
			for (int x=offsetX; x<offsetX+halfsize*2; x++) 
				for (int y=offsetY; y<offsetY+halfsize*2; y++)
					for (int z=offsetZ; z<offsetZ+halfsize*2; z++)
				{
					if (x+dataOffsetX<0 || x+dataOffsetX>=data.sizeX-1 ||
					    y+dataOffsetY<0 || y+dataOffsetY>=data.sizeY-1 ||
					    z+dataOffsetZ<0 || z+dataOffsetZ>=data.sizeZ-1) curNode = backgroundData.GetClosestNode(x, y, z); 
					else curNode = data.GetNode(x+dataOffsetX, y+dataOffsetY, z+dataOffsetZ);
					curType=curNode.type; 
					curExists=curNode.exists;
					
					if (curType != refType) 
					{
						allSame = false;
						break;
					}
				}
		
		//if all blocks are same - setting node
		if (allSame)
		{
			exists = refExists;
			type = refType;
			nodes = VoxelandData.nullArray;
		}
		
		//if not the same - iterating in child nodes
		else
		{
			nodes = new VoxelandOctNode[8];
			
			for (int i=0;i<8;i++) nodes[i] = new VoxelandOctNode();
			
			nodes[0].GenerateFromTwoOctrees(data, backgroundData, offsetX, offsetY, offsetZ, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			nodes[1].GenerateFromTwoOctrees(data, backgroundData, offsetX, offsetY, offsetZ+halfsize, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			nodes[2].GenerateFromTwoOctrees(data, backgroundData, offsetX, offsetY+halfsize, offsetZ, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			nodes[3].GenerateFromTwoOctrees(data, backgroundData, offsetX, offsetY+halfsize, offsetZ+halfsize, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			
			nodes[4].GenerateFromTwoOctrees(data, backgroundData, offsetX+halfsize, offsetY, offsetZ, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			nodes[5].GenerateFromTwoOctrees(data, backgroundData, offsetX+halfsize, offsetY, offsetZ+halfsize, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			nodes[6].GenerateFromTwoOctrees(data, backgroundData, offsetX+halfsize, offsetY+halfsize, offsetZ, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
			nodes[7].GenerateFromTwoOctrees(data, backgroundData, offsetX+halfsize, offsetY+halfsize, offsetZ+halfsize, halfsize/2, dataOffsetX,dataOffsetY,dataOffsetZ);
		}
	}
	
	
	public void  GenerateFromArray ( int[] blocksArray ,   bool[] existsArray, 
	                                int offsetX ,   int offsetY ,   int offsetZ ,   int halfsize , 
	                                int maxX ,   int maxY ,   int maxZ  )
	{
		int endX = Mathf.Min(maxX, offsetX+halfsize*2);
		int endY = Mathf.Min(maxY, offsetY+halfsize*2);
		int endZ = Mathf.Min(maxZ, offsetZ+halfsize*2);
		
		//finding if all the blocks in scope of node are the same
		int curType = 0; bool  curExists = false;
		
		int pos = offsetZ*maxY*maxX + offsetY*maxX + offsetX;
		if (pos < blocksArray.Length) 
		{
			curType = blocksArray[pos];
			curExists = existsArray[pos];
		}
		
		bool  allSame = true;
		
		if (halfsize != 0)
			for (int x=offsetX; x<endX; x++) 
				for (int y=offsetY; y<endY; y++)
					for (int z=offsetZ; z<endZ; z++)
				{
					if (blocksArray[ z*maxY*maxX + y*maxX + x ] != curType || existsArray[ z*maxY*maxX + y*maxX + x ] != curExists)
					{
						allSame = false;
						break;
					}
				}
		
		//if all blocks are same - setting node
		if (allSame)
		{
			exists = curExists;
			type = curType;
			nodes = VoxelandData.nullArray;
		}
		
		//if not the same - iterating in child nodes
		else
		{
			nodes = new VoxelandOctNode[8];
			
			for (int i=0;i<8;i++) nodes[i] = new VoxelandOctNode();
			
			nodes[0].GenerateFromArray( blocksArray,existsArray, offsetX, offsetY, offsetZ, halfsize/2, maxX,maxY,maxZ);
			nodes[1].GenerateFromArray( blocksArray,existsArray, offsetX, offsetY, offsetZ+halfsize, halfsize/2, maxX,maxY,maxZ);
			nodes[2].GenerateFromArray( blocksArray,existsArray, offsetX, offsetY+halfsize, offsetZ, halfsize/2, maxX,maxY,maxZ);
			nodes[3].GenerateFromArray( blocksArray,existsArray, offsetX, offsetY+halfsize, offsetZ+halfsize, halfsize/2, maxX,maxY,maxZ);
			
			nodes[4].GenerateFromArray( blocksArray,existsArray, offsetX+halfsize, offsetY, offsetZ, halfsize/2, maxX,maxY,maxZ);
			nodes[5].GenerateFromArray( blocksArray,existsArray, offsetX+halfsize, offsetY, offsetZ+halfsize, halfsize/2, maxX,maxY,maxZ);
			nodes[6].GenerateFromArray( blocksArray,existsArray, offsetX+halfsize, offsetY+halfsize, offsetZ, halfsize/2, maxX,maxY,maxZ);
			nodes[7].GenerateFromArray( blocksArray,existsArray, offsetX+halfsize, offsetY+halfsize, offsetZ+halfsize, halfsize/2, maxX,maxY,maxZ);
			

		}
	}
	*/
	
	public void SaveToIntList (System.Collections.Generic.List<int> list)
	{
		if (nodes==null || nodes.Length==0) list.Add(type); //if solid node
		else 
		{
			list.Add(-1);
			for (int n=0; n<8; n++) nodes[n].SaveToIntList(list);
		}
	}
	
	public void LoadFromIntList (System.Collections.Generic.List<int> list, bool[] arrayOfTypes)
	{
		if (list[0] != -1) //if solid node
		{
			type = list[0];
			exists = arrayOfTypes[type];
			list.RemoveAt(0);
		}
		else
		{
			list.RemoveAt(0);
			
			nodes = new VoxelandOctNode[8];
			for (int n=0;n<8;n++) 
			{
				nodes[n] = new VoxelandOctNode();
				nodes[n].LoadFromIntList(list, arrayOfTypes);
			}
		}
	}
	
	public VoxelandOctNode Copy ()
	{
		VoxelandOctNode result = new VoxelandOctNode();
		
		result.exists = exists;
		result.type = type;
		
		if (nodes!=null && nodes.Length!=0) 
		{
			result.nodes = new VoxelandOctNode[8];
			for (int n=0; n<8; n++) result.nodes[n] = nodes[n].Copy();
		}
		
		return result;
	}
	
}

[System.Serializable]
public class ChunkData
{
	public int size;
	public VoxelandOctNode[] nodes;
	
	public VoxelandOctNode GetNode (int x, int y, int z)  //returns the node at given coordinates
	{
		return nodes[y/size].GetNode(x, y-(y/size)*size, z, size/2);
	}
	
	public byte GetBlock (int x, int y, int z)
	{
		return (byte)GetNode(x,y,z).type;
	}
	public void  SetBlock (int x, int y, int z, int type, bool filled)
	{ 
		VoxelandOctNode topNode = nodes[y/size];
		VoxelandOctNode node = topNode.SetNode(x, y-(y/size)*size, z, size/2);
	
		node.exists = filled;
		node.type = type;
		
		//merging nodes if possible
		topNode.Collapse();
	}
	
	public bool Exists (int x, int y, int z)
	{ 
		return GetNode(x,y,z).exists;
	}
	
	public int GetHeight (int x, int z)
	{
		for (int i = nodes.Length-1; i>=0; i--)
		{
			int result = nodes[i].GetHeight(x, z, size/2);
			if (result >= 0) return result + i*size;
		}
		return 0;
	}
	
	public int GetTopPoint ()
	{

		return nodes.Length * size -1;
	}
	
	public int GetBottomPoint ()
	{
		return 0;
	}
	/*
	public void RefreshExistMatrix ()
	{
		if (Chunk.existMatrix==null) Chunk.existMatrix = new bool[size * (size*nodes.Length) * size];

		for (int i = 0; i<nodes.Length; i++)
		{
			nodes[i].GetExistMatrix(
				Chunk.existMatrix, size, size*nodes.Length, size, 
				0, size*i, 0, 
				size, size*(i+1), size, 
				size/2);
		}
	}
	*/
	public void  Visualize (int offsetX, int offsetZ, bool node, bool fill)
	{
		if (nodes==null || nodes.Length==0) return; 
		
		for (int i=0; i<nodes.Length; i++) 
			nodes[i].Visualize(offsetX, i*size, offsetZ, size/2, node, fill);
	}
	
	public void UpgradeFromData (VoxelandData data, int sz, int offsetX, int offsetZ)
	{
		size = sz;
		nodes = new VoxelandOctNode[data.sizeY/size+1];
		for (int i=0; i<nodes.Length; i++) nodes[i] = new VoxelandOctNode();
		
		for (int y=0; y<data.sizeY; y++)
			for (int x=0; x<size; x++)
				for (int z=0; z<size; z++)
		{
			VoxelandOctNode node = data.GetClosestNode(x+offsetX, y, z+offsetZ);
			SetBlock(x,y,z, node.type, node.exists);
		}
	}

}


public class VoxelandData : ScriptableObject
{
	//int[ matrix,,];
	//float[ ambientMatrix,,];
	public enum VoxelandDataGenerateBlend {additive=0, replace=1}; 
	
	static public VoxelandOctNode[] nullArray = new VoxelandOctNode[0];
	
	[HideInInspector] public  int[] blocks;
	[HideInInspector] public float[] ambient;
	[HideInInspector] public bool[] exists;
	
	[HideInInspector] public VoxelandOctNode[] octree;
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
	
	[HideInInspector] public bool[ ] guiGenerateExtend = new bool[ 10];
	[HideInInspector] public bool[ ] guiGenerateCheck = {true, true, true, true, true, true, true, true, true, true};
	
	[HideInInspector] public int genPlanarType = 1;
	[HideInInspector] public int genPlanarLevel = 1;
	
	[HideInInspector] public int genNoiseType = 1;
	[HideInInspector] public bool  genNoiseAdditive;
	[HideInInspector] public int genNoiseFractals = 6;
	[HideInInspector] public float genNoiseFractalMin = 3; 
	[HideInInspector] public float genNoiseFractalMax = 100; 
	[HideInInspector] public float genNoiseValueMin = 5;
	[HideInInspector] public float genNoiseValueMax = 50;
	[HideInInspector] public int genErosionGulliesIterations = 40;
	[HideInInspector] public float genErosionGulliesMudAmount = 0.02f;
	[HideInInspector] public float genErosionGulliesBlurValue = 0.1f;
	[HideInInspector] public int genErosionGulliesDeblur = 5;
	[HideInInspector] public float genErosionGulliesWind = 1.5f;
	[HideInInspector] public int genErosionSedimentType = 2;
	[HideInInspector] public int genErosionSedimentBlurIterations = 500;
	[HideInInspector] public int genErosionSedimentHollowIterations = 50;
	[HideInInspector] public float genErosionSedimentAdjustLevel = -2;
	[HideInInspector] public float genErosionSedimentDepth = 1.5f;
	[HideInInspector] public int genCoverType = 3;
	[HideInInspector] public int genCoverBlurIterations = 30;
	[HideInInspector] public float genCoverPlanarDelta = 0.7f;
	[HideInInspector] public float genCoverPikesDelta = 0.018f;
	
	Material tempMat;
/*	
	private System.Collections.Generic.List<VoxelandUndoNodes> undoList = new System.Collections.Generic.List<VoxelandUndoNodes>();
	
	public class VoxelandUndoNodes
	{
		public System.Collections.Generic.List<int> nums = new System.Collections.Generic.List<int>();
		public System.Collections.Generic.List<VoxelandOctNode> nodes = new System.Collections.Generic.List<VoxelandOctNode>();
	}
	
	public void  RegisterUndo ()
	{
		undoList.Add( new VoxelandUndoNodes() );
		if (undoList.Count > 10) undoList.RemoveAt(0);
	}
	
	public void  AddUndoNode ( int num  )
	{
		VoxelandUndoNodes nodesList= undoList[undoList.Count-1];
		for (int i=0;i<nodesList.nodes.Count;i++) 
			if (nodesList.nums[i] == num) return;
		
		nodesList.nums.Add(num);
		nodesList.nodes.Add( octree[num].Copy() );
	}
	
	public void  PerformUndo (){
		if (undoList.Count==0) return;
		
		VoxelandUndoNodes nodesList= undoList[undoList.Count-1];
		
		for (int i=0;i<nodesList.nodes.Count;i++)
		{
			octree[ nodesList.nums[i] ] = nodesList.nodes[i];
		}
		
		undoList.RemoveAt(undoList.Count-1);
	}
	
	public void  Upgrade (){
		if (blocks.Length==0 || exists.Length==0) { Debug.Log("Voxeland: No outdated data found. Possibly already upgraded"); return; }
		
		Debug.Log("Voxeland: Upgrading terrain data to new octree format. If something went wrong DO NOT save your project and make a backup of your data file.");
		
		nodesX = Mathf.CeilToInt(1.0f*sizeX/biggestNode);
		nodesY = Mathf.CeilToInt(1.0f*sizeY/biggestNode);
		nodesZ = Mathf.CeilToInt(1.0f*sizeZ/biggestNode);
		
		octree = new VoxelandOctNode[ nodesX * nodesY * nodesZ ];
		
		for (int x=0; x<nodesX; x++) 
			for (int y=0; y<nodesY; y++)
				for (int z=0; z<nodesZ; z++) 
			{
				int i = z*nodesY*nodesX + y*nodesX + x;
				
				octree[i] = new VoxelandOctNode();
				octree[i].GenerateFromArray(blocks,exists,
				                            x*biggestNode, y*biggestNode, z*biggestNode, biggestNode/2, //offset in blocks
				                            sizeX, sizeY, sizeZ); //block array dimensions
			}
		
		blocks = new int[0];
		ambient = new float[0];
		exists = new bool[ 0];
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
		#endif
	}
*/	
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
	
	public bool Exists ( int x ,   int y ,   int z  ){ 
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
	
	public int GetTopPoint ( int sx ,   int sz ,   int ex ,   int ez  ){
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
	}
	
	public int GetBottomPoint ( int sx ,   int sz ,   int ex ,   int ez  ){
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
	/*
void  Blur ( int sx ,   int sy ,   int sz ,   int ex ,   int ey ,   int ez ,   bool sphere  ){
	 bool[ ] matrix = GetExistMatrix(sx, sy, sz, ex, ey, ez);
	int matrixSizeX = ex-sx;
	int matrixSizeY = ey-sy;
	int matrixSizeZ = ez-sz;
	
	
	for (int x = 1; x<=matrixSizeX-1; x++)
		for (int y = 1; y<=matrixSizeY-1; y++)
			for (int z = 1; z<=matrixSizeZ-1; z++)
	{
		int i = z*matrixSizeY*matrixSizeX + y*matrixSizeX + x;
		int numExistAround;
		
		if (matrix[i+1]) numExistAround++;
		if (matrix[i-1]) numExistAround++;
		if (matrix[i+matrixSizeX]) numExistAround++;
		if (matrix[i-matrixSizeY]) numExistAround++;
		if (matrix[i+matrixSizeY*matrixSizeX]) numExistAround++;
		if (matrix[i-matrixSizeY*matrixSizeX]) numExistAround++;
		
		//if exists
		if (matrix[i])
		{
			if (numExistAround <= 2 && 
				sx+x >= 0 && sx+x < sizeX &&
				sy+y >= 0 && sy+y < sizeY &&
				sz+z >= 0 && sz+z < sizeZ ) 
					SetBlock(sx+x, sy+y, sz+z, 0, false);
		}
	}		
}
*/
	
	
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
		GeneratePlanar(planarMap, filledLevel);
		
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
	
	public void  Resize ( int newX ,   int newY ,   int newZ ,   int newNode  )
	{
		int nX= Mathf.CeilToInt(1.0f*newX/newNode);
		int nY= Mathf.CeilToInt(1.0f*newY/newNode);
		int nZ= Mathf.CeilToInt(1.0f*newZ/newNode);
		
		VoxelandOctNode[] newOctree= new VoxelandOctNode[ nX * nY * nZ ];
		
		for (int x=0; x<nX; x++) 
			for (int y=0; y<nY; y++)
				for (int z=0; z<nZ; z++) 
			{
				int i = z*nY*nX + y*nX + x;
				
				newOctree[i] = new VoxelandOctNode();
				//newOctree[i].GenerateFromOctree(this, x*newNode, y*newNode, z*newNode, newNode/2, 0,0,0);
			}
		
		octree = newOctree;
		sizeX = newX; sizeY = newY; sizeZ = newZ;
		biggestNode = newNode;
		nodesX = nX; nodesY = nY; nodesZ = nZ;
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
		#endif
	}
	
	public void  Offset ( int stepX ,   int stepY ,   int stepZ  ){
		VoxelandOctNode[] newOctree= new VoxelandOctNode[ nodesX * nodesY * nodesZ ];
		
		for (int x=0; x<nodesX; x++) 
			for (int y=0; y<nodesY; y++)
				for (int z=0; z<nodesZ; z++) 
			{
				int i = z*nodesY*nodesX + y*nodesX + x;
				
				newOctree[i] = new VoxelandOctNode();
				//newOctree[i].GenerateFromOctree(this, x*biggestNode, y*biggestNode, z*biggestNode, biggestNode/2, -stepX, -stepY, -stepZ);
			}
		
		octree = newOctree;
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
		#endif
	}
	
	public void  Insert ( VoxelandData data ,   int stepX ,   int stepY ,   int stepZ  ){
		VoxelandOctNode[] newOctree= new VoxelandOctNode[ nodesX * nodesY * nodesZ ];
		
		for (int x=0; x<nodesX; x++) 
			for (int y=0; y<nodesY; y++)
				for (int z=0; z<nodesZ; z++) 
			{
				int i = z*nodesY*nodesX + y*nodesX + x;
				
				newOctree[i] = new VoxelandOctNode();
				//newOctree[i].GenerateFromTwoOctrees(data, this, x*biggestNode, y*biggestNode, z*biggestNode, biggestNode/2, -stepX, -stepY, -stepZ);
			}
		
		octree = newOctree;
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
		#endif
	}
	
	static public VoxelandData New ( int newX ,   int newY ,   int newZ ,   int newNode ,   int filledLevel  ){
		#if UNITY_EDITOR
		string path= UnityEditor.EditorUtility.SaveFilePanel(
			"Save Voxeland Data",
			"Assets",
			"NewTerrainData.asset",
			"asset");
		if (path==null) return null;
		path = path.Replace(Application.dataPath, "Assets");
		
		VoxelandData data = ScriptableObject.CreateInstance<VoxelandData>();
		data.Clear(newX, newY, newZ, newNode, filledLevel);
		UnityEditor.AssetDatabase.CreateAsset (data, path);
		
		return data;
		#else
		return null;
		#endif
	}
	
	public void  LoadFromText (string from, bool[] existsArray)
	{  
		#if UNITY_EDITOR
		if(!System.IO.File.Exists(from)) return;
		
		System.IO.StreamReader reader= System.IO.File.OpenText(from); 
		
		//calc exists array
		nodesX = System.Int32.Parse(reader.ReadLine()); 
		nodesY = System.Int32.Parse(reader.ReadLine()); 
		nodesZ = System.Int32.Parse(reader.ReadLine());
		biggestNode = System.Int32.Parse(reader.ReadLine());
		
		sizeX = nodesX*biggestNode;
		sizeY = nodesY*biggestNode;
		sizeZ = nodesZ*biggestNode;
		
		newSizeX = sizeX;
		newSizeY = sizeY;
		newSizeZ = sizeZ;
		newBiggestNode = biggestNode;
		
		octree = new VoxelandOctNode[ nodesX * nodesY * nodesZ ];
		
		//for (int x=0; x<nodesX; x++) 
		//	for (int y=0; y<nodesY; y++)
		//		for (int z=0; z<nodesZ; z++) 
		for (int i=0; i<octree.Length; i++) 
		{
			//int i = z*nodesY*nodesX + y*nodesX + x;
			
			octree[i] = new VoxelandOctNode();
			
			System.Collections.Generic.List<int> nodeIntList = new System.Collections.Generic.List<int>();
			
			string nodeListString = reader.ReadLine();
			for (int c=0; c<nodeListString.Length; c++) nodeIntList.Add( int.Parse(nodeListString[c].ToString())-1 );
			
			octree[i].LoadFromIntList(nodeIntList, existsArray);
		}
		
		reader.Close(); 
		
		UnityEditor.EditorUtility.SetDirty(this);
		#endif
	}
	
	
	public void  SaveToText ( string to  )
	{
		/*
		#if UNITY_EDITOR
		System.IO.StreamWriter writer= System.IO.File.CreateText(to); 
		
		//setting dimensions
		writer.WriteLine(nodesX);
		writer.WriteLine(nodesY);
		writer.WriteLine(nodesZ);
		writer.WriteLine(biggestNode);
		
		for (int n=0; n<octree.Length; n++)
		{
			System.Collections.Generic.List<int> nodeIntList = new System.Collections.Generic.List<int>();
			octree[n].SaveToIntList(nodeIntList);
			
			string nodeString = "";
			for (int i=0; i<nodeIntList.Count; i++) 
			{ char ch = (char)(nodeIntList[i]+1); nodeString += ch; }
			//nodeString += Mathf.Min(10, nodeIntList[i]+1).ToString();
			
			writer.WriteLine(nodeString);
		}
		
		//for (int i=0; i<target.blocks.Length; i++) writer.WriteLine (target.blocks[i]);
		
		writer.Close(); 
		#endif
		*/
	}
	
	
	public float[,] GetHeighmap ()
	{
		float[,] map = new float[sizeX, sizeZ];
		for (int x=0; x<sizeX; x++)
			for (int z=0; z<sizeZ; z++)
				for (int y=0; y<sizeY; y++)
					if (Exists(x,y,z)) map[x,z] = y;
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
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
		#endif
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
	
	
	//land generation algorithms
	//takes map as ref and changes it
	public void  GeneratePlanar ( float[  , ] map,  int level  ){
		int mapSizeX = map.GetLength(0);
		int mapSizeZ = map.GetLength(1);
		
		for (int x=0; x<mapSizeX; x++)
			for (int z=0; z<mapSizeZ; z++)
				map[x,z] = level;
	}
	
	public void  GenerateFromTexture ( float[,] map)
	{
		
	}
	
	public void  GenerateNoise ( float[,] map,  int fractals,   float fractalMin ,   float fractalMax ,   float valueMin ,   float valueMax  )
	{
		int mapSizeX = map.GetLength(0);
		int mapSizeZ = map.GetLength(1);
		
		float fractalStep = (fractalMax-fractalMin) / fractals;
		float valueStep = (valueMax-valueMin) / fractals;
		
		for (int i = fractals-1; i>=0; i--)
		{
			float curFractal = fractalMin + fractalStep*i;
			float curValue = valueMin + valueStep*i;
			float offset = Random.value * curFractal;
			
			for (int x=0; x<mapSizeX; x++)
				for (int z=0; z<mapSizeZ; z++)
			{
				map[x,z] += Mathf.PerlinNoise(x/curFractal + offset, z/curFractal + offset) * curValue;
				//int t=0;
			}
		}
	}
	
	public void  GenerateErosionGullies (float[,] map, float[,] refHeights, int iterations, float mudAmount, float blurValue, int deblurIterations)
	{
		int x=0; int z=0;
		//note that trhe map we generate is substracted
		
		int mapSizeX = map.GetLength(0);
		int mapSizeZ = map.GetLength(1);
		
		//baking refheight to map and getting top point
		float topPoint = 0;
		float bottomPoint = Mathf.Infinity;
		for (x=0; x<mapSizeX; x++)
			for (z=0; z<mapSizeZ; z++)
		{
			map[x,z] = refHeights[x,z];
			if (refHeights[x,z] > topPoint) topPoint = refHeights[x,z];
			if (refHeights[x,z] < bottomPoint) bottomPoint = refHeights[x,z];
		}
		
		//iterating
		for (int iteration=0; iteration<iterations; iteration++)
		{
			//calculating water flow vectors
			Vector4[,] flowDirections = new Vector4[mapSizeX, mapSizeZ]; //X and Y are x-flow direction. Z and W are z-flow disrection. Y and W are nagative
			float[,] flowStrength = new float[mapSizeX,mapSizeZ];
			float[,] maxMud = new float[mapSizeX,mapSizeZ];
			Vector4 flow;
			for (x=1; x<mapSizeX-1; x++)
				for (z=1; z<mapSizeZ-1; z++)
			{
				//flow direction
				flow = new Vector4(
					Mathf.Max(map[x-1,z]-map[x+1,z], 0),
					Mathf.Max(map[x+1,z]-map[x-1,z], 0),
					Mathf.Max(map[x,z-1]-map[x,z+1], 0),
					Mathf.Max(map[x,z+1]-map[x,z-1], 0));
				float flowSum = flow.x + flow.y + flow.z + flow.w;
				
				if (flowSum > 0.1f) flowDirections[x,z] = flow / flowSum;
				else flowDirections[x,z] = new Vector4(0.25f, 0.25f, 0.25f, 0.25f);
				
				//flow strength
				float flowMax = Mathf.Max (map[x,z]-map[x-1,z], map[x,z]-map[x+1,z]);
				flowMax = Mathf.Max (map[x,z]-map[x,z-1], flowMax);
				flowMax = Mathf.Max (map[x,z]-map[x,z+1], flowMax);
				
				flowStrength[x,z] = flowMax;
				
				//max mud
				maxMud[x,z] = Mathf.Max((map[x-1,z] + map[x+1,z] + map[x,z-1] + map[x,z+1]) * 0.25f - map[x,z], 0) * 0.5f;
				
				//maxMud[x,z] = Mathf.
			}
			
			
			float[,] waterCur = new float[mapSizeX,mapSizeZ];
			float[,] waterSum = new float[mapSizeX,mapSizeZ];
			
			//cast rain and creating int height
			int[] intHeight = new int[mapSizeX * mapSizeZ];
			for (x=0; x<mapSizeX; x++)
				for (z=0; z<mapSizeZ; z++)
			{
				waterCur[x,z] = 1;
				intHeight[z*sizeX + x] = (int)(map[x,z]*4);
			}
			
			//torrents flow
			for (int yf=(int)(topPoint*4); yf>=bottomPoint-5; yf-=1)
				for (z=1; z<mapSizeZ-1; z++)
			{
				int zcoord = z*sizeX;
				for (x=1; x<mapSizeX-1; x++)	
				{
					if (intHeight[zcoord + x] != yf) continue;
					
					flow = flowDirections[x,z];
					float wCur= waterCur[x,z];
					
					waterCur[x+1,z] += wCur * flow.x;
					waterCur[x-1,z] += wCur * flow.y;				
					waterCur[x,z+1] += wCur * flow.z;
					waterCur[x,z-1] += wCur * flow.w;
					
					waterSum[x,z] += waterCur[x,z];
					waterCur[x,z] = 0;
				}
			}
			
			//raising mud		
			for (x=0; x<mapSizeX; x++)
				for (z=0; z<mapSizeZ; z++)
			{
				map[x,z] -= Mathf.Clamp(waterSum[x,z],0,100) * flowStrength[x,z] * mudAmount;
			}
			
			//blurring
			for (x=1; x<mapSizeX-1; x++)
				for (z=1; z<mapSizeZ-1; z++)
					map[x,z] = map[x,z]*(1-blurValue) + map[x+1,z]*blurValue*0.25f + map[x-1,z]*blurValue*0.25f + map[x,z-1]*blurValue*0.25f + map[x,z+1]*blurValue*0.25f;
		}
		
		//extracting map
		for (x=0; x<mapSizeX; x++)
			for (z=0; z<mapSizeZ; z++)
				map[x,z] = Mathf.Max(0, refHeights[x,z] - map[x,z]);
		/*
	//blurring
	for (iteration=0; iteration<deblurIterations; iteration++)
		for (x=1; x<mapSizeX-1; x++)
			for (z=1; z<mapSizeZ-1; z++)
	{
		map[x,z] = map[x,z]/2 + map[x+1,z]*0.125f + map[x-1,z]*0.125f + map[x,z-1]*0.125f + map[x,z+1]*0.125f;
	}
	*/
		
		//wind erosion
		for (x=0; x<mapSizeX; x++)
			for (z=0; z<mapSizeZ; z++)
		{
			map[x,z] += Random.value*genErosionGulliesWind;
		}
	}
	
	
	public void  GenerateSediment ( float[  , ] map,  float[  , ] refHeights,  int blurIterations ,    int hollowIterations ,   float deep  )
	{
		int x=0; int z=0;
		
		int mapSizeX = map.GetLength(0);
		int mapSizeZ = map.GetLength(1);
		
		//baking refheight to map and getting top point
		for (x=0; x<mapSizeX; x++)
			for (z=0; z<mapSizeZ; z++)
		{
			map[x,z] = refHeights[x,z] + genErosionSedimentAdjustLevel;
		}
		
		//blurring
		for (int iteration=0; iteration<blurIterations; iteration++)
			for (x=1; x<mapSizeX-1; x++)
				for (z=1; z<mapSizeZ-1; z++)
			{
				map[x,z] = Mathf.Max(0, map[x,z]/2 + map[x+1,z]*0.125f + map[x-1,z]*0.125f + map[x,z-1]*0.125f + map[x,z+1]*0.125f);
			}
		
		
		//extracting map
		for (x=0; x<mapSizeX; x++)
			for (z=0; z<mapSizeZ; z++)
		{
			map[x,z] = Mathf.Max(0, map[x,z] - refHeights[x,z])*deep;
			//refHeights[x,z] -= map[x,z];
		}
		
	}
	
	public void  GenerateCover ( float[  , ] map,  float[  , ] refHeights,  float blurIterations ,   float normalDelta ,   float pikesDelta  )
	{
		int x=0; int z=0;
		
		int mapSizeX = map.GetLength(0);
		int mapSizeZ = map.GetLength(1);
		
		//creating hollows arrays
		float[,] hollows = new float[mapSizeX, mapSizeZ];
		float[,] normals = new float[mapSizeX, mapSizeZ];
		for (x=1; x<mapSizeX-1; x++)
			for (z=1; z<mapSizeZ-1; z++)
		{
			hollows[x,z] = (refHeights[x-1,z]*0.25f + refHeights[x+1,z]*0.25f + refHeights[x,z-1]*0.25f + refHeights[x,z+1]*0.25f) - refHeights[x,z];
			normals[x,z] = 
				Mathf.Abs( (map[x-1,z]+refHeights[x-1,z]) - (map[x+1,z]+refHeights[x+1,z]) ) +
					Mathf.Abs( (map[x,z-1]+refHeights[x,z-1]) - (map[x,z+1]+refHeights[x,z+1]) );
		}	
		
		//blurring hollows
		for (int iteration=0; iteration<blurIterations; iteration++)
			for (x=1; x<mapSizeX-1; x++)
				for (z=1; z<mapSizeZ-1; z++)
			{
				hollows[x,z] = hollows[x,z]*0.33f + hollows[x+1,z]*0.1675f + hollows[x-1,z]*0.1675f + hollows[x,z-1]*0.1675f + hollows[x,z+1]*0.1675f;
				normals[x,z] = normals[x,z]*0.33f + normals[x+1,z]*0.1675f + normals[x-1,z]*0.1675f + normals[x,z-1]*0.1675f + normals[x,z+1]*0.1675f;
			}
		
		//adding grass depending on normal
		for (x=0; x<mapSizeX; x++)
			for (z=0; z<mapSizeZ; z++)
		{
			//if ((1/normals[x,z]-normalDelta) + (hollows[x,z]-pikesDelta) > 0) map[x,z]+=1;
			if (1/normals[x,z]-normalDelta > 0) map[x,z]+=1;
			if (hollows[x,z]-pikesDelta > 0) map[x,z]+=1;
			map[x,z] = Mathf.Clamp01(map[x,z]);
		}
		
		//HeightmapToTexture(map, 1);
	}
	
	
	public void  GenerateAll ()
	{
		//int x=0; int z=0;
		
		float[,] emptyMap= new float[newSizeX, newSizeZ];
		
		//progress bar data
		float progress = 0;
		float totalProgress = 0;
		if (guiGenerateCheck[1]) totalProgress += 1;
		if (guiGenerateCheck[2]) totalProgress += 2;
		if (guiGenerateCheck[3]) totalProgress += 5;
		if (guiGenerateCheck[4]) totalProgress += 5;
		if (guiGenerateCheck[5]) totalProgress += 2;
		
		//generating
		float[,] planarMap;
		if (guiGenerateCheck[1]) 
		{
			DisplayProgress("Planar", 0);
			progress += 1;
			
			planarMap = new float[newSizeX, newSizeZ];
			GeneratePlanar(planarMap, genPlanarLevel);
		}
		else planarMap = emptyMap;
		
		float[,] noiseMap;
		if (guiGenerateCheck[2]) 
		{
			DisplayProgress("Noise", progress/totalProgress);
			progress += 2;
			
			noiseMap = new float[newSizeX, newSizeZ];
			GenerateNoise(noiseMap, genNoiseFractals, genNoiseFractalMin, genNoiseFractalMax, genNoiseValueMin, genNoiseValueMax);
		}
		else noiseMap = emptyMap;
		
		if (guiGenerateCheck[3]) 
		{
			DisplayProgress("Erosion Gullies", progress/totalProgress);
			progress += 5;
			
			float[,] erosionMap= new float[newSizeX, newSizeZ];
			GenerateErosionGullies(erosionMap, SumHeightmaps(planarMap, noiseMap, emptyMap, emptyMap), 
			                       genErosionGulliesIterations, genErosionGulliesMudAmount, genErosionGulliesBlurValue, genErosionGulliesDeblur);
			SubstractHeightmap(planarMap, noiseMap, emptyMap, emptyMap, erosionMap, 1);
		}
		
		float[,] sedimentMap;
		if (guiGenerateCheck[4]) 
		{
			DisplayProgress("Erosion Sediment", progress/totalProgress);
			progress += 3;
			
			sedimentMap = new float[newSizeX, newSizeZ];
			GenerateSediment(sedimentMap, SumHeightmaps(planarMap, noiseMap, emptyMap, emptyMap), genErosionSedimentBlurIterations, genErosionSedimentHollowIterations, genErosionSedimentDepth);
			SubstractHeightmap(planarMap, noiseMap, emptyMap, emptyMap, sedimentMap, 0.5f);
		}
		else sedimentMap = emptyMap;
		
		float[,] coverMap;
		if (guiGenerateCheck[5]) 
		{
			DisplayProgress("Cover", progress/totalProgress);
			progress += 2;
			
			coverMap = new float[newSizeX, newSizeZ];
			GenerateCover(coverMap, SumHeightmaps(planarMap, noiseMap, sedimentMap, emptyMap), genCoverBlurIterations, genCoverPlanarDelta, genCoverPikesDelta);
			SubstractHeightmap(planarMap, noiseMap, sedimentMap, emptyMap, coverMap, 1);
		}
		else coverMap = emptyMap;
		
		DisplayProgress("Setting Heightmap", 0.99f);
		
		SetHeightmap(planarMap, noiseMap, sedimentMap, coverMap, 
		             genPlanarType, genNoiseType, genErosionSedimentType, genCoverType, 
		             newSizeY, newBiggestNode);
		
		//setting new size
		sizeX = newSizeX;
		sizeY = newSizeY;
		sizeZ = newSizeZ;
		biggestNode = newBiggestNode;
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.ClearProgressBar();
		#endif
		/*
	bool[ ] guiGenerateCheck = new bool[ 5];

[HideInInspector] int genPlanarType = 1;
[HideInInspector] int genPlanarLevel = 1;

[HideInInspector] int genNoiseType = 1;
[HideInInspector] VoxelandDataGenerateBlend genNoiseBlend;
[HideInInspector] int genNoiseLevel = 1;
*/
	}	
	
	public void  DisplayProgress ( string info ,   float progress  )
	{
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.DisplayProgressBar("Generate Land... Please wait", info, 1);
		UnityEditor.EditorUtility.DisplayProgressBar("Generate Land... Please wait", info, progress);
		#endif
	}

	
}

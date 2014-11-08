
using UnityEngine;
using System.Collections.Generic;

#region Processing order scheme
//											Calc Ambient x.1
//												|
// Calc Mesh 1.y --	Compile Mesh 2.y	--	Compile Ambient 3.2
//						|					
// 					Build Grass					
//
// Build Prefabs
#endregion

namespace Voxeland {

public class Chunk : MonoBehaviour 
{
	#region Vert, Face, Block
	
	public class VoxelandVert
	{
		public Vector3 pos;
		public Vector3 relaxed;
		public Vector3 normal;
		
		public bool  processed;
		
		public float relax = 0; //1f;
		public byte relaxCount = 0;
		
		public VoxelandVert[] neigs; //max 6 - or no 7 somehow...
		public byte neigCount;
		
		public VoxelandFace[] faces = new VoxelandFace[6];
		
		public ushort num; public ushort num_lo; //for welded mesh compile
		public int coords = 0;
		
		static public Vector3[] posTable = {
			new Vector3(0,1,1), new Vector3(0.5f,1,1), new Vector3(1,1,1), new Vector3(1,1,0.5f), new Vector3(1,1,0), new Vector3(0.5f,1,0), new Vector3(0,1,0), new Vector3(0,1,0.5f), new Vector3(0.5f,1,0.5f),
			new Vector3(1,0,1), new Vector3(0.5f,0,1), new Vector3(0,0,1), new Vector3(0,0,0.5f), new Vector3(0,0,0), new Vector3(0.5f,0,0), new Vector3(1,0,0), new Vector3(1,0,0.5f), new Vector3(0.5f,0,0.5f),
			new Vector3(1,1,0), new Vector3(1,1,0.5f), new Vector3(1,1,1), new Vector3(1,0.5f,1), new Vector3(1,0,1), new Vector3(1,0,0.5f), new Vector3(1,0,0), new Vector3(1,0.5f,0), new Vector3(1,0.5f,0.5f), 
			new Vector3(0,1,1), new Vector3(0,1,0.5f), new Vector3(0,1,0), new Vector3(0,0.5f,0), new Vector3(0,0,0), new Vector3(0,0,0.5f), new Vector3(0,0,1), new Vector3(0,0.5f,1), new Vector3(0,0.5f,0.5f), 
			new Vector3(0,1,0), new Vector3(0.5f,1,0), new Vector3(1,1,0), new Vector3(1,0.5f,0), new Vector3(1,0,0), new Vector3(0.5f,0,0), new Vector3(0,0,0), new Vector3(0,0.5f,0), new Vector3(0.5f,0.5f,0),
			new Vector3(1,1,1), new Vector3(0.5f,1,1), new Vector3(0,1,1), new Vector3(0,0.5f,1), new Vector3(0,0,1), new Vector3(0.5f,0,1), new Vector3(1,0,1), new Vector3(1,0.5f,1), new Vector3(0.5f,0.5f,1) };
		
		static public Vector3[] normalTable = {new Vector3(0,1,0), new Vector3(0,-1,0), new Vector3(1,0,0), new Vector3(-1,0,0), new Vector3(0,0,1), new Vector3(0,0,-1)};
		
		
		public VoxelandVert ()
		{
			//coords = Mathf.FloorToInt( Random.Range(0, 2000000000) );
			coords = Chunk.vertUniqueNum;
			Chunk.vertUniqueNum++;
			if (Chunk.vertUniqueNum > 2000000000) Chunk.vertUniqueNum = 0;
		}
		
		#region functions
		public void  AddNeig ( VoxelandVert vert  )
		{
			if (neigs==null) neigs = new VoxelandVert[7];
			if (neigCount==0) { neigs[0]=vert; neigCount=1; return; }
			
			switch (neigCount)
			{
			case 0:
				neigs[0]=vert; 
				neigCount=1;
				break;
				
			case 1:
				if (neigs[0].coords!=vert.coords) 
				{
					neigs[1]=vert; 
					neigCount=2;
				}
				break;
				
			default:
				for (int i=0; i<neigCount; i++) 
					if (neigs[i].coords == vert.coords) return; //already added
				neigs[neigCount]=vert; 
				neigCount++;
				break;
			}
		}
		
		
		public void  AddFace ( VoxelandFace face  )
		{
			for (int i=0; i<6; i++) 
			{
				if (faces[i]!=null) 
				{
					if (faces[i] == face) return; //already added
				}
				else 
				{
					faces[i] = face; 
					break; 
				}
			}
		}
		
		public void Replace ( VoxelandVert v2  ) //removing v2, 
		{
			//assigning this to all v2 faces
			for (int f = 0; f<6; f++) 
				if (v2.faces[f]!=null) for (int v=0; v<8; v++)
					if (v2.faces[f].verts[v] == v2)  v2.faces[f].verts[v] = this;
			
			for (int i = 0; i<6; i++) 
			{
				//if (v2.neigs[i]!=null) AddNeig(v2.neigs[i]);
				if (v2.faces[i]!=null) AddFace(v2.faces[i]);
			}
		}
		
		public Vector3  GetRelax (){
			Vector3 sum= new Vector3(0,0,0);
			int divider=0;
			
			for (int i=0;i<6;i++)
			{
				if (neigs[i]==null) break;
				sum += neigs[i].pos - pos;
				divider ++;
			}
			
			return sum/divider * relax;
		}
		
		
		
		public Vector3  GetAveragePos (){
			Vector3 sum= new Vector3(0,0,0);
			int divider=0;
			
			for (int i=0;i<6;i++)
			{
				if (neigs[i]==null) break;
				sum += neigs[i].pos;
				divider ++;
			}
			
			return sum/divider;
		}
		
		public Vector4 GetBlend (List<Texture> usedTextures)
		{
			Vector4 blend= new Vector4(0,0,0,0);
			Texture tex = null;
			
			for (int f=0;f<6;f++)
			{
				if (faces[f]==null) break;
				tex = faces[f].GetTexture();
				
				if (usedTextures.Count>0 && tex==usedTextures[0]) { blend.x++; break; } //&& faces[f].bump==usedBumpTextures[0]
				if (usedTextures.Count>1 && tex==usedTextures[1]) { blend.y++; break; }
				if (usedTextures.Count>2 && tex==usedTextures[2]) { blend.z++; break; }
				if (usedTextures.Count>3 && tex==usedTextures[3]) { blend.w++; break; }
			}
			
			float sum = blend.x + blend.y + blend.z + blend.w;
			
			if (sum > 0.01f) return (blend/sum);
			else return blend; //ie (0,0,0,0)
		}
		
		public Vector3  GetSmoothedNormal (){
			Vector3 sum= new Vector3(0,0,0);
			int divider=0;
			
			for (int i=0;i<6;i++)
			{
				if (neigs[i]==null) break;
				sum += neigs[i].normal;
				divider ++;
			}
			
			return sum/divider;
		}
		
		public float  GetAmbient ()
		{
			float sum = 0;
			int divider=0;
			
			for (int i=0;i<6;i++)
			{
				if (faces[i]==null) break;
				if (!faces[i].block.visible) continue;
				sum += faces[i].ambient;
				divider ++;
			}
			
			return sum/divider;
		}
		
		static public void ResetProcessed (System.Collections.Generic.List<VoxelandFace> faces)
		{
			for (int f=0;f<faces.Count;f++) 
				for (int v=0;v<9;v++)
					faces[f].verts[v].processed = false;	
		}
		
		#endregion
	}
	
	
	public class VoxelandFace
	{
		public VoxelandVert[] verts; //always 9
		public bool  visible;
		
		//int[] coords = new int[9];
		public Texture tex;
		public Texture bump;
		//public byte type;
		
		public float ambient;
		
		public VoxelandFace() { verts = new VoxelandVert[9]; }
		
		#region functions
		public void  LinkVerts ()
		{
			//for (int v=0; v<9; v++) verts[v].coords = coords[v];
			
			verts[0].AddNeig(verts[7]); verts[0].AddNeig(verts[1]);
			verts[1].AddNeig(verts[0]); verts[1].AddNeig(verts[2]); verts[1].AddNeig(verts[8]);
			verts[2].AddNeig(verts[1]); verts[2].AddNeig(verts[3]);
			verts[3].AddNeig(verts[4]); verts[3].AddNeig(verts[2]); verts[3].AddNeig(verts[8]);
			verts[4].AddNeig(verts[3]); verts[4].AddNeig(verts[5]);
			verts[5].AddNeig(verts[6]); verts[5].AddNeig(verts[4]); verts[5].AddNeig(verts[8]);
			verts[6].AddNeig(verts[5]); verts[6].AddNeig(verts[7]);
			verts[7].AddNeig(verts[6]); verts[7].AddNeig(verts[0]); verts[7].AddNeig(verts[8]);
			verts[8].AddNeig(verts[1]); verts[8].AddNeig(verts[3]); verts[8].AddNeig(verts[5]); verts[8].AddNeig(verts[7]);
		}
		
		static public int[] dirToPosX = {0,0,1,-1,0,0};
		static public int[] dirToPosY = {1,-1,0,0,0,0};
		static public int[] dirToPosZ = {0,0,0,0,-1,1};
		
		static public int[] neigX = {1,1,0,0,1,1}; //if there is shift on this axis?
		static public int[] neigY = {0,0,1,1,1,1};
		static public int[] neigZ = {1,1,1,1,0,0};
		
		static public int[] opposite = {1,0,3,2,5,4};
		
		static public int[] prewPoint = {7,0,1,2,3,4,5,6};
		//	static int[] thisPoint = [0,1,2,3,4,5,6,7];
		static public int[] nextPoint = {1,2,3,4,5,6,7,0};
		
		//	static int weldThisBlock [ //first dir, second dir, 3 first verts and 3 second verts
		
		public VoxelandBlock block; //backward link to block. Against ideology.
		public byte dir;
		
		public void Weld ( VoxelandFace f2 ,   int p1 ,   int p2  ) //welding two points only
		{
			//no vert - creating it
			if (verts[p1]==null && f2.verts[p2]==null) verts[p1] = f2.verts[p2] = new VoxelandVert();
			
			//one of the verts exist - assign it to the other one
			else if (f2.verts[p2]==null && verts[p1]!=null) f2.verts[p2] = verts[p1];
			else if (verts[p1]==null && f2.verts[p2]!=null) verts[p1] = f2.verts[p2];
			
			verts[p1].AddFace(this);
			verts[p1].AddFace(f2);
			
			//if both of the verts exist - welding
			if (verts[p1]!=null && f2.verts[p2]!=null && verts[p1].coords != f2.verts[p2].coords) verts[p1].Replace(f2.verts[p2]);
		}
		
		public void Weld ( VoxelandFace fB ,     int pA1 ,   int pA2 ,   int pA3 ,     int pB1 ,   int pB2 ,   int pB3  ) //welding three points at once
		{
			Weld(fB, pA1, pB1); Weld(fB, pA2, pB2); Weld(fB, pA3, pB3);
		}
		
		public void Weld ( VoxelandFace fB ,   int pA2 ,   int pB2 ,   bool inverted  )  //welding three poins (side) using mid-point number
		{
			Weld(fB, prewPoint[pA2], nextPoint[pB2]); 
			Weld(fB, pA2, pB2); 
			Weld(fB, nextPoint[pA2], prewPoint[pB2]);
		}
		
		public void  Weld ( VoxelandBlock block2 ,   int dir2 ,   int p1 ,   int p2  )
		{
			if (block2!=null && block2.faces[dir2]!=null) Weld( block2.faces[dir2], p1, p2, true);
		}
		
		public void Weld ( VoxelandBlock block2 ,   int dir2 ,   int p1 ,   int p2 ,   bool check  )  //same as above, but works only when no p1 (for cross-block)
		{
			if (verts[p1]==null && block2!=null && block2.faces[dir2]!=null && block2.faces[dir2].verts[p2]==null) 
				Weld( block2.faces[dir2], p1, p2, true);
		}
		
		public void Weld ( VoxelandBlock block2 ,   int dir2 ,     int pA1 ,   int pA2 ,   int pA3 ,     int pB1 ,   int pB2 ,   int pB3  ) //welding three poins (side) using mid-point number
		{
			if (verts[pA2]==null && block2!=null && block2.faces[dir2]!=null && block2.faces[dir2].verts[pB2]==null) 
			{
				VoxelandFace fB = block2.faces[dir2];
				Weld(fB, pA1, pB1); Weld(fB, pA2, pB2); Weld(fB, pA3, pB3);
			}
		}
		
		public void  SetVertNormals ()
		{
			verts[0].normal += Vector3.Cross(verts[7].pos-verts[0].pos, verts[1].pos-verts[0].pos); 
			verts[2].normal += Vector3.Cross(verts[1].pos-verts[0].pos, verts[3].pos-verts[2].pos);
			verts[4].normal += Vector3.Cross(verts[3].pos-verts[0].pos, verts[5].pos-verts[4].pos);
			verts[0].normal += Vector3.Cross(verts[5].pos-verts[0].pos, verts[7].pos-verts[6].pos);
		}
		
		public Texture GetTexture ()
		{
			if (verts[8].normal.y>0.7f && block.type.differentTop) return block.type.topTexture;
			else return block.type.texture;
		}
		
		public Texture GetBump ()
		{
			if (verts[8].normal.y>0.7f && block.type.differentTop) return block.type.topBumpTexture;
			else return block.type.bumpTexture;
		}
		#endregion
	}
	
	
	public class VoxelandBlock
	{
		public int x;
		public int y;
		public int z;
		
		public VoxelandFace[] faces; //max 8
		
		public bool  visible;
		public VoxelandBlockType type;
		//public float ambient;
		public Chunk chunk;
		
		public VoxelandBlock ( int nx ,   int ny ,   int nz  )
		{ 
			x=nx; y=ny; z=nz;
			faces = new VoxelandFace[6]; 
		}
		
		public Vector3 GetCenter ()
		{
			return new Vector3(x*1f+0.5f, y*1f+0.5f, z*1f+0.5f);
		}
	}
	
	#endregion
	
	static public int vertUniqueNum; //used to give each vert unique coords
	
	public VoxelandTerrain land;
	
	public int coordX;
	public int coordZ;
	
	public int offsetX; //at which data block num invisible (overlapped) blocks starts
	public int offsetZ;	
	public int size = 32; //it is real chunk size, including invisible (overlapped) blocks
	//public int height = 128; //replaced with height
	//clones of land.offet and land.chunkSize actually
	
	public MeshFilter hiFilter;
	public MeshFilter loFilter;
	public MeshCollider collision;
	
	[System.NonSerialized] public System.Collections.Generic.Dictionary<int,Transform> prefabs;
	[System.NonSerialized] public System.Collections.Generic.Dictionary<int,int> prefabTypes;
	
	public MeshFilter grassFilter;
	public MeshFilter constructorFilter;
	public MeshCollider constructorCollider;

	[System.NonSerialized] public System.Collections.Generic.List<VoxelandFace> visibleFaces = new List<VoxelandFace>();
	
	public enum VoxelandFaceDir {top=0, bottom=1, front=2, back=3, left=4, right=5}; //y,z,x: left and right are on x axis
	
	#region vars Ambient
	[System.NonSerialized] public byte[] ambient;
	[System.NonSerialized] public bool[] ambientBool;
	[System.NonSerialized] public bool[] ambientExist;
	[System.NonSerialized] public int ambientSize;
	[System.NonSerialized] public int ambientHeight;
	[System.NonSerialized] public int ambientBottomPoint;
	[System.NonSerialized] public int ambientTopPoint;
	#endregion
	
	//in-progress marks
	public enum Progress { notCalculated=0, threadStarted=1, calculated=2, applied=3, dontChange=4 };
	[System.NonSerialized] public Progress terrainProgress = Progress.notCalculated;
	[System.NonSerialized] public Progress ambientProgress = Progress.notCalculated;
	[System.NonSerialized] public Progress constructorProgress = Progress.notCalculated;
	[System.NonSerialized] public Progress grassProgress = Progress.notCalculated;
	[System.NonSerialized] public Progress prefabsProgress = Progress.notCalculated;
	
	static public string[] shaderMainTexNames = new string[] {"_MainTex", "_MainTex2", "_MainTex3", "_MainTex4"};
	static public string[] shaderBumpTexNames = new string[] {"_BumpMap", "_BumpMap2", "_BumpMap3", "_BumpMap4"};
	
	#region vars Apply
	[System.NonSerialized] public Vector3[] vertices;
	[System.NonSerialized] public Vector3[] normals;
	[System.NonSerialized] public Vector4[] tangents;
	[System.NonSerialized] public Vector2[] uvs;
	[System.NonSerialized] public Vector2[] uv1;
	[System.NonSerialized] public Color[] colors;
	[System.NonSerialized] public int[] tris; //*8*3
	
	[System.NonSerialized] public Vector3[] vertices_lo;
	[System.NonSerialized] public Vector3[] normals_lo;
	[System.NonSerialized] public Vector4[] tangents_lo;
	[System.NonSerialized] public Vector2[] uvs_lo;
	[System.NonSerialized] public Vector2[] uv1_lo;
	[System.NonSerialized] public Color[] colors_lo;
	[System.NonSerialized] public int[] tris_lo;
	#endregion

	
	
	#region function CreateChunk
	static public Chunk CreateChunk (VoxelandTerrain land, int x, int z)
	{
		GameObject chunkObj = new GameObject("Chunk");
		
		chunkObj.transform.parent = land.transform;
		chunkObj.transform.localPosition = new Vector3(x*(land.chunkSize-land.overlap*2), 0, z*(land.chunkSize-land.overlap*2));
		chunkObj.transform.localScale = new Vector3(1,1,1);
		chunkObj.layer = land.gameObject.layer;
		
		
		Chunk chunk= chunkObj.AddComponent<Chunk>();
		//land.terrain[ land.chunkCountX*z + x ] = chunk;
		
		chunk.land = land;
		chunk.coordX = x;
		chunk.coordZ = z;
		chunk.offsetX = x*(land.chunkSize-land.overlap*2); 
		chunk.offsetZ = z*(land.chunkSize-land.overlap*2);
		chunk.size = land.chunkSize;

		//hi filter
		GameObject hiObj = new GameObject ("HiResChunk");
		hiObj.transform.parent = chunk.transform;
		hiObj.transform.localPosition = new Vector3(0,0,0); 
		hiObj.transform.localScale = new Vector3(1,1,1);
		hiObj.layer = chunkObj.layer;
		chunk.hiFilter = hiObj.AddComponent<MeshFilter>();
		hiObj.AddComponent<MeshRenderer>();
		
		//lo filter and collision
		GameObject loObj = new GameObject ("LoResChunk");
		loObj.transform.parent = chunk.transform;
		loObj.transform.localPosition = new Vector3(0,0,0); 
		loObj.transform.localScale = new Vector3(1,1,1);
		loObj.layer = chunkObj.layer;
		chunk.collision = loObj.AddComponent<MeshCollider>();
		chunk.loFilter = loObj.AddComponent<MeshFilter>();
		loObj.AddComponent<MeshRenderer>();
		
		//hiding chunk objects
		if (land.hideChunks) chunk.transform.hideFlags = HideFlags.HideInHierarchy;
		//and DontSave is made with OnWillSaveAssets processor in Editor
		
		if (!land.activeChunks.Contains(chunk)) land.activeChunks.Add(chunk);
		
		//copy static flag
		#if UNITY_EDITOR
		UnityEditor.StaticEditorFlags flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(land.gameObject);
		UnityEditor.GameObjectUtility.SetStaticEditorFlags(chunkObj, flags);
		UnityEditor.GameObjectUtility.SetStaticEditorFlags(hiObj, flags);
		UnityEditor.GameObjectUtility.SetStaticEditorFlags(loObj, flags);
		#endif
		
		return chunk;
	}
	#endregion
	
	#region function SwitchLod
	public void SwitchLod (bool lod)
	{
		if (hiFilter == null) return;
		
		//if lod
		if (lod && hiFilter.renderer.enabled)
		{
			if (hiFilter.renderer.enabled) hiFilter.renderer.enabled = false;
			//if (!loFilter.renderer.enabled) loFilter.renderer.enabled = true;
			loFilter.renderer.sharedMaterial = hiFilter.renderer.sharedMaterial;
		}
		
		//if main
		if (!lod && loFilter.renderer.sharedMaterials.Length != 0) //loFilter.renderer.enabled)
		{
			if (!hiFilter.renderer.enabled) hiFilter.renderer.enabled = true;
			//if (loFilter.renderer.enabled) loFilter.renderer.enabled = false;
			loFilter.renderer.sharedMaterials = new Material[0];
		}
	}
	#endregion
	
	

	#region Terrain

	public void CalculateTerrain (object stateInfo) { CalculateTerrain(); }
	public void CalculateTerrain ()
	{
		terrainProgress = Progress.threadStarted;
		//bool isMainThread = System.Threading.Thread.CurrentThread.ManagedThreadId == Voxeland.mainThreadId;
		//bool isMainThread = false;

		int x=0; int y=0; int z=0; //int nx=0; int ny=0; int nz=0;
		int dir = 0; int v=0; int f=0; int i = 0;
		VoxelandBlock block; VoxelandFace face; VoxelandVert vert;// VoxelandFace nface;
		//Vector3 pos;
		
		System.Collections.Generic.List<VoxelandFace> faces = new System.Collections.Generic.List<VoxelandFace>();
		
		#region Calculating top and bottom points
		int topPoint = land.data.GetTopPoint(offsetX, offsetZ, offsetX+size, offsetZ+size)+1;
		int bottomPoint = Mathf.Max(0, land.data.GetBottomPoint(offsetX, offsetZ, offsetX+size, offsetZ+size)-2);
		int shortHeight = topPoint - bottomPoint;
		
		//emergency exit if chunk contains no blocks
		if (topPoint<=1) { visibleFaces.Clear(); terrainProgress = Progress.calculated; return; }
		#endregion
		
		bool[] existMatrix = new bool[size*size*shortHeight];
		land.data.GetExistMatrix (existMatrix,  offsetX, bottomPoint, offsetZ,  offsetX+size, topPoint, offsetZ+size);

		VoxelandBlock[] blocks = new VoxelandBlock[size*shortHeight*size];	
		
		#region Creating all faces
		if (land.profile) Profiler.BeginSample ("Creating all faces");
		
		//for (y=bottomPoint; y<topPoint; y++)
		for (y=0; y<shortHeight; y++)
			for (x=0; x<size; x++)	
				for (z=0; z<size; z++)
			{
				i = z*shortHeight*size + y*size + x;
				
				//if exists
				//if (land.data.GetExist(x+offsetX,y,z+offsetZ))
				if (existMatrix[z*size*shortHeight + y*size + x])
					for (dir = 0; dir<6; dir++)
				{
					//if has face at this side
					if (x+VoxelandFace.dirToPosX[dir] >= 0 && x+VoxelandFace.dirToPosX[dir] < size &&  //cap sides here
					    y+VoxelandFace.dirToPosY[dir] >= 0 && y+VoxelandFace.dirToPosY[dir] < shortHeight &&
					    z+VoxelandFace.dirToPosZ[dir] >= 0 && z+VoxelandFace.dirToPosZ[dir] < size &&
					    !existMatrix[ (z+VoxelandFace.dirToPosZ[dir])*size*shortHeight + (y+VoxelandFace.dirToPosY[dir])*size + x+VoxelandFace.dirToPosX[dir] ])
					{
						if (blocks[i]==null) 
						{ 
							blocks[i] = new VoxelandBlock(x,y+bottomPoint,z); 
							
							if (x<land.overlap || x>=size-land.overlap ||	
							    z<land.overlap || z>=size-land.overlap ||
							    y == 0) 
									blocks[i].visible = false;
							else blocks[i].visible = true;
							
							byte typeNum = land.data.GetBlock(x+offsetX, y+bottomPoint, z+offsetZ);
							if (typeNum < land.types.Length) blocks[i].type = land.types[typeNum];
							else blocks[i].type = land.types[0];
							
							blocks[i].chunk = this;
						}
						
						face = new VoxelandFace();
						faces.Add(face);
						
						blocks[i].faces[dir] = face;
						
						face.block = blocks[i];
						face.dir = (byte)dir;
						
						//setting face type, and adding it to used types array
						//face.type = (byte)land.data.GetBlock(x+offsetX, y+bottomPoint, z+offsetZ);
						//if (!usedTypes.Contains(face.type)) usedTypes.Add(face.type);
						
						face.visible = face.block.type.filled; //land.data.exist[face.block.type]; //land.types[face.block.type].visible;

						//setting coords
						for (v=0;v<9;v++)
						{
							//pos = VoxelandVert.posTable[ dir*9 + v ] + new Vector3(x,y,z);
							//face.coords[v] = 1000000 + pos.x*20000 + pos.y*200 + pos.z*2;
						}
					}
				}
			}
		if (land.profile) Profiler.EndSample ();
		#endregion
		
		#region Welding internal, Welding neig
		if (land.profile) Profiler.BeginSample("Welding internal, Welding neig");
	
		int max = size-1;
		int maxHeight = shortHeight-1;
		
		for (y=0; y<shortHeight; y++)
			for (x=0; x<size; x++)	
				for (z=0; z<size; z++)
		{
				int stepz = shortHeight*size;
				i = z*stepz + y*size + x;
				
				if (blocks[i]!=null) for (dir = 0; dir<6; dir++)
				{
					block = blocks[i];
					face = block.faces[dir];	
					if (face==null) continue;
					
					//weld this block
					switch (dir)
					{
					case 0: face.Weld( block, 5,  0,1,2, 2,1,0); face.Weld( block, 2,  2,3,4, 2,1,0);
						face.Weld( block, 4,  4,5,6, 2,1,0); face.Weld( block, 3,  6,7,0, 2,1,0); 
						
						if (x>0) face.Weld( blocks[i-1], 0,  6,7,0, 4,3,2);
						if (z>0) face.Weld( blocks[i-stepz], 0,  4,5,6, 2,1,0);
						
						break;
						
					case 1: face.Weld( block, 5,  0,1,2, 6,5,4); face.Weld( block, 3,  2,3,4, 6,5,4); 
						face.Weld( block, 4,  4,5,6, 6,5,4); face.Weld( block, 2,  6,7,0, 6,5,4); 
						
						if (x>0) face.Weld( blocks[i-1], 1,  2,3,4, 0,7,6);
						if (z>0) face.Weld( blocks[i-stepz], 1,  4,5,6, 2,1,0);
						
						break;
						
					case 2: face.Weld( block, 0,  0,1,2, 4,3,2); face.Weld( block, 5,  2,3,4, 0,7,6);
						face.Weld( block, 1,  4,5,6, 0,7,6); face.Weld( block, 4,  6,7,0, 4,3,2); 
						
						if (y>0) face.Weld( blocks[i-size], 2,  4,5,6, 2,1,0);
						if (z>0) face.Weld( blocks[i-stepz], 2,  6,7,0, 4,3,2);
						
						break;
						
					case 3: face.Weld( block, 0,  0,1,2, 0,7,6); face.Weld( block, 4,  2,3,4, 0,7,6);
						face.Weld( block, 1,  4,5,6, 4,3,2); face.Weld( block, 5,  6,7,0, 4,3,2); 
						
						if (y>0) face.Weld( blocks[i-size], 3,  4,5,6, 2,1,0);
						if (z>0) face.Weld( blocks[i-stepz], 3,  2,3,4, 0,7,6);
						
						break;
						
					case 4: face.Weld( block, 0,  0,1,2, 6,5,4); face.Weld( block, 2,  2,3,4, 0,7,6);
						face.Weld( block, 1,  4,5,6, 6,5,4); face.Weld( block, 3,  6,7,0, 4,3,2); 
						
						if (y>0) face.Weld( blocks[i-size], 4,  4,5,6, 2,1,0);
						if (x>0) face.Weld( blocks[i-1], 4,  6,7,0, 4,3,2);
						
						break;
						
					case 5: face.Weld( block, 0,  0,1,2, 2,1,0); face.Weld( block, 3,  2,3,4, 0,7,6);
						face.Weld( block, 1,  4,5,6, 2,1,0); face.Weld( block, 2,  6,7,0, 4,3,2); 
						
						if (y>0) face.Weld( blocks[i-size], 5,  4,5,6, 2,1,0);
						if (x>0) face.Weld( blocks[i-1], 5,  2,3,4, 0,7,6);
						
						break;
					}
				}
		}
		if (land.profile) Profiler.EndSample ();
		#endregion
		
		#region Cross-welding
		if (land.profile) Profiler.BeginSample ("Cross-Welding");
		
		for (y=0; y<shortHeight; y++)
			for (x=0; x<size; x++)	
				for (z=0; z<size; z++)
		{
				int stepz = shortHeight*size;
				i = z*stepz + y*size + x;
				int j = i;
				
				if (blocks[i]!=null) for (dir = 0; dir<6; dir++)
				{
					face = blocks[i].faces[dir];	
					if (face==null) continue;
					
					//weld cross block
					if (dir==0 && y<maxHeight)
					{
						j = i+size;
						if (x>0) face.Weld( blocks[j-1], 2,  6,7,0, 6,5,4); 		//x-1,y+1,z
						if (z>0) face.Weld( blocks[j-stepz], 5,  4,5,6, 6,5,4); 	//x,y+1,z-1
						if (x<max) face.Weld( blocks[j+1], 3,  2,3,4, 6,5,4); 	//x+1,y+1,z
						if (z<max) face.Weld( blocks[j+stepz], 4,  0,1,2, 6,5,4); 	//x,y+1,z+1
					}
					
					else if (dir==1 && y>0)
					{
						j = i-size;
						if (x>0) face.Weld( blocks[j-1], 2,  2,3,4, 2,1,0);		//x-1,y-1,z
						if (z>0) face.Weld( blocks[j-stepz], 5,  4,5,6, 2,1,0);		//x,y-1,z-1
						if (x<max) face.Weld( blocks[j+1], 3,  6,7,0, 2,1,0);		//x+1,y-1,z
						if (z<max) face.Weld( blocks[j+stepz], 4,  0,1,2, 2,1,0);		//x,y-1,z+1
					}
					
					else if (dir==2 && x<max)
					{
						j = i+1;
						if (y>0) face.Weld( blocks[j-size], 0,  4,5,6, 0,7,6);		//x+1,y-1,z
						if (z>0) face.Weld( blocks[j-stepz], 5,  6,7,0, 4,3,2);		//x+1,y,z-1
						if (y<maxHeight) face.Weld( blocks[j+size], 1,  0,1,2, 4,3,2);//x+1,y+1,z
						if (z<max) face.Weld( blocks[j+stepz], 4,  2,3,4, 0,7,6);		//x+1,y,z+1
					}
					
					else if (dir==3 && x>0)
					{
						j = i-1;
						if (y>0) face.Weld( blocks[j-size], 0,  4,5,6, 4,3,2);		//x-1,y-1,z
						if (z>0) face.Weld( blocks[j-stepz], 5,  2,3,4, 0,7,6);		//x-1,y,z-1
						if (y<maxHeight) face.Weld( blocks[j+size], 1,  0,1,2, 0,7,6);//x-1,y+1,z
						if (z<max) face.Weld( blocks[j+stepz], 4,  6,7,0, 4,3,2);		//x-1,y,z+1
					}
					
					else if (dir==4 && z>0)
					{
						j = i-stepz;
						if (y>0) face.Weld( blocks[j-size], 0,  4,5,6, 2,1,0);		//x,y-1,z-1
						if (x>0) face.Weld( blocks[j-1], 2,  6,7,0, 4,3,2);		//x-1,y,z-1
						if (y<maxHeight) face.Weld( blocks[j+size], 1,  0,1,2, 2,1,0);//x,y+1,z-1
						if (x<max) face.Weld( blocks[j+1], 3,  2,3,4, 0,7,6);		//x+1,y,z-1
					}
					
					else if (dir==5 && z<max)
					{
						j = i+stepz;
						if (y>0) face.Weld( blocks[j-size], 0,  4,5,6, 6,5,4);		//x,y-1,z+1
						if (x>0) face.Weld( blocks[j-1], 2,  2,3,4, 0,7,6);		//x-1,y,z+1
						if (y<maxHeight) face.Weld( blocks[j+size], 1,  0,1,2, 6,5,4);//x,y+1,z+1
						if (x<max) face.Weld( blocks[j+1], 3,  6,7,0, 4,3,2);		//x+1,y,z+1
					}
					
					for (v=0;v<9;v++)
					{
						vert = face.verts[v];
						
						//setting new vert
						if (vert==null)
						{
							vert = new VoxelandVert();
							face.verts[v] = vert;
						}
						
						vert.pos = VoxelandVert.posTable[ dir*9 + v ] + new Vector3(x,y,z);
						
						vert.AddFace(face);
					}
					
					//face.LinkVerts();
				}
		}
		if (land.profile) Profiler.EndSample ();
		#endregion
		
		#region Linking (adding verts neigs)
		if (land.profile) Profiler.BeginSample ("Linking (adding vert neigs)");
		for (f=0;f<faces.Count;f++) faces[f].LinkVerts();
		if (land.profile) Profiler.EndSample ();
		#endregion
		
		#region Lifting on BottomPoint
		for (f=0;f<faces.Count;f++) 
		{
			face = faces[f];
			for (v=0;v<9;v++)
				if (!face.verts[v].processed)
			{ 
				face.verts[v].pos += new Vector3(0,bottomPoint,0);
				face.verts[v].processed = true;
			}
		}
		
		for (f=0;f<faces.Count;f++) //clearing 
			for (v=0;v<9;v++)
				faces[f].verts[v].processed = false;
		#endregion
				
		#region Relaxing
		if (land.profile) Profiler.BeginSample ("Relaxing, normals, etc.");
		
		//setting vert relax
		for (f=0;f<faces.Count;f++) 
		{
			//for (v=0;v<9;v++) faces[f].verts[v].relax = 1; //Mathf.Min(faces[f].verts[v].relax, faces[f].block.type.smooth);
			
			for (v=0;v<9;v++) 
			{
				faces[f].verts[v].relax += faces[f].block.type.smooth;
				faces[f].verts[v].relaxCount += 1;
			}
		}
		
		for (f=0;f<faces.Count;f++) 
			for (v=0;v<9;v++) 
				if (faces[f].verts[v].relaxCount != 0)
		{
			faces[f].verts[v].relax = faces[f].verts[v].relax / faces[f].verts[v].relaxCount;
			faces[f].verts[v].relaxCount = 0;
		}
		
		//averaging corners
		for (f=0;f<faces.Count;f++) 
		{
			face = faces[f];
			
			if (!face.verts[0].processed) { face.verts[0].pos += face.verts[0].GetRelax()*2; face.verts[0].processed=true; }
			if (!face.verts[2].processed) { face.verts[2].pos += face.verts[2].GetRelax()*2; face.verts[2].processed=true; }
			if (!face.verts[4].processed) { face.verts[4].pos += face.verts[4].GetRelax()*2; face.verts[4].processed=true; }
			if (!face.verts[6].processed) { face.verts[6].pos += face.verts[6].GetRelax()*2; face.verts[6].processed=true; }
		}
		
		//averaging mid verts
		for (f=0;f<faces.Count;f++) 
		{
			face = faces[f];
			
			face.verts[1].pos = (face.verts[0].pos + face.verts[2].pos) * 0.5f;
			face.verts[3].pos = (face.verts[2].pos + face.verts[4].pos) * 0.5f;
			face.verts[5].pos = (face.verts[4].pos + face.verts[6].pos) * 0.5f;
			face.verts[7].pos = (face.verts[6].pos + face.verts[0].pos) * 0.5f;
			
			face.verts[8].pos = (face.verts[0].pos + face.verts[2].pos + face.verts[4].pos + face.verts[6].pos) * 0.25f;
			
			//returning processed flags
			face.verts[0].processed = false;
			face.verts[2].processed = false;
			face.verts[4].processed = false;
			face.verts[6].processed = false;
		}
		
		
		//seconary relax
		for (f=0;f<faces.Count;f++) 
		{
			face = faces[f];
			
			for (v=0;v<9;v++)
				if (!face.verts[v].processed)
			{ 
				face.verts[v].relaxed = face.verts[v].GetRelax();
				face.verts[v].processed = true;
			}
		}
		
		for (f=0;f<faces.Count;f++) 
			for (v=0;v<9;v++)
				faces[f].verts[v].processed = false;
		
		for (f=0;f<faces.Count;f++) 
		{
			face = faces[f];
			
			for (v=0;v<9;v++) 
				if (!face.verts[v].processed)
			{ 
				face.verts[v].pos += face.verts[v].relaxed;
				face.verts[v].processed = true;
			}
		}
		if (land.profile) Profiler.EndSample();
		#endregion
		
		#region Setting normals
		for (f=0;f<faces.Count;f++) 
		{
			face = faces[f];
			
			face.verts[0].normal += Vector3.Cross(face.verts[1].pos-face.verts[0].pos, face.verts[7].pos-face.verts[0].pos).normalized; 
			face.verts[2].normal += Vector3.Cross(face.verts[3].pos-face.verts[2].pos, face.verts[1].pos-face.verts[2].pos).normalized;
			face.verts[4].normal += Vector3.Cross(face.verts[5].pos-face.verts[4].pos, face.verts[3].pos-face.verts[4].pos).normalized;
			face.verts[6].normal += Vector3.Cross(face.verts[7].pos-face.verts[6].pos, face.verts[5].pos-face.verts[6].pos).normalized;
			
			face.verts[1].normal += Vector3.Cross(face.verts[8].pos-face.verts[1].pos, face.verts[0].pos-face.verts[1].pos).normalized +
				Vector3.Cross(face.verts[2].pos-face.verts[1].pos, face.verts[8].pos-face.verts[1].pos).normalized; 
			face.verts[3].normal += Vector3.Cross(face.verts[8].pos-face.verts[3].pos, face.verts[2].pos-face.verts[3].pos).normalized +
				Vector3.Cross(face.verts[4].pos-face.verts[3].pos, face.verts[8].pos-face.verts[3].pos).normalized; 
			face.verts[5].normal += Vector3.Cross(face.verts[8].pos-face.verts[5].pos, face.verts[4].pos-face.verts[5].pos).normalized +
				Vector3.Cross(face.verts[6].pos-face.verts[5].pos, face.verts[8].pos-face.verts[5].pos).normalized; 
			face.verts[7].normal += Vector3.Cross(face.verts[8].pos-face.verts[7].pos, face.verts[6].pos-face.verts[7].pos).normalized +
				Vector3.Cross(face.verts[0].pos-face.verts[7].pos, face.verts[8].pos-face.verts[7].pos).normalized; 
			
			face.verts[8].normal = Vector3.Cross(face.verts[1].pos-face.verts[5].pos, face.verts[3].pos-face.verts[7].pos).normalized;	
		}
		
		//smooth normals
		/*
		if (land.normalsSmooth == VoxelandNormalsSmooth.smooth)
		{
			for (int v=0; v<verts.Count; v++) verts[v].relaxed = verts[v].GetSmoothedNormal();
			for (v=0; v<verts.Count; v++) verts[v].normal = verts[v].normal*0.6f + verts[v].relaxed*0.4f;
		}
		*/
		
		//random normals
		if (land.normalsRandom > 0.01f)
		{
			for (x=0; x<size; x++)
				for (z=0; z<size; z++)
					for (y=0; y<shortHeight; y++)
				{
					i = z*shortHeight*size + y*size + x;
					
					if (blocks[i]==null) continue;
					if (!blocks[i].visible) continue;
					
					Vector3 normalRandom = new Vector3(Random.value-0.5f, Random.value-0.5f, Random.value-0.5f)*1;
					
					for (f=0;f<6;f++) if (blocks[i].faces[f]!=null && blocks[i].faces[f].visible)
						for (v=0;v<9;v++) blocks[i].faces[f].verts[v].normal += normalRandom;
				}
		}
		#endregion
		
		#region Set visible faces
		//visibleFaces.Clear();
		visibleFaces = new System.Collections.Generic.List<VoxelandFace>();
		for (x=0; x<size; x++)
			for (z=0; z<size; z++)
				for (y=0; y<shortHeight; y++)
			{
				i = z*shortHeight*size + y*size + x;
				
				if (blocks[i]==null) continue;
				if (!blocks[i].visible) continue;
				for (f=0;f<6;f++) if (blocks[i].faces[f]!=null && blocks[i].faces[f].visible) 
					visibleFaces.Add(blocks[i].faces[f]);
			}
		#endregion
	
		terrainProgress = Progress.calculated;
	}
	

	public void ApplyTerrain ()
	{		
		//if (hiFilter == null) return;
		//int x; int y; int z; int i; 
		VoxelandFace face = null;
		int f; int v;

		//lightmap pre-data
		float lightmapTile = Mathf.Ceil(Mathf.Sqrt(visibleFaces.Count));
		float step = 1.0f/lightmapTile;
		float padding = step*land.lightmapPadding; //step*0.2f;
		
		#region Clearing Meshes
		DestroyImmediate(hiFilter.sharedMesh);
		hiFilter.mesh = new Mesh ();
		//hiFilter.sharedMesh.Clear();
		DestroyImmediate(loFilter.sharedMesh);
		loFilter.mesh = new Mesh ();
		//loFilter.sharedMesh.Clear();
		#endregion
		
		#region Exit if no faces
		if (visibleFaces.Count == 0)
		{
			terrainProgress = Progress.applied;
			return;
		}
		#endregion
		
		#region Get Used Textures
		Profiler.BeginSample ("Get Used Textures");
		
		List<Texture> usedTextureList = new List<Texture>();
		List<Texture> usedBumpList = new List<Texture>();
		
		//Texture mainTex;
		//Texture bumpTex;
		for (f=0; f<visibleFaces.Count; f++)
		{
			face = visibleFaces[f];
			
			Texture faceTex = face.GetTexture();
			Texture faceBump = face.GetBump();
			
			if (usedTextureList.Contains(faceTex) && usedBumpList.Contains(faceBump)) continue; //if already in list - skipping
			
			usedTextureList.Add(faceTex);
			usedBumpList.Add(faceBump);
		}
		
		Profiler.EndSample ();
		#endregion
		
		#region Creating Arrays
		Profiler.BeginSample ("Creating Arrays");
		
		//calculating number of verts
		int vertCount = 0;
		int vertCount_lo = 0;
		if (!land.weldVerts) { vertCount = visibleFaces.Count*9; vertCount_lo = visibleFaces.Count*4; }
		else
		{
			VoxelandVert.ResetProcessed(visibleFaces);
			
			//hipoly
			for (f=0;f<visibleFaces.Count;f++) 
			{
				face= visibleFaces[f];
				
				for (v=0;v<9;v++)
				{
					if (face.verts[v].processed) continue;
					
					vertCount++;
					if (v==0 || v==2 || v==4 || v==6) vertCount_lo++;
					face.verts[v].processed = true;
				}
			}
			
			VoxelandVert.ResetProcessed(visibleFaces);
		}
		
		vertices = new Vector3[vertCount];
		normals = new Vector3[vertCount];
		tangents = new Vector4[vertCount];
		uvs = new Vector2[vertCount];
		uv1 = new Vector2[vertCount];
		colors = new Color[vertCount];
		tris = new int[visibleFaces.Count*24]; //*8*3
		
		vertices_lo = new Vector3[vertCount_lo];
		normals_lo = new Vector3[vertCount_lo];
		tangents_lo = new Vector4[vertCount_lo];
		uvs_lo = new Vector2[vertCount_lo];
		uv1_lo = new Vector2[vertCount_lo];
		colors_lo = new Color[vertCount_lo];
		tris_lo = new int[visibleFaces.Count*6];

		Profiler.EndSample ();
		#endregion
		
		#region Filling Arrays
		Profiler.BeginSample ("Filling Arrays");
		
		//filling arrays
		ushort counter = 0;
		ushort counter_lo = 0;
		//int[] vertNums = new int[9];
		//int[] vertNums_lo = new int[4];
		for (f=0;f<visibleFaces.Count;f++)
		{
			face = visibleFaces[f];
			
			//verts
			//Vector3 binormal = (face.verts[3].pos-face.verts[7].pos).normalized;
			
			for (v=0;v<9;v++)
			{
				if (land.weldVerts && face.verts[v].processed) continue;
				
				vertices[counter] = face.verts[v].pos;
				normals[counter] = face.verts[v].normal.normalized;
				colors[counter] = face.verts[v].GetBlend(usedTextureList);
				
				//Vector3 tangent = Vector3.Cross(normals[counter], binormal);
				//tangent = Vector3.Cross(normals[counter], tangent);
				tangents[counter] = new Vector4(0, 0, 0, -1); //new Vector4(tangent.x, tangent.y, tangent.z, -1);
				//tangents[counter] = Vector4(binormal.x, binormal.y, binormal.z, -1);
				
				face.verts[v].num = counter;
				
				//lopoly mesh
				if (v==0 || v==2 || v==4 || v==6)
				{
					vertices_lo[counter_lo] = face.verts[v].pos;
					normals_lo[counter_lo] = normals[counter];//(face.verts[v*2].normal + Vector3(Random.value, Random.value, Random.value)*0.25f).normalized;
					colors_lo[counter_lo] = colors[counter];
					tangents_lo[counter_lo] = tangents[counter];
					
					face.verts[v].num_lo = counter_lo;
					
					counter_lo++;
				}
				
				face.verts[v].processed = true;
				counter++;	
				
			}
			
			//resetting processed
			//	for (v=0;v<9;v++)
			//		visibleFaces[f].verts[v].processed = false;
			
			if (!land.weldVerts)
			{
				//uvs
				Random.seed = face.block.x*1000 + face.block.y*100 + face.block.z*10 + face.dir;
				float uStep = (Mathf.Floor(Random.value*4)) * 0.25f;
				float vStep = (Mathf.Floor(Random.value*4)) * 0.25f;
				
				uvs[f*9] = new Vector2(uStep+0.25f,vStep);
				uvs[f*9+1] = new Vector2(uStep+0.125f,vStep);
				uvs[f*9+2] = new Vector2(uStep,vStep);
				uvs[f*9+3] = new Vector2(uStep,vStep+0.125f);
				uvs[f*9+4] = new Vector2(uStep,vStep+0.25f);
				uvs[f*9+5] = new Vector2(uStep+0.125f,vStep+0.25f);
				uvs[f*9+6] = new Vector2(uStep+0.25f,vStep+0.25f);
				uvs[f*9+7] = new Vector2(uStep+0.25f,vStep+0.125f);
				uvs[f*9+8] = new Vector2(uStep+0.125f,vStep+0.125f);
				
				uvs_lo[f*4] = uvs[f*9];
				uvs_lo[f*4+1] = uvs[f*9+2];
				uvs_lo[f*4+2] = uvs[f*9+4];
				uvs_lo[f*4+3] = uvs[f*9+6];
				
				//lightmap
				int rowNum = Mathf.FloorToInt(f/lightmapTile);
				int lineNum = (int)(f-rowNum*lightmapTile);
				
				uv1[f*9] = new Vector2(lineNum*step+padding, rowNum*step+padding);
				uv1[f*9+2] = new Vector2(lineNum*step+step-padding, rowNum*step+padding);
				uv1[f*9+4] = new Vector2(lineNum*step+step-padding, rowNum*step+step-padding);
				uv1[f*9+6] = new Vector2(lineNum*step+padding, rowNum*step+step-padding);
				uv1[f*9+1] = (uv1[f*9] + uv1[f*9+2]) * 0.5f;
				uv1[f*9+3] = (uv1[f*9+2] + uv1[f*9+4]) * 0.5f;
				uv1[f*9+5] = (uv1[f*9+4] + uv1[f*9+6]) * 0.5f;
				uv1[f*9+7] = (uv1[f*9] + uv1[f*9+6]) * 0.5f;
				uv1[f*9+8] = (uv1[f*9] + uv1[f*9+2] + uv1[f*9+4] + uv1[f*9+6]) * 0.25f;
			}
			
			//triangles
			if (!land.weldVerts)
			{
				tris[f*24] = f*9+7;    tris[f*24+1] = f*9;     tris[f*24+2] = f*9+1;
				tris[f*24+3] = f*9+1;  tris[f*24+4] = f*9+8;   tris[f*24+5] = f*9+7;
				
				tris[f*24+6] = f*9+8;  tris[f*24+7] = f*9+1;   tris[f*24+8] = f*9+2;
				tris[f*24+9] = f*9+2;  tris[f*24+10] = f*9+3;  tris[f*24+11] = f*9+8;
				
				tris[f*24+12] = f*9+5; tris[f*24+13] = f*9+8;  tris[f*24+14] = f*9+3;
				tris[f*24+15] = f*9+3; tris[f*24+16] = f*9+4;  tris[f*24+17] = f*9+5;
				
				tris[f*24+18] = f*9+6; tris[f*24+19] = f*9+7;  tris[f*24+20] = f*9+8;
				tris[f*24+21] = f*9+8; tris[f*24+22] = f*9+5;  tris[f*24+23] = f*9+6;
				
				tris_lo[f*6] = f*4;       tris_lo[f*6+1] = f*4 + 1;   tris_lo[f*6+2] = f*4 + 3;
				tris_lo[f*6+3] = f*4 + 1; tris_lo[f*6+4] = f*4 + 2;   tris_lo[f*6+5] = f*4 + 3;
			}
			else
			{
				tris[f*24] = face.verts[7].num;    tris[f*24+1] = face.verts[0].num;     tris[f*24+2] = face.verts[1].num;
				tris[f*24+3] = face.verts[1].num;  tris[f*24+4] = face.verts[8].num;   tris[f*24+5] = face.verts[7].num;
				
				tris[f*24+6] = face.verts[8].num;  tris[f*24+7] = face.verts[1].num;   tris[f*24+8] = face.verts[2].num;
				tris[f*24+9] = face.verts[2].num;  tris[f*24+10] = face.verts[3].num;  tris[f*24+11] = face.verts[8].num;
				
				tris[f*24+12] = face.verts[5].num; tris[f*24+13] = face.verts[8].num;  tris[f*24+14] = face.verts[3].num;
				tris[f*24+15] = face.verts[3].num; tris[f*24+16] = face.verts[4].num;  tris[f*24+17] = face.verts[5].num;
				
				tris[f*24+18] = face.verts[6].num; tris[f*24+19] = face.verts[7].num;  tris[f*24+20] = face.verts[8].num;
				tris[f*24+21] = face.verts[8].num; tris[f*24+22] = face.verts[5].num;  tris[f*24+23] = face.verts[6].num;
				
				tris_lo[f*6] = face.verts[0].num_lo;      tris_lo[f*6+1] = face.verts[2].num_lo;   tris_lo[f*6+2] = face.verts[6].num_lo;
				tris_lo[f*6+3] = face.verts[2].num_lo; tris_lo[f*6+4] = face.verts[4].num_lo;    tris_lo[f*6+5] = face.verts[6].num_lo; 
			}
		}
		
		Profiler.EndSample ();
		#endregion
		
		#region Dropping lopoly boundary verts
		for (v=0; v<vertices_lo.Length; v++)
		{
			if (vertices_lo[v].x < land.overlap+0.5f ||
				vertices_lo[v].x > size - land.overlap - 0.5f ||
				vertices_lo[v].z < land.overlap+0.5f ||
				vertices_lo[v].z > size - land.overlap - 0.5f) 
					vertices_lo[v] -= normals_lo[v]*0.1f; //new Vector3(vertices_lo[v].x, vertices_lo[v].y-0.2f, vertices_lo[v].z);
		}
		#endregion
		
		#region Creating New Meshes
		Mesh hi = hiFilter.sharedMesh;
		Mesh lo = loFilter.sharedMesh;
		#endregion
		
		#region Assignin Arrays
		hi.vertices = vertices; 
		hi.normals = normals;
		hi.uv = uvs;
		hi.uv1 = uv1;
		hi.tangents = tangents;
		hi.colors = colors;
		hi.triangles = tris;
		
		lo.vertices = vertices_lo; 
		lo.normals = normals_lo;
		lo.uv = uvs_lo;
		lo.uv1 = uv1_lo;
		lo.tangents = tangents_lo;
		lo.colors = colors_lo;
		lo.triangles = tris_lo;
		
		hi.RecalculateBounds();
		lo.RecalculateBounds();
		//hi.RecalculateNormals();
		
		normals = null;
		uvs = null;
		uv1 = null;
		tangents = null;
		colors = null;
		tris = null;
		
		vertices_lo = null; 
		normals_lo = null;
		uvs_lo = null;
		uv1_lo = null;
		tangents_lo = null;
		colors_lo = null;
		tris_lo = null;
		#endregion
		
		#region Reset Collider
		Profiler.BeginSample ("Reset Collider");
		if (land.generateCollider) collision.sharedMesh = loFilter.sharedMesh;
		Profiler.EndSample ();
		#endregion
		
		#region Assigning Material
		Material material = hiFilter.renderer.sharedMaterial;
		if (!material) 
		{
			if (land.landShader!=null) material = new Material(land.landShader);
			else material = new Material(Shader.Find("VertexLit"));
			hiFilter.renderer.sharedMaterial = material;
		}
		if (loFilter.renderer.sharedMaterials.Length != 0) loFilter.renderer.sharedMaterial = material;

		material.SetColor("_Ambient", land.landAmbient);
		material.SetColor("_SpecColor", land.landSpecular);
		material.SetFloat("_Shininess", land.landShininess);
		material.SetFloat("_BakeAmbient", land.landBakeAmbient);
		material.SetFloat("_Tile", land.landTile);
		
		for (int t=0; t<usedTextureList.Count; t++)
		{
			if (t >= shaderMainTexNames.Length) break;
			material.SetTexture(shaderMainTexNames[t], usedTextureList[t]);
			material.SetTexture(shaderBumpTexNames[t], usedBumpList[t]);
		}
		#endregion

		#region Hiding wireframe
		#if UNITY_EDITOR
		if (land.hideWire)
		{
			UnityEditor.EditorUtility.SetSelectedWireframeHidden(hiFilter.renderer, true);
			UnityEditor.EditorUtility.SetSelectedWireframeHidden(loFilter.renderer, true); 
		}
		#endif
		#endregion

		#if UNITY_EDITOR
		if (land.generateLightmaps) UnityEditor.Unwrapping.GenerateSecondaryUVSet(hiFilter.sharedMesh);
		#endif

		terrainProgress = Progress.applied;
	}
	
	
	public void Release ()
	{
		if (hiFilter.sharedMesh != null) DestroyImmediate(hiFilter.sharedMesh);
		if (loFilter.sharedMesh != null) DestroyImmediate(loFilter.sharedMesh);
		
		visibleFaces = null;
	}
	
	#endregion
	
	
	#region Ambient
	
	public void CalculateAmbient (object stateInfo) { CalculateAmbient(); }
	public void CalculateAmbient ()
	{
		ambientProgress = Progress.threadStarted;
		
		//initializing repeated vars
		byte max = 250;
		int x; int y; int z; int i; int j;
		int itaration=0;
		
		#region Finding Top and Bottom
		if (land.profile) Profiler.BeginSample ("Top and Bottom");
		
		ambientSize= size + (land.ambientMargins-land.overlap)*2;
		ambientTopPoint = land.data.GetTopPoint(offsetX, offsetZ,
			offsetX+land.chunkSize, offsetZ+land.chunkSize) +2; //+1 to make non-inclusive, and +1 to place top-light
		ambientBottomPoint = Mathf.Max(0,
			land.data.GetBottomPoint(offsetX, offsetZ,
			offsetX+land.chunkSize, offsetZ+land.chunkSize)-1);
		ambientHeight = ambientTopPoint-ambientBottomPoint;
		
		if (land.profile) Profiler.EndSample ();
		#endregion
		
		#region Preparing Arrays
		if (land.profile) Profiler.BeginSample ("Preparing Arrays");
		
		//re-creating arrays if their size do not match (usually when ambientHeight changed)
		int arrayLength = ambientSize*ambientSize*ambientHeight;
		if (ambient == null || ambient.Length != arrayLength) 
		{
			ambient = new byte[arrayLength];
			ambientBool = new bool[arrayLength];
			ambientExist = new bool[arrayLength];
		}
		
		//calculating exists array
		int ambientOffsetX= offsetX - land.ambientMargins + land.overlap;
		int ambientOffsetZ= offsetZ - land.ambientMargins + land.overlap;
		
		if (land.profile) Profiler.EndSample ();
		#endregion
		
		#region Get Exist
		if (land.profile) Profiler.BeginSample ("Get Exist");
		
		land.data.GetExistMatrix(ambientExist,
			ambientOffsetX, ambientBottomPoint, ambientOffsetZ,
			ambientOffsetX+ambientSize, ambientTopPoint, ambientOffsetZ+ambientSize);	
		
		if (land.profile) Profiler.EndSample ();
		#endregion

		#region Casting Top Light
		if (land.profile) Profiler.BeginSample ("Casting Top Light");
		
		for (x=0; x<ambientSize; x++) 
			for (z=0; z<ambientSize; z++) 
				ambientBool[z*ambientHeight*ambientSize + (ambientHeight-1)*ambientSize + x] = true;
		
		if (land.profile) Profiler.EndSample ();
		#endregion
		
		#region Top Light Pyramid
		if (land.profile) Profiler.BeginSample ("Top Light Pyramid");
		
		//filling ambient bool array
		for (y=ambientHeight-2; y>=0; y--)
			for (x=0; x<ambientSize; x++)
				for (z=0; z<ambientSize; z++)
			{
				i = z*ambientHeight*ambientSize + y*ambientSize + x;
				ambientBool[i] = false; //clearing old array
				
				if (ambientExist[i]) continue;
				if (x==0||x==ambientSize-1||z==0||z==ambientSize-1) { ambientBool[i] = true; continue; }
				
				if (ambientBool[i+ambientSize]) //if block above has light
				{
					if (y%2 == 0) ambientBool[i] = true; //shrinking pyramid every second line
					else if(ambientBool[i+ambientSize+1] && ambientBool[i+ambientSize-1] &&
							ambientBool[i+ambientSize+(ambientHeight*ambientSize)] && ambientBool[i+ambientSize-(ambientHeight*ambientSize)])
								ambientBool[i] = true;
					else ambientBool[i] = false;
				}
			}
		
		if (land.profile) Profiler.EndSample ();
		#endregion

		#region Other Pyramids (commented out)
		/*
		//baking values from exist to ambient
		//for (i=0; i<ambient.Length; i++) { if (ambientExist[i]) ambient[i] = max; else ambient[i] = 0; }
		
		for (i=0; i<ambient.Length; i++) { if (ambientBool[i]) ambient[i] = max; ambientBool[i] = false; }
		for (x=0; x<ambientSize; x++) for (z=0; z<ambientSize; z++) ambientBool[z*ambientHeight*ambientSize + (ambientHeight-1)*ambientSize + x] = true;
			
		//positive z light pyramid
		for (y=ambientHeight-2; y>=0; y--)
			for (x=1; x<ambientSize-1; x++)
				for (z=1; z<chunkEnd; z++)
		{
			i = z*ambientHeight*ambientSize + y*ambientSize + x;
	
			if (!exists[i])
			{
				if (ambientBool[i+ambientSize-ambientHeight*ambientSize] &&
					ambientBool[i+1+ambientSize] &&
					ambientBool[i-1+ambientSize]) ambientBool[i]=true;
	
				if (z%2 && !ambientBool[i-ambientHeight*ambientSize]) ambientBool[i]=false; //shrinking by z every 2 line
				if (y%4 && !ambientBool[i+ambientSize]) ambientBool[i]=false; //shrinking by y every 4 line
			}
		}
		for (i=0; i<ambient.Length; i++) { if (ambientBool[i]) ambient[i] = max; ambientBool[i] = false; }
		for (x=0; x<ambientSize; x++) for (z=0; z<ambientSize; z++) ambientBool[z*ambientHeight*ambientSize + (ambientHeight-1)*ambientSize + x] = true;
		
		//negative z light pyramid
		for (y=ambientHeight-2; y>=0; y--)
			for (x=1; x<ambientSize-1; x++)
				for (z=ambientSize-2; z>=chunkStart; z--)
		{
			i = z*ambientHeight*ambientSize + y*ambientSize + x;
	
			if (!exists[i])
			{
				if (ambientBool[i+ambientSize+ambientHeight*ambientSize] &&
					ambientBool[i+1+ambientSize] &&
					ambientBool[i-1+ambientSize]) ambientBool[i]=true;
	
				if (z%2 && !ambientBool[i+ambientHeight*ambientSize]) ambientBool[i]=false; //shrinking by z every 2 line
				if (y%4 && !ambientBool[i+ambientSize]) ambientBool[i]=false; //shrinking by y every 4 line
			}
		}
		for (i=0; i<ambient.Length; i++) { if (ambientBool[i]) ambient[i] = max; ambientBool[i] = false; }
		for (x=0; x<ambientSize; x++) for (z=0; z<ambientSize; z++) ambientBool[z*ambientHeight*ambientSize + (ambientHeight-1)*ambientSize + x] = true;
		
		
		//positive x light pyramid
		for (y=ambientHeight-2; y>=0; y--)
			for (x=1; x<chunkEnd; x++)
				for (z=1; z<ambientSize-1; z++)
		{
			i = z*ambientHeight*ambientSize + y*ambientSize + x;
	
			if (!exists[i])
			{
				if (ambientBool[i+ambientSize-1] &&
					ambientBool[i+ambientHeight*ambientSize+ambientSize] &&
					ambientBool[i-ambientHeight*ambientSize+ambientSize]) ambientBool[i]=true;
	
				if (x%2 && !ambientBool[i-1]) ambientBool[i]=false; //shrinking by z every 2 line
				if (y%4 && !ambientBool[i+ambientSize]) ambientBool[i]=false; //shrinking by y every 4 line
			}
		}
		for (i=0; i<ambient.Length; i++) { if (ambientBool[i]) ambient[i] = max; ambientBool[i] = false; }
		for (x=0; x<ambientSize; x++) for (z=0; z<ambientSize; z++) ambientBool[z*ambientHeight*ambientSize + (ambientHeight-1)*ambientSize + x] = true;
		
		//negative x light pyramid
		for (y=ambientHeight-2; y>=0; y--)
			for (x=ambientSize-2; x>=chunkStart; x--)
				for (z=1; z<ambientSize-1; z++) //for (z=chunkStart; z<chunkEnd; z++)
		{
			i = z*ambientHeight*ambientSize + y*ambientSize + x;
	
			if (!exists[i])
			{
				if (ambientBool[i+ambientSize+1] &&
					ambientBool[i+ambientHeight*ambientSize+ambientSize] &&
					ambientBool[i-ambientHeight*ambientSize+ambientSize]) ambientBool[i]=true;
	
				//if (z!=chunkStart && z!=chunkEnd-1 &&
				//	(!ambientBool[i+ambientHeight*ambientSize+ambientSize] ||
				//	!ambientBool[i-ambientHeight*ambientSize+ambientSize])) ambientBool[i]=false;
				
				if (x%2 && !ambientBool[i+1]) ambientBool[i]=false; //shrinking by z every 2 line
				if (y%4 && !ambientBool[i+ambientSize]) ambientBool[i]=false; //shrinking by y every 4 line
			}
		}
		for (i=0; i<ambient.Length; i++) { if (ambientBool[i]) ambient[i] = max; ambientBool[i] = false; }
		for (x=0; x<ambientSize; x++) for (z=0; z<ambientSize; z++) ambientBool[z*ambientHeight*ambientSize + (ambientHeight-1)*ambientSize + x] = true;
		*/
		#endregion
	
		#region Removing Single Blocks
		if (land.profile) Profiler.BeginSample ("Removing Single Blocks");
		
		int numLightedNeigs = 0;
		for (itaration=0; itaration<2; itaration++)
		{
			//taking values from bool ambient, setting to ambient
			for (y=1; y<ambientHeight-1; y++)
				for (x=1; x<ambientSize-1; x++)
					for (z=1; z<ambientSize-1; z++)
				{
					i = z*ambientHeight*ambientSize + y*ambientSize + x;
					numLightedNeigs = 0;
					
					if (!ambientBool[i]) {ambient[i]=0; continue;}
					
					if (ambientBool[i+1]) numLightedNeigs++;
					if (ambientBool[i-1]) numLightedNeigs++;
					if (ambientBool[i+ambientSize]) numLightedNeigs++;
					if (ambientBool[i-ambientSize]) numLightedNeigs++;
					if (ambientBool[i+ambientHeight*ambientSize]) numLightedNeigs++;
					if (ambientBool[i-ambientHeight*ambientSize]) numLightedNeigs++;
					
					if (numLightedNeigs <= 2) ambient[i]=0;
					else ambient[i] = max;
				}
				
			//copy from ambient to bool ambient
			for (i=0; i<ambient.Length; i++) { if (ambient[i] > 1) ambientBool[i] = true; else ambientBool[i] = false; }
		}
		
		//returning top-light
		for (x=0; x<ambientSize; x++) 
			for (z=0; z<ambientSize; z++) 
				ambient[z*ambientHeight*ambientSize + (ambientHeight-1)*ambientSize + x] = max;
		
		if (land.profile) Profiler.EndSample ();
		#endregion
		
		#region Spreading
		if (land.profile) Profiler.BeginSample ("Spreading");
		
		int delta = Mathf.CeilToInt(1.0f*max/(land.ambientSpread+1));
		
		for (itaration=0; itaration<2; itaration++)
		{				
			//blurring x
			for (y=0; y<ambientHeight; y++)
				for (z=0; z<ambientSize; z++)
			{
				j = z*ambientHeight*ambientSize + y*ambientSize;
				
				for (x=1; x<ambientSize-1; x++)
				{
					i = j+x;
					if (ambientExist[i] || ambient[i] == max) continue;
					ambient[i] = (byte)Mathf.Max(ambient[i], ambient[i-1]-delta);
				}
				
				for (x=ambientSize-2; x>0; x--)
				{
					i = j+x;
					if (ambientExist[i] || ambient[i] == max) continue;
					ambient[i] = (byte)Mathf.Max(ambient[i], ambient[i+1]-delta);
				}
			}
			 
			//blurring z
			for (y=0; y<ambientHeight; y++)
				for (x=0; x<ambientSize; x++) //TODO: should use not all the ambient width, but only chunk part
			{
				j = y*ambientSize + x;
				
				for (z=1; z<ambientSize-1; z++)
				{
					i = j+ambientHeight*ambientSize*z;
					if (ambientExist[i] || ambient[i] == max) continue;
					//ambient[i] = Mathf.Max(ambient[i], (ambient[i] + ambient[i+ambientHeight*ambientSize*(z-1)])*0.5f); //Quadratic blur
					ambient[i] = (byte)Mathf.Max(ambient[i], ambient[i-ambientHeight*ambientSize]-delta); //prew-delta. Linear blur
				}
				
				for (z=ambientSize-2; z>0; z--)
				{
					i = j+ambientHeight*ambientSize*z;
					if (ambientExist[i] || ambient[i] == max) continue;
					ambient[i] = (byte)Mathf.Max(ambient[i], ambient[i+ambientHeight*ambientSize]-delta);
				}
			}
			
			//blurring y
			for (z=0; z<ambientSize; z++)
				for (x=0; x<ambientSize; x++)
			{
				j = z*ambientHeight*ambientSize + x;
				
				for (y=1; y<ambientHeight-1; y++)
				{
					i = j+ambientSize*y;
					if (ambientExist[i] || ambient[i] == max) continue;
					ambient[i] = (byte)Mathf.Max(ambient[i], ambient[i-+ambientSize]-delta);
				}
				
				for (y=ambientHeight-2; y>0; y--)
				{
					i = j+ambientSize*y;
					if (ambientExist[i] || ambient[i] == max) continue;
					ambient[i] = (byte)Mathf.Max(ambient[i], ambient[i+ambientSize]-delta);
				}
			}
		}
		
		if (land.profile) Profiler.EndSample ();
		#endregion
	
		#region Blurring
		if (land.profile) Profiler.BeginSample ("Blurring");
		
		for (itaration=0; itaration<2; itaration++)
		for (y=ambientHeight-2; y>=1; y--)
			for (z=1; z<ambientSize-1; z++)
				for (x=1; x<ambientSize-1; x++)
			{
				i = z*ambientHeight*ambientSize + y*ambientSize + x;
				if (ambientExist[i]) continue;
				
				int sum = ambient[i]*2;
				int div = 2;
				
				if (!ambientExist[i-1]) { sum+=ambient[i-1]; div++; }
				if (!ambientExist[i+1]) { sum+=ambient[i+1]; div++; }
				if (!ambientExist[i-ambientSize]) { sum+=ambient[i-ambientSize]; div++; }
				if (!ambientExist[i+ambientSize]) { sum+=ambient[i+ambientSize]; div++; }
				if (!ambientExist[i-ambientHeight*ambientSize]) { sum+=ambient[i-ambientHeight*ambientSize]; div++; }
				if (!ambientExist[i+ambientHeight*ambientSize]) { sum+=ambient[i+ambientHeight*ambientSize]; div++; }
				
				ambient[i] = (byte)(1.0f*sum/div);
			}

		if (land.profile) Profiler.EndSample ();
		#endregion
		
		ambientProgress = Progress.calculated;
	}
		
	public void ApplyAmbient () 
	{
		byte max = 250;

		#region Setting Face Ambient (to use vert avg)
		Profiler.BeginSample ("Setting Face Ambient");
		
		for (int f=0; f<visibleFaces.Count; f++)
		{
			VoxelandBlock block = visibleFaces[f].block;
			
			//chunk coords to ambient
			int oppositeX=block.x+land.ambientMargins-land.overlap; 
			int oppositeY=block.y-ambientBottomPoint; 
			int oppositeZ=block.z+land.ambientMargins-land.overlap;
			
			switch (visibleFaces[f].dir)
			{
			case 0: oppositeY++; break;
			case 1: oppositeY--; break;
			case 2: oppositeX++; break;
			case 3: oppositeX--; break;
			case 4: oppositeZ--; break;
			case 5: oppositeZ++; break;
			}
			
			int i = oppositeZ*ambientHeight*ambientSize + oppositeY*ambientSize + oppositeX;
			visibleFaces[f].ambient = 1.0f*ambient[i]/max; //(1.0f*ambient[i]*ambient[i])/(max*max);	
		}
		
		Profiler.EndSample();
		#endregion
		
		#region Setting Vert Colors
		Profiler.BeginSample ("Setting Vert Colors");
		
		Color[] colors = hiFilter.sharedMesh.colors;
		Color[] colors_lo = loFilter.sharedMesh.colors;
		
		VoxelandVert.ResetProcessed(visibleFaces);
		
		if (!land.weldVerts) //using counter if verts not welded
		{
			int counter = 0;
			int counter_lo = 0;
			for (int f=0; f<visibleFaces.Count; f++)
			{
				VoxelandFace face = visibleFaces[f];
				
				for (int v=0;v<9;v++)
				{
					colors[counter].a = face.verts[v].GetAmbient(); // [face.verts[v].num]
					if (v==0 || v==2 || v==4 || v==6) 
					{
						colors_lo[counter_lo].a = colors[counter].a; //[face.verts[v].num_lo]
						counter_lo++;
					}
					counter++;
				} 
			}
		}
		
		else //using num if verts are welded
		{
			for (int f=0; f<visibleFaces.Count; f++)
			{
				VoxelandFace face = visibleFaces[f];
				
				for (int v=0;v<9;v++)
				{
					colors[ face.verts[v].num ].a = face.verts[v].GetAmbient(); // [face.verts[v].num]
					if (v==0 || v==2 || v==4 || v==6) 
						colors_lo[ face.verts[v].num_lo ].a = colors[ face.verts[v].num ].a; //[face.verts[v].num_lo]
				} 
			}
		}

		hiFilter.sharedMesh.colors = colors; 
		loFilter.sharedMesh.colors = colors_lo;
		
		Profiler.EndSample ();
		#endregion
		
		ambientProgress = Progress.applied;
	}
	
	#endregion
	
	#region BuildGrass
	
	public void  BuildGrass ()
	{
		int[] faceVert0 = {0,1,7,8};
		int[] faceVert1 = {1,2,8,3};
		int[] faceVert2 = {8,3,5,4};
		int[] faceVert3 = {7,8,6,5};
		
		List<Vector3> verts = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<Color> colors = new List<Color>();
		List<List<int>> tris = new List<List<int>>();
		byte[] typeToSubmesh = new byte[128]; for (int i=0; i<typeToSubmesh.Length; i++) typeToSubmesh[i] = 250; //250 is empty

		#region Verts and tris
		for (int f=0; f<visibleFaces.Count; f++) 
		{
			if (!visibleFaces[f].block.visible) continue;
			if (!visibleFaces[f].block.type.grass) continue;
			if (visibleFaces[f].verts[8].normal.y<0.7f) continue;
			
			VoxelandFace face = visibleFaces[f];
				
			byte grassType = land.data.GetGrass(face.block.x+offsetX, face.block.z+offsetZ);
			if (grassType == 0) continue;

			Random.seed = face.block.x*1000 + face.block.y*100 + face.block.z*10 + face.dir;
			//if (Random.value>0.33f) continue;
			
			for (int i=0; i<4; i++)
			{
				#region Verts
				//positions
				Vector3 v0 = face.verts[faceVert0[i]].pos;
				Vector3 v1 = face.verts[faceVert1[i]].pos;
				Vector3 v2 = face.verts[faceVert2[i]].pos;
				Vector3 v3 = face.verts[faceVert3[i]].pos;
				
				verts.AddRange( new Vector3[] { 
					v0 + (v0-v2)*1.2f + new Vector3(0,-0.1f,0),
					(v0+v2)*0.5f + new Vector3(0,0.5f+Random.value*0.4f,0),
					v2 - (v0-v2)*1.2f + new Vector3(0,-0.1f,0),
				
					v1 + (v1-v3)*1.2f + new Vector3(0,-0.1f,0),
					(v1+v3)*0.5f + new Vector3(0,0.5f+Random.value*0.4f,0),
					v3 - (v1-v3)*1.2f + new Vector3(0,-0.1f,0) });
				
				
				//uvs
				float uStep = (Mathf.Floor(Random.value*2)) * 0.5f;
				float vStep = (Mathf.Floor(Random.value*2)) * 0.5f;
				
				uvs.AddRange( new Vector2[] { 
					new Vector2(uStep,vStep), 
					new Vector2(uStep+0.5f,vStep), 
					new Vector2(uStep+0.5f,vStep+0.5f) }); 
				
				uStep = (Mathf.Floor(Random.value*2)) * 0.5f;
				vStep = (Mathf.Floor(Random.value*2)) * 0.5f;
				
				uvs.AddRange( new Vector2[] { 
					new Vector2(uStep+0.5f,vStep+0.5f),
					new Vector2(uStep,vStep+0.5f),
					new Vector2(uStep,vStep) });
				
				
				//normals and colors
				for (int j=0;j<6;j++)
				{
					normals.Add(face.verts[8].normal.normalized);
					
					//color is vert animation strength (and alpha is ambient)
					if (j==1) colors.Add( new Color(Random.value*0.5f+0.5f,Random.value,Random.value*0.5f+0.5f,face.ambient) );
					else if (j==4) colors.Add( new Color(Random.value*0.5f,Random.value,Random.value*0.5f+0.5f,face.ambient) );
					else colors.Add( new Color(0.5f,0,0.5f,face.ambient) );
				}
				#endregion
				
				#region Tris
				if (typeToSubmesh[grassType] == 250) 
				{
					typeToSubmesh[grassType] = (byte)tris.Count;
					tris.Add(new List<int>());
				}
				
				int vert = verts.Count-1;
				tris[ typeToSubmesh[grassType] ].AddRange( new int[] {
					vert-5, vert-4, vert-3, vert-2, vert-1, vert });
				#endregion
			}
		}
		#endregion
		
		#region Materials
		//calculating materials num
		int matNum = 0;
		for (int i=0; i<typeToSubmesh.Length; i++) if (typeToSubmesh[i] != 250) matNum++;
		
		if (tris.Count != 0)
		{
			if (grassFilter == null) grassFilter = CreateFilter("Grass");
			grassFilter.renderer.castShadows = false;
			Material[] grassMaterials = new Material[tris.Count];
			
			matNum = 0; 
			for (int h=0; h<typeToSubmesh.Length; h++)
			{
				if (typeToSubmesh[h] == 250) continue;
				if (land.grass.Length <= h) continue;
				
				grassMaterials[typeToSubmesh[h]] = land.grass[h].material;
				matNum++; 
			}
			grassFilter.renderer.sharedMaterials = grassMaterials;
		}
		#endregion

		
		if (grassFilter != null) //if some constructor is found - setting mesh. No need to check collider
			ApplyMesh(grassFilter.sharedMesh, verts, normals, uvs, colors, tris);
		
		grassProgress = Progress.applied;
		
		
		/*
		VoxelandData data = land.data;
		VoxelandBlockType[] types = land.types;
		
		int[] faceVert0 = {0,1,7,8};
		int[] faceVert1 = {1,2,8,3};
		int[] faceVert2 = {8,3,5,4};
		int[] faceVert3 = {7,8,6,5};
		

		
		//adding or deleting grass mesh

		if (num==0 && grassFilter!=null) { DestroyImmediate(grassFilter.gameObject); }
		if (num==0) return;
		
		//if (land.grassMaterial!=null) land.grassMaterial.SetColor("_Ambient", land.landAmbient);
				//obj.renderer.castShadows = false;
		
		grassFilter.sharedMesh = new Mesh ();
		grassFilter.sharedMesh.Clear();
		
		//init arrays
		Vector3[] grassVerts = new Vector3[num*24];
		Vector3[] grassNormals = new Vector3[num*24];
		Color[] grassColors = new Color[num*24];
		Vector2[] grassUvs = new Vector2[num*24];
		int[] grassTris = new int[num*24];
		

		//hiding wireframe
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetSelectedWireframeHidden(grassFilter.renderer, true);
		#endif
		*/
	}
	
	#endregion
	
	#region BuildPrefabs
	
	public void BuildPrefabs () //re-creating not all the prefabs, but only non-existing
	{
		if (prefabs==null) prefabs = new System.Collections.Generic.Dictionary<int,Transform>();
		if (prefabTypes==null) prefabTypes = new System.Collections.Generic.Dictionary<int,int>();
		
		int topPoint = land.data.GetTopPoint(offsetX, offsetZ, offsetX+size, offsetZ+size)+1;
		int bottomPoint = Mathf.Max(0, land.data.GetBottomPoint(offsetX, offsetZ, offsetX+size, offsetZ+size)-2);
		
		for (int y=bottomPoint; y<topPoint; y++)
			for (int x=land.overlap; x<size-land.overlap; x++)	
				for (int z=land.overlap; z<size-land.overlap; z++)
			{
				int type = land.data.GetBlock(x+offsetX,y,z+offsetZ);
				bool  hasPrefab = (land.types.Length > type && land.types[type].obj!=null);
				int coord = x*1000000 + y*1000 + z;
				Transform prefab;
				prefabs.TryGetValue(coord, out prefab);
				
				//removing prefab if it is not in data
				if (!hasPrefab && prefab!=null) DestroyImmediate(prefab.gameObject);
				
				//removing prefab if it has wrong type
				if (hasPrefab && prefab!=null)
					if (type != prefabTypes[coord]) DestroyImmediate(prefab.gameObject);
				
				//creating prefab
				if (hasPrefab && !prefab)
				{
					Transform objTfm = (Transform)Instantiate(land.types[type].obj, new Vector3(0,0,0), Quaternion.identity);
					objTfm.parent = transform;
					objTfm.localPosition = new Vector3(x+0.5f,y,z+0.5f);
					prefabs.Add(coord, objTfm);
					prefabTypes.Add(coord, type);
				}
			}
			
		prefabsProgress = Progress.applied;
	}
	

	
	#endregion
	
	#region BuildConstructor
	/*
	public void BuildConstructor () //DungeonConstructor
	{
		Constructor constructor = null;
		
		List<Vector3> verts = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> tris = new List<int>();
		
		List<Vector3> colliderVerts = new List<Vector3>();
		List<int> colliderTris = new List<int>();
		
		for (int y=0; y<height; y++)
			for (int x=land.overlap; x<size-land.overlap; x++)	
				for (int z=land.overlap; z<size-land.overlap; z++)
			{
				int type = land.data.GetBlock(x+offsetX,y,z+offsetZ);
				if (land.types.Length <= type || land.types[type].visible || land.types[type].constructor == null) continue; //if not a constructor
				
				constructor = land.types[type].constructor;

				if (constructorFilter == null) 
				{
					constructorFilter = CreateFilter("Constructor");
					if (constructor.material != null) constructorFilter.renderer.sharedMaterial = constructor.material;
				}
				
				if (constructorCollider == null) constructorCollider = CreateCollider(constructorFilter);
				
				//finding element
				int rotation;
				ConstructorElement element = constructor.GetElement(land.data, x+offsetX,y,z+offsetZ, out rotation);
				element.ToMesh(rotation, false, x,y,z, verts, uvs, tris);
				element.ToMesh(rotation, true, x,y,z, colliderVerts, uvs, colliderTris);
			}
		
		
		if (constructorFilter != null) //if some constructor is found - setting mesh. No need to check collider
		{	
			//ApplyMesh(constructorFilter.sharedMesh, verts, null, null, null, tris);

			Mesh constructorColliderMesh = constructorCollider.sharedMesh;
			//ApplyMesh(constructorColliderMesh, verts, null, null, null, tris);
			constructorCollider.sharedMesh = null;
			constructorCollider.sharedMesh = constructorColliderMesh;
		}
		
		constructorProgress = Progress.applied;
	}
	*/
	#endregion
	
	public MeshFilter CreateFilter (string name) //creates new object with filter and mesh
	{
		GameObject obj = new GameObject(name);
		obj.transform.parent = transform;
		obj.transform.localPosition = new Vector3(0,0,0);
		obj.transform.localScale = new Vector3(1,1,1);
		obj.layer = gameObject.layer;
		MeshFilter objFilter = obj.AddComponent<MeshFilter>();
		obj.AddComponent<MeshRenderer>();
		objFilter.sharedMesh = new Mesh ();

		//if (land==null) land = transform.parent.GetComponent<Voxeland>();
		//if (land!=null) obj.renderer.material = mat;
		
		#if UNITY_EDITOR
		//hiding wireframe
		UnityEditor.EditorUtility.SetSelectedWireframeHidden(objFilter.renderer, true);
		
		//copy static flag
		UnityEditor.StaticEditorFlags flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(land.gameObject);
		UnityEditor.GameObjectUtility.SetStaticEditorFlags(obj, flags);
		#endif
		
		return objFilter;
	}
	
	public MeshCollider CreateCollider (MeshFilter filter) //creates new object with filter and mesh
	{
		MeshCollider collider = filter.gameObject.AddComponent<MeshCollider>();
		collider.sharedMesh = new Mesh ();
		return collider;
	}
	
	public void ApplyMesh (Mesh mesh, List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs, List<Color> colors, List<List<int>> tris)	
	{
		mesh.Clear();
		
		//setting verts
		mesh.vertices = verts.ToArray();
		if (normals != null) mesh.normals = normals.ToArray();
		if (colors != null) mesh.colors = colors.ToArray();
		if (uvs != null) mesh.uv = uvs.ToArray();
			else 
			{ 
				Vector2[] tempuv = new Vector2[verts.Count]; 
				for (int i=0; i<tempuv.Length; i++) tempuv[i] = new Vector2(0,0);
				mesh.uv = tempuv;
			}
		
		//setting triangles
		mesh.subMeshCount = tris.Count;
		for (int i=0; i<tris.Count; i++)
		{
			mesh.SetTriangles(tris[i].ToArray(), i);
		}
		
		mesh.RecalculateBounds();
		if (normals == null) mesh.RecalculateNormals(); //if not custom normals - recalculating them
	}
}

} //namespace




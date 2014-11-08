
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Voxeland 
{
	[System.Serializable]
	public class VoxelandBlockType
	{
		#region class BlockType
		public string name;
		
		public bool filled;
		//public bool visible = true;
		public bool differentTop;
		
		//public Constructor constructor; //DungeonConstructor
		public Texture texture;
		public Texture bumpTexture;
		public Texture topTexture;
		public Texture topBumpTexture;
		
		public bool  grass;
		public Texture grassTexture;
		
		public float smooth = 1f; 
		
		public Transform obj; 
		
		public VoxelandBlockType (string n, bool f) { name = n; filled = f; }
		#endregion
	}
	
	[System.Serializable]
	public class VoxelandGrassType 
	{
		#region class GrassType
		public string name;
		//public Texture texture;
		//public Texture bumpTexture;
		public Material material;
		
		public VoxelandGrassType (string n) { name = n; }
		#endregion
	}
	
	//[ExecuteInEditMode] //to rebuild on scene loading. Only OnEnable uses it
	public class VoxelandTerrain : MonoBehaviour
	{
		public enum RebuildType {none, all, terrain, ambient, constructor, grass, prefab}; 
		
		public Data data;
		public DataHolder dataholder;
		
		public VoxelandBlockType[] types;
		public VoxelandGrassType[] grass;
		public int selected; //negative is for grass
		
		
		public int chunkSize = 32;
		//public int chunkHeight = 256;
		//public int chunkCountX;
		//public int chunkCountZ;
		
		[System.NonSerialized] public List<Chunk> activeChunks = null;
	
		public int overlap = 2;
	
		public float grassAnimState = 0.5f;
		public float grassAnimSpeed = 0.75f;
		public bool  grassAnimInEditor;
		
		public Highlight highlight;
		public Material highlightMaterial;
		
		public int brushSize = 0;
		public bool  brushSphere;
		
		public bool  playmodeEdit;
		//public bool  independPlaymode;
		public bool  usingPlayData = false;
		
		public bool  weldVerts = true;
		
		public bool infinite = true;
		public int displayCenterX;
		public int displayCenterZ;
		public int displayExtend = 256;
		
		#region vars Far
		public bool useFar = true;
		public Far far;
		public int lastFarX;
		public int lastFarZ;
		public bool farTrisChanged;
		#endregion
		
		#region vars Material
		public Shader landShader;
		public Material grassMaterial;
		public Color landAmbient = new Color(0.5f,0.5f,0.5f,1);
		public Color landSpecular = new Color(0,0,0,1);
		public float landShininess;
		public float landBakeAmbient;
		public float landTile = 0.5f;
		#endregion
		
		public bool  ambient = true;
		public int ambientMargins = 5;
		public int ambientSpread = 4;
		
		public float normalsRandom = 0;
		//public VoxelandNormalsSmooth normalsSmooth = VoxelandNormalsSmooth.mesh;
		
		public bool displayArea = false;
		
		#region vars GUI
		public bool  guiData;
		public bool  guiTypes = true;
		public bool  guiGrass = true;
		public bool  guiGenerate = false;
		public bool  guiExport;
		public bool  guiLod;
		public bool  guiTerrainMaterial;
		public bool  guiAmbient;
		public bool  guiMaterials;
		public bool  guiSettings;
		public bool  guiDebug;
		public bool  guiRebuild = true;
		public bool  guiArea = false;
		public bool  guiImportExport = false;
		public bool guiFar = true;
		public int guiFarChunks = 25;
		public int guiFarDensity = 10;
		public bool guiBake;
		public bool guiBakeLightmap;
		public int guiBakeSize = 100;
		public Transform guiBakeTfm;
		public bool guiGeneratorOverwrite = false;
		public int guiGeneratorCenterX = 0;
		public int guiGeneratorCenterZ = 0;
		public bool guiGeneratorUseCamPos = true;
		#endregion
	
		public bool generateLightmaps = false; //this should be always off exept baking with lightmaps
		public float lightmapPadding = 0.1f;
		public bool  saveMeshes = true;
		public bool  hideChunks = true;
		public bool hideWire = true;
		
		public float lodDistance = 50;
		private int lastLodX = 0;
		private int lastLodZ = 0;
		
		//public float removeDistance = 100;
		public float generateDistance = 100;
		public bool  autoGenerate = true;
		public int lastRebuildX = 0;
		public int lastRebuildZ = 0;
		
		//public bool  perFrameRebuilding = false; 
		//public Vector3 oldCamPivot; //for freezing camera on rebuild
		//Vector3 oldCamPos;
		//int rebuildChunksPerFrame = 1;
		
		public bool  generateCollider = true; 
		public bool  generateLod = true;
		
		
		public Generator generator; 
		
		#if UNITY_EDITOR
		private System.Diagnostics.Stopwatch timerSinceSetBlock = new System.Diagnostics.Stopwatch(); //to save byte list
		#endif
		
		//update speed-ups
		//[System.NonSerialized] public Vector2 camCoords = new Vector2(0,0); //coords of last camera position
		[System.NonSerialized] Ray oldAimRay;
		
		[System.NonSerialized] public Chunk highlightedChunk;
		[System.NonSerialized] public Chunk.VoxelandFace highlightedFace;
		[System.NonSerialized] public Transform highlightedObj;
		
		int[] oppositeDirX = {0,0,1,-1,0,0};
		int[] oppositeDirY = {1,-1,0,0,0,0};
		int[] oppositeDirZ = {0,0,0,0,-1,1};
		
		#if UNITY_EDITOR
		private int mouseButton = 0; //for EditorUpdate, currently pressed mouse button
		#endif
		
		//public static int mainThreadId;
		public bool multiThreadEdit;
		
		public bool chunksUnparented = false; //for saving scene without meshes. If chunks unparented they should be returned
		
		#region undo
		public List<int> undoCoordsX = new List<int>();
		public List<int> undoCoordsZ = new List<int>();
		public List<int> undoExtend = new List<int>();
		#endregion
		
		public Vector3 GetCameraPos (bool isEditor)
		{
			Vector3 pos;
			
			#if UNITY_EDITOR
			if (isEditor && UnityEditor.SceneView.lastActiveSceneView != null) 
				pos = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;
			else pos = Camera.main.transform.position;
			#else
			pos = Camera.main.transform.position;
			#endif
			
			
			return transform.InverseTransformPoint(pos);
		}
		
		//public void OnEnable () { Display(); }
	
		public void  Update ()
		{
			//if in scene view while in playmode
			#if UNITY_EDITOR
			//if (!UnityEditor.EditorApplication.isPlaying) return;
			if (UnityEditor.SceneView.lastActiveSceneView == UnityEditor.EditorWindow.focusedWindow) return;
			#endif
			
			Vector3 localCamPos = GetCameraPos(false);
			
			
			
			//refresh
			if (infinite && autoGenerate)
			{
				if (!Input.GetMouseButton(1) && !Input.GetMouseButton(2)) 
					Refresh((int)localCamPos.x, (int)localCamPos.z, (int)generateDistance);
			}
			//else if (!infinite && autoGenerate) Display(displayCenterX, displayCenterZ, displayExtend);
			//else Display(lastRebuildX, lastRebuildZ, (int)generateDistance);
			
			//display
			Display();
			
			//lods
			SwitchLods(localCamPos, lodDistance);
			
			//edit
			if (playmodeEdit) Edit(Camera.main, Input.mousePosition,
				Input.GetMouseButtonDown(0),
				Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift),
				Input.GetKey(KeyCode.LeftControl)||Input.GetKey(KeyCode.RightControl));
		}
		
		public void EditorUpdate ()
		{
			#if UNITY_EDITOR
			
			#region Removing Delegate in case it was not removed
			if (this == null) 
			{ 
				UnityEditor.SceneView.onSceneGUIDelegate -= GetMouseButton;
				UnityEditor.EditorApplication.update -= EditorUpdate;
				
				//Debug.Log("Removing Delegates"); 
				return;
			}
			#endregion
			
			
			if (UnityEditor.EditorApplication.isPlaying ||
				UnityEditor.SceneView.lastActiveSceneView == null ||
			    UnityEditor.EditorWindow.focusedWindow == null || 
			    UnityEditor.EditorWindow.focusedWindow.GetType() == System.Type.GetType("UnityEditor.GameView,UnityEditor")
			    ) return;
			    
			Vector3 localCamPos = GetCameraPos(true);
			
			//Refresh
			if (infinite && autoGenerate)
			{
				if (mouseButton == 0) Refresh((int)localCamPos.x, (int)localCamPos.z, (int)generateDistance);
			}

			Display();
			
			SwitchLods(localCamPos, lodDistance);
			
			//Edit is done with in onSceneGui - it can catch mouse events
			
			//removing highlight
			if (UnityEditor.Selection.activeGameObject != gameObject && highlight != null) 
			{
				highlight.Clear();
				//if (highlight.filter!=null && highlight.filter.sharedMesh!=null) DestroyImmediate(highlight.filter.sharedMesh);
				//DestroyImmediate(highlight.gameObject);
			}
			#endif
		}
		
		public void Refresh (int x, int z, int extend) //creates new chunks, removes old on camera pos change (or manual call). Called every frame
		{
			#region Load dataholder data 
			#if UNITY_EDITOR
			if (dataholder != null)
			{
				string path = UnityEditor.EditorApplication.currentScene;
				path.Replace(".unity", ".asset");
				path.Replace(".UNITY", ".ASSET");
				Debug.Log(path);
				
				data = ScriptableObject.CreateInstance<Data>();
				//data.compressed = dataholder.data.SaveToByteList();
				//data.areas = new Voxeland.Data.Area[dataholder.data.areas.Length];
				data.New();
				for (int a=0; a<data.areas.Length; a++) 
				{
					data.areas[a].columns = new Data.ListWrapper[dataholder.data.areas[a].columns.Length]; 
					for (int c=0; c<data.areas[a].columns.Length; c++) 
					{
						data.areas[a].columns[c] = new Data.ListWrapper();
						data.areas[a].columns[c].list = dataholder.data.areas[a].columns[c].list;
					}
					//data.areas[a].grass = new Data.ListWrapper(); data.areas[a].grass.list = dataholder.data.areas[a].grass.list;
					data.areas[a].initialized = dataholder.data.areas[a].initialized;
					data.areas[a].serializable = dataholder.data.areas[a].serializable;
				}
				data.compressed = data.SaveToByteList();
				data.name = "VoxelandData";
				UnityEditor.EditorUtility.SetDirty(data);

				DestroyImmediate(dataholder.gameObject);
			}
			#endif
			#endregion
			
			#region loading compressed data
			if (data.areas == null && data.compressed.Count != 0) 
			{
				//if (benchmark) Debug.Log("Loading Byte List");
				data.LoadFromByteList(data.compressed);
			}
			#endregion
			
			#region Clearing if no terrain
			if (activeChunks == null) Rebuild(); //same as in display
			#endregion
			
			if (Mathf.Abs(x-lastRebuildX)>extend/3 || Mathf.Abs(z-lastRebuildZ)>extend/3)
			{
				if (benchmark) Debug.Log("Refreshing: " + x + " " + z);
				
				GetChunksInRange(x, z, extend, true, true); //createInside, removeOutside //just creating new chunks, do not modify them. And removing out-of area chunks
				lastRebuildX = x; lastRebuildZ = z;
				
				#region Rebuild Far Mesh
				if (useFar && infinite)
				{
					if (far==null) far = Far.Create(this);
					far.subdiv = guiFarDensity;
					far.chunks = guiFarChunks;
					far.Build(x,z);
				}
				#endregion
			}
		}
		
		public void SwitchLods (Vector3 camPos, float dist) //called every frame
		{
			int x = (int)camPos.x; int z = (int)camPos.z;
			
			if (Mathf.Abs(x-lastLodX)>dist/4 || Mathf.Abs(z-lastLodZ)>dist/4)
			{
				float csx = (x-chunkSize/2-dist) / (chunkSize-overlap*2); 
				float csz = (z-chunkSize/2-dist) / (chunkSize-overlap*2);
				float cex = (x-chunkSize/2+dist) / (chunkSize-overlap*2); 
				float cez = (z-chunkSize/2+dist) / (chunkSize-overlap*2);
				
				for (int i=0; i<activeChunks.Count; i++)
				{
					if (activeChunks[i].coordX < csx || activeChunks[i].coordX > cex || activeChunks[i].coordZ < csz || activeChunks[i].coordZ > cez) 
						activeChunks[i].SwitchLod(true);
					else activeChunks[i].SwitchLod(false);
				}
				
				lastLodX = x; lastLodZ = z;
			}
		}
	
		public void Display () //refreshing terrain using last camera coordinates
		{			
			if (profile) Profiler.BeginSample ("Display");
			
			/*
			//hiding data
			if (dataholder == null) return;
			if (hideChunks) dataholder.transform.hideFlags = HideFlags.HideInHierarchy;
			else dataholder.transform.hideFlags = HideFlags.None;
			*/
			
			
			#region Clearing if no terrain
			if (activeChunks == null) Rebuild();
			#endregion
			
			#region Returning unparented chunks
			if (chunksUnparented)
			{
				for (int i=0; i<activeChunks.Count; i++) activeChunks[i].transform.parent = transform;
				if (far != null) far.transform.parent = transform;
				if (highlight != null) highlight.transform.parent = transform;
				chunksUnparented = false;
			}
			#endregion
			
			#region Save Compressed Data after delay
			#if UNITY_EDITOR
			if (timerSinceSetBlock.ElapsedMilliseconds > 2000 && mouseButton == 0 && !UnityEditor.EditorApplication.isPlaying) 
			{
				data.compressed = data.SaveToByteList();
				UnityEditor.EditorUtility.SetDirty(data);
				timerSinceSetBlock.Stop();
				timerSinceSetBlock.Reset();
			}
			#endif
			#endregion
			
			#region Setting exist types
			for (int i=0; i<types.Length; i++)
				data.exist[i] = types[i].filled;
			#endregion
			
			#region Hide flags that were set by editor onwillsave processor
			#if UNITY_EDITOR
			if (!saveMeshes && transform.childCount!=0)
			{
				if (transform.GetChild(0).gameObject.hideFlags == HideFlags.HideAndDontSave) 
					foreach (Transform child in transform)
						SetHideFlagsRecursively(HideFlags.HideInHierarchy, child);
						
				if (transform.GetChild(0).gameObject.hideFlags == HideFlags.DontSave) 
					foreach (Transform child in transform)
						SetHideFlagsRecursively(HideFlags.None, child);
			}
			#endif
			#endregion
			
			#region Animating grass
			grassAnimState += Time.deltaTime * grassAnimSpeed;
			grassAnimState = Mathf.Repeat(grassAnimState, 6.283185307179586476925286766559f);
			Shader.SetGlobalFloat("_GrassAnimState", grassAnimState);
			#endregion
	
			#region Finishing Rebuild
			if (profile) Profiler.BeginSample ("Rebuild");
			int unbuildChunksCount = 0; //for benchmarking
			
			for (int i=0; i<activeChunks.Count; i++)
			{
				//starting benchmark
				if (benchmark && !benchmarkTimer.IsRunning && activeChunks[i].terrainProgress != Chunk.Progress.applied) 
				{
					benchmarkTimer.Reset();
					benchmarkTimer.Start();
					benchmarkChunks = 0;
				}
				
				//calculating mesh
				if (activeChunks[i].terrainProgress == Chunk.Progress.notCalculated) 
				{
					if (multiThreadEdit) 
					{
						activeChunks[i].terrainProgress = Chunk.Progress.threadStarted; 
						ThreadPool.QueueUserWorkItem(new WaitCallback(activeChunks[i].CalculateTerrain));
					}
					else activeChunks[i].CalculateTerrain();
				}
				
				//apply meshes 
				if (activeChunks[i].terrainProgress == Chunk.Progress.calculated) 
				{
					activeChunks[i].ApplyTerrain();
					benchmarkChunks++;
					farTrisChanged = true;
				}
				
				//calculate ambient
				if (activeChunks[i].ambientProgress == Chunk.Progress.notCalculated) 
				{
					if (multiThreadEdit) 
					{
						activeChunks[i].ambientProgress = Chunk.Progress.threadStarted; 
						ThreadPool.QueueUserWorkItem(new WaitCallback(activeChunks[i].CalculateAmbient));
					}
					else activeChunks[i].CalculateAmbient();
				}
				
				//apply ambient
				if (activeChunks[i].ambientProgress == Chunk.Progress.calculated && 
				    activeChunks[i].terrainProgress == Chunk.Progress.applied) activeChunks[i].ApplyAmbient();
				  
				//calculate constructor
				//if (activeChunks[i].constructorProgress == Chunk.Progress.notCalculated) activeChunks[i].BuildConstructor();
				
				//calculate grass
				if (activeChunks[i].grassProgress == Chunk.Progress.notCalculated && 
				    (activeChunks[i].terrainProgress == Chunk.Progress.calculated || activeChunks[i].terrainProgress == Chunk.Progress.applied) )
					activeChunks[i].BuildGrass();
				
				//calculate prefab
				if (activeChunks[i].prefabsProgress == Chunk.Progress.notCalculated) activeChunks[i].BuildPrefabs();
				
	
				if (benchmarkTimer.IsRunning && activeChunks[i].terrainProgress != Chunk.Progress.applied) unbuildChunksCount++;
			}
			if (profile) Profiler.EndSample();
			#endregion
			
			#region Stopping benchmark
			if (profile) Profiler.BeginSample ("Stopping Benchmak");
			
			if (benchmarkTimer.IsRunning && unbuildChunksCount == 0)
			{
				benchmarkTimer.Stop();
				Debug.Log(
					"" + benchmarkChunks + " chunks rebuilt in " + (0.001f * benchmarkTimer.ElapsedMilliseconds) + " seconds\n" + 
					benchmarkChunks  / (0.001f * benchmarkTimer.ElapsedMilliseconds) + " chunks per sec., " +
					((chunkSize-overlap*2)*(chunkSize-overlap*2)*benchmarkChunks) / (0.001f * benchmarkTimer.ElapsedMilliseconds) + " columns per sec.");
			}
			if (profile) Profiler.EndSample();
			#endregion
		
			if (profile) Profiler.EndSample();
		}
		
		public void Edit (Camera activeCam, Vector2 mousePos, bool mouseDown, bool shift, bool control)
		{
			Ray aimRay = activeCam.ScreenPointToRay(mousePos);
			
			//getting controls
			bool add = mouseDown && !shift && !control;
			bool dig = mouseDown && shift && !control;
			bool smooth = mouseDown && control && !shift;
			bool replace = mouseDown && control && shift;
	
			Edit(aimRay, add, dig, smooth, replace);
		}
		
		public void  Edit (Ray aimRay, bool add, bool dig, bool smooth, bool replace)
		{
			#region Getting aim ray change
			if (Mathf.Approximately(aimRay.origin.x, oldAimRay.origin.x) && 
				Mathf.Approximately(aimRay.origin.y, oldAimRay.origin.y) && 
				Mathf.Approximately(aimRay.origin.z, oldAimRay.origin.z) &&
				Mathf.Approximately(aimRay.direction.x, oldAimRay.direction.x) && 
				Mathf.Approximately(aimRay.direction.y, oldAimRay.direction.y) && 
				Mathf.Approximately(aimRay.direction.z, oldAimRay.direction.z) &&
				!add && !dig && !smooth && !replace) return;
			oldAimRay = aimRay;
			#endregion
			
			#region Aiming
			GetCoordsData coordsData;
			bool hitDetected = GetCoordsByRay(aimRay, out coordsData);
			#endregion
			
			#region Drawing highlight
			if (hitDetected)
			{
				if (highlight == null) highlight = Highlight.Create(this);
				
				if (coordsData.type == GetCoordsType.face)
				{
					if (brushSize==0) highlight.DrawFace(coordsData.face);
					else if (brushSphere) highlight.DrawSphere(new Vector3(coordsData.x+0.5f, coordsData.y+0.5f, coordsData.z+0.5f), brushSize);
					else highlight.DrawBox(new Vector3(coordsData.x+0.5f, coordsData.y+0.5f, coordsData.z+0.5f), new Vector3(brushSize,brushSize,brushSize));
				}
				
				if (coordsData.type == GetCoordsType.obj)
				{
					highlight.DrawBox(coordsData.collider.bounds.center, coordsData.collider.bounds.extents);
				}
			}
			#endregion
			
			if (hitDetected && (add || dig || smooth || replace))
			{
				#region Setting block
				if (selected>0) 
				{
					//setting big-brushing blocks
					if (brushSize > 0 && types[selected].obj == null)
					{
						if (add) SetBlocks(coordsData.x, coordsData.y, coordsData.z, (byte)selected, brushSize, brushSphere, false);
						else if (dig) SetBlocks(coordsData.x, coordsData.y, coordsData.z, 0, brushSize, brushSphere, false);
						else if (smooth) BlurBlocks(coordsData.x, coordsData.y, coordsData.z, brushSize);
						else if (replace) SetBlocks(coordsData.x, coordsData.y, coordsData.z, (byte)selected, brushSize, brushSphere, true);
					}
					
					//setting single block
					else
					{
						if (add) SetBlock(coordsData.x+oppositeDirX[coordsData.dir], coordsData.y+oppositeDirY[coordsData.dir], coordsData.z+oppositeDirZ[coordsData.dir], (byte)selected);
						else if (dig) SetBlock(coordsData.x, coordsData.y, coordsData.z, 0);
						else if (replace) SetBlock(coordsData.x, coordsData.y, coordsData.z, (byte)selected);
					}
				}
				#endregion 
				
				/*
				#region Setting object
				if (selected>0 && !filled) 
				{
					if (add) SetBlock(coordsData.x+oppositeDirX[coordsData.dir], coordsData.y+oppositeDirY[coordsData.dir], coordsData.z+oppositeDirZ[coordsData.dir], (byte)selected);
					else if (replace) SetBlock(coordsData.x, coordsData.y, coordsData.z, (byte)selected);
					else if (dig) SetBlock(coordsData.x, coordsData.y, coordsData.z, 0);
				}
				#endregion
				*/
				
				#region Setting grass
				if (selected<0)
				{
					if (add || replace) SetGrass(coordsData.x, coordsData.z,(byte)(-selected),brushSize,brushSphere);
					else if (dig) SetGrass(coordsData.x, coordsData.z, 0, brushSize,brushSphere);
				}
				#endregion
			}
		}
		
		public void Rebuild () //clears all terrain and creates new one
		{
			//if (data != null) data.LoadFromByteList(data.compressed);
			
			#region Clearing
			if (activeChunks==null) activeChunks = new List<Chunk>();
			activeChunks.Clear();
			for(int i=transform.childCount-1; i>=0; i--) DestroyImmediate(transform.GetChild(i).gameObject); //destroing everything
			#endregion
			
			#region Re-create
			lastRebuildX = 100000000;
			lastRebuildZ = 100000000;
			lastLodX = 100000000;
			lastLodZ = 100000000;
			lastFarX = 100000000;
			lastFarZ = 100000000;
			
			if (infinite && !autoGenerate && activeChunks==null) { activeChunks = new List<Chunk>(); data.LoadFromByteList(data.compressed); return; }
			
			if (infinite) 
			{
				#if UNITY_EDITOR
				Vector3 camPos = GetCameraPos(true);
				#else
				Vector3 camPos = GetCameraPos(false);
				#endif
				
				Refresh((int)camPos.x, (int)camPos.z, (int)generateDistance);
			}
			else Refresh(displayCenterX, displayCenterZ, displayExtend);

			Display();
			#endregion
		}
		
		
		public enum GetCoordsType {none, face, obj, constructor};
		
		public class GetCoordsData
		{
			public GetCoordsType type;
			public int x; public int y; public int z; public byte dir;
			public Chunk.VoxelandFace face;
			public Collider collider;
			public int triIndex;
			public Chunk chunk;
		}
		
		//public bool GetCoordsByRay (Ray aimRay, out int x, out int y, out int z, out byte dir, out Chunk.VoxelandFace face, bool drawHighlight) 
		public bool GetCoordsByRay (Ray aimRay, out GetCoordsData data) 
		{
			//x=0; y=0; z=0; dir=0; face = null;
			data = new GetCoordsData();
			
			RaycastHit hit; 
			if (!Physics.Raycast(aimRay, out hit) || //if was not hit
			    !hit.collider.transform.IsChildOf(transform)) //if was hit in object that is not child of voxeland
					return false;
			
			
			#region Terrain block
			if (hit.collider.gameObject.name == "LoResChunk")
			{
				Chunk chunk = hit.collider.transform.parent.GetComponent<Chunk>();
				
				if (chunk == null || chunk.visibleFaces==null || chunk.visibleFaces.Count==0) return false;
				
				data.type = GetCoordsType.face;
				data.face = chunk.visibleFaces[hit.triangleIndex/2];
				data.x = data.face.block.x + data.face.block.chunk.offsetX;
				data.y = data.face.block.y;
				data.z = data.face.block.z + data.face.block.chunk.offsetZ;
				data.dir = data.face.dir;
				
				data.triIndex = hit.triangleIndex;
				data.collider = hit.collider;
				
				return true;
			}
			#endregion
			
			#region Constructor
			/*
			if (hit.collider.gameObject.name == "Constructor" && hit.collider.transform.IsChildOf(transform))
			{
				Chunk chunk = hit.collider.transform.parent.GetComponent<Chunk>();
				
				if (chunk == null) return false;
				
				if (Mathf.Abs(hit.normal.y) > Mathf.Abs(hit.normal.x) && Mathf.Abs(hit.normal.y) > Mathf.Abs(hit.normal.z))
				{
					if (hit.normal.y > 0) data.dir = 0;
					else data.dir = 1;
				}
				else if (Mathf.Abs(hit.normal.x) > Mathf.Abs(hit.normal.y) && Mathf.Abs(hit.normal.x) > Mathf.Abs(hit.normal.z))
				{
					if (hit.normal.x > 0) data.dir = 2;
					else data.dir = 3;
				}
				else
				{
					if (hit.normal.z > 0) data.dir = 5;
					else data.dir = 4;
				}
	
				data.x = (int)(hit.point.x - hit.normal.x*0.1f);
				data.y = (int)(hit.point.y - hit.normal.y*0.1f);
				data.z = (int)(hit.point.z - hit.normal.z*0.1f);
				
				return true;
			}
			*/
			#endregion
			
			#region Object block
			else
			{
				Transform parent = hit.collider.transform.parent;
				while (parent != null)
				{
					if (parent.name=="Chunk" && parent.IsChildOf(transform))
					{
						data.chunk = parent.GetComponent<Chunk>();
						break;
					}
					parent = parent.parent;
				}
				
				if (data.chunk == null) return false; //aiming other obj
				
				data.type = GetCoordsType.obj;
				data.collider = hit.collider;
				
				Vector3 pos= hit.collider.transform.localPosition;
				
				data.x = (int)pos.x + data.chunk.offsetX; 
				data.y = (int)pos.y; 
				data.z = (int)pos.z + data.chunk.offsetZ;
	
				return true;
			}
			#endregion	
		}
		
		public Chunk[] GetChunksInRange (int x, int z, int range) { return GetChunksInRange(x,z,range,false,false); }
		public Chunk[] GetChunksInRange (int x, int z, int range, bool createInside, bool removeOutside)
		{
			#region Chunks min and max
			int visChunkSize = chunkSize-overlap*2;
			
			int csx = (int)(1.0f*(x-range-overlap) / visChunkSize); 
			int cex = (int)(1.0f*(x+range-overlap) / visChunkSize); 
			int csz = (int)(1.0f*(z-range-overlap) / visChunkSize); 
			int cez = (int)(1.0f*(z+range-overlap) / visChunkSize); 
			
			if (x-range<0) csx--; if (x+range<0) cex--;
			if (z-range<0) csz--; if (z+range<0) cez--;
			
			/*if (!infinite)
			{
				csx = Mathf.Max(csx, 0); cex = Mathf.Min(cex, chunkCountX-1); 
				csz = Mathf.Max(csz, 0); cez = Mathf.Min(cez, chunkCountZ-1);
			}*/
			#endregion
	
			#region defining array dimensions and filling array
			int chunksCountX = Mathf.Abs(cex-csx)+1;
			int chunksCountZ = Mathf.Abs(cez-csz)+1; 
			Chunk[] chunks = new Chunk[chunksCountX*chunksCountZ];
			#endregion
			
			#region filling existant chunks list (and removing outside)
			for (int i=activeChunks.Count-1; i>=0; i--)
			{
				if (activeChunks[i].coordX < csx || activeChunks[i].coordX > cex ||
				    activeChunks[i].coordZ < csz || activeChunks[i].coordZ > cez) //out of range
				{
				    if (removeOutside) 
				    {
						DestroyImmediate(activeChunks[i].gameObject);
						activeChunks.RemoveAt(i);
						farTrisChanged = true;
				    }
				}
				   
				//if in range 
				else chunks[ (activeChunks[i].coordZ-csz)*chunksCountX + (activeChunks[i].coordX-csx) ] = activeChunks[i];
			}
			#endregion
			
			#region creating non-existant chunks
			if (createInside)
				for (int cx=0; cx<chunksCountX; cx++)
					for (int cz=0; cz<chunksCountZ; cz++)
			{
				if (chunks[cz*chunksCountX + cx] == null) 
				{
					chunks[cz*chunksCountX + cx] = Chunk.CreateChunk(this,cx+csx,cz+csz);
					
					//createdChunksStatistics++;
				}
			}
			#endregion
			
			return chunks;
		}
	
	
		public byte GetBlock (int x, int y, int z) { return data.GetBlock(x,y,z); }
		
		public void SetBlock (int x, int y, int z, byte type) { SetBlocks(x,y,z,type, 0, false, false); } 
		
		public void SetBlocks (int x, int y, int z, byte type, int extend, bool spherify, bool replace) //x,y,z are center, extend is half size (radius)
		{
			#if UNITY_EDITOR
			data.RegisterUndo(x,z,extend);
			//UnityEditor.Undo.RecordObject(this, "Voxeland Set Block");
			
			undoCoordsX.Add(x);
			undoCoordsZ.Add(z);
			undoExtend.Add(extend);
			
			if (undoCoordsX.Count > 16)
			{
				undoCoordsX.RemoveAt(0);
				undoCoordsZ.RemoveAt(0);
				undoExtend.RemoveAt(0);
			}
			
			timerSinceSetBlock.Reset();
			timerSinceSetBlock.Start();
			#endif

			int minX = x-extend; int minY = y-extend; int minZ = z-extend;
			int maxX = x+extend; int maxY = y+extend; int maxZ = z+extend;
			
			for (int xi = minX; xi<=maxX; xi++)
				for (int yi = minY; yi<=maxY; yi++) 
					for (int zi = minZ; zi<=maxZ; zi++)
			{
				if (spherify && Mathf.Abs(Mathf.Pow(xi-x,2)) + Mathf.Abs(Mathf.Pow(yi-y,2)) + Mathf.Abs(Mathf.Pow(zi-z,2)) - 1 > extend*extend) continue;
				if (replace && !data.GetExist(xi,yi,zi)) continue;
				
				data.SetBlock(xi, yi, zi, type, types[type].filled);
			}
			
			ResetProgress(x,z,extend); //TODO: optimize rebuild
		}
	
		public void  BlurBlocks (int x, int y, int z, int extend)
		{
			#if UNITY_EDITOR
			data.RegisterUndo(x,z,extend);
			//UnityEditor.Undo.RecordObject(this, "Voxeland Set Block");
			
			undoCoordsX.Add(x);
			undoCoordsZ.Add(z);
			undoExtend.Add(extend);
			
			if (undoCoordsX.Count > 16)
			{
				undoCoordsX.RemoveAt(0);
				undoCoordsZ.RemoveAt(0);
				undoExtend.RemoveAt(0);
			}
			
			timerSinceSetBlock.Reset();
			timerSinceSetBlock.Start();
			#endif
			
			//getting exists matrix
			bool[] existsMatrix = new bool[(extend*2)*(extend*2)*(extend*2)];
			data.GetExistMatrix(existsMatrix, x-extend, y-extend, z-extend, x+extend, y+extend, z+extend);
			
			//blurring exists matrix
			float[] blurMatrix = new float[existsMatrix.Length];
			for (int i=0; i<existsMatrix.Length; i++) 
				if (existsMatrix[i]) blurMatrix[i] = 1.0f;
			
			for (int iteration=0; iteration<10; iteration++)
				for (int ix=1;ix<extend*2-1;ix++)
					for (int iy=1;iy<extend*2-1;iy++)
						for (int iz=1;iz<extend*2-1;iz++)
					{
						int i = iz*extend*extend*4 + iy*extend*2 + ix;
						
						blurMatrix[i] = blurMatrix[i]*0.4f + 
							(blurMatrix[i-1] + blurMatrix[i+1] + blurMatrix[i-extend*2] + blurMatrix[i+extend*2] + blurMatrix[i-extend*extend*4] + blurMatrix[i+extend*extend*4])*0.1f;
					}
			
			//setting blocks
			for (int ix=0;ix<extend*2;ix++)
				for (int iy=0;iy<extend*2;iy++)
					for (int iz=0;iz<extend*2;iz++)
				{
					if ( Mathf.Pow(ix-extend,2) + Mathf.Pow(iy-extend,2) + Mathf.Pow(iz-extend,2) > extend*extend ) continue;
					
					int i = iz*extend*extend*4 + iy*extend*2 + ix;
					if (blurMatrix[i] < 0.5f) data.SetBlock(x-extend+ix, y-extend+iy, z-extend+iz, 0, false);
					else 
					{	
						byte closestType = 0;
						if (closestType == 0) closestType = data.GetBlock(x-extend+ix, y-extend+iy, z-extend+iz);
						if (closestType == 0) closestType = data.GetBlock(x-extend+ix, y-extend+iy-1, z-extend+iz);
						if (closestType == 0) closestType = data.GetBlock(x-extend+ix, y-extend+iy+1, z-extend+iz);
						if (closestType == 0) closestType = data.GetBlock(x-extend+ix-1, y-extend+iy, z-extend+iz);
						if (closestType == 0) closestType = data.GetBlock(x-extend+ix+1, y-extend+iy, z-extend+iz);
						if (closestType == 0) closestType = data.GetBlock(x-extend+ix, y-extend+iy, z-extend+iz-1);
						if (closestType == 0) closestType = data.GetBlock(x-extend+ix, y-extend+iy, z-extend+iz+1);
						if (closestType == 0) closestType = data.GetBlock(x-extend+ix, y-extend+iy-2, z-extend+iz);
						if (closestType == 0) closestType = data.GetBlock(x-extend+ix, y-extend+iy+2, z-extend+iz);
						if (closestType == 0) closestType = (byte)selected;
						
						data.SetBlock(x-extend+ix, y-extend+iy, z-extend+iz, closestType, true);
					}
				}
			
			ResetProgress(x,z,extend);
		}
		
		public void SetGrass (int x, int z, byte type, int extend, bool spherify)
		{
			#if UNITY_EDITOR
			timerSinceSetBlock.Reset();
			timerSinceSetBlock.Start();
			#endif
			
			int minX = x-extend; int minZ = z-extend;
			int maxX = x+extend; int maxZ = z+extend;
			
			for (int xi = minX; xi<=maxX; xi++)
				for (int zi = minZ; zi<=maxZ; zi++)
					if (!spherify || Mathf.Abs(Mathf.Pow(xi-x,2)) + Mathf.Abs(Mathf.Pow(zi-z,2)) - 1 <= extend*extend) 
						data.SetGrass(xi, zi, type);
						
			Chunk[] chunks = GetChunksInRange(x,z,extend);
			foreach (Chunk chunk in chunks) if (chunk!=null) chunk.grassProgress = Chunk.Progress.notCalculated;
		}
		
		public void ResetProgress (int x, int z, int extend)
		{
			Chunk[] chunks = GetChunksInRange(x,z,extend+overlap);
			foreach (Chunk chunk in chunks) if (chunk!=null) chunk.terrainProgress = Chunk.Progress.notCalculated;
			
			chunks = GetChunksInRange(x,z,extend+ambientMargins);
			foreach (Chunk chunk in chunks) if (chunk!=null) chunk.ambientProgress = Chunk.Progress.notCalculated;
			
			chunks = GetChunksInRange(x,z,extend);
			foreach (Chunk chunk in chunks) 
			{
				if (chunk==null) continue; 
				//chunk.constructorProgress = Chunk.Progress.notCalculated;
				chunk.grassProgress = Chunk.Progress.notCalculated;
				chunk.prefabsProgress = Chunk.Progress.notCalculated;
			}
		}
	
		#region Debug
		public bool  alwaysRebuild = false;
		public bool  alwaysSetBlock = false;
		public bool  visualizeData = false;
		public bool  visualizeFill = false;
		public bool  visualizeChunk = false;
		public bool visualizeAmbient = false;
		public bool visualizeHeightmap = false;
		public bool visualizeTopBottom = false;
		public bool rotateVizualisation = false;
		public int ambientBlurDisplayLayer = 15;
		public int ambientBlurDisplayChunk = 5;
		public bool visualizeNormals = false;
		//public bool visualizeConstructor = false;
		
		public bool benchmark = false;
		private System.Diagnostics.Stopwatch benchmarkTimer = new System.Diagnostics.Stopwatch();
		private int benchmarkChunks = 0;
	
		public bool profile = false;
		
		public List<byte> tempByteList = new List<byte>();
	
		public void  Visualize ()
		{
			GetCoordsData coordsData;
			bool hitDetected = GetCoordsByRay(oldAimRay, out coordsData);
			
			int x=coordsData.x; int z=coordsData.z; Chunk.VoxelandFace face = coordsData.face;
	
			#region Data
			//if (visualizeData && hitDetected) 
			//	data.Visualize(x,z);
			#endregion
	
	/*		
			#region Ambient
			if (visualizeAmbient && highlightedChunk != null)
			{ 
				//highlightedChunk.BuildAmbient();
			                      
				
				
				//int ambientSpread = 6;
				//int ambientSize = chunkSize + ambientSpread*2;
				
				for (int x=0; x<chunkSize; x++)
					for (int z=0; z<chunkSize; z++)
						for (int y=0; y<chunkHeight; y++)
					{
						if ((!rotateVizualisation && highlightedFace.block.x != x) || (rotateVizualisation && highlightedFace.block.z != z)) continue;
						if (y<highlightedChunk.ambientBottomPoint || y>highlightedChunk.ambientTopPoint) continue;
						
						Gizmos.color = new Color(0f, 1f, 0f, 1f);
						
						//if (y>temp[z*chunkSize + x])
						int ambX = x+ambientMargins-overlap; 
						int ambY = y-highlightedChunk.ambientBottomPoint; 
						int ambZ = z+ambientMargins-overlap;
						
						byte val = highlightedChunk.ambient[ ambZ*highlightedChunk.ambientHeight*highlightedChunk.ambientSize + 
						                                    ambY*highlightedChunk.ambientSize + 
						                                    ambX ];
						
						if (val > 1) 				
							Gizmos.DrawCube(
								new Vector3(highlightedChunk.offsetX+x+0.5f, y+0.5f, highlightedChunk.offsetZ+z+0.5f), 
								new Vector3(val*0.9f/250,val*0.9f/250,val*0.9f/250));
					}
			}
			#endregion
	*/		
	
			
			#region Fill
			if (visualizeFill && hitDetected)
			{
				
				int topPoint = data.GetTopPoint(face.block.chunk.offsetX, face.block.chunk.offsetZ, 
				                                           face.block.chunk.offsetX+chunkSize, face.block.chunk.offsetZ+chunkSize);
				
				int bottomPoint = data.GetBottomPoint(face.block.chunk.offsetX, face.block.chunk.offsetZ,
				                                                 face.block.chunk.offsetX+chunkSize, face.block.chunk.offsetZ+chunkSize);
				
				bool[] matrix = new bool[topPoint - bottomPoint];
	
				data.GetExistMatrix(matrix, x+1,bottomPoint,z, x+1+1,topPoint,z+1);
				
				for (int i=0; i<matrix.Length; i++)
				{
					if (matrix[i]) Gizmos.color = new Color(0f, 1f, 0f, 1f);
					else Gizmos.color = new Color(1f, 0f, 0f, 1f);
					
				//	Gizmos.DrawCube(
				//		new Vector3(x+1+0.5f, bottomPoint+i, z+0.5f), 
				//		new Vector3(0.9f, 0.9f, 0.9f));
				}
				
				/*
				data.WriteByteColumn(x,z);
				for (int i=0; i<Data.byteColumnLength; i++)
				{
					Gizmos.color = new Color(Data.byteColumn[i] / 4f, 1-(Data.byteColumn[i] / 4f), 0f, 1f);
	
					Gizmos.DrawCube(
							new Vector3(x+0.5f, i+0.5f, z+0.5f), 
							new Vector3(0.9f, 0.9f, 0.9f));
				}
				*/
			}
			#endregion
		
			#region Top Bottom
			if (visualizeTopBottom && hitDetected)
			{
				Gizmos.color = new Color(0f, 1f, 0f, 1f);
	
				//top point
				int topPoint = data.GetTopPoint(face.block.chunk.offsetX, face.block.chunk.offsetZ, 
				                                           face.block.chunk.offsetX+chunkSize, face.block.chunk.offsetZ+chunkSize);
				                                           
				int bottomPoint = data.GetBottomPoint(face.block.chunk.offsetX, face.block.chunk.offsetZ,
				                                                 face.block.chunk.offsetX+chunkSize, face.block.chunk.offsetZ+chunkSize);
				
				Gizmos.DrawWireCube(
					new Vector3(face.block.chunk.offsetX+chunkSize/2, bottomPoint + (topPoint-bottomPoint)/2, face.block.chunk.offsetZ+chunkSize/2), 
					new Vector3(chunkSize,topPoint-bottomPoint,chunkSize));
					
				topPoint = data.GetTopPoint(x,z,x,z);
				bottomPoint = data.GetBottomPoint(x,z,x,z);
				Gizmos.DrawCube(
					new Vector3(x+0.5f, bottomPoint + (topPoint-bottomPoint)/2, z+0.5f), 
					new Vector3(1,topPoint-bottomPoint,1));
			}
			#endregion
	/*		
			#region Normals
			if (visualizeNormals && highlightedChunk!=null)
			{
				Vector4[] normals = highlightedChunk.hiFilter.sharedMesh.tangents;
				Vector3[] verts = highlightedChunk.hiFilter.sharedMesh.vertices;
				
				float colorStep = 1f/verts.Length;
				
				Vector3 start = new Vector3(highlightedChunk.offsetX,0,highlightedChunk.offsetZ);
				Vector3 small = new Vector3(0.01f,0.01f,0.01f);
				Vector3 normal = new Vector3(0,0,0);
				
				for (int v=0; v<normals.Length; v++)
				{
					normal.x = normals[v].x; normal.y = normals[v].y; normal.z = normals[v].z; 
					
					Gizmos.color = new Color(1f-colorStep*v, colorStep*v, 0, 1);
					Gizmos.DrawLine(verts[v]+start, verts[v]+start + (normal*0.25f));
					//Gizmos.DrawLine(verts[v]+start-small, verts[v]+start + small);
					Gizmos.DrawCube(verts[v]+start, small);
				}
			}
			#endregion
			
			#region Constructor
			if (visualizeConstructor && highlightedFace != null)
			{
				int x = highlightedFace.block.x+highlightedFace.block.chunk.offsetX;
				int y = highlightedFace.block.y;
				int z = highlightedFace.block.z+highlightedFace.block.chunk.offsetZ;
				
				int rotation = 0;
				if (types[highlightedFace.block.type].constructor != null) 
					types[highlightedFace.block.type].constructor.GetElement(data, x,y,z, out rotation).Visualize(x,y,z);
			}
			#endregion
			
	*/
		}
		
		public void VisualizeArea ()
		{
			int areaStartX=0; int areaStartZ=0; int areaSize=0; 
			Gizmos.color = new Color(0.5f, 0.75f, 1f, 1f);
			
			float density = 64;
			for (int x=0; x<density+1; x++)
				for (int z=0; z<density+1; z++)
			{
				float xCoord = areaStartX + x*areaSize/density;
				float zCoord = areaStartZ + z*areaSize/density;
				
				if (x!=density) Gizmos.DrawLine( 
				    new Vector3(xCoord, data.GetTopPoint((int)xCoord, (int)zCoord), zCoord), 
				    new Vector3(xCoord+areaSize/density, data.GetTopPoint((int)(xCoord+areaSize/density), (int)zCoord), zCoord) );
				if (z!=density) Gizmos.DrawLine( 
				    new Vector3(xCoord, data.GetTopPoint((int)xCoord, (int)zCoord), zCoord), 
				    new Vector3(xCoord, data.GetTopPoint((int)xCoord, (int)(zCoord+areaSize/density)), zCoord+areaSize/density) );
			}
			
			
			//Gizmos.DrawCube( new Vector3(areaX*areaSize - areaSize*50 + areaSize/2, 0, areaZ*areaSize - areaSize*50 + areaSize/2),
			//	new Vector3(areaSize,100,areaSize) );
		}
		
		public void DrawLine (int x1, int z1, int x2, int z2)
		{
			//int h1 = data.
		}
		#endregion
		
		#if UNITY_EDITOR
		public void OnDrawGizmos()
		{
			if (!this.enabled) return;
			
			Profiler.BeginSample("DrawGizmos");
	
			UnityEditor.EditorApplication.update -= EditorUpdate;	
			UnityEditor.SceneView.onSceneGUIDelegate -= GetMouseButton; 	
				
			//registering delegates (removing in Editor OnDisable)
			UnityEditor.SceneView.onSceneGUIDelegate += GetMouseButton; //20 ms on mouse button pressed
			UnityEditor.EditorApplication.update += EditorUpdate; //5 ms when camera moved
			
			//releasing right button if mouse is not in scene view
			if (mouseButton == 1 &&
			    UnityEditor.SceneView.lastActiveSceneView != null &&
				UnityEditor.EditorWindow.mouseOverWindow != UnityEditor.SceneView.lastActiveSceneView &&
			    Event.current.button != 1) 
			    	mouseButton = 0;
			
			//visualizing
			Visualize();
			if (displayArea) VisualizeArea();
			
			Profiler.EndSample(); 
		}
		
		private bool wasRepaintEvent = false;
		public void GetMouseButton (UnityEditor.SceneView sceneview) //is registered in OnDrawGizmos
		{
			if (UnityEditor.SceneView.lastActiveSceneView == null) return;
			
			//setting mouse position and modifiers
			
	
			
			//if mouse is within scene view scope - all is clear
			if (UnityEditor.SceneView.mouseOverWindow == UnityEditor.SceneView.lastActiveSceneView)
			{
				//Debug.Log("SceneEvent: " + Event.current);
				if (Event.current.type == EventType.MouseDown) mouseButton = Event.current.button;
				if (Event.current.type == EventType.MouseUp) mouseButton = 0;
			}
			
			//if mouse travalled away from scene view
			else
			{
				if (mouseButton == 2)
				{
					if (wasRepaintEvent && Event.current.type == EventType.Repaint) mouseButton = 0;
					else if (Event.current.type == EventType.Repaint) wasRepaintEvent = true;
					else if (Event.current.type == EventType.MouseDrag) wasRepaintEvent = false;
				}
				
				//releasing button 1 in OnDrawGizmos	
			}
		}
	
		static public void SetHideFlagsRecursively (HideFlags flag, Transform transform)
		{
			transform.gameObject.hideFlags = flag;
			foreach (Transform child in transform) SetHideFlagsRecursively(flag,child);
		}
		#endif
	}

}//namespace
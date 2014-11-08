
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Voxeland 
{
	[CustomEditor(typeof(VoxelandTerrain))]
	public class VoxelandEditor : Editor
	{
		VoxelandTerrain script; //aka target
		
		System.Reflection.FieldInfo undoCallback;
		
		int newChunkSize = -1; //int newChunkHeight = -1; int newChunkCountX = -1; int newChunkCountZ = -1;
		bool allowUndo = false;
		
		public void OnDisable ()
		{
			script = (VoxelandTerrain)target;
			
			//assigning delegates in Voxeland.OnDrawGizmos to make run withous selecting Voxeland
			UnityEditor.SceneView.onSceneGUIDelegate -= script.GetMouseButton;
			UnityEditor.EditorApplication.update -= script.EditorUpdate;
		}
		
		
		public void  OnSceneGUI ()
		{
			if (script == null) script = (VoxelandTerrain)target;
			if (!script.enabled) return;
			
			Vector2 mousePos = Event.current.mousePosition; 
			mousePos.y = Screen.height - mousePos.y - 40;
			
			//undo-redo
			if (Event.current.commandName == "UndoRedoPerformed") 
			{
				allowUndo = !allowUndo;
				if (!allowUndo || script.undoCoordsX.Count==0) return;
				
				script.data.PerformUndo();
				
				script.ResetProgress(
					script.undoCoordsX[ script.undoCoordsX.Count-1 ],
					script.undoCoordsZ[ script.undoCoordsZ.Count-1 ],
					script.undoExtend[ script.undoExtend.Count-1 ]);
				
				script.undoCoordsX.RemoveAt(script.undoCoordsX.Count-1);
				script.undoCoordsZ.RemoveAt(script.undoCoordsZ.Count-1);
				script.undoExtend.RemoveAt(script.undoExtend.Count-1);
			}
			
			
			//script.Display (UnityEditor.SceneView.lastActiveSceneView.camera.transform.position); //is done in EditorUpdate delegate
			script.Edit(UnityEditor.SceneView.lastActiveSceneView.camera, mousePos,
			     Event.current.type == EventType.MouseDown && Event.current.button == 0,
			     Event.current.shift,
			     Event.current.control);
			
			//un-selecting chunks
			Selection.activeGameObject = script.transform.gameObject;
			
		}
		
		#region Layout Instruments
		private int margin;
		private int lineHeight = 17;
		private Rect lastRect;
		private int inspectorWidth;
	
		public void NewLine () { NewLine(lineHeight); }
		public void NewLine (int height) 
		{ 
			lastRect = GUILayoutUtility.GetRect(1, height, "TextField");
			inspectorWidth = (int)lastRect.width + 12;
			lastRect.x = margin;
			lastRect.width = 0;
		}
		
		public void MoveLine (int offset) { MoveLine(offset, lineHeight); }
		public void MoveLine (int offset, int height)
		{
			lastRect = new Rect (margin, lastRect.y + offset, inspectorWidth - margin, height);
		}
	
		public Rect AutoRect () { return AutoRect(1f); }
		public Rect AutoRect (float width) { return AutoRect((int)((inspectorWidth-margin)*width - 3)); }
		public Rect AutoRect (int width)
		{
			lastRect = new Rect (lastRect.x+lastRect.width + 3, lastRect.y, width, lastRect.height);
			return lastRect;
		}
		
		public void MoveTo (float offset) { MoveTo((int)((inspectorWidth-margin)*offset - 3)); }
		public void MoveTo (int offset)
		{
			lastRect = new Rect (margin+offset, lastRect.y, 0, lastRect.height);
		}
		
		public void Resize (int left, int top, int right, int bottom)
		{
			lastRect = new Rect(lastRect.x-left, lastRect.y-top, lastRect.width+left+right, lastRect.height+top+bottom);
		}
		#endregion
		
		#region Array Instruments (test)
		VoxelandBlockType[]  ArrayAdd ( VoxelandBlockType[] array ,   VoxelandBlockType type  ){
			VoxelandBlockType[] newArray= new VoxelandBlockType[ array.Length+1 ];
			for (int i=0; i<array.Length; i++) newArray[i] = array[i];
			newArray[ array.Length ] = type;
			return newArray;
		}
		
		VoxelandBlockType[]  ArrayRemoveAt ( VoxelandBlockType[] array ,   int at  ){
			VoxelandBlockType[] newArray= new VoxelandBlockType[ array.Length-1 ];
			for (int i=0; i<array.Length-1; i++) 
			{
				if (i>=at) newArray[i] = array[i+1];
				else newArray[i] = array[i];
			}
			return newArray;
		}
		
		int ArraySwitch ( VoxelandBlockType[] array ,   int num1 ,   int num2  )
		{
			if (num1<0 || num1>=array.Length ||
			    num2<0 || num2>=array.Length) return num1;
			
			VoxelandBlockType tmp= array[num1];
			array[num1] = array[num2];
			array[num2] = tmp;
			
			return num2;
		}
		#endregion
		
		
		public override void  OnInspectorGUI ()
		{
			script = (VoxelandTerrain)target;
			
			EditorGUI.indentLevel = 0;
	
			margin = 15;
			NewLine(); script.brushSize = EditorGUI.IntSlider (AutoRect(), "Brush Size:", script.brushSize, 0, 6);
			NewLine(); script.brushSphere = EditorGUI.Toggle(AutoRect(), "Spherify", script.brushSphere);
			NewLine(50); EditorGUI.HelpBox(AutoRect(), "Press Left Click to add block,\nShift-Left Click to dig block,\nCtrl-Left Click to smooth blocks,\nCtrl-Shift-Left Click to replace block", MessageType.None);
	
			#region Rebuild and Bake
			NewLine(); if (GUI.Button(AutoRect(), "Rebuild")) 
			{ 
				//script.data.SetExists(script.types);
				script.Rebuild();
				//if (!script.dynamicRebuilding) script.Rebuild();
			}
			
			/*
			if (GUI.Button(AutoRect(0.5f), "Bake")) // && EditorUtility.DisplayDialog("Bake the terrain geometry to meshes", "This will destroy Voxeland component. Please save your work before baking.", "Bake", "Cancel")) 
			{
				script.dynamicRebuilding = false;
				script.Clear();
				//if (!script.dynamicRebuilding) script.Rebuild();
				script.enabled = false;
			}
			*/
			#endregion
	
			#region Block Types
			NewLine(); script.guiTypes = EditorGUI.Foldout(AutoRect(), script.guiTypes, "Block Types");
			if (script.guiTypes) 
			{
				margin = 40; 
				
				if (script.types==null || script.types.Length==0) script.types = new VoxelandBlockType[] {new VoxelandBlockType("Empty", false), new VoxelandBlockType("Ground", true)};
				
				for (int t=1; t<script.types.Length; t++) DrawType(script.types[t], t, t==script.selected);
	
				ArrayList typesArray = new ArrayList(script.types);
				if (ArrayButtons(typesArray, ref script.selected))
					script.types = (VoxelandBlockType[]) typesArray.ToArray(typeof(VoxelandBlockType));
				
				margin = 15;
			}
			#endregion
			
			#region Grass
			NewLine(); script.guiGrass = EditorGUI.Foldout(AutoRect(), script.guiGrass, "Grass");
			if (script.guiGrass) 
			{
				margin = 40; 
				
				if (script.grass==null || script.grass.Length==0) script.grass = new VoxelandGrassType[] {new VoxelandGrassType("Empty")};
				
				for (int t=1; t<script.grass.Length; t++) DrawGrass(script.grass[t], t, t==-script.selected);
				
				ArrayList typesArray = new ArrayList(script.grass);
				if (ArrayButtons(typesArray, ref script.selected))
					script.grass = (VoxelandGrassType[]) typesArray.ToArray(typeof(VoxelandGrassType));
				
				margin = 15;
				
			}
			#endregion	
			
			#region Generator
			NewLine(); script.guiGenerate = EditorGUI.Foldout(AutoRect(), script.guiGenerate, "Generator");
			if (script.guiGenerate)
			{
				margin = 30;
				
				if (script.generator==null) script.generator = new Generator();
				
				if (script.infinite)
				{
					NewLine(); script.generator.mapSizeX = EditorGUI.IntField(AutoRect(), "Generate Range:", script.generator.mapSizeX/2) * 2; 
					script.generator.mapSizeZ = script.generator.mapSizeX;
					NewLine(); EditorGUI.LabelField(AutoRect(), "Generate Center:");
					margin = 45;
					NewLine(); script.guiGeneratorUseCamPos = EditorGUI.ToggleLeft(AutoRect(), "Use Camera Position", script.guiGeneratorUseCamPos);
					NewLine();
					
					EditorGUI.BeginDisabledGroup (script.guiGeneratorUseCamPos);
					EditorGUI.PrefixLabel(AutoRect(0.1f), new GUIContent("X:"));
					script.guiGeneratorCenterX = EditorGUI.IntField(AutoRect(0.4f), script.guiGeneratorCenterX);
					EditorGUI.PrefixLabel(AutoRect(0.1f), new GUIContent("Z:"));
					script.guiGeneratorCenterZ = EditorGUI.IntField(AutoRect(0.4f), script.guiGeneratorCenterZ);
					EditorGUI.EndDisabledGroup();
					margin = 30;
				}
				else
				{
					script.guiGeneratorUseCamPos = false;
					script.generator.mapSizeX = script.displayExtend;
					script.guiGeneratorCenterX = script.displayCenterX;
					script.guiGeneratorCenterZ = script.displayCenterZ;
				}

				NewLine(); script.generator.seed = EditorGUI.IntField(AutoRect(), "Seed:", script.generator.seed);
				NewLine(); script.guiGeneratorOverwrite = EditorGUI.Toggle(AutoRect(), "Overwrite:", script.guiGeneratorOverwrite);
				
				NewLine(5);
				
				NewLine(); script.generator.level = EditorGUI.FloatField(AutoRect(), "Initial Level", script.generator.level);
				NewLine(); script.generator.noise = EditorGUI.ToggleLeft(AutoRect(), "Perlin Noise", script.generator.noise);
				if (script.generator.noise)
				{
					margin = 45;
					NewLine(); script.generator.noiseType = (byte)EditorGUI.IntField(AutoRect(), "Type", script.generator.noiseType);
					NewLine(); script.generator.fractals = EditorGUI.IntField(AutoRect(), "Fractals", script.generator.fractals); 
					NewLine(); script.generator.fractalMin = EditorGUI.FloatField(AutoRect(), "Fractal Min", script.generator.fractalMin);
					NewLine(); script.generator.fractalMax = EditorGUI.FloatField(AutoRect(), "Fractal Max", script.generator.fractalMax);  
					NewLine(); script.generator.valueMin = EditorGUI.FloatField(AutoRect(), "Value Min", script.generator.valueMin);   
					NewLine(); script.generator.valueMax = EditorGUI.FloatField(AutoRect(), "Value Max", script.generator.valueMax);
					margin = 30;
				}
				
				NewLine(); script.generator.valley = EditorGUI.ToggleLeft(AutoRect(), "Valley", script.generator.valley);
				if (script.generator.valley)
				{
					margin = 45;
					NewLine(); script.generator.valleyType = (byte)EditorGUI.IntField(AutoRect(), "Type", script.generator.valleyType);
					NewLine(); script.generator.valleyLevel = EditorGUI.FloatField(AutoRect(), "Level", script.generator.valleyLevel);
					NewLine(); script.generator.valleySize = EditorGUI.FloatField(AutoRect(), "Size", script.generator.valleySize);
					NewLine(); script.generator.valleyOpacity = EditorGUI.FloatField(AutoRect(), "Opacity", script.generator.valleyOpacity);
					margin = 30;
				}
				
				NewLine(); script.generator.terrace = EditorGUI.ToggleLeft(AutoRect(), "Terrace", script.generator.terrace);
				if (script.generator.terrace)
				{
					margin = 45;
					NewLine(); script.generator.minTerrace = EditorGUI.FloatField(AutoRect(), "Min Terrace", script.generator.minTerrace);
					NewLine(); script.generator.maxTerrace = EditorGUI.FloatField(AutoRect(), "Max Terrace", script.generator.maxTerrace);
					NewLine(); script.generator.terraceIncline = EditorGUI.FloatField(AutoRect(), "Incline", script.generator.terraceIncline);
					NewLine(); script.generator.terraceInclineLength = EditorGUI.FloatField(AutoRect(), "Incline Length", script.generator.terraceInclineLength);
					NewLine(); script.generator.terraceOpacity = EditorGUI.FloatField(AutoRect(), "Opacity", script.generator.terraceOpacity);
					margin = 30;
				}
				
				NewLine(); script.generator.plateau = EditorGUI.ToggleLeft(AutoRect(), "Plateau", script.generator.plateau);
				if (script.generator.plateau)
				{
					margin = 45;
					NewLine(); script.generator.plateauLevel = EditorGUI.FloatField(AutoRect(), "Level", script.generator.plateauLevel);
					NewLine(); script.generator.plateauOpacity = EditorGUI.FloatField(AutoRect(), "Opacity", script.generator.plateauOpacity);
					margin = 30;
				}
				
				NewLine(); script.generator.erosion = EditorGUI.ToggleLeft(AutoRect(), "Erosion", script.generator.erosion);
				if (script.generator.erosion)
				{
					margin = 45;
					NewLine(); script.generator.erosionType = (byte)EditorGUI.IntField(AutoRect(), "Sediment Type", script.generator.erosionType);
					NewLine(); script.generator.erosionAmount = EditorGUI.FloatField(AutoRect(), "Erosion Amount", script.generator.erosionAmount);
					NewLine(); script.generator.sedimentAmount = EditorGUI.FloatField(AutoRect(), "Sediment Amount", script.generator.sedimentAmount);
					NewLine(); script.generator.erosionIterations = EditorGUI.IntField(AutoRect(), "Iterations",  script.generator.erosionIterations);
					NewLine(); script.generator.sedimentLower = EditorGUI.FloatField(AutoRect(), "Lower Sediment", script.generator.sedimentLower);
					NewLine(); script.generator.erosionTorrentMax = EditorGUI.FloatField(AutoRect(), "Torrent Max", script.generator.erosionTorrentMax);
					NewLine(); script.generator.erosionBlur = EditorGUI.FloatField(AutoRect(), "Torrent Blur", script.generator.erosionBlur);			
					NewLine(); script.generator.erosionNoise = EditorGUI.FloatField(AutoRect(), "Additional Noise", script.generator.erosionNoise);
					margin = 30;
				}
				
				NewLine(); script.generator.grass = EditorGUI.ToggleLeft(AutoRect(), "Grass", script.generator.grass);
				if (script.generator.grass)
				{
					margin = 45;
					NewLine(); script.generator.grassType = (byte)EditorGUI.IntField(AutoRect(), "Grass Type", script.generator.grassType);
					NewLine(); script.generator.grassNoiseSize = EditorGUI.FloatField(AutoRect(), "Noise Size", script.generator.grassNoiseSize);
					NewLine(); script.generator.grassDensity = EditorGUI.FloatField(AutoRect(), "Density", script.generator.grassDensity);
					margin = 30;
				}
				
				NewLine(5);

				NewLine(); if (GUI.Button(AutoRect(), "Generate in Range"))
				{
					if (script.guiGeneratorUseCamPos)
					{
						Vector3 camPos = script.GetCameraPos(true);
						script.data.Generate(script.generator, (int)camPos.x, (int)camPos.z, script.generator.mapSizeX/2, script.guiGeneratorOverwrite);
						
						script.ResetProgress((int)camPos.x, (int)camPos.z, script.generator.mapSizeX/2);
						script.Refresh((int)camPos.x, (int)camPos.z, (int)Mathf.Min(script.generator.mapSizeX/2, script.generateDistance));
						script.Display();
						if (script.useFar) script.far.Build((int)camPos.x, (int)camPos.z);
					}
					else
					{
						script.data.Generate(script.generator, script.guiGeneratorCenterX, script.guiGeneratorCenterZ, script.generator.mapSizeX/2, script.guiGeneratorOverwrite);
						script.ResetProgress(script.guiGeneratorCenterX, script.guiGeneratorCenterZ, script.generator.mapSizeX/2);
					}
					
					script.Rebuild();
				}
				
				
				NewLine(); if (GUI.Button(AutoRect(), "Generate Grass"))
				{
					bool[] originalMask = new bool[script.generator.mapSizeX*script.generator.mapSizeX];
					for (int i=0; i<originalMask.Length; i++) originalMask[i] = true;
					
					script.generator.grassMap = new bool[script.generator.mapSizeX * script.generator.mapSizeX];
					script.generator.Grass();
					script.data.SetGrassmap(script.generator.grassMap, originalMask, 0, 0, script.generator.mapSizeX/2, 1);
				}
				
				
				NewLine(); if (GUI.Button(AutoRect(), "Clear Terrain") && EditorUtility.DisplayDialog("Warning", "This will remove all terrain data.\nAre you sure you wish to continue?", "OK", "Cancel"))
				{
					script.data = new Data();
					script.Rebuild();
					/*
					Vector3 camPos = script.GetCameraPos(true);
					script.ResetProgress((int)camPos.x, (int)camPos.z, script.generator.mapSizeX/2);
					script.Refresh((int)camPos.x, (int)camPos.z, (int)script.generateDistance);
					if (script.useFar) script.far.Build((int)camPos.x, (int)camPos.z);
					*/
				}
	
				margin = 15;
			}
			#endregion
	
			#region Materials
			NewLine(); script.guiMaterials = EditorGUI.Foldout(AutoRect(), script.guiMaterials, "Materials");
			if (script.guiMaterials)
			{
				margin = 30;
				
				#region Terrain
				NewLine(); script.guiTerrainMaterial = EditorGUI.Foldout(AutoRect(), script.guiTerrainMaterial, "Terrain");
				if (script.guiTerrainMaterial)
				{
					margin = 45;
					NewLine(); Shader newLandShader = (Shader)EditorGUI.ObjectField(AutoRect(), "Land Shader", script.landShader, typeof(Shader), false);
					if (newLandShader != script.landShader) { script.landShader = newLandShader; RefreshMaterials(); }
					NewLine(); Color newLandSpecular= EditorGUI.ColorField (AutoRect(), "Land Specular", script.landSpecular);
					if (newLandSpecular != script.landSpecular) { script.landSpecular = newLandSpecular; RefreshMaterials(); }
					NewLine(); float newLandShininess= EditorGUI.Slider (AutoRect(), "Land Shininess:", script.landShininess, 0.01f, 1);
					if (newLandShininess != script.landShininess) { script.landShininess = newLandShininess; RefreshMaterials(); }
					NewLine(); float newLandBakeAmbient= EditorGUI.Slider (AutoRect(), "Land Bake Ambient:", script.landBakeAmbient, 0, 20);
					if (newLandBakeAmbient != script.landBakeAmbient) { script.landBakeAmbient = newLandBakeAmbient; RefreshMaterials(); }
					NewLine(); float newLandTile= EditorGUI.Slider (AutoRect(), "Triplanar Tile:", script.landTile, 0.1f, 10f);
					if (newLandTile != script.landTile) { script.landTile = newLandTile; RefreshMaterials(); }
					margin = 30;
				}
				#endregion
				
				#region Additional Ambient
				NewLine(); script.guiAmbient = EditorGUI.Foldout(AutoRect(), script.guiAmbient, "Additional Ambient");
				if (script.guiAmbient)
				{
					margin=45;
					NewLine(); script.ambient = EditorGUI.ToggleLeft(AutoRect(), "On", script.ambient);
					NewLine(); script.landAmbient = EditorGUI.ColorField (AutoRect(), "Color", script.landAmbient);
					NewLine(); script.ambientSpread = EditorGUI.IntField(AutoRect(), "Spread", script.ambientSpread);
					NewLine(); script.ambientMargins = Mathf.Max(2, EditorGUI.IntField(AutoRect(), "Margins", script.ambientMargins)); //max(overlap,margins)
					margin=30;
				}
				#endregion
	
				NewLine(); script.grassMaterial = (Material)EditorGUI.ObjectField(AutoRect(), "Grass Material", script.grassMaterial, typeof(Material), false);
				NewLine(); script.grassAnimSpeed = EditorGUI.FloatField(AutoRect(), "Grass Animation Speed", script.grassAnimSpeed);
				
				NewLine(); script.highlightMaterial = (Material)EditorGUI.ObjectField(AutoRect(), "Highlight Material", script.highlightMaterial, typeof(Material), false);
				
				margin = 15;
			}
			#endregion
			
			#region Far
			NewLine(); script.guiFar = EditorGUI.Foldout(AutoRect(), script.guiFar, "Horizon Plane");
			if (script.guiFar)
			{
				margin = 30;
				
				if (!script.infinite) 
				{
					NewLine(30); 
					EditorGUI.HelpBox(AutoRect(), 
						"Horizon Plane could not be used without\ndynamic build on non-infinite terrain", MessageType.None);
				}
				
				EditorGUI.BeginDisabledGroup(!script.infinite);
				
				NewLine(); script.useFar = EditorGUI.Toggle(AutoRect(), "Use Horizon Plane", script.useFar);
				if (script.useFar)
				{
					bool farShouldBeRebuilt = false;
					if (script.far==null) { script.far = Far.Create(script); farShouldBeRebuilt = true; }
					
					NewLine();
					script.guiFarChunks = EditorGUI.IntField(AutoRect(), "Extent (in chunks)", script.guiFarChunks);
					NewLine();
					script.guiFarDensity = EditorGUI.IntField(AutoRect(), "Density", script.guiFarDensity);
					
					if (script.guiFarChunks != script.far.chunks || script.guiFarDensity != script.far.subdiv || farShouldBeRebuilt)
					{
						script.far.chunks = script.guiFarChunks;
						script.far.subdiv = script.guiFarDensity;
						Vector3 camPos = script.GetCameraPos(true);
						script.far.Build((int)camPos.x, (int)camPos.z);
					} 
				}
				
				EditorGUI.EndDisabledGroup();
				
				margin = 15;
			}
			else if (script.far!=null) DestroyImmediate(script.far.gameObject);
			#endregion
			
			
			#region Settings  
			NewLine(); script.guiSettings = EditorGUI.Foldout(AutoRect(), script.guiSettings, "Settings");
			if (script.guiSettings)
			{
				margin = 30;
				
				NewLine(); EditorGUI.PrefixLabel(AutoRect(0.25f), new GUIContent("Data:"));
				script.data = EditorGUI.ObjectField(AutoRect(0.5f), script.data, typeof(Voxeland.Data), false) as Voxeland.Data;
				if (GUI.Button(AutoRect(0.25f), "Save")) SaveData();

				#region Infinite
				NewLine(); script.infinite = EditorGUI.Toggle(AutoRect(), "Infinite", script.infinite);
				if (!script.infinite)
				{
					margin = 45;
					NewLine(); 
						EditorGUI.LabelField(AutoRect(0.47f), "Terrain Center:");
						EditorGUI.PrefixLabel(AutoRect(0.07f), new GUIContent("X:"));
						script.displayCenterX = EditorGUI.IntField(AutoRect(0.2f), script.displayCenterX);
						EditorGUI.PrefixLabel(AutoRect(0.07f), new GUIContent("Z:"));
						script.displayCenterZ = EditorGUI.IntField(AutoRect(0.2f), script.displayCenterZ);
					NewLine(); script.displayExtend = EditorGUI.IntField(AutoRect(), "Terrain Extend:", script.displayExtend);
					NewLine(5);
					margin = 30;
				}
				#endregion
				
				#region Lod and Dynamic
				NewLine(); script.lodDistance = EditorGUI.FloatField(AutoRect(), "LOD Distance", script.lodDistance);
				if (script.infinite)
				{
					NewLine(); script.generateDistance = EditorGUI.FloatField(AutoRect(), "Build Distance", script.generateDistance);
					NewLine(); script.autoGenerate = EditorGUI.Toggle(AutoRect(), "Auto Generate", script.autoGenerate);
				}
				if (!script.autoGenerate && script.infinite)
				{
					NewLine(); if (GUI.Button(AutoRect(), "Generate Now"))
					{
						script.lastRebuildX = 1000000;
						script.lastRebuildZ = 1000000;
						
						Vector3 localCamPos = script.GetCameraPos(true);
						script.Refresh((int)localCamPos.x, (int)localCamPos.z, (int)script.generateDistance);
					}
				}
				#endregion
				
				#region Resize
				NewLine(); 
				if (newChunkSize < 0) newChunkSize = script.chunkSize;
				EditorGUI.PrefixLabel(AutoRect(0.6f), new GUIContent("Chunk Size:"));
				newChunkSize = EditorGUI.IntField(AutoRect(0.2f), newChunkSize);
				if (GUI.Button(AutoRect(0.2f), "Set")) script.chunkSize = newChunkSize;
				#endregion
				

				NewLine(); script.playmodeEdit = EditorGUI.Toggle(AutoRect(), "Playmode Edit", script.playmodeEdit);
				
				NewLine(); script.weldVerts = EditorGUI.Toggle(AutoRect(), "Weld Verts", script.weldVerts);
				NewLine(); script.multiThreadEdit = EditorGUI.Toggle(AutoRect(), "Multithread (experimental)", script.multiThreadEdit);
				//if (!script.multiThreadEdit && script.threadsId!=0) script.StopThreads(); //turning off all threads if multithreaded edit off
				
				NewLine(); script.generateLightmaps = EditorGUI.Toggle (AutoRect(), "Generate Lightmaps:", script.generateLightmaps);
				//NewLine(); script.lightmapPadding = EditorGUI.Slider (AutoRect(), "Lightmap Padding:", script.lightmapPadding, 0, 0.5f);
				
				NewLine(); script.saveMeshes = EditorGUI.Toggle(AutoRect(), "Save Meshes with Scene", script.saveMeshes);
				
				margin = 15;
			}
			#endregion
			
			#region Import and Export
			NewLine(); script.guiImportExport = EditorGUI.Foldout(AutoRect(), script.guiImportExport, "Import and Export");
			if (script.guiImportExport)
			{
				#region Import 2 Data
				NewLine(); if (GUI.Button(AutoRect(), "Import v2 Data"))
				{
					string path= UnityEditor.EditorUtility.OpenFilePanel(
						"Load Voxeland v2 Data",
						"Assets",
						"asset");
					if (path!=null)
					{
						//TODO: import data
						
						path = path.Replace(Application.dataPath, "Assets");
						VoxelandData oldData = (VoxelandData)AssetDatabase.LoadAssetAtPath(path, typeof(VoxelandData));
						
						script.data.Load20(oldData);
						for (int i=0; i<script.types.Length; i++) script.types[i].smooth=1f;
						script.data.compressed = script.data.SaveToByteList();
						EditorUtility.SetDirty(script.data);
						script.Rebuild();
					}
				}
				#endregion
				
				#region Import Heightmap
				NewLine(); if (GUI.Button(AutoRect(), "Import Heightmap"))
				{
					string path= UnityEditor.EditorUtility.OpenFilePanel(
						"Load Heightmap",
						"Assets",
						"png");
					if (path!=null)
					{
						path = path.Replace(Application.dataPath, "Assets");
						Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
						if (texture == null) return;
						
						if (texture.width != texture.height) { EditorUtility.DisplayDialog("Error", "Only square textures allowed", "Cancel"); return; }
						
						Color[] pixels = texture.GetPixels(0, 0, texture.width, texture.width);
						float[] heightmap = new float[pixels.Length];
						bool[] mask = new bool[pixels.Length];
						
						for (int i=0; i<heightmap.Length; i++) 
						{
							heightmap[i] = (pixels[i].r + pixels[i].g + pixels[i].b) / 3.0f * 255;
							mask[i] = true;
						}
						
						script.data.Clear(0, 0, texture.width/2);
						script.data.SetHeightmap (heightmap, mask, 0, 0, texture.width/2, 1);
						script.data.compressed = script.data.SaveToByteList();
						EditorUtility.SetDirty(script.data);
						script.Rebuild();
					}
				}
				#endregion
				
				#region TXT
				NewLine();
				if (GUI.Button(AutoRect(), "Load TXT")) 
				{ 
					string path= UnityEditor.EditorUtility.OpenFilePanel(
						"Load Data from String",
						"Assets",
						"txt");
					if (path!=null)
					{
						path = path.Replace(Application.dataPath, "Assets");
						
						using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open))
							using (System.IO.StreamReader reader = new System.IO.StreamReader(fs))
								script.data.LoadFromString( reader.ReadToEnd() );
						script.Rebuild(); 
					}
					script.data.compressed = script.data.SaveToByteList();
					EditorUtility.SetDirty(script.data);	
				}
				
				NewLine(10);
				NewLine();
				if (GUI.Button(AutoRect(), "Save TXT")) 
				{
					string path= UnityEditor.EditorUtility.SaveFilePanel(
						"Save Data to String",
						"Assets",
						"data.txt", 
						"txt");
					if (path!=null)
					{
						path = path.Replace(Application.dataPath, "Assets");
						
						using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
							using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fs))
								writer.Write(script.data.SaveToString());
					}
				}
				#endregion
				
				#region Bake  
				NewLine(); 
				if (GUI.Button(AutoRect(), "Bake meshes to Scene"))
				{
					Bake();
				}
				
				NewLine(); 
				if (GUI.Button(AutoRect(), "Bake meshes to Assets"))
				{
					string path= EditorUtility.SaveFolderPanel("Save meshes to directory", "Assets", "VoxelandBakedMeshes");
					if (path==null) return;
					path = path.Replace(Application.dataPath, "Assets");

					Bake();
					
					/*
					Mesh[] meshes = new Mesh[script.activeChunks.Count];
					for (int i=0; i<script.activeChunks.Count; i++) meshes[i] = script.activeChunks[i].hiFilter.sharedMesh;
					AssetDatabase.CreateAsset(meshes, path);
					*/
					
					for (int i=0; i<script.activeChunks.Count; i++)
					{
						AssetDatabase.CreateAsset(script.activeChunks[i].hiFilter.sharedMesh, path + "/" + script.transform.name + "_Chunk" + i + ".asset");
					}
					AssetDatabase.SaveAssets();
					
					/*
					AssetDatabase.CreateAsset(script.activeChunks[0].hiFilter.sharedMesh, path);
					
					for (int i=1; i<script.activeChunks.Count; i++)
						AssetDatabase.AddObjectToAsset(script.activeChunks[i].hiFilter.sharedMesh, script.activeChunks[0].hiFilter.sharedMesh);
					*/
					
					//AssetDatabase.CreateAsset(script.transform, path);
				}
				
				NewLine(); script.guiBakeLightmap = EditorGUI.Toggle(AutoRect(), "Bake Lightmap", script.guiBakeLightmap);
				if (script.guiBakeLightmap) 
				{
					NewLine(30); 
					EditorGUI.HelpBox(AutoRect(), "Warning: Enabling Lightmaps can greatly\nincrease baking time.", MessageType.None);
				}
				#endregion
				
				NewLine(); if (GUI.Button(AutoRect(), "Save Data Asset")) SaveData();
				
			}
			#endregion
			
			#region Debug
			
			NewLine(); script.guiDebug = EditorGUI.Foldout(AutoRect(), script.guiDebug, "Debug");
			if (script.guiDebug)
			{
				margin = 30; 
				
				//NewLine(); script.alwaysRebuild = EditorGUI.Toggle(AutoRect(), "Always Rebuild", script.alwaysRebuild);
				//NewLine(); script.alwaysSetBlock = EditorGUI.Toggle(AutoRect(), "Always Set Block", script.alwaysSetBlock);
				
				//NewLine(); script.visualizeData = EditorGUI.Toggle(AutoRect(), "Visualize Data", script.visualizeData);
				//NewLine(); script.visualizeFill = EditorGUI.Toggle(AutoRect(), "Visualize Fill", script.visualizeFill);
				//NewLine(); script.visualizeTopBottom = EditorGUI.Toggle(AutoRect(), "Visualize Top Bottom", script.visualizeTopBottom);
				//NewLine(); script.visualizeChunk = EditorGUI.Toggle(AutoRect(), "Visualize Chunk", script.visualizeChunk);
				//NewLine(); script.visualizeHeightmap = EditorGUI.Toggle(AutoRect(), "Visualize Heightmap", script.visualizeHeightmap);
				//NewLine(); script.visualizeAmbient = EditorGUI.Toggle(AutoRect(), "Visualize Ambient", script.visualizeAmbient);
				//NewLine(); script.rotateVizualisation = EditorGUI.Toggle(AutoRect(), "Rotate Vizualisation", script.rotateVizualisation);
				//NewLine(); script.visualizeNormals = EditorGUI.Toggle(AutoRect(), "Visualize Normals", script.visualizeNormals);
				//NewLine(); script.visualizeConstructor = EditorGUI.Toggle(AutoRect(), "Visualize Constructor", script.visualizeConstructor);
				NewLine(); script.benchmark = EditorGUI.Toggle(AutoRect(), "Benchmark", script.benchmark);
				NewLine(); script.profile = EditorGUI.Toggle(AutoRect(), "Profile", script.profile);
				NewLine(); script.hideChunks = EditorGUI.Toggle(AutoRect(), "Hide Chunk objects", script.hideChunks);
				NewLine(); script.hideWire = EditorGUI.Toggle(AutoRect(), "Hide Chunk wire", script.hideWire);
				
				margin = 15;
			}
			#endregion
		
		}
	
		
		bool ArrayButtons (ArrayList array, ref int selected) //returns true if array list should be converted
		{
			bool change = false;
			margin = 40;
			
			NewLine();
			if (GUI.Button(AutoRect(0.25f), "Up") && selected != 1)
			{
				object tmp = array[selected];
				array[selected] = script.types[script.selected-1];
				array[selected-1] = tmp;
				selected--;
				change = true;
			}
			
			if (GUI.Button(AutoRect(0.25f), "Down") && selected < array.Count-1)
			{
				object tmp = array[selected];
				array[selected] = script.types[script.selected+1];
				array[selected+1] = tmp;
				selected++;
				change = true;
			}
			
			if (GUI.Button(AutoRect(0.25f), "Add"))
			{
				System.Type type = array[0].GetType();
				System.Reflection.MethodInfo memberwiseCloneMethod = type.GetMethod ("MemberwiseClone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); 
				array.Add(memberwiseCloneMethod.Invoke (array[0], null));
				change = true;
			}
			
			if (GUI.Button(AutoRect(0.25f), "Remove"))
			{
				array.RemoveAt(selected);
				change = true;
			}
			
			return change;
			
			
		}
		
		void DrawType (VoxelandBlockType type, int num, bool selected)
		{
			margin = 18;
	
			#region Selected
			if (selected)
			{
				NewLine(3);
				
				//drawing backgroung rect
				int backHeight = lineHeight*6 + 85; //lineHeight*2;
				//if (type.filled && type.visible) backHeight = lineHeight*6 + 85;
				//if (type.filled && !type.visible) backHeight = lineHeight*3 + 40;
				if (!type.filled) backHeight = lineHeight*2 + 40;
				
				Rect backRect = new Rect(lastRect.x, lastRect.y, inspectorWidth-margin, backHeight);
				GUI.Box(backRect, "");
				
				//texture thumb
				NewLine(25); AutoRect(0);
				GUI.Box(AutoRect(25), "");
				Resize(-1,-1,-1,-1);
				if (type.filled && type.texture != null) EditorGUI.DrawPreviewTexture(lastRect, type.texture);
				Resize(0,-3,0,-3);
				type.name = EditorGUI.TextField(AutoRect(inspectorWidth-margin-38), type.name); 
				
				margin = 50;
	
				NewLine();
				type.filled = EditorGUI.ToggleLeft(AutoRect(0.32f), "Filled", type.filled);
				//if (type.filled) type.visible = EditorGUI.ToggleLeft(AutoRect(0.32f), "Visible", type.visible);
				
				//terrain
				if (type.filled) // && type.visible)
				{
					NewLine(); 
					EditorGUI.LabelField(AutoRect(0.53f), "Texture and Bump:");
					EditorGUI.PrefixLabel(AutoRect(80), new GUIContent("Different Top:"));
					type.differentTop = EditorGUI.Toggle(AutoRect(20), type.differentTop);
					
					NewLine(50);
					type.texture = (Texture)EditorGUI.ObjectField(AutoRect(50), type.texture, typeof(Texture), false);
					type.bumpTexture = (Texture)EditorGUI.ObjectField(AutoRect(50), type.bumpTexture, typeof(Texture), false);
	
					if (type.differentTop)
					{
						MoveTo(0.49f); AutoRect(1);
						type.topTexture = (Texture)EditorGUI.ObjectField(AutoRect(50), type.topTexture, typeof(Texture), false);
						type.topBumpTexture = (Texture)EditorGUI.ObjectField(AutoRect(50), type.topBumpTexture, typeof(Texture), false);
					}
					
					NewLine(5);
					NewLine(); EditorGUI.PrefixLabel(AutoRect(0.28f), new GUIContent("Smooth"));
					type.smooth = EditorGUI.Slider(AutoRect(0.7f), type.smooth, 0, 1);
					NewLine(); type.grass = EditorGUI.ToggleLeft(AutoRect(0.98f), "Grass", type.grass);	
				}
				
				//constructor
				/*
				if (type.filled && !type.visible) //DungeonConstructor
				{
					NewLine();
					type.constructor = (Constructor)EditorGUI.ObjectField(AutoRect(0.98f), "Constructor:", type.constructor, typeof(Constructor), false);
					if (type.constructor != null) type.smooth = 0;
				}
				*/
				
				//prefab
				NewLine(); 
				EditorGUI.PrefixLabel(AutoRect(0.3f), new GUIContent("Prefab:"));
				type.obj = (Transform)EditorGUI.ObjectField(AutoRect(0.7f), type.obj, typeof(Transform), false);
				
				NewLine(5);
				
			}
			#endregion
			
			#region Un-Selected
			else
			{
				//texture thumb
				NewLine(25); AutoRect(0);
				if (GUI.Button(AutoRect(25), "", "Box")) script.selected = (byte)num;
				Resize(-1,-1,-1,-1);
				if (type.filled && type.texture != null) EditorGUI.DrawPreviewTexture(lastRect, type.texture);
				Resize(0,-3,0,-3);
	
				if (GUI.Button(AutoRect(), type.name, "Label")) script.selected = (byte)num;
			}
			#endregion
		}
		
		void DrawGrass (VoxelandGrassType type, int num, bool selected)
		{
			margin = 18;
			
			#region Selected
			if (selected)
			{
				NewLine(3); 
				
				//drawing backgroung rect
				int backHeight = lineHeight*2 + 20;
	
				Rect backRect = new Rect(lastRect.x, lastRect.y, inspectorWidth-margin, backHeight);
				GUI.Box(backRect, "");
				
				//texture thumb
				NewLine(25); AutoRect(0);
				GUI.Box(AutoRect(25), "");
				Resize(-1,-1,-1,-1);
				if (type.material!=null && type.material.mainTexture!=null) EditorGUI.DrawPreviewTexture(lastRect, type.material.mainTexture);
				Resize(0,-3,0,-3);
				type.name = EditorGUI.TextField(AutoRect(inspectorWidth-margin-38), type.name); 
				
				margin = 60;
				NewLine(); 
				EditorGUI.LabelField(AutoRect(0.3f), "Material:");
					
				type.material = (Material)EditorGUI.ObjectField(AutoRect(0.69f), type.material, typeof(Material), false); 
				//type.bumpTexture = (Texture)EditorGUI.ObjectField(AutoRect(53), type.bumpTexture, typeof(Texture), false);
	
				NewLine(5);
				
			}
			#endregion
			
			#region Un-Selected
			else
			{
				//texture thumb
				NewLine(25); AutoRect(0);
				if (GUI.Button(AutoRect(25), "", "Box")) script.selected = -num;
				Resize(-1,-1,-1,-1);
				if (type.material!=null && type.material.mainTexture!=null) EditorGUI.DrawPreviewTexture(lastRect, type.material.mainTexture);
				Resize(0,-3,0,-3);
				
				if (GUI.Button(AutoRect(), type.name, "Label")) script.selected = -num;
			}
			#endregion
		}
		
	
		public void RefreshMaterials ()
		{
			for (int c=0; c<script.activeChunks.Count; c++)
			{
				if (!script.activeChunks[c]) continue;
				
				Material mat = null;
				if (script.activeChunks[c].hiFilter!=null) mat = script.activeChunks[c].hiFilter.renderer.sharedMaterial;
				else if (script.activeChunks[c].loFilter!=null) mat = script.activeChunks[c].loFilter.renderer.sharedMaterial;
				if (!mat) continue;
				
				mat.shader = script.landShader;
				mat.SetColor("_Ambient", script.landAmbient);
				mat.SetColor("_SpecColor", script.landSpecular);
				mat.SetFloat("_Shininess", script.landShininess);
				mat.SetFloat("_BakeAmbient", script.landBakeAmbient);
				mat.SetFloat("_Tile", script.landTile);
			}
		}
		
		public void SaveData ()
		{
				string path= UnityEditor.EditorUtility.SaveFilePanel(
					"Save Data as Unity Asset",
					"Assets",
					"VoxelandData.asset", 
					"asset");
				if (path!=null)
				{
					path = path.Replace(Application.dataPath, "Assets");
					
					AssetDatabase.CreateAsset(script.data, path);
					AssetDatabase.SaveAssets();
				}
		}
		
		public void Bake ()
		{
			if (script.infinite)
			{
				EditorUtility.DisplayDialog("Error", "Baking is not possible on infinite terrain.\nTurn off infinite terrain in settings and\nadjust terrain center and extends.", "OK");	
				return;
			}
			
			bool oldSaveMeshes = script.saveMeshes;
			bool oldHideChunks = script.hideChunks;
			bool oldHideWire = script.hideWire;
			bool oldMultiThreadEdit = script.multiThreadEdit;
			
			script.saveMeshes = true;
			script.hideChunks = false;
			script.hideWire = false;
			script.multiThreadEdit = false;
			script.generateLightmaps = script.guiBakeLightmap;
			
			script.Rebuild();
			script.SwitchLods(new Vector3(script.displayCenterX, 0, script.displayCenterZ), 2000000000);
			
			//removing lod renderers
			foreach (Transform chunk in script.transform)
				foreach(Transform child in chunk.transform)
					if (child.name == "LoResChunk")
						DestroyImmediate(child.GetComponent<MeshRenderer>());
			
			script.saveMeshes = oldSaveMeshes;
			script.hideChunks = oldHideChunks;
			script.hideWire = oldHideWire;
			script.multiThreadEdit = oldMultiThreadEdit;
			script.generateLightmaps = false;

			script.enabled = false;
			OnDisable();
		}
		
		#region Test Lightmaps
		[MenuItem("GameObject/GenerateUV")]
		static void Test ()
		{
			UnityEditor.Unwrapping.GenerateSecondaryUVSet(Selection.activeTransform.GetComponent<MeshFilter>().sharedMesh);
		}
		#endregion
	}
	
	
	#region Create terrain
	class VoxelandCreate : EditorWindow 
	{
		enum InitialTerrainType  {empty, flat, generate};
		int chunkSize = 30;
		int generateRange = 256;
		int level = 50;
		bool infinite = false;
		InitialTerrainType initialTerrain = InitialTerrainType.flat;
		
		//int chunkHeight = 128;
		//int overlap = 2;
		
		//int chunksX = 10;
		//int chunksZ = 10;
		
		//bool cap;
		
		
		[MenuItem("GameObject/Create Other/Voxeland Terrain")]
		static void  Init ()
		{
			//FIXME_VAR_TYPE window= new VoxelandCreateDataWindow();
			VoxelandCreate window= ScriptableObject.CreateInstance<VoxelandCreate>();
			window.position = new Rect(Screen.width/2,Screen.height/2, 370, 170);
			window.ShowUtility();
		}
		
		#region Layout Instruments
		private int margin = 0;
		private int lineHeight = 17;
		private Rect lastRect;
		private int inspectorWidth;
		
		public void NewLine () { NewLine(lineHeight); }
		public void NewLine (int height) 
		{ 
			lastRect = GUILayoutUtility.GetRect(1, height, "TextField");
			inspectorWidth = (int)lastRect.width; 
			lastRect.x = margin;
			lastRect.width = 0;
		}
		
		public void MoveLine (int offset) { MoveLine(offset, lineHeight); }
		public void MoveLine (int offset, int height)
		{
			lastRect = new Rect (margin, lastRect.y + offset, inspectorWidth - margin, height);
		}
		
		public Rect AutoRect () { return AutoRect(1f); }
		public Rect AutoRect (float width) { return AutoRect((int)((inspectorWidth-margin)*width - 3)); }
		public Rect AutoRect (int width)
		{
			lastRect = new Rect (lastRect.x+lastRect.width + 3, lastRect.y, width, lastRect.height);
			return lastRect;
		}
		
		public void MoveTo (float offset) { MoveTo((int)((inspectorWidth-margin)*offset - 3)); }
		public void MoveTo (int offset)
		{
			lastRect = new Rect (margin+offset, lastRect.y, 0, lastRect.height);
		}
		
		public void Resize (int left, int top, int right, int bottom)
		{
			lastRect = new Rect(lastRect.x-left, lastRect.y-top, lastRect.width+left+right, lastRect.height+top+bottom);
		}
		#endregion
		
		void  OnGUI ()
		{
			NewLine(); chunkSize = EditorGUI.IntField(AutoRect(), "Chunk Size:", chunkSize);
			
			NewLine(15);
			NewLine(); initialTerrain = (InitialTerrainType)EditorGUI.EnumPopup(AutoRect(), "Initial Terrain:", initialTerrain);
			NewLine(); level = EditorGUI.IntField(AutoRect(), "Initial Terrain Level:", level);
			NewLine(); 
			if (infinite) generateRange = EditorGUI.IntField(AutoRect(), "Initial Terrain Size:", generateRange);
			else generateRange = EditorGUI.IntField(AutoRect(), "Terrain Size:", generateRange);
			NewLine(); infinite = EditorGUI.ToggleLeft(AutoRect(), "Infinite", infinite);			
			//chunksX = EditorGUI.IntField(AutoRect(0.5f), "Chunks X:", chunksX);
			//chunksZ = EditorGUI.IntField(AutoRect(0.5f), "Chunks Z:", chunksZ);
			
			NewLine(15);
			NewLine(); if (GUI.Button(AutoRect(), "Create Terrain"))
			{ 
				GameObject terrainObj= new GameObject("Voxeland");
				VoxelandTerrain terrain= terrainObj.AddComponent<VoxelandTerrain>();
				
				terrain.chunkSize = chunkSize;
				terrain.landShader = Shader.Find("Voxeland/TerrainBump4Triplanar");
				terrain.infinite = infinite;
				
				//setting initial types
				terrain.types = new VoxelandBlockType[] {new VoxelandBlockType("Empty", false), new VoxelandBlockType("Ground", true)};
				terrain.grass = new VoxelandGrassType[] {new VoxelandGrassType("Empty")};
				terrain.selected = 1;
				
				//creating initial data
				terrain.data = ScriptableObject.CreateInstance<Data>();
				terrain.data.name = "VoxelandData";
				terrain.data.New();
				
				if (initialTerrain == InitialTerrainType.generate) terrain.data.Generate(terrain.generator, 0,0, generateRange/2, true);
				if (initialTerrain == InitialTerrainType.flat) terrain.data.SetLevel(level, 0,0, generateRange/2, 1);
				
				this.Close();
				terrain.Rebuild();
			}
		}
		
	}
	#endregion
	
	#region processor Do Not save Meshes
	[ExecuteInEditMode]
	public class VoxelandModificationProcessor : UnityEditor.AssetModificationProcessor  
	{
		static string[] OnWillSaveAssets (string[] paths) 
		{
			for (int p=0; p<paths.Length; p++)
				if (paths[p].EndsWith(".unity")) //when saving scene
			{	
				VoxelandTerrain[] voxelands = (VoxelandTerrain[])GameObject.FindObjectsOfType(typeof(VoxelandTerrain));
				
				for (int i=0; i<voxelands.Length; i++)
					if (!voxelands[i].saveMeshes) 
					{
						if (voxelands[i].hideChunks)
						{ 
							foreach (Transform child in voxelands[i].transform)
								VoxelandTerrain.SetHideFlagsRecursively(HideFlags.HideAndDontSave, child);
						}
						else 
						{
							foreach (Transform child in voxelands[i].transform)
								VoxelandTerrain.SetHideFlagsRecursively(HideFlags.DontSave, child);
						}
						for (int c=0; c<voxelands[i].activeChunks.Count; c++) voxelands[i].activeChunks[c].transform.parent = null;
						if (voxelands[i].far != null) voxelands[i].far.transform.parent = null;
						if (voxelands[i].highlight != null) voxelands[i].highlight.transform.parent = null;
						voxelands[i].chunksUnparented = true;
					}
			}
			
			return paths;
		}
	}
	#endregion
	
} //namespace
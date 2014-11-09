using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.IO;

namespace Planetary {

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
	private Planet planet;

	public enum Tab {
		GENERATION, LOD, PAINTING, SHADER
	}
	private Tab currentTab = Tab.GENERATION;
	private string[] tabTexts = new string[4] {"GENERATE", "LOD", "PAINT", "SHADER"};
	private string[] brushTexts = new string[3] {"ADD", "SUBSTRACT", "SET"};

	private bool showColliderTab = false;

	private Vector3 lastPoint, lastNormal;

	#region Menu
	
	[MenuItem("GameObject/Create Other/PlanetaryTerrain/Planet")]
	private static void CreatePlanet() {
		PlanetaryTerrain.CreatePlanet();
	}

	[MenuItem("GameObject/Create Other/PlanetaryTerrain/Atmosphere")]
	private static void CreateAtmosphere() {
		PlanetaryTerrain.CreateAtmosphere(1.1f);
	}

	[MenuItem("GameObject/Create Other/PlanetaryTerrain/Hydrosphere")]
	private static void CreateHydrosphere() {
		PlanetaryTerrain.CreateHydrosphere(10.02f);
	}
	
	#endregion

	SerializedProperty terrainAsset, frequencyScale, radius, heightVariation, meshResolution, subdivisions;
	SerializedProperty generateOnStart, randomizeSeeds, seed, createBorders, logGenerationTimes, showDebug;
	//SerializedProperty useLod, maxLodLevel, lodDistances;
	SerializedProperty lodTarget, simultaneousSubdivisions, useAngleCulling, rendererCullingAngle, lodUpdateInterval;
	//SerializedProperty generateColliders;

	void OnEnable() {
		// the planet being edited
		planet = (Planet)target;
		planet.LoadModule();

		terrainAsset = serializedObject.FindProperty("terrainAsset");
		frequencyScale = serializedObject.FindProperty("frequencyScale");
		radius = serializedObject.FindProperty("radius");
		heightVariation = serializedObject.FindProperty("heightVariation");
		meshResolution = serializedObject.FindProperty("meshResolution");
		subdivisions = serializedObject.FindProperty("subdivisions");

		createBorders = serializedObject.FindProperty("createBorders");
		logGenerationTimes = serializedObject.FindProperty("logGenerationTimes");
		showDebug = serializedObject.FindProperty("showDebugInfo");

		generateOnStart = serializedObject.FindProperty("generateOnStart");
		randomizeSeeds = serializedObject.FindProperty("randomizeSeeds");
		seed = serializedObject.FindProperty("seed");

		//useLod = serializedObject.FindProperty("useLod");
		//maxLodLevel = serializedObject.FindProperty("maxLodLevel");
		//lodDistances = serializedObject.FindProperty("lodDistances");

		lodTarget = serializedObject.FindProperty("lodTarget");
		simultaneousSubdivisions = serializedObject.FindProperty("simultaneousSubdivisions");
		useAngleCulling = serializedObject.FindProperty("useAngleCulling");
		rendererCullingAngle = serializedObject.FindProperty("rendererCullingAngle");
		lodUpdateInterval = serializedObject.FindProperty("lodUpdateInterval");

		//generateColliders = serializedObject.FindProperty("generateColliders");

		// hide wireframes
		Renderer[] renderers = planet.gameObject.GetComponentsInChildren<Renderer>();
		for(int i = 0; i < renderers.Length; i++){
			EditorUtility.SetSelectedWireframeHidden(renderers[i], true);
		}
	}

	void OnDisable() {
		// show surface wireframes
		if(planet != null) {
			Renderer[] renderers = planet.gameObject.GetComponentsInChildren<Renderer>();
			for(int i = 0; i < renderers.Length; i++){
				EditorUtility.SetSelectedWireframeHidden(renderers[i], false);
			}
		}
	}

	override public void OnInspectorGUI() {
		serializedObject.Update();

		// toolbar
		EditorGUILayout.Space();
		currentTab = (Tab)GUILayout.Toolbar((int)currentTab, tabTexts, EditorStyles.toolbarButton);
		
		switch(currentTab) {
		case Tab.GENERATION:
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Terrain Module:", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			terrainAsset.objectReferenceValue = EditorGUILayout.ObjectField(terrainAsset.objectReferenceValue, typeof(TextAsset), false);

			if(GUILayout.Button("Edit Module", GUILayout.Width(90f))) {
				ModuleEditorWindow.Init();
				((ModuleEditorWindow)ModuleEditorWindow.window).Load((TextAsset)terrainAsset.objectReferenceValue, planet);
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Frequency scale:");
			frequencyScale.floatValue = EditorGUILayout.FloatField(frequencyScale.floatValue);
		    EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Mesh Settings", EditorStyles.boldLabel);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Radius:");
			radius.floatValue = EditorGUILayout.FloatField(radius.floatValue);
		    EditorGUILayout.EndHorizontal();
				
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Height variation:");
			heightVariation.floatValue = EditorGUILayout.FloatField(heightVariation.floatValue);
		    EditorGUILayout.EndHorizontal();
				
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Mesh resolution:");
			meshResolution.intValue = EditorGUILayout.IntField(meshResolution.intValue);
		    EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Subdivisions:");
			subdivisions.intValue = EditorGUILayout.IntField(subdivisions.intValue);
		    EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			
			generateOnStart.boolValue = EditorGUILayout.Toggle("Generate on start", generateOnStart.boolValue);
			if(generateOnStart.boolValue)
				randomizeSeeds.boolValue = EditorGUILayout.Toggle("Randomize seeds", randomizeSeeds.boolValue);
			else
				randomizeSeeds.boolValue = false;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Random seed:");
			seed.floatValue = EditorGUILayout.FloatField(seed.floatValue);
			if(GUILayout.Button("Rnd", GUILayout.Width(90f))) {
				seed.floatValue = (float)UnityEngine.Random.Range(-1000f, 1000f);
			}
			EditorGUILayout.EndHorizontal();
			
			if(GUILayout.Button("Generate now")) {
				//Undo.RegisterFullObjectHierarchyUndo(planet.gameObject);//, "Generate planet");
				Generate(planet);
			}
			if(GUILayout.Button("Clear")) {
				//Undo.RegisterFullObjectHierarchyUndo(planet.gameObject);//, "Clear surfaces");
				planet.ClearSurfaces();
			}

			// COLLIDERS
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Colliders", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("Lower level colliders are automatically disabled when a higher level collider is generated.", MessageType.Info);
			
			showColliderTab = EditorGUILayout.Foldout(showColliderTab, "Show");
			if(showColliderTab) {
				EditorGUILayout.LabelField("Level 0:");
				planet.generateColliders[0] = EditorGUILayout.Toggle("Generate colliders", planet.generateColliders[0]);
				//planet.disableCollidersOnSubdivision[0] = EditorGUILayout.Toggle("Disable when subdividing", planet.disableCollidersOnSubdivision[0]);
				if(planet.useLod) {
					for(int i = 0; i < planet.lodDistances.Length; i++) {
						EditorGUILayout.LabelField("Level " + (i+1).ToString() + ":");
						planet.generateColliders[i+1] = EditorGUILayout.Toggle("Generate colliders", planet.generateColliders[i+1]);
						//planet.disableCollidersOnSubdivision[i+1] = EditorGUILayout.Toggle("Disable when subdividing", planet.disableCollidersOnSubdivision[i+1]);
					}
				}
			}

			// EXPORT
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);
			
			EditorGUILayout.HelpBox("Creates a prefab with current settings. Also saves meshes and material to disc if they have been generated.", MessageType.Info);
			
			if(GUILayout.Button("Save to Assets")) {
				string path = EditorUtility.SaveFilePanelInProject("Select Folder", "", "", "");
				if(path != "") {
					Material mat = null;

					Surface[] surfaces = planet.GetComponentsInChildren<Surface>(true);

					for(int i = 0; i < surfaces.Length; i++) {
						if(surfaces[i].mesh != null) {
							AssetDatabase.CreateAsset(surfaces[i].mesh, path + surfaces[i].name.ToString() + "_" + i.ToString() + ".asset");
							if(mat == null) {
								mat = surfaces[i].renderer.sharedMaterial;
								AssetDatabase.CreateAsset(mat, path + surfaces[i].name.ToString() + "Material.mat");
							}
						}
					}
					PrefabUtility.CreatePrefab(path + ".prefab", planet.gameObject);
					
					AssetDatabase.SaveAssets();
				}
			}

			break;
		case Tab.LOD:
			// LOD
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("LOD", EditorStyles.boldLabel);
			planet.useLod = EditorGUILayout.Toggle("Use LOD", planet.useLod);
			
			if(planet.useLod) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("LOD target:");
				lodTarget.objectReferenceValue = EditorGUILayout.ObjectField(lodTarget.objectReferenceValue, typeof(Transform), true);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("LOD levels:");
				planet.maxLodLevel = EditorGUILayout.IntField(planet.maxLodLevel);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Level loading distances:");
				EditorGUILayout.EndHorizontal();
				
				for(int i = 0; i < planet.lodDistances.Length; i++) {
					planet.lodDistances[i] = EditorGUILayout.FloatField("Level " + (i+1).ToString() + ":", planet.lodDistances[i]);
				}

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Minimum angle (planet-viewer dot planet-surface):");
				EditorGUILayout.EndHorizontal();

				for(int i = 0; i < planet.lodDots.Length; i++) {
					planet.lodDots[i] = EditorGUILayout.FloatField("Dot angle " + (i+1).ToString() + ":", planet.lodDots[i]);
				}
				
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Performance:", EditorStyles.boldLabel);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("LOD update interval:");
				lodUpdateInterval.floatValue = EditorGUILayout.FloatField(lodUpdateInterval.floatValue);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Simultaneous subdivisions:");
				simultaneousSubdivisions.intValue = EditorGUILayout.IntField(simultaneousSubdivisions.intValue);
				EditorGUILayout.EndHorizontal();
				
				useAngleCulling.boolValue = EditorGUILayout.Toggle("Use angle culling:", useAngleCulling.boolValue);
				
				if(useAngleCulling.boolValue) {
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Renderer culling angle:");
					rendererCullingAngle.floatValue = EditorGUILayout.FloatField(rendererCullingAngle.floatValue);
					EditorGUILayout.EndHorizontal();
				}

				createBorders.boolValue = EditorGUILayout.Toggle("Create surface skirts", createBorders.boolValue);
				logGenerationTimes.boolValue = EditorGUILayout.Toggle("Log generation times", logGenerationTimes.boolValue);
				showDebug.boolValue = EditorGUILayout.Toggle("Show LOD debug", showDebug.boolValue);
			}
			break;
		case Tab.PAINTING:
			if(planet.uv1type != Planet.UV.CONTINUOUS)
				EditorGUILayout.HelpBox("Terrain painting requires UV1 type to be Continuous in the SHADER tab during painting, switch to it and regenerate.", MessageType.Warning);

			EditorGUILayout.LabelField("Brush settings", EditorStyles.boldLabel);

			planet.useBrushTexture = EditorGUILayout.Toggle("Use brush texture:", planet.useBrushTexture);
			if(planet.useBrushTexture) {
				planet.brushTexture = (Texture2D)EditorGUILayout.ObjectField(planet.brushTexture, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
				planet.useBrushAlpha = EditorGUILayout.Toggle("Use alpha value:", planet.useBrushAlpha);
				EditorGUILayout.HelpBox("Brush texture must be Read/Write enabled in the texture import settings", MessageType.Info);
			}

			EditorGUILayout.LabelField("Brush mode:");
			planet.brushMode = (Planet.BrushMode)GUILayout.Toolbar((int)planet.brushMode, brushTexts, EditorStyles.toolbarButton);
			planet.brushSize = EditorGUILayout.Slider("Brush size", planet.brushSize, 0.001f, .5f);
			planet.brushStrength = EditorGUILayout.Slider("Brush strength", planet.brushStrength, 0f, 1f);

			switch(planet.brushMode) {
			case Planet.BrushMode.SET:
				planet.brushSetValue = EditorGUILayout.Slider("Set to", planet.brushSetValue, planet.lowLimit, planet.highLimit);
				break;
			}

			planet.brushFalloff = EditorGUILayout.Toggle("Use fall off:", planet.brushFalloff);
			if(planet.brushFalloff) {
				planet.falloff = EditorGUILayout.CurveField("Brush falloff", planet.falloff);
			}
			
			planet.lowLimit = EditorGUILayout.FloatField("Low limit", planet.lowLimit);
			planet.highLimit = EditorGUILayout.FloatField("High limit", planet.highLimit);

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);

			planet.modifierResolution = EditorGUILayout.IntField("Painting resolution", planet.modifierResolution);
			planet.useBicubicInterpolation = EditorGUILayout.Toggle("Use bicubic interpolation:", planet.useBicubicInterpolation);

			if(GUILayout.Button("Clear all")) {
				planet.ClearModifiers();
				Generate(planet);
			}
			break;
		case Tab.SHADER:
			EditorGUILayout.LabelField("Terrain material:");
			planet.terrainMaterial = (Material)EditorGUILayout.ObjectField(planet.terrainMaterial, typeof(Material), false);

			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("UVs", EditorStyles.boldLabel);
			planet.uv1type = (Planet.UV)EditorGUILayout.EnumPopup("UV1 type:", planet.uv1type);
			planet.uv2 = EditorGUILayout.Toggle("Generate UV2:", planet.uv2);
			if(planet.uv2)
				planet.uv2type = (Planet.UV)EditorGUILayout.EnumPopup("UV2 type:", planet.uv2type);

			EditorGUILayout.HelpBox("Default shaders use Continuous UV1 for splat texturing. Global maps use Spherical UV2 and Surface textures use Surface UV2.", MessageType.Info);
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Texture generation", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("Texture nodes from the module file will appear here.", MessageType.Info);

			if(planet.Terrain != null) {
				if(planet.textureNodeInstances == null)
					planet.textureNodeInstances = new Planet.TextureNodeInstance[0];

				if(planet.textureNodeInstances.Length != planet.Terrain.textureNodes.Count) {
					Planet.TextureNodeInstance[] newInstances = new Planet.TextureNodeInstance[planet.Terrain.textureNodes.Count];
					for(int i = 0; i < planet.Terrain.textureNodes.Count; i++) {
						bool createNewInstance = true;
						if(i < planet.textureNodeInstances.Length) {
							if(planet.textureNodeInstances[i] != null) {
								newInstances[i] = planet.textureNodeInstances[i];
								createNewInstance = false;
							}
						}
						if(createNewInstance) {
							newInstances[i] = new Planet.TextureNodeInstance(planet.Terrain.textureNodes[i]);
						}
					}
					planet.textureNodeInstances = newInstances;
				}
				
				for(int i = 0; i < planet.textureNodeInstances.Length; i++) {
					Planet.TextureNodeInstance tni = planet.textureNodeInstances[i];
					EditorGUILayout.LabelField(tni.node.textureId, EditorStyles.boldLabel);

					EditorGUILayout.LabelField("Material property name:");
					tni.materialPropertyName = EditorGUILayout.TextField(tni.materialPropertyName);
					
					tni.generateSurfaceTextures = EditorGUILayout.Toggle("Generate surface textures:", tni.generateSurfaceTextures);
					if(tni.generateSurfaceTextures) {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.PrefixLabel("Texture resolution:");
						planet.textureResolution = EditorGUILayout.IntField(planet.textureResolution);
						EditorGUILayout.EndHorizontal();
					}
					
					tni.generateGlobalMap = EditorGUILayout.Toggle("Generate global map:", tni.generateGlobalMap);
					if(tni.generateGlobalMap) {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.PrefixLabel("Global map width:");
						tni.globalMapSize = EditorGUILayout.IntField(tni.globalMapSize);
						EditorGUILayout.EndHorizontal();
					}
					if(GUILayout.Button("Save GlobalMap")) {
						for(int ii = 0; ii < planet.textureNodeInstances.Length; ii++)
							planet.textureNodeInstances[ii].node = planet.Terrain.textureNodes[ii];

						string savepath = EditorUtility.SaveFilePanel("Export texture", Application.dataPath, tni.materialPropertyName, "");
						if(savepath.Length != 0) {
							
							float progress = 1;
							float total = 2;
							
							EditorUtility.DisplayProgressBar("Planet generator", "Generating texture...", progress / total);
							Texture2D texture = GlobalMapUtility.Generate(tni.globalMapSize, tni.node.GetModule());
							progress++;
							EditorUtility.DisplayProgressBar("Planet generator", "Saving texture...", progress / total);
							File.WriteAllBytes(savepath + ".png", texture.EncodeToPNG());
							
							EditorUtility.ClearProgressBar();
							AssetDatabase.Refresh();
						}
					}

					EditorGUILayout.Space();
				}
	
				if(planet.Terrain.textureNodes.Count == 0) {
					EditorGUILayout.LabelField("Module has no texture outputs defined.", EditorStyles.label);
				}
			}

			EditorGUILayout.HelpBox("Surface textures are generated for each surface. Global maps are generated once during start.", MessageType.Info);

			break;
		}
		
		EditorGUILayout.Space();

		serializedObject.ApplyModifiedProperties();
	}

	public void OnSceneGUI() {
		if(currentTab == Tab.PAINTING) {
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

			if(Event.current.type == EventType.MouseMove) {
				Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				RaycastHit hit;
				
				if(Physics.Raycast(ray, out hit)) {
					lastPoint = hit.point;
					lastNormal = hit.normal;

					SceneView.RepaintAll();
				}
			}
			
			if(Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag){
				if(Event.current.button == 0) {
					Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
					RaycastHit hit;
					
					if(Physics.Raycast(ray, out hit)) {
						lastPoint = hit.point;
						lastNormal = hit.normal;

						Surface s = hit.collider.gameObject.GetComponent<Surface>();
						if(s != null) {
							planet.Paint(s, hit.textureCoord);
						}
					}
					
					Event.current.Use();
				}
			}

			switch(planet.brushMode) {
			case Planet.BrushMode.ADD:
				Handles.color = Color.green;
				break;
			case Planet.BrushMode.SUBSTRACT:
				Handles.color = Color.red;
				break;
			case Planet.BrushMode.SET:
				Handles.color = Color.yellow;
				break;
			}

			Handles.DrawWireDisc(lastPoint, lastNormal, ((planet.brushSize * (planet.meshResolution * planet.radius)) / planet.meshResolution) * .75f);
		}
	}
	
	private void Generate(Planet planet) {
		
		float progress = 1;
		float total = planet.subdivisions * planet.subdivisions * 6;
		planet.Generate(() => { 
			EditorUtility.DisplayProgressBar("Planet generator", "Generating...", progress / total);
			progress++;
		});
		
		EditorUtility.ClearProgressBar();
	}
}

}
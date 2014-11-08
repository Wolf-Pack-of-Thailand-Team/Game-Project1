using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class NodeWindow
{
	public Node node;
	
	public Color connectionColor;
	
	private int windowIndex;
	private static int indexCount = 0;
		
	private GeneratorNode generatorNode;
	private OperatorNode operatorNode;
	private OutputNode outputNode;
	private MacroNode macroNode;
	
	private Texture2D preview;
	private enum PreviewSize { x1=64, x2=128, x4=256, x8=512 }
	private PreviewSize previewSize = PreviewSize.x1;
	private bool viewControls = false;
	private float tx, ty, zoom = 1f;
	private bool useLivePreview = false;
	public Planet livePreview = null;

	private float minValue, maxValue;

	private int controlHeight = 30;
	
	private bool autoGeneratePreview = true;
	private bool showParameters = false;
	
	private bool showExport = false;
	private int exportSize = 1024;
	
	private enum WINDOWTYPE {
		GENERATOR, OPERATOR, OUTPUT, MACRO
	}
	private WINDOWTYPE type;
	
	public NodeWindow(Node node) {
		this.windowIndex = indexCount;
		indexCount++;
		
		// cast to specific type
		if(node is GeneratorNode) {
			generatorNode = (GeneratorNode)node;
			type = WINDOWTYPE.GENERATOR;
		}
		if(node is OperatorNode){
			operatorNode = (OperatorNode)node;
			type = WINDOWTYPE.OPERATOR;
		}
		if(node is OutputNode) {
			outputNode = (OutputNode)node;
			type = WINDOWTYPE.OUTPUT;
			previewSize = PreviewSize.x4;
			//autoGeneratePreview = false;
		}
		if(node is MacroNode) {
			macroNode = (MacroNode)node;
			type = WINDOWTYPE.MACRO;
		}
		
		// also save general reference
		this.node = node;
		
		// random connection color
		connectionColor = new Color(Random.Range(100, 200) / 255f, Random.Range(100, 200) / 255f, Random.Range(100, 200) / 255f);
		
		preview = Generate((int)previewSize);
		node.rect.height = controlHeight + (int)previewSize;
		node.rect.width = (int)previewSize + 20;
		if(node.rect.width < 200)
			node.rect.width = 200;
	}
	
	public Rect Show() {
		node.rect = SerializableRect.FromRect(GUI.Window(windowIndex, node.rect.ToRect(), DoGUI, type.ToString()));
		return node.rect.ToRect();
	}

	public void UpdatePreview() {
		preview = Generate((int)previewSize);
	}
	
	public void DoGUI(int id) {
		bool generate = false;

		node.rect.height =  (int)previewSize;
		node.rect.width = (int)previewSize + 70;
		if(node.rect.width < 160)
			node.rect.width = 160;

		GUILayout.BeginArea(new Rect(5f, 20f, node.rect.width - 10, 1000f));

		// preview image
		GUILayout.BeginHorizontal();
		GUILayout.Box(preview);
		GUILayout.BeginVertical();

		if(GUILayout.Button("Update", EditorStyles.miniButton, GUILayout.Width(50))) {
			generate = true;
		}

		EditorGUILayout.LabelField("Preview:", EditorStyles.miniLabel, GUILayout.Width(50));
		previewSize = (PreviewSize)EditorGUILayout.EnumPopup(previewSize, GUILayout.Width(50));

		viewControls = EditorGUILayout.Foldout(viewControls, "");
		if(viewControls) {
			EditorGUILayout.LabelField("Zoom:", EditorStyles.miniLabel, GUILayout.Width(50));
			zoom = GUILayout.HorizontalSlider(zoom, 1f, .1f, GUILayout.Width(50));
			EditorGUILayout.LabelField("Translate:", EditorStyles.miniLabel);
			tx = GUILayout.HorizontalSlider(tx, 0f, 1f, GUILayout.Width(50));
			ty = GUILayout.HorizontalSlider(ty, 0f, 1f, GUILayout.Width(50));

			node.rect.height += controlHeight * 3f;
		}

		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		GUILayout.Label("Value range: " + minValue.ToString("0.0") + " to " + maxValue.ToString("0.0"), EditorStyles.miniLabel);

		switch(type) {
		case WINDOWTYPE.GENERATOR:
			GeneratorGUI();
			break;
		case WINDOWTYPE.OPERATOR:
			OperatorGUI();
			break;
		case WINDOWTYPE.OUTPUT:
			OutputGUI();
			break;
		case WINDOWTYPE.MACRO:
			MacroGUI();
			break;
		}

		//autoGeneratePreview = GUILayout.Toggle(autoGeneratePreview, "Auto-generate:");

		node.rect.height += controlHeight * 1.5f;
		
		// generation
		if(generate || (GUI.changed && autoGeneratePreview)) {
			preview = Generate((int)previewSize);
		}

		GUILayout.EndArea();
        GUI.DragWindow();
    }
	
	private void GeneratorGUI() {
		// generator type selection
		generatorNode.type = (GeneratorNode.GENERATORTYPE)EditorGUILayout.EnumPopup(generatorNode.type);
		node.rect.height += controlHeight;
		if(generatorNode.type != generatorNode.lastType) {
			preview = Generate((int)previewSize);
			generatorNode.lastType = generatorNode.type;
		}
		
		// parameters
		showParameters = EditorGUILayout.Foldout(showParameters, "Parameters");
		node.rect.height += controlHeight;
		
		if(showParameters) {
			if(generatorNode.type == GeneratorNode.GENERATORTYPE.BILLOW || generatorNode.type == GeneratorNode.GENERATORTYPE.FBM || 
			   generatorNode.type == GeneratorNode.GENERATORTYPE.RIDGED) {
				GUILayout.Label("Frequency:");
				generatorNode.frequency = EditorGUILayout.FloatField(generatorNode.frequency);
				node.rect.height += controlHeight;

				GUILayout.Label("Lacunarity:");
				generatorNode.lacunarity = EditorGUILayout.Slider(generatorNode.lacunarity, 0f, 2f);
				node.rect.height += controlHeight;

				GUILayout.Label("Gain:");
				generatorNode.gain = EditorGUILayout.Slider(generatorNode.gain, 0f, 2f);
				node.rect.height += controlHeight;

				EditorGUILayout.LabelField("Octaves:");
				generatorNode.octaves = EditorGUILayout.IntField(generatorNode.octaves);
				node.rect.height += controlHeight * 2f;

				EditorGUILayout.LabelField("Seed:");
				generatorNode.seed = EditorGUILayout.FloatField(generatorNode.seed);
				node.rect.height += controlHeight * 2f;
			}
			// constvalue
			if(generatorNode.type == GeneratorNode.GENERATORTYPE.CONST) {
				GUILayout.Label("Value:");
				generatorNode.constValue = EditorGUILayout.Slider(generatorNode.constValue, -1f, 1f);
				node.rect.height += controlHeight;
			}
		}
	}
	
	private void OperatorGUI() {
		// generator type selection
		operatorNode.type = (OperatorNode.OPERATORTYPE)EditorGUILayout.EnumPopup(operatorNode.type);
		if(operatorNode.type != operatorNode.lastType) {
			operatorNode.SetInputs();
			preview = Generate((int)previewSize);
			operatorNode.lastType = operatorNode.type;
		}
		node.rect.height += controlHeight * 1.5f;
		
		// parameters
		//showParameters = EditorGUILayout.Foldout(showParameters, "Parameters");
		//if(showParameters) {
			// min max
			if(operatorNode.type == OperatorNode.OPERATORTYPE.CLAMP) {
				GUILayout.Label("Min:");
				operatorNode.min = EditorGUILayout.Slider(operatorNode.min, -1.0f, 1.0f);
				GUILayout.Label("Max:");
				operatorNode.max = EditorGUILayout.Slider(operatorNode.max, -1.0f, 1.0f);
				node.rect.height += controlHeight * 2.5f;
			}
			
			if(operatorNode.type == OperatorNode.OPERATORTYPE.TERRACE) {
				GUILayout.Label("Min:");
				operatorNode.min = EditorGUILayout.Slider(operatorNode.min, -1.0f, 1.0f);
				GUILayout.Label("Max:");
				operatorNode.max = EditorGUILayout.Slider(operatorNode.max, -1.0f, 1.0f);
				GUILayout.Label("Power:");
				operatorNode.power = EditorGUILayout.Slider(operatorNode.power, 0f, 1.0f);
				node.rect.height += controlHeight * 4f;
			}
			
			// exponent
			if(operatorNode.type == OperatorNode.OPERATORTYPE.EXPONENT) {
				GUILayout.Label("Exponent:");
				operatorNode.exponent = EditorGUILayout.Slider(operatorNode.exponent, -10.0f, 10f);
				node.rect.height += controlHeight;
			}
			// x, y, z
			if(operatorNode.type == OperatorNode.OPERATORTYPE.TRANSLATE) {
				GUILayout.Label("X:");
				operatorNode.x = EditorGUILayout.Slider(operatorNode.x, -10.0f, 10.0f);
				GUILayout.Label("Y:");
				operatorNode.y = EditorGUILayout.Slider(operatorNode.y, -10.0f, 10.0f);
				GUILayout.Label("Z:");
				operatorNode.z = EditorGUILayout.Slider(operatorNode.z, -10.0f, 10.0f);
				node.rect.height += controlHeight * 4f;
			}
			// curve
			if(operatorNode.type == OperatorNode.OPERATORTYPE.CURVE) {
				if(operatorNode.curve == null) {
					if(operatorNode.keyframes != null)
						operatorNode.curve = new AnimationCurve(SerializableKeyframe.ToKeyframeArray(operatorNode.keyframes));
					else
						operatorNode.curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
				}
				
				try {
				operatorNode.curve = EditorGUILayout.CurveField(operatorNode.curve, GUILayout.Width(40), GUILayout.Height(30));
				}
				catch {}
				operatorNode.keyframes = SerializableKeyframe.FromKeyframeArray(operatorNode.curve.keys);
				node.rect.height += controlHeight;
			}
			// exponent
			if(operatorNode.type == OperatorNode.OPERATORTYPE.WEIGHT) {
				GUILayout.Label("Weight:");
				operatorNode.min = EditorGUILayout.Slider(operatorNode.min, 0f, 1f);
				GUILayout.Label("Target:");
				operatorNode.max = EditorGUILayout.Slider(operatorNode.max, -1f, 1f);
				node.rect.height += controlHeight * 2.5f;
			}
		//}
	}

	private void OutputGUI() {
		useLivePreview = EditorGUILayout.Foldout(useLivePreview, "Live preview");
		if(useLivePreview) {
			livePreview = (Planet)EditorGUILayout.ObjectField("Live preview:", livePreview, typeof(Planet), true);
			if(livePreview != null) {
				if(GUILayout.Button("Update Mesh")) {
					TerrainModule tm = new TerrainModule();
					tm.module = outputNode.GetModule();
					livePreview.Terrain = tm;
					livePreview.Generate(null, false);
				}
			}

			EditorGUILayout.HelpBox("Drag a planet from scene to preview mesh while editing the module", MessageType.Info);
			node.rect.height += controlHeight * 2.5f;
		}
		node.rect.height += controlHeight * 1.5f;
		
		// export options
		showExport = EditorGUILayout.Foldout(showExport, "Export textures");
		if(showExport) {
			// size
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Texture size:");
			exportSize = EditorGUILayout.IntField(exportSize);
			EditorGUILayout.EndHorizontal();
			
			if(GUILayout.Button("Save as PNG")) {
				string savepath = EditorUtility.SaveFilePanel("Export texture", Application.dataPath, "planet", "");
				if(savepath.Length != 0) {
					
					float progress = 0;
					float total = 2;
					
					Texture2D temp;
					EditorUtility.DisplayProgressBar("Planet generator", "Generating texture...", progress / total);
					temp = Generate(exportSize);
					progress++;
					EditorUtility.DisplayProgressBar("Planet generator", "Saving texture...", progress / total);
					File.WriteAllBytes(savepath + "_heightmap.png", temp.EncodeToPNG());
					
					EditorUtility.ClearProgressBar();
					AssetDatabase.Refresh();
				}
			}
			
			node.rect.height += controlHeight * 2f;
		}
	}
	
	private void MacroGUI() {
		// module file selection
		GUILayout.Label("Module file:");
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.TextField(macroNode.path);
		if(GUILayout.Button("...", GUILayout.Width(30))) {
			string absolutePath = EditorUtility.OpenFilePanel("Open", "/", "txt");
			string relativePath = absolutePath.Substring(absolutePath.IndexOf("Assets/"));
			macroNode.path = relativePath;
			preview = Generate((int)previewSize);
		}
		EditorGUILayout.EndHorizontal();
		node.rect.height += controlHeight * 2;

		GUILayout.Label("Frequency scale:");
		macroNode.frequencyScale = EditorGUILayout.FloatField(macroNode.frequencyScale);
		node.rect.height += controlHeight * 1.5f;
	}
	
	/// <summary>
	/// Generates a preview texture 
	/// </summary>
	private Texture2D Generate(int size) {
		Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
		Color[] colors = new Color[size * size];
		
		// get module from the node
		ModuleBase module = null;
		switch(type) {
		case WINDOWTYPE.GENERATOR:
			module = generatorNode.GetModule();
			node.title = generatorNode.type.ToString();
			break;
		case WINDOWTYPE.OPERATOR:
			module = operatorNode.GetModule();
			node.title = operatorNode.type.ToString();
			break;
		case WINDOWTYPE.OUTPUT:
			module = outputNode.GetModule();
			break;
		case WINDOWTYPE.MACRO:
			module = macroNode.GetModule();
			break;
		}
		// check that node is not null
		if(module != null) {
			minValue = Mathf.Infinity;
			maxValue = Mathf.NegativeInfinity;

			for(int y = 0; y < size; y++) {
				for(int x = 0; x < size; x++) {
					float value = module.GetValue(Surface.SperifyPoint(new Vector3(-1f + (x / (float)size) * zoom + tx, (y / (float)size) * zoom - 1f + ty, 1f)));
					if(value < minValue)
						minValue = value;
					if(value > maxValue)
						maxValue = value;
					value = (value + 1f) / 2f;
					colors[y * size + x] = new Color(value, value, value);
				}
			}
			texture.SetPixels(colors);
			texture.Apply();
		}

		return texture;
	}
}


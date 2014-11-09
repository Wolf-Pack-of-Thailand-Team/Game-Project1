using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Planetary {

public class ModuleEditorWindow: EditorWindow {
	
	public static EditorWindow window = null;
	
	private enum VIEW {
		MODULE, MESH
	}
	private VIEW view = VIEW.MODULE;
	
	private TerrainModule settings;
	private List<NodeWindow> windows = new List<NodeWindow>();
	private List<NodeWindow> toBeRemoved = new List<NodeWindow>();
	
	private bool isConnecting = false;
	private bool isInput = false;
	private Node selectedNode;
	private int selectedPort = 0;
	
	private string savepath = "";
	private string loadpath = "";
	
	private Vector2 scrollPos = Vector2.zero;
	private Rect scrollRect = new Rect(0, 0, 4096, 4096);
	
    [MenuItem("Window/PlanetaryTerrain Module Editor")]
    static public void Init() {
       	window = EditorWindow.GetWindow<ModuleEditorWindow>();
    }
	
	void Awake() {
		New();
	}
	
	private void New() {
		// create starting nodes
		settings = new TerrainModule();
		savepath = "";
		
		GeneratorNode g1 = new GeneratorNode(100, 100);
		g1.seed = Random.Range(-100000, 100000);
		OutputNode output = new OutputNode(700, 100);
		
		output.Connect(g1, 0);
		
		settings.nodes.Add(g1);
		settings.nodes.Add(output);
		
		// create windows to represent nodes
		windows.Clear();
		
		windows.Add(new NodeWindow(g1));
		windows.Add(new NodeWindow(output));
	}
	
	public void Load(string loadpath) {
		if(loadpath == "")
			return;
		savepath = loadpath;
		
		if(loadpath.Length != 0) {
			windows.Clear();
			settings = TerrainModule.Load(loadpath, true, 0, 1f);
			if(settings != null) {
				foreach(Node n in settings.nodes) {
					windows.Add(new NodeWindow(n));
				}
			}
		}
	}
	
	public void Load(TextAsset ta) {
		Load(ta, null);
	}
	public void Load(TextAsset ta, Planet livePreview) {
		if(ta == null)
			return;
		savepath = AssetDatabase.GetAssetPath(ta);
		
		windows.Clear();
		settings = TerrainModule.LoadTextAsset(ta, true, 0, 1f);
		if(settings != null) {
			foreach(Node n in settings.nodes) {
				NodeWindow nw = new NodeWindow(n);
				
				if(n is OutputNode)
					nw.livePreview = livePreview;
					
				windows.Add(nw);
			}
		}
	}
	
	public void Save(string suggestedName) {
		savepath = EditorUtility.SaveFilePanel("Save module", "/", suggestedName, "txt");
		if(savepath.Length != 0) {
			settings.Save(savepath);
			AssetDatabase.Refresh();
		}	
	}
	
    public void OnGUI()
    {
		GUI.Box(new Rect(0, 0, 260, 40), "");
		GUI.Label(new Rect(10, 10, 60, 20), "File:");
		GUI.Label(new Rect(270, 10, 800, 20), savepath);
			
		if(GUI.Button(new Rect(60, 10, 60, 20), "New")) {
			New();
		}
		if(GUI.Button(new Rect(125, 10, 60, 20), "Save")) {
			if(savepath != "") {
				int last = savepath.LastIndexOf("/") + 1;
				Save(savepath.Substring(last));
			}
			else
				Save("New Module");
		}
		if(GUI.Button(new Rect(190, 10, 60, 20), "Load")) {
			loadpath = EditorUtility.OpenFilePanel("Load module", "/", "txt");
			Load(loadpath);
		}
		
		switch(view) {
		case VIEW.MODULE:
			ShowModuleViewGUI();
			break;
		case VIEW.MESH:
			
			break;
		}
	}
	
	private void ShowModuleViewGUI() {
		GUI.Label(new Rect(10, 50, 160, 20), "Create Nodes:");

		if(GUI.Button(new Rect(10, 70, 100, 20), "Generator")) {
			GeneratorNode g = new GeneratorNode((int)scrollPos.x + 200, (int)scrollPos.y + 200);
			g.seed = Random.Range(-10000, 10000);
			settings.nodes.Add(g);
			windows.Add(new NodeWindow(g));
		}
		if(GUI.Button(new Rect(120, 70, 100, 20), "Operator")) {
			OperatorNode o = new OperatorNode((int)scrollPos.x + 200, (int)scrollPos.y + 200);
			settings.nodes.Add(o);
			windows.Add(new NodeWindow(o));
		}
		if(GUI.Button(new Rect(230, 70, 100, 20), "Macro")) {
			MacroNode m = new MacroNode((int)scrollPos.x + 200, (int)scrollPos.y + 200);
			settings.nodes.Add(m);
			windows.Add(new NodeWindow(m));
		}
		if(GUI.Button(new Rect(380, 70, 100, 20), "Color Node")) {
			ColorNode c = new ColorNode((int)scrollPos.x + 200, (int)scrollPos.y + 200);
			settings.nodes.Add(c);
			windows.Add(new NodeWindow(c));
		}
		if(GUI.Button(new Rect(490, 70, 100, 20), "Texture Output")) {
			TextureNode t = new TextureNode((int)scrollPos.x + 200, (int)scrollPos.y + 200);
			settings.nodes.Add(t);
			windows.Add(new NodeWindow(t));
		}
		
		scrollPos = GUI.BeginScrollView(new Rect(10, 95, this.position.width -10, this.position.height - 105), scrollPos, scrollRect);
		
        BeginWindows();
		
		foreach(NodeWindow n in windows) {
			GUI.color = Color.white;

			// show the window
			Rect rect = n.Show();
			
			// delete button
			if(!(n.node is OutputNode)) {
				if(GUI.Button(new Rect(rect.x + rect.width - 20,rect.y - 16, 20, 16), "x")) {
					toBeRemoved.Add(n);
				}
			}
			
			// input port buttons
			if(n.node.Inputs != null) {
				for(int i = 0; i < n.node.Inputs.Length; i++) {
					Rect input = new Rect(rect.x - 14, 
						                  (rect.y + rect.height / 2 - 7) + i * 20f - (n.node.Inputs.Length-1) * 20f / 2,
						                  14, 
						                  14);
					
					if(!isConnecting) {
						if(GUI.Button(input, "")) {
							isConnecting = true;
							isInput = true;
							selectedNode = n.node;
							selectedPort = i;
						}
					}	
					else {
						if(!isInput) {
							if(GUI.Button(input, "")) {
								n.node.Connect(selectedNode, i);
								n.UpdatePreview();
								isConnecting = false;
							}
						}
					}
					// draw connections
					if(n.node.Inputs[i] != null)
						DrawBezier(n.node.Inputs[i].Rect.ToRect(), input, n.connectionColor);
				}
			}
			
			// output button
			if(n.node.HasOutput) {
				Rect output = new Rect(rect.x + rect.width - 2, 
						               rect.y + rect.height / 2 - 7,
						               14, 
						               14);

				if(n.node.module != null) {
					if(n.node.module.colorOutput)
						GUI.color = Color.green;
					else
						GUI.color = Color.white;
				}

				if(!isConnecting) {
					if(GUI.Button(output, "")) {
						isConnecting = true;
						isInput = false;
						selectedNode = n.node;
					}
				}
				else {
					if(isInput) {
						if(GUI.Button(output, "")) {
							selectedNode.Connect(n.node, selectedPort);
							UpdateNodeWindow(selectedNode);
							isConnecting = false;
						}
					}
				}
			}
		}
		
        EndWindows();
		
		GUI.EndScrollView();
	}
	
	public void Update() {
		if(isConnecting && Input.GetMouseButtonDown(1)) {
			isConnecting = false;
		}
		
		if(toBeRemoved.Count > 0) {
			foreach(NodeWindow nw in toBeRemoved) {
				settings.nodes.Remove(nw.node);
				windows.Remove(nw);
			}
			toBeRemoved.Clear();
		}
	}

	private void UpdateNodeWindow(Node n) {
		for(int i = 0; i < windows.Count; i++) {
			if(windows[i].node == n) {
				windows[i].UpdatePreview();
				break;
			}
		}
	}
	
	private void DrawBezier(Rect wr, Rect wr2, Color color) {
		Handles.DrawBezier(new Vector3(wr.x + wr.width + 8, wr.y + wr.height / 2, 0f),
							new Vector3(wr2.x, wr2.y + wr2.height / 2, 0f),
							new Vector2(wr.x + wr.width + Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr.y + wr.height / 2),
							new Vector2(wr2.x - Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr2.y + wr2.height / 2),
							color, null, 2f);
    }
}

}
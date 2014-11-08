using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

[System.Serializable()]
public class MacroNode : Node {
	
	public string path;
	
	public float frequencyScale = 1f;
	
	[System.NonSerialized] private ModuleBase module;
	
	public MacroNode(int x, int y) : base("Macro", new SerializableRect(x, y, 200, 140)) {
		
	}
	
	override public ModuleBase GetModule() {
		if(path == null)
			return null;
		if(path == "")
			return null;
		
		TerrainModule tm = TerrainModule.Load(path, false, 0, frequencyScale);
		if(tm != null)
			this.module = tm.module;
		else {
			Debug.LogError("MacroNode: Terrain module could not be loaded from " + path);
			this.module = new ModuleBase();
		}
		
		return this.module;
	}
}

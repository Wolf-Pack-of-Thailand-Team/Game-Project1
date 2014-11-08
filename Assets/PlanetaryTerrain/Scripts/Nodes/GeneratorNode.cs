using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

[System.Serializable()]
public class GeneratorNode : Node {
	
	[System.NonSerialized] private ModuleBase module;
	
	public enum GENERATORTYPE {
		FBM, RIDGED, BILLOW, CONST
	}
	public GENERATORTYPE type, lastType;
	
	public float frequency = 1.0f;
	public float lacunarity = 2.0f;
	public float persistence = 0.5f;
	public float gain = 0.5f;
	public float constValue = 0.5f;
	public int octaves = 4;
	public bool distance = true;
	
	// for outside seeding
	public bool hasSeed = false;
	public float seed = 0;
	
	public GeneratorNode(int x, int y) : base("Generator", new SerializableRect(x, y, 200, 140)) {
		type = lastType = GeneratorNode.GENERATORTYPE.FBM;
	}
	
	override public ModuleBase GetModule() {
		hasSeed = false;
		switch(type) {
			case GENERATORTYPE.CONST:
				module = new Const(constValue);
				break;
			case GENERATORTYPE.FBM:
				module = new Fbm(frequency, lacunarity, gain, octaves, seed, false, false);
				hasSeed = true;
				break;
			case GENERATORTYPE.RIDGED:
				module = new Fbm(frequency, lacunarity, gain, octaves, seed, true, false);
				hasSeed = true;
				break;
			case GENERATORTYPE.BILLOW:
				module = new Fbm(frequency, lacunarity, gain, octaves, seed, false, true);
				hasSeed = true;
				break;
		}
		
		return this.module;
	}
}

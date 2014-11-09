using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

namespace Planetary {

[System.Serializable()]
public class GeneratorNode : Node {
	
	public enum GENERATORTYPE {
		FBM, RIDGED, BILLOW, CONST, IQ, SWISS, JORDAN, CELL, TEXTURE
	}
	public GENERATORTYPE type, lastType;
	
	public float frequency = 1.0f;
	public float lacunarity = 2.0f;
	public float persistence = 0.5f;
	public float gain = 0.5f;
	public float constValue = 0.5f;
	public int octaves = 4;
	public bool distance = true;
	public bool derivative = false;

	// advanced
	public float ridgedWarp = 0.05f;
	public float warp = 0.35f;
	public float warp0 = 0.4f;
	public float gain1 = 0.8f;
	public float damp0 = 1.0f;
	public float damp = 0.8f;
	public float damp_scale = 1.0f;

	// cell noise
	public VoronoiNoise.DistanceFunction distanceFunction;
	public VoronoiNoise.CombineFunction combineFunction;

	// for outside seeding
	public bool hasSeed = false;
	public float seed = 0;

	// texture
	public string texturePath = "";
	[System.NonSerialized] public Texture2D texture;
	
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
				module = new Fbm(frequency, lacunarity, gain, octaves, seed, false, false, derivative);
				hasSeed = true;
				break;
			case GENERATORTYPE.RIDGED:
				module = new Fbm(frequency, lacunarity, gain, octaves, seed, true, false, derivative);
				hasSeed = true;
				break;
			case GENERATORTYPE.BILLOW:
				module = new Fbm(frequency, lacunarity, gain, octaves, seed, false, true, derivative);
				hasSeed = true;
				break;
			case GENERATORTYPE.IQ:
				module = new AdvancedNoise(AdvancedNoise.Type.IQ, frequency, lacunarity, gain, octaves, seed);
				hasSeed = true;
				break;
			case GENERATORTYPE.SWISS:
				module = new AdvancedNoise(AdvancedNoise.Type.SWISS, frequency, lacunarity, gain, octaves, seed);
				((AdvancedNoise)module).ridgedWarp = ridgedWarp;
				hasSeed = true;
				break;
			case GENERATORTYPE.JORDAN:
				module = new AdvancedNoise(AdvancedNoise.Type.JORDAN, frequency, lacunarity, gain, octaves, seed);
				AdvancedNoise tn = (AdvancedNoise)module;
				tn.warp = warp;
				tn.warp0 = warp0;
				tn.gain1 = gain1;
				tn.damp0 = damp0;
				tn.damp = damp;
				tn.damp_scale = damp_scale;
				hasSeed = true;
				break;
			case GENERATORTYPE.CELL:
				module = new CellNoise(frequency, gain, octaves, seed, distanceFunction, combineFunction);
				hasSeed = true;
				break;
			case GENERATORTYPE.TEXTURE:
				texture = null;
				if(texturePath != "" && texturePath != null) {
					texture = TextureSource.LoadTexture(texturePath);
				}
				module = new TextureSource(texture);
				break;
		}

		SetOutputOptions();		
		return this.module;
	}
}

}
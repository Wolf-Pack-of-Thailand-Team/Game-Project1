using UnityEngine;
using System.Collections;

namespace Planetary {

public class CellNoise : ModuleBase {
	
	public int octaves;
	public float frequency, amplitude, seed;

	public VoronoiNoise.DistanceFunction distanceFunction;
	public VoronoiNoise.CombineFunction combineFunction;
	
	public CellNoise(float frequency, float amplitude, int octaves, float seed, VoronoiNoise.DistanceFunction distanceFunction, VoronoiNoise.CombineFunction combineFunction) {
		this.frequency = frequency;
		this.amplitude = amplitude;
		this.octaves = octaves;
		this.seed = seed;
		this.distanceFunction = distanceFunction;
		this.combineFunction = combineFunction;
	}
	
	public override float GetValue(Vector3 position) {
		float gain = 1f;
		float value = 0f;
	
		for(int i = 0; i < octaves; i++) {
			value += VoronoiNoise.Noise3D(new Vector3(position.x * gain * frequency, 
													position.y * gain * frequency, 
													position.z * gain * frequency), seed,
													distanceFunction, combineFunction) * amplitude / gain;
			gain *= 2f;
		}
		return Output(value);
	}
}

}
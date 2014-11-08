using UnityEngine;
using System.Collections;

public class Fbm : ModuleBase {
	
	public int octaves;
	public float frequency, lacunarity, persistence, gain, seed;
	private bool ridged = false, billow = false;
	
	public Fbm(float frequency, float lacunarity, float gain, int octaves, float seed, bool ridged, bool billow) {
		this.frequency = frequency;
		this.lacunarity = lacunarity;
		this.octaves = octaves;
		this.gain = gain;
		this.seed = seed;
		this.ridged = ridged;
		this.billow = billow;
	}
	
	public override float GetValue(Vector3 position) {
		float freq = frequency;
		float amplitude = 1.0f;
		float total = 0f;
		
		for(int i = 0; i < octaves; i++) {
			float value = ((float)PerlinNoise.Noise((position.x + seed) * freq, 
													 (position.y + seed) * freq, 
													 (position.z + seed) * freq));
			total += value * amplitude;
			
			freq *= lacunarity;
			amplitude *= gain;
		}
		
		if(ridged)
			total = 1f - Mathf.Abs(total) * 2f;
		if(billow)
			total = Mathf.Abs(total) * 2f;
		
		return total;
	}
}

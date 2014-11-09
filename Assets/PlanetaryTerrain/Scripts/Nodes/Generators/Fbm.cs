using UnityEngine;
using System.Collections;

namespace Planetary {

public class Fbm : ModuleBase {
	
	public int octaves;
	public float frequency, lacunarity, persistence, gain, seed;
	private bool ridged = false, billow = false, derivative = false;
	
	public Fbm(float frequency, float lacunarity, float gain, int octaves, float seed, bool ridged, bool billow, bool derivative) {
		this.frequency = frequency;
		this.lacunarity = lacunarity;
		this.octaves = octaves;
		this.gain = gain;
		this.seed = seed;
		this.ridged = ridged;
		this.billow = billow;
		this.derivative = derivative;
	}
	
	public override float GetValue(Vector3 position) {
		float freq = frequency;
		float amplitude = 1.0f;
		float total = 0f;

		Vector3 dn = Vector3.zero;

		for(int i = 0; i < octaves; i++) {
			Vector4 value = PerlinNoise.dNoise((position.x + seed) * freq, 
												(position.y + seed) * freq, 
			                                   (position.z + seed) * freq);

			if(ridged || billow)
				total += Mathf.Abs(value.x) * amplitude;
			else
				total += value.x * amplitude;

			if(derivative) {
				dn.x += value.y * total;
				dn.y += value.z * total;
				dn.z += value.w * total;
			}
			
			freq *= lacunarity;
			amplitude *= gain;
		}

		if(ridged)
			total = -(total * 2f - 1f);
		else if(billow)
			total = total * 2f - 1f;

		if(derivative) {
			total = ((Mathf.Abs(dn.x) + Mathf.Abs(dn.y) + Mathf.Abs(dn.z)) / 3f);
		}

		return Output(total);
	}
}

}
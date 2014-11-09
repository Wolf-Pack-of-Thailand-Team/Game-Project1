using UnityEngine;
using System.Collections;

namespace Planetary {

public class AdvancedNoise : ModuleBase {
	
	public int octaves;
	public float frequency, lacunarity, persistence, gain, seed;

	public enum Type {
		SWISS, IQ, JORDAN
	}
	public Type type;

	// swiss
	public float ridgedWarp = 0.05f;

	// jordan
	public float warp = 0.35f;
	public float warp0 = 0.4f;
	public float gain1 = 0.8f;
	public float damp0 = 1.0f;
	public float damp = 0.8f;
	public float damp_scale = 1.0f;
	
	public AdvancedNoise(Type t, float frequency, float lacunarity, float gain, int octaves, float seed) {
		this.type = t;
		this.frequency = frequency;
		this.lacunarity = lacunarity;
		this.octaves = octaves;
		this.gain = gain;
		this.seed = seed;
	}
	
	public override float GetValue(Vector3 position) {
		switch(type) {
		case Type.SWISS:
			return Output(Swiss(position));
		case Type.IQ:
			return Output(IQ(position));
		case Type.JORDAN:
			return Output(Jordan(position));
		}
		return 0f;
	}

	float IQ(Vector3 p) {
		float sum = 0.5f;
		float freq = frequency, amp = 1.0f;
		Vector3 dsum = Vector3.zero;
		for(int i = 0; i < octaves; i++) {
			Vector4 value = PerlinNoise.dNoise((p.x + seed) * freq, 
			                                   (p.y + seed) * freq, 
			                                   (p.z + seed) * freq);
			dsum.x += value.y;
			dsum.y += value.z;
			dsum.z += value.w;
			sum += amp * value.x / (1f + Vector3.Dot(dsum, dsum));
			freq *= lacunarity;
			amp *= gain;
		}
		return sum;
	}

	float Swiss(Vector3 p) {
		float sum = 0f;
		float freq = frequency, amp = 1.0f;
		Vector3 dsum = Vector3.zero;
		for(int i = 0; i < octaves; i++) {
			Vector4 value = PerlinNoise.dNoise((p.x + seed + ridgedWarp * dsum.x) * freq, 
			                                   (p.y + seed + ridgedWarp * dsum.y) * freq, 
			                                   (p.z + seed + ridgedWarp * dsum.z) * freq);
			sum += amp * (1f - Mathf.Abs(value.x));
			dsum.x += amp * value.y * -value.x;
			dsum.y += amp * value.z * -value.x;
			dsum.z += amp * value.w * -value.x;
			freq *= lacunarity;
			amp *= gain * Mathf.Clamp01(sum);
		}
		return ((sum - .3f) / 1.6f) * 2f - 1f;
	}

	float Jordan(Vector3 p) {
		float amp = gain1;
		float freq = frequency;
		float damped_amp = amp * gain;

		Vector4 n = PerlinNoise.dNoise((p.x + seed) * freq, 
		                               (p.y + seed) * freq, 
		                               (p.z + seed) * freq);
		
		Vector4 n2 = n * n.x;
		Vector3 d2 = new Vector3(n2.y, n2.z, n2.w);
		float sum = n2.x;
		Vector3 dsum_warp = warp0 * d2;
		Vector3 dsum_damp = damp0 * d2;
		
		for(int i = 1; i < octaves; i++) {
			n = PerlinNoise.dNoise((p.x + seed) * freq + dsum_warp.x, 
			                       (p.y + seed) * freq + dsum_warp.y, 
			                       (p.z + seed) * freq + dsum_warp.z);
			n2 = n * n.x;
			d2.x = n2.y; 
			d2.y = n2.z; 
			d2.z = n2.w;
			
			sum += damped_amp * n2.x;
			dsum_warp += warp * d2;
			dsum_damp += damp * d2;
			freq *= lacunarity;
			amp *= gain;
			damped_amp = amp * (1f - damp_scale/(1f + Vector3.Dot(dsum_damp, dsum_damp)));
		}
		return sum;
	}
}

}
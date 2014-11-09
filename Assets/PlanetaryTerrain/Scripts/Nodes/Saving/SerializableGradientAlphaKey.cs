using UnityEngine;
using System.Collections;

namespace Planetary {

[System.Serializable()]
public class SerializableGradientAlphaKey
{
	public float time, alpha;
	
	public SerializableGradientAlphaKey() {
	}
	
	public SerializableGradientAlphaKey(float time, float alpha) {
		this.time = time;
		this.alpha = alpha;
	}
	
	public SerializableGradientAlphaKey(GradientAlphaKey colorKey) {
		FromKeyframe(colorKey);
	}
	
	public GradientAlphaKey ToGradientColorKey() {
		return new GradientAlphaKey(alpha, time);
	}
	
	public void FromKeyframe(GradientAlphaKey gck) {
		this.time = gck.time;
		this.alpha = gck.alpha;
	}
	
	public static GradientAlphaKey[] ToGradientColorKeys(SerializableGradientAlphaKey[] incoming) {
		GradientAlphaKey[] keys = new GradientAlphaKey[incoming.Length];
		for(int i = 0; i < keys.Length; i++)
			keys[i] = incoming[i].ToGradientColorKey();
		return keys;
	}
	
	public static SerializableGradientAlphaKey[] FromGradientColorKeys(GradientAlphaKey[] ks) {
		SerializableGradientAlphaKey[] keys = new SerializableGradientAlphaKey[ks.Length];
		for(int i = 0; i < keys.Length; i++)
			keys[i] = new SerializableGradientAlphaKey(ks[i]);
		return keys;
	}
}

}
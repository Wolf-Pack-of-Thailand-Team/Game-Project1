using UnityEngine;
using System.Collections;

namespace Planetary {

[System.Serializable()]
public class SerializableGradientColorKey
{
	public float time;
	public SerializableColor color;
	
	public SerializableGradientColorKey() {
	}
	
	public SerializableGradientColorKey(float time, Color color) {
		this.time = time;
		this.color = new SerializableColor(color);
	}
	
	public SerializableGradientColorKey(GradientColorKey colorKey) {
		FromKeyframe(colorKey);
	}
	
	public GradientColorKey ToGradientColorKey() {
		return new GradientColorKey(color.ToColor(), time);
	}
	
	public void FromKeyframe(GradientColorKey gck) {
		this.time = gck.time;
		this.color = new SerializableColor(gck.color);
	}
	
	public static GradientColorKey[] ToGradientColorKeys(SerializableGradientColorKey[] incoming) {
		GradientColorKey[] keys = new GradientColorKey[incoming.Length];
		for(int i = 0; i < keys.Length; i++)
			keys[i] = incoming[i].ToGradientColorKey();
		return keys;
	}
	
	public static SerializableGradientColorKey[] FromGradientColorKeys(GradientColorKey[] ks) {
		SerializableGradientColorKey[] keys = new SerializableGradientColorKey[ks.Length];
		for(int i = 0; i < keys.Length; i++)
			keys[i] = new SerializableGradientColorKey(ks[i]);
		return keys;
	}
}

}
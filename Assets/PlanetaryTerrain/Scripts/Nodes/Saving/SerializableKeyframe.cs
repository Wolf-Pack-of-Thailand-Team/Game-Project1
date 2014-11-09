using UnityEngine;
using System.Collections;

namespace Planetary {

[System.Serializable()]
public class SerializableKeyframe
{
	public float inTangent, outTangent, time, value;
	
	public SerializableKeyframe() {
	}
	
	public SerializableKeyframe(float inTangent, float outTangent, float time, float value) {
		this.inTangent = inTangent;
		this.outTangent = outTangent;
		this.time = time;
		this.value = value;
	}
	
	public SerializableKeyframe(Keyframe keyframe) {
		FromKeyframe(keyframe);
	}
	
	public Keyframe ToKeyframe() {
		return new Keyframe(time, value, inTangent, outTangent);
	}
	
	public void FromKeyframe(Keyframe k) {
		this.inTangent = k.inTangent;
		this.outTangent = k.outTangent;
		this.time = k.time;
		this.value = k.value;
	}
	
	public static Keyframe[] ToKeyframeArray(SerializableKeyframe[] sks) {
		Keyframe[] keys = new Keyframe[sks.Length];
		for(int i = 0; i < keys.Length; i++)
			keys[i] = sks[i].ToKeyframe();
		return keys;
	}
	
	public static SerializableKeyframe[] FromKeyframeArray(Keyframe[] ks) {
		SerializableKeyframe[] keys = new SerializableKeyframe[ks.Length];
		for(int i = 0; i < keys.Length; i++)
			keys[i] = new SerializableKeyframe(ks[i]);
		return keys;
	}
}

}
using UnityEngine;

namespace Planetary {

public class BezierCurve {

	public static float Evaluate(float time, SerializableKeyframe[] keyframes) {
		if(keyframes == null)
			return -1f;

		SerializableKeyframe key1 = keyframes[0];
		SerializableKeyframe key2 = keyframes[keyframes.Length-1];
		
		float closestDistance = Mathf.Infinity;
		int closestIndex = keyframes.Length;
		for(int i = 0; i < keyframes.Length; i++) {
			if(time >= keyframes[i].time) {
				float distance = time - keyframes[i].time;
				if(distance <= closestDistance) {
					closestDistance = distance;
					key1 = keyframes[i];
					closestIndex = i;
				}
			}
			else
				break;
		}
		
		if(closestIndex+1 >= keyframes.Length)
			return key2.value;
		else {
			key2 = keyframes[closestIndex+1];
		}
		
		float dt = (time - key1.time) / (key2.time - key1.time);
		//float tanLength = Mathf.Abs(key1.time - key2.time) * 0.3333f;
		//float p1 = key1.value;
		//float p2 = key2.value; 
		//float t1 = p1 + tanLength * key1.outTangent;
		//float t2 = p2 - tanLength * key2.inTangent;
		//float value = CubicSpline(dt, p1, t1, t2, p2);

		float value = Bezier(dt, key1, key2);
		return value;
	}
	
   public static float Bezier(float t, SerializableKeyframe key1, SerializableKeyframe key2) {
		float dt = key2.time - key1.time;
	     
		float m0 = key1.outTangent * dt;
		float m1 = key2.inTangent * dt;
	     
		float t2 = t * t;
	    float t3 = t2 * t;
	     
	    float a = 2 * t3 - 3 * t2 + 1;
	    float b = t3 - 2 * t2 + t;
	    float c = t3 - t2;
	    float d = -2 * t3 + 3 * t2;
		
	    return a * key1.value + b * m0 + c * m1 + d * key2.value;
    }
	
	public static float CubicSpline(float t, float p0, float p1, float p2, float p3) {
		float u = 1f - t;
		float tt = t*t;
		float uu = u*u;
		float uuu = uu * u;
		float ttt = tt * t;
		 
	 	float p = uuu * p0;
		p += 3f * uu * t * p1;
		p += 3f * u * tt * p2;
		p += ttt * p3;
		return p;
	}
}

}
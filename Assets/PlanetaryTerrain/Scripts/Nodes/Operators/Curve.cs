using UnityEngine;
using System.Collections;

namespace Planetary {

public class Curve : ModuleBase {

	private ModuleBase module1;

	public SerializableKeyframe[] keyframes;

	public Curve(SerializableKeyframe[] keyframes, ModuleBase m1) {
		this.keyframes = keyframes;
		module1 = m1;
	}
	
	public override float GetValue(Vector3 position) {
		if(module1 == null)
			return 0f;

		// convert to 0-1 range, multiply with curve and convert back to -1-1 range
		return Output(BezierCurve.Evaluate(Mathf.Clamp01((module1.GetValue(position) + 1f) / 2f), keyframes) * 2f - 1f);
	}
}

}
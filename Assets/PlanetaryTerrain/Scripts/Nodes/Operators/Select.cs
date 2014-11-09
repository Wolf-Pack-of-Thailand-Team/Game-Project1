using UnityEngine;
using System.Collections;

namespace Planetary {

public class Select : ModuleBase {
	
	private ModuleBase module1;

	public float target = 0f, range = .1f;
	
	public Select(ModuleBase m1, float t, float r) {
		module1 = m1;
		this.target = t;
		this.range = r;
	}
	
	public override float GetValue(Vector3 position) {
		return Output((range - Mathf.Min(Mathf.Abs(module1.GetValue(position) - target), range)) / range);
	}
}

}
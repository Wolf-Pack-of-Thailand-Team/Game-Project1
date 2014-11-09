using UnityEngine;
using System.Collections;

namespace Planetary {

public class Clamp : ModuleBase {
	private ModuleBase module1;
	public float min, max;
	
	public Clamp(float min, float max, ModuleBase m1) {
		this.min = min;
		this.max = max;
		module1 = m1;
	}
	
	public override float GetValue(Vector3 position) {
		return Output(Mathf.Clamp(module1.GetValue(position), min, max));
	}
}

}
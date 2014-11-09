using UnityEngine;
using System.Collections;

namespace Planetary {

public class Weight : ModuleBase {
	private ModuleBase module1, module2, module3;
	
	public float weight = .5f, target = .5f;
	
	public Weight(ModuleBase m1, float weight, float target) {
		module1 = m1;
		this.weight = weight;
		this.target = target;
	}
	
	public override float GetValue(Vector3 position) {
		return Output(Mathf.Lerp(module1.GetValue(position), target, weight));
	}
}

}
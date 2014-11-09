using UnityEngine;
using System.Collections;

namespace Planetary {

public class Warp : ModuleBase {
	
	public float power = 1f;
	private ModuleBase module1, module2;
	
	public Warp(float power, ModuleBase m1, ModuleBase m2) {
		this.power = power;
		module1 = m1;
		module2 = m2;
	}
	
	public override float GetValue(Vector3 position) {
		Vector3 up = -position.normalized;
		Vector3 direction = Vector3.Lerp(position, position + up, module2.GetValue(position) * power);
		return Output(module1.GetValue(direction));
	}
}

}
using UnityEngine;
using System.Collections;

namespace Planetary {

public class Power : ModuleBase {

	private ModuleBase module1, module2;
	
	public Power(ModuleBase m1, ModuleBase m2) {
		module1 = m1;
		module2 = m2;
	}
	
	public override float GetValue(Vector3 position) {
		return Output(Mathf.Pow(module1.GetValue(position), module2.GetValue(position)));
	}
}

}
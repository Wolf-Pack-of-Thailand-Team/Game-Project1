using UnityEngine;
using System.Collections;

namespace Planetary {

public class Max : ModuleBase {

	private ModuleBase module1, module2;
	
	public Max(ModuleBase m1, ModuleBase m2) {
		module1 = m1;
		module2 = m2;
	}
	
	public override float GetValue(Vector3 position) {
		return Output(Mathf.Max(module1.GetValue(position), module2.GetValue(position)));
	}
}

}
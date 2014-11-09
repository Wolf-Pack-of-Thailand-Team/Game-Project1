using UnityEngine;
using System.Collections;

namespace Planetary {

public class Multiply : ModuleBase {
	private ModuleBase module1, module2;
	
	public Multiply(ModuleBase m1, ModuleBase m2) {
		module1 = m1;
		module2 = m2;
	}
	
	public override float GetValue(Vector3 position) {
		return Output(module1.GetValue(position) * module2.GetValue(position));
	}
}

}
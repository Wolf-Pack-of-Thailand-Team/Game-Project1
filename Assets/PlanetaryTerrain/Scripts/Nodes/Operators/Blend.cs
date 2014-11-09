using UnityEngine;
using System.Collections;

namespace Planetary {

public class Blend : ModuleBase {
	private ModuleBase module1, module2, module3;
	
	public Blend(ModuleBase m1, ModuleBase m2, ModuleBase m3) {
		module1 = m1;
		module2 = m2;
		module3 = m3;
	}
	
	public override float GetValue(Vector3 position) {
		return Output((module2.GetValue(position) - module1.GetValue(position)) * ((module3.GetValue(position) + 1f) / 2f) + module1.GetValue(position));
	}
}

}
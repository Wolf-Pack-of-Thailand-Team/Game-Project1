using UnityEngine;
using System.Collections;

namespace Planetary {

public class Abs : ModuleBase {
	private ModuleBase module1;
	
	public Abs(ModuleBase m1) {
		module1 = m1;
	}
	
	public override float GetValue(Vector3 position) {
		return Output(Mathf.Abs(module1.GetValue(position)));
	}
}

}
using UnityEngine;
using System.Collections;

namespace Planetary {

public class Invert : ModuleBase {

	private ModuleBase module1;
	
	public Invert(ModuleBase m1) {
		module1 = m1;
	}
	
	public override float GetValue(Vector3 position) {
		return Output(-module1.GetValue(position));
	}
}

}
using UnityEngine;
using System.Collections;

public class Abs : ModuleBase {

	private ModuleBase module1;
	
	public Abs(ModuleBase m1) {
		module1 = m1;
	}
	
	public override float GetValue(Vector3 position) {
		return Mathf.Abs(module1.GetValue(position));
	}
}

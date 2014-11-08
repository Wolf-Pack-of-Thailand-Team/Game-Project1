using UnityEngine;
using System.Collections;

public class Exponent : ModuleBase {

	private ModuleBase module1;
	public float exponent;
	
	public Exponent(float exp, ModuleBase m1) {
		exponent = exp;
		module1 = m1;
	}
	
	public override float GetValue(Vector3 position) {
		return Mathf.Pow(module1.GetValue(position), exponent);
	}
}

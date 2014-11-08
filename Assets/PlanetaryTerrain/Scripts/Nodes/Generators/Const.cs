using UnityEngine;
using System.Collections;

public class Const : ModuleBase {
	public float constValue = 0f;
	
	public Const(float value) {
		constValue = value;
	}
	
	public override float GetValue(Vector3 position) {
		return constValue;
	}
}

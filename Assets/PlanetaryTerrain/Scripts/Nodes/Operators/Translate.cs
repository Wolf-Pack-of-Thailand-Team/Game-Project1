using UnityEngine;
using System.Collections;

namespace Planetary {

public class Translate : ModuleBase {

	private ModuleBase module1;
	public float x, y, z;
	
	public Translate(float x, float y, float z, ModuleBase m1) {
		module1 = m1;
		this.x = x;
		this.y = y;
		this.z = z;
	}
	
	public override float GetValue(Vector3 position) {
		position.x += x;
		position.y += y;
		position.z += z;
		return Output(module1.GetValue(position));
	}
}

}
using UnityEngine;
using System.Collections;

namespace Planetary {

public class Terrace : ModuleBase {

	private ModuleBase module1;
	public float min, max, power;
	
	public Terrace(float min, float max, float power, ModuleBase m1) {
		module1 = m1;
		this.min = min;
		this.max = max;
		this.power = power;
	}
	
	public override float GetValue(Vector3 position) {
		float value = module1.GetValue(position);
		
		if(value >= min && value <= max) {
			//float mid = (min + max) / 2f;
			float diff = ((value+1f - min+1f) / (max+1f - min+1f)) / 2f;
			return Mathf.Lerp(min, max, diff * power);
		}
		
		return Output(value);
	}
}

}
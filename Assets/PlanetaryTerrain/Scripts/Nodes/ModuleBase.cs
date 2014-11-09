using UnityEngine;
using System.Collections;

namespace Planetary {

public class ModuleBase {

	public bool colorOutput = false;

	public float minValue = -1f, maxValue = 1f;
	public bool normalizeOutput = false;
	public bool zeroToOneRange = false;
	
	public virtual float GetValue(Vector3 position) {
		return 0f;
	}

	public virtual Color32 GetColor(Vector3 position) {
		return new Color32();
	}

	protected float Output(float value) {
		if(normalizeOutput) {
			
			float range = (maxValue - minValue);
			value = (value - minValue) / range;
			if(!zeroToOneRange)
				value = value * 2f - 1f;
		}
		return value;
	}
}

}
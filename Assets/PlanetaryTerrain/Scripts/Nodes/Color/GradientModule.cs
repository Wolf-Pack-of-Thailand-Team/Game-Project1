
using UnityEngine;
using System.Collections;

namespace Planetary {

public class GradientModule : ModuleBase {
	
	private ModuleBase module1;

	public Gradient gradient;
	
	public GradientModule(ModuleBase m1, Gradient g) {
		colorOutput = true;
		module1 = m1;
		gradient = g;
	}
	
	public override Color32 GetColor(Vector3 position) {
		if(gradient != null && module1 != null)
			return gradient.Evaluate((module1.GetValue(position) + 1f) / 2f);
		else
			return Color.cyan;
	}
}

}

using UnityEngine;
using System.Collections;

namespace Planetary {

public class AddColorModule : ModuleBase {
	
	private ModuleBase module1, module2;
	
	public AddColorModule(ModuleBase m1, ModuleBase m2) {
		colorOutput = true;
		module1 = m1;
		module2 = m2;
	}
	
	public override Color32 GetColor(Vector3 position) {
		if(module1 != null && module2 != null)
			return (Color)module1.GetColor(position) + (Color)module2.GetColor(position);
		return Color.cyan;
	}
}

}
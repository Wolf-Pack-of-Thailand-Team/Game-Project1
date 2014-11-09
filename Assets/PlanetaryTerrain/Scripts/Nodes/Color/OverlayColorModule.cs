
using UnityEngine;
using System.Collections;

namespace Planetary {

public class OverlayColorModule : ModuleBase {
	
	private ModuleBase module1, module2;
	
	public OverlayColorModule(ModuleBase m1, ModuleBase m2) {
		colorOutput = true;
		module1 = m1;
		module2 = m2;
	}
	
	public override Color32 GetColor(Vector3 position) {
		if(module1 == null && module2 == null)
			return Color.cyan;

		Color c2 = module2.GetColor(position);
		return Color.Lerp(module1.GetColor(position), c2, c2.a);
	}
}

}
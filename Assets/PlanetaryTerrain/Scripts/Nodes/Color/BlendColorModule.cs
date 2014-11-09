
using UnityEngine;
using System.Collections;

namespace Planetary {

public class BlendColorModule : ModuleBase {
	
	private ModuleBase module1, module2, module3;
	
	public BlendColorModule(ModuleBase m1, ModuleBase m2, ModuleBase m3) {
		colorOutput = true;
		module1 = m1;
		module2 = m2;
		module3 = m3;
	}
	
	public override Color32 GetColor(Vector3 position) {
		return Color.Lerp(module1.GetColor(position), module2.GetColor(position), module3.GetValue(position));
	}
}

}
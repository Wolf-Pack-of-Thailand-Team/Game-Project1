
using UnityEngine;
using System.Collections;

namespace Planetary {

public class MultiplyColorModule : ModuleBase {
	
	private ModuleBase module1, module2;
	
	public MultiplyColorModule(ModuleBase m1, ModuleBase m2) {
		colorOutput = true;
		module1 = m1;
		module2 = m2;
	}
	
	public override Color32 GetColor(Vector3 position) {
		if(module1 == null && module2 == null)
			return Color.cyan;

		if(module1.colorOutput && module2.colorOutput)
			return (Color)module1.GetColor(position) * (Color)module2.GetColor(position);
		if(module1.colorOutput && !module2.colorOutput)
			return (Color)module1.GetColor(position) * module2.GetValue(position);
		if(!module1.colorOutput && module2.colorOutput)
			return module1.GetValue(position) * (Color)module2.GetColor(position);

		float value = module1.GetValue(position) * module2.GetValue(position);
		return new Color(value, value, value);
	}
}

}
using UnityEngine;
using System.Collections;

namespace Planetary {

public class ColorSource : ModuleBase {
	
	private Color color;
	
	public ColorSource(Color c) {
		color = c;
		colorOutput = true;
	}
	public override float GetValue(Vector3 position) {
		return color.grayscale;
	}
	public override Color32 GetColor(Vector3 position) {
		return color;
	}
}

}
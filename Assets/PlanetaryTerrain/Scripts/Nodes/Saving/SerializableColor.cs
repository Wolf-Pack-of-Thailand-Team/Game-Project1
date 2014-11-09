using UnityEngine;
using System.Collections;

namespace Planetary {

[System.Serializable()]
public class SerializableColor
{
	public float r = 0f, g = 0f, b = 0f, a = 0f;
	
	public SerializableColor() {
	}
	
	public SerializableColor(float r, float g, float b, float a) {
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public SerializableColor(Color color) {
		FromColor(color);
	}
	
	public Color ToColor() {
		return new Color(r, g, b, a);
	}
	
	public void FromColor(Color c) {
		this.r = c.r;
		this.g = c.g;
		this.b = c.b;
		this.a = c.a;
	}
}

}
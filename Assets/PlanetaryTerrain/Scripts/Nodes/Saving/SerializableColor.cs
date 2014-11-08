using UnityEngine;
using System.Collections;

[System.Serializable()]
public class SerializableColor
{
	public float r = 0f, g = 0f, b = 0f;
	
	public SerializableColor() {
	}
	
	public SerializableColor(float r, float g, float b) {
		this.r = r;
		this.g = g;
		this.b = b;
	}
	
	public Color ToColor() {
		return new Color(r, g, b);
	}
	
	public void FromColor(Color c) {
		this.r = c.r;
		this.g = c.g;
		this.b = c.b;
	}
}


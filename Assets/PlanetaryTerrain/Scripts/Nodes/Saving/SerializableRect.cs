using UnityEngine;
using System.Collections;

namespace Planetary {

[System.Serializable()]
public class SerializableRect
{
	public float x, y, width, height;
	
	public SerializableRect(float x, float y, float width, float height) {
		this.x = x;
		this.y = y;
		this.height = height;
		this.width = width;
	}
	
	public Rect ToRect() {
		return new Rect(x, y, width, height);
	}
	
	public static SerializableRect FromRect(Rect rect) {
		return new SerializableRect(rect.x, rect.y, rect.width, rect.height);
	}
}

}
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

namespace Planetary {

[System.Serializable()]
public class OutputNode : Node
{
	public enum OUTPUTTYPE {
		HEIGHTMAP, COLORMAP, NORMALMAP
	}
	public OUTPUTTYPE type, lastType;

	[OptionalField]
	public bool normalize = false;

	// junk data from previous serialization version..
	public bool seamless = false;
	public bool spherical = false;
	
	public float scale = 1.0f;
	public SerializableColor color1 = new SerializableColor(0, 0, 0, 0);
	public SerializableColor color2 = new SerializableColor(0, 0, 1, 0);
	public SerializableColor color3 = new SerializableColor(0, 1, 0, 0);
	public SerializableColor color4 = new SerializableColor(1, 1, 1, 0);
	public float color1Pos = -1f;
	public float color2Pos = -0.5f;
	public float color3Pos = 0.5f;
	public float color4Pos = 1f;

	public OutputNode(int x, int y) : base("Heightmap", new SerializableRect(x, y, 300, 400)) {
		CreateInputs(1);
		hasOutput = false;
		previewSize = 256;
		
		this.type = lastType = OUTPUTTYPE.HEIGHTMAP;
	}
	
	override public ModuleBase GetModule() {
		if(inputs[0] == null)
			return null;
		this.module = inputs[0].GetModule();
		
		SetOutputOptions();
		return module;
	}
}

}
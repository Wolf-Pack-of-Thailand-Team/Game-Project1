using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

[System.Serializable()]
public class OutputNode : Node
{
	[System.NonSerialized] private ModuleBase module;
	
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
	public SerializableColor color1 = new SerializableColor(0, 0, 0);
	public SerializableColor color2 = new SerializableColor(0, 0, 1);
	public SerializableColor color3 = new SerializableColor(0, 1, 0);
	public SerializableColor color4 = new SerializableColor(1, 1, 1);
	public float color1Pos = -1f;
	public float color2Pos = -0.5f;
	public float color3Pos = 0.5f;
	public float color4Pos = 1f;

	public OutputNode(int x, int y) : base("Output", new SerializableRect(x, y, 300, 400)) {
		CreateInputs(1);
		hasOutput = false;
		previewSize = 256;
		
		this.type = lastType = OUTPUTTYPE.HEIGHTMAP;
	}
	
	override public ModuleBase GetModule() {
		if(inputs[0] == null)
			return null;
		
		// add clamp module to ensure return value within -1 to 1 range
		//module = new Clamp(-1f, 1f, inputs[0].GetModule());
		return inputs[0].GetModule();
	}
}


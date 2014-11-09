
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

namespace Planetary {

[System.Serializable()]
public class TextureNode : Node
{
	public string textureId = "_ColorMap";

	public TextureNode(int x, int y) : base("Texture Output", new SerializableRect(x, y, 300, 400)) {
		CreateInputs(1);
		hasOutput = false;
		previewSize = 256;
	}
	
	override public ModuleBase GetModule() {
		if(inputs[0] == null)
			return null;
		return inputs[0].GetModule();
	}
}

}
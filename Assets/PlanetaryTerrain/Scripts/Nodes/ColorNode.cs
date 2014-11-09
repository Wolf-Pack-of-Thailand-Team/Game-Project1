
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Planetary {

[System.Serializable()]
public class ColorNode : Node
{
	public enum COLORTYPE {
		GRADIENT, ADD, MULTIPLY, BLEND, OVERLAY, COLOR
	}
	public COLORTYPE type, lastType;

	[System.NonSerialized] public Gradient gradient;

	public SerializableGradientColorKey[] gradientColorKeys = null;
	public SerializableGradientAlphaKey[] gradientAlphaKeys = null;

	public SerializableColor color;
	
	public ColorNode(int x, int y) : base("Color Node", new SerializableRect(x, y, 200, 150)) {
		type = lastType = ColorNode.COLORTYPE.COLOR;

		gradient = new Gradient();
		if(gradientColorKeys != null && gradientAlphaKeys != null) {
			gradient.colorKeys = SerializableGradientColorKey.ToGradientColorKeys(gradientColorKeys);
			gradient.alphaKeys = SerializableGradientAlphaKey.ToGradientColorKeys(gradientAlphaKeys);
		}

		color = new SerializableColor(Color.black);

		SetInputs();
	}
	
	override public ModuleBase GetModule() {
		// check that has inputs
		if(type != COLORTYPE.COLOR) {
			for(int i = 0; i < inputs.Length; i++) {
				if(inputs[i] == null) {
					return null;
				}
			}
		}
		
		// get module
		switch(type) {
		case COLORTYPE.GRADIENT:
			if(gradient == null) {
				gradient = new Gradient();
				if(gradientColorKeys != null)
					gradient.colorKeys = SerializableGradientColorKey.ToGradientColorKeys(gradientColorKeys);
				if(gradientAlphaKeys != null)
					gradient.alphaKeys = SerializableGradientAlphaKey.ToGradientColorKeys(gradientAlphaKeys);
			}
			if(gradientColorKeys == null)
				gradientColorKeys = SerializableGradientColorKey.FromGradientColorKeys(gradient.colorKeys);
			if(gradientAlphaKeys == null)
				gradientAlphaKeys = SerializableGradientAlphaKey.FromGradientColorKeys(gradient.alphaKeys);

			module = new GradientModule(inputs[0].GetModule(), gradient);
			break;
		case COLORTYPE.ADD:
			module = new AddColorModule(inputs[0].GetModule(), inputs[1].GetModule());
			break;
		case COLORTYPE.MULTIPLY:
			module = new MultiplyColorModule(inputs[0].GetModule(), inputs[1].GetModule());
			break;
		case COLORTYPE.BLEND:
			module = new BlendColorModule(inputs[0].GetModule(), inputs[1].GetModule(), inputs[2].GetModule());
			break;
		case COLORTYPE.COLOR:
			module = new ColorSource(color.ToColor());
			break;
		case COLORTYPE.OVERLAY:
			module = new OverlayColorModule(inputs[0].GetModule(), inputs[1].GetModule());
			break;
		}

		SetOutputOptions();
		
		return this.module;
	}
	
	public void SetInputs() {
		switch(type) {
		case COLORTYPE.GRADIENT:
			CreateInputs(1);
			break;
		case COLORTYPE.ADD:
			CreateInputs(2);
			break;
		case COLORTYPE.MULTIPLY:
			CreateInputs(2);
			break;
		case COLORTYPE.BLEND:
			CreateInputs(3);
			break;
		case COLORTYPE.COLOR:
			CreateInputs(0);
			break;
		case COLORTYPE.OVERLAY:
			CreateInputs(2);
			break;
		}
	}
}

}
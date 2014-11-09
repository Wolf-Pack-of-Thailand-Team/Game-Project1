using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

namespace Planetary {

[System.Serializable()]
public class OperatorNode : Node
{
	public enum OPERATORTYPE {
		ABS, ADD, BLEND, CLAMP, EXPONENT, INVERT,
		MAX, MIN, MULTIPLY, POWER, SUBTRACT,
		TERRACE, TRANSLATE, DIVIDE, CURVE, WEIGHT,
		WARP, SELECT
	}
	public OPERATORTYPE type, lastType;
	
	public float min = -1f;
	public float max = 1f;
	public float exponent = 1.0f;
	public float x = 0.5f;
	public float y = 0.5f;
	public float z = 0.5f;
	public float scale = 1f;
	public float scaleBias = 0.5f;
	public float fallOff = 0f;
	public float power = 1f;
	public bool inverted = false;
	
	public float in1 = -1f;
	public float out1 = 1f;
	public float in2 = -1f;
	public float out2 = 1f;
	
	[System.NonSerialized] public AnimationCurve curve;
	public SerializableKeyframe[] keyframes;
	
	public OperatorNode(int x, int y) : base("Operator", new SerializableRect(x, y, 200, 150)) {
		type = lastType = OperatorNode.OPERATORTYPE.ABS;
		SetInputs();
		
		curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		if(keyframes != null)
			curve.keys = SerializableKeyframe.ToKeyframeArray(keyframes);
	}
	
	override public ModuleBase GetModule() {
		// check that has inputs
		for(int i = 0; i < inputs.Length; i++) {
			if(inputs[i] == null) {
				return null;
			}
		}
		
		// get module
		switch(type) {
			case OPERATORTYPE.ABS:
				module = new Abs(inputs[0].GetModule());
				break;
			case OPERATORTYPE.ADD:
				module = new Add(inputs[0].GetModule(), inputs[1].GetModule());
				break;
			case OPERATORTYPE.BLEND:
				module = new Blend(inputs[0].GetModule(), inputs[1].GetModule(), inputs[2].GetModule());
				break;
			case OPERATORTYPE.CLAMP:
				module = new Clamp(min, max, inputs[0].GetModule());
				break;
			case OPERATORTYPE.EXPONENT:
				module = new Exponent(exponent, inputs[0].GetModule());
				break;
			case OPERATORTYPE.INVERT:
				module = new Invert(inputs[0].GetModule());
				break;
			case OPERATORTYPE.MAX:
				module = new Max(inputs[0].GetModule(), inputs[1].GetModule());
				break;
			case OPERATORTYPE.MIN:
				module = new Min(inputs[0].GetModule(), inputs[1].GetModule());
				break;
			case OPERATORTYPE.MULTIPLY:
				module = new Multiply(inputs[0].GetModule(), inputs[1].GetModule());
				break;
			case OPERATORTYPE.POWER:
				module = new Power(inputs[0].GetModule(), inputs[1].GetModule());
				break;
			case OPERATORTYPE.SUBTRACT:
				module = new Subtract(inputs[0].GetModule(), inputs[1].GetModule());
				break;
			case OPERATORTYPE.TERRACE:
				module = new Terrace(min, max, power, inputs[0].GetModule());
				break;
			case OPERATORTYPE.TRANSLATE:
				module = new Translate(x, y, z, inputs[0].GetModule());
				break;
			case OPERATORTYPE.DIVIDE:
				module = new Divide(inputs[0].GetModule(), inputs[1].GetModule());
				break;
			case OPERATORTYPE.CURVE:
				if(curve == null) {
					curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
					if(keyframes != null)
						curve.keys = SerializableKeyframe.ToKeyframeArray(keyframes);
				}
				if(keyframes == null)
					keyframes = SerializableKeyframe.FromKeyframeArray(curve.keys);
				module = new Curve(keyframes, inputs[0].GetModule());
				break;
			case OPERATORTYPE.WEIGHT:
				module = new Weight(inputs[0].GetModule(), min, max);
				break;
			case OPERATORTYPE.WARP:
				module = new Warp(power, inputs[0].GetModule(), inputs[1].GetModule());
				break;
			case OPERATORTYPE.SELECT:
				module = new Select(inputs[0].GetModule(), min, max);
				break;
		}
		
		SetOutputOptions();
		return this.module;
	}
	
	public void SetInputs() {
		switch(type) {
			case OPERATORTYPE.ABS:
				CreateInputs(1);
				break;
			case OPERATORTYPE.ADD:
				CreateInputs(2);
				break;
			case OPERATORTYPE.BLEND:
				CreateInputs(3);
				break;
			case OPERATORTYPE.CLAMP:
				CreateInputs(1);
				break;
			case OPERATORTYPE.EXPONENT:
				CreateInputs(1);
				break;

			case OPERATORTYPE.INVERT:
				CreateInputs(1);
				break;
			case OPERATORTYPE.MAX:
				CreateInputs(2);
				break;
			case OPERATORTYPE.MIN:
				CreateInputs(2);
				break;
			case OPERATORTYPE.MULTIPLY:
				CreateInputs(2);
				break;
			case OPERATORTYPE.POWER:
				CreateInputs(2);
				break;
			case OPERATORTYPE.SUBTRACT:
				CreateInputs(2);
				break;
			case OPERATORTYPE.TERRACE:
				CreateInputs(1);
				break;
			case OPERATORTYPE.TRANSLATE:
				CreateInputs(1);
				break;
			case OPERATORTYPE.DIVIDE:
				CreateInputs(2);
				break;
			case OPERATORTYPE.CURVE:
				CreateInputs(1);
				break;
			case OPERATORTYPE.WARP:
				CreateInputs(2);
				break;
		}
	}
}

}
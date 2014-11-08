using UnityEngine;
using System.Collections.Generic;

[System.Serializable()]
public class Node
{
	public string title;
	public SerializableRect rect;
	public int previewSize = 64;
	
	protected Node[] inputs;
	private int lastInputCount = 0;
	protected bool hasOutput = true;

	#region Properties
	
	public SerializableRect Rect {
		get {
			return this.rect;
		}
	}
	public Node[] Inputs {
		get {
			return this.inputs;
		}
	}
	public bool HasOutput {
		get {
			return this.hasOutput;
		}
	}
	#endregion
	
	public Node(string title, SerializableRect rect) {
		this.title = title;
		this.rect = rect;
	}
	
	public virtual ModuleBase GetModule() {
		return null;
	}
	
	public void CreateInputs(int count) {
		if(count != lastInputCount) {
			Node[] oldInputs = inputs;
			this.inputs = new Node[count];
			for(int i = 0; i < inputs.Length; i++) {
				if(oldInputs != null) {
					if(i < oldInputs.Length)
						inputs[i] = oldInputs[i];
				}
				else
					inputs[i] = null;
			}
			
			lastInputCount = count;
		}
	}
	
	public void Connect(Node node, int port) {
		if(port < inputs.Length)
			inputs[port] = node;
	}
	
	public void Disconnect(int port) {
		if(port < inputs.Length)
			inputs[port] = null;
	}
}


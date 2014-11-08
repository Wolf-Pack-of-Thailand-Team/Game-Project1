using UnityEngine;
using System.Collections;

public class HelpGui : MonoBehaviour 
{
	public bool displayHelp = true;
	public VoxelandDemo.CameraController cameraController;
	
	public int helpWidth = 400;
	public int helpHeight = 400;
	
	string helpText = "Voxeland Demo Quick Manual:\n"+
		"- W,A,S,D: walk;\n"+
			"- left click: add block;\n"+
			"- shift + click: dig block;\n"+
			"- ctrl + click: smooth terrain;\n"+
			"- ctrl + shift + click: replace blocks;\n"+
			"~: select grass;\n"+
			"1: select Cliff;\n"+
			"2: select Mud;\n"+
			"3: select Torch;\n"+
			"[: increase brush size;\n"+
			"]: decrease brush size;\n"+
			"F5: save;\n"+
			"F6: load;\n"+
			"F1: hide/show this manual";

	void OnGUI ()
	{
		if (cameraController == null) cameraController = FindObjectOfType(typeof(VoxelandDemo.CameraController)) as VoxelandDemo.CameraController;
		
		if (displayHelp)
		{
			GUI.BeginGroup(new Rect((Screen.width-helpWidth)/2, (Screen.height-helpHeight)/2, helpWidth, helpHeight));
			GUI.Box(new Rect(0, 0, helpWidth, helpHeight), helpText);
			if (GUI.Button(new Rect(helpWidth/2-50, helpHeight-40, 100, 25), "OK")) displayHelp = false;
			GUI.EndGroup();
		}
		
		if (displayHelp) cameraController.lockCursor = false;
		else cameraController.lockCursor = true;
		

	}
}

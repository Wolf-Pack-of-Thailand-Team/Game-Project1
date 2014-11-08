using UnityEngine;
using System.Collections;

namespace VoxelandDemo
{

	public class VoxelandController : MonoBehaviour 
	{
		public Voxeland.VoxelandTerrain land;
		public VoxelandDemo.CameraController cameraController;
		
		public int helpWidth = 400;
		public int helpHeight = 400;
		
		public int messageWidth = 600;
		public int messageHeight = 100;
		
		public bool displayHelp = true;
		public bool displaySave = false;
		public bool displayLoad = false;
		public bool displayNew = false;
		
		string helpText = "Voxeland Demo Quick Manual:\n\n"+
			"W,A,S,D: walk;\n"+
				"left click: add block;\n"+
				"right click: dig block;\n"+
				"~: select grass;\n"+
				"1: select Cliff;\n"+
				"2: select Mud;\n"+
				"3: select Torch;\n"+
				"4: select Tree;\n"+
				"[: increase brush size;\n"+
				"]: decrease brush size;\n"+
				"F5: save;\n"+
				"F6: load;\n"+
				"F1: hide/show this manual";
		
		void Update ()
		{
			if (land == null) land = FindObjectOfType(typeof(Voxeland.VoxelandTerrain)) as Voxeland.VoxelandTerrain;
			if (!displayHelp && !displaySave && !displayLoad && !displayNew)
			land.Edit(Camera.main.ViewportPointToRay(new Vector3(0.5f,0.5f,0.5f)),
			    Input.GetMouseButtonDown(0),
			    Input.GetMouseButtonDown(1),
			    false, false);
		}
		
		void OnGUI () 
		{
			if (land == null) land = FindObjectOfType(typeof(Voxeland.VoxelandTerrain)) as Voxeland.VoxelandTerrain;
			if (cameraController == null) cameraController = FindObjectOfType(typeof(VoxelandDemo.CameraController)) as VoxelandDemo.CameraController;
			
			if (Input.GetKey("`")) land.selected = -1;
			if (Input.GetKey("1")) land.selected = 1;
			if (Input.GetKey("2")) land.selected = 2;
			if (Input.GetKey("3")) land.selected = 3;
			if (Input.GetKey("4")) land.selected = 4;
			
			if (Input.GetKeyDown("]")) land.brushSize++;
			if (Input.GetKeyDown("[")) land.brushSize--;
			land.brushSize = Mathf.Max(land.brushSize,0);

			//help screen
			if (Input.GetKeyDown(KeyCode.F1)) displayHelp = !displayHelp;
			if (displayHelp)
			{
				GUI.BeginGroup(new Rect((Screen.width-helpWidth)/2, (Screen.height-helpHeight)/2, helpWidth, helpHeight));
				GUI.Box(new Rect(0, 0, helpWidth, helpHeight), "");
				GUI.Label(new Rect(20, 0, helpWidth-20, helpHeight), helpText);
				if (GUI.Button(new Rect(helpWidth/2-50, helpHeight-40, 100, 25), "OK")) displayHelp = false;
				GUI.EndGroup();
			}
			
			string guiText= null;
			if (land.selected > 0) guiText = "Selected: " + land.types[land.selected].name + "\nBrush size: " + land.brushSize;
			else guiText = "Selected: " + land.grass[-land.selected].name + "\nBrush size: " + land.brushSize;
			GUI.Box(new Rect(Screen.width-130, Screen.height-60, 110, 40), guiText);
			
			//save
			if (Input.GetKeyDown(KeyCode.F5)) 
			{
				using (System.IO.FileStream fs = new System.IO.FileStream(Application.persistentDataPath + "/VoxelandData.txt", System.IO.FileMode.Create))
					using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fs))
						writer.Write(land.data.SaveToString());
				displaySave = true;
			}
			if (displaySave)
			{
				GUI.BeginGroup(new Rect((Screen.width-messageWidth)/2, (Screen.height-messageHeight)/2, messageWidth, messageHeight));
				GUI.Box(new Rect(0, 0, messageWidth, messageHeight), "Voxeland data was saved to:\n" + Application.persistentDataPath + "/VoxelandData.txt");
				if (GUI.Button(new Rect(messageWidth/2-50, messageHeight-40, 100, 25), "OK")) displaySave = false;
				GUI.EndGroup();
			}
			
			//load
			if (Input.GetKeyDown(KeyCode.F6)) 
			{
				using (System.IO.FileStream fs = new System.IO.FileStream(Application.persistentDataPath + "/VoxelandData.txt", System.IO.FileMode.Open))
					using (System.IO.StreamReader reader = new System.IO.StreamReader(fs))
						land.data.LoadFromString( reader.ReadToEnd() );
				land.Rebuild();
				displayLoad = true;
			}
			if (displayLoad)
			{
				GUI.BeginGroup(new Rect((Screen.width-messageWidth)/2, (Screen.height-messageHeight)/2, messageWidth, messageHeight));
				GUI.Box(new Rect(0, 0, messageWidth, messageHeight), "Voxeland data was loaded");
				if (GUI.Button(new Rect(messageWidth/2-50, messageHeight-40, 100, 25), "OK")) displayLoad = false;
				GUI.EndGroup();
			}
			
			if (displayHelp || displaySave || displayLoad || displayNew) cameraController.lockCursor = false;
			else cameraController.lockCursor = true;
		}
	}

}

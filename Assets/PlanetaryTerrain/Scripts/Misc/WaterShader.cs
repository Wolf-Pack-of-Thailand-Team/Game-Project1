using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class WaterShader : MonoBehaviour {
	
	public Camera mainCamera;
	
	void Start() {
		if(mainCamera == null)
			mainCamera = Camera.main;
	}
	
	void Update () {
		renderer.sharedMaterial.SetVector("_CameraPos", mainCamera.transform.position);
		renderer.sharedMaterial.SetFloat("_WaveTime", Time.time * 0.005f);
	}
}

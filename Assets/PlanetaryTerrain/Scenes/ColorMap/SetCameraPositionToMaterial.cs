using UnityEngine;
using System.Collections;

public class SetCameraPositionToMaterial : MonoBehaviour {

	public Transform cameraTransform;
	public string propertyName = "_CameraPos";
	public Material material;

	private Vector4 positionVector = Vector4.zero;

	void Update () {
		positionVector.Set(cameraTransform.position.x, cameraTransform.position.y, cameraTransform.position.z, 0f);
		material.SetVector(propertyName, positionVector);
	}
}

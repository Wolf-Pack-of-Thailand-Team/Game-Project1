using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Planetary {

[ExecuteInEditMode()]
public class SurfaceObjects : MonoBehaviour {

	public List<Transform> surfaceObjects;
	public Surface surface;

	public float objectChance = .2f;
	public float minHeight = .2f, maxHeight = .25f;
	public float minPolarity = 0f, maxPolarity = .7f;

	public Vector3 scale = Vector3.one, rotation = Vector3.zero;
	public Vector3 scaleVariation = Vector3.zero, rotationVariation = Vector3.zero;
	
	public void Populate(Surface s, Transform objectTemplate) {
		surface = s;
		
		if(surfaceObjects != null)
			DestroyExistingObjects();
		
		surfaceObjects = new List<Transform>();
		
		GameObject objectsParent = new GameObject("Objects");
		objectsParent.transform.parent = this.transform;
		
		Vector3[] vertices = surface.mesh.vertices;
		Vector3[] normals = surface.mesh.normals;
		Color[] colors = surface.mesh.colors; 
		// colors contain planet data
		// r = height
		// g = polarity
		
		for(int i = 0; i < vertices.Length; i++) {
			if(colors[i].r >= minHeight && colors[i].r <= maxHeight && colors[i].g >= minPolarity && colors[i].g <= maxPolarity) {
				if(Random.Range(0f, 1f) < objectChance) {
					Transform newObject = PlacementHelper.PlaceObject(objectTemplate, objectsParent.transform, vertices[i], normals[i], scale, rotation, scaleVariation, rotationVariation);
					surfaceObjects.Add(newObject);
				}
			}
		}
	}
	
	void OnDestroy() {
		DestroyExistingObjects();
	}
	
	void DestroyExistingObjects() {
		if(surfaceObjects != null) {
			for(int i = 0; i < surfaceObjects.Count; i++) {
				if(surfaceObjects[i] != null)
					DestroyImmediate(surfaceObjects[i].gameObject);
			}
		}
	}
}

}
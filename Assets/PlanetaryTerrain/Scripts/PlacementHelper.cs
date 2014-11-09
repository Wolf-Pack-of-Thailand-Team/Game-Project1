using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Planetary {

[ExecuteInEditMode()]
public class PlacementHelper : MonoBehaviour {

	public Planet planet;

	public Transform objectToPlace;

	public Vector3 scale = Vector3.one, rotation = Vector3.zero;
	public Vector3 scaleVariation = Vector3.zero, rotationVariation = Vector3.zero;

	public bool automaticPerVertexPlacement = true;
	public int lodLevel = 0;
	public float objectChance = .2f;
	public float minHeight = .2f, maxHeight = .25f;
	public float minPolarity = 0f, maxPolarity = .7f;

	public List<Transform> objects = new List<Transform>();
	public List<SurfaceObjects> surfaceObjects = new List<SurfaceObjects>();

	/// <summary>
	/// Keeps connections to planet
	/// </summary>
	void OnEnable() {
		if(planet == null)
			planet = GetComponent<Planet>();

		if(planet != null)
			planet.SurfaceGenerated += OnSurfaceGenerated;
	}

	/// <summary>
	/// Clears the manually added objects.
	/// </summary>
	public void ClearObjects() {
		if(objects != null) {
			for(int i = 0; i < objects.Count; i++) {
				if(objects[i] != null)
					DestroyImmediate(objects[i].gameObject);
			}
			objects.Clear();
		}
	}

	/// <summary>
	/// Generates the objects
	/// </summary>
	public void GenerateObjects() {
		for(int i = 0; i < planet.surfaces.Count; i++) {
			OnSurfaceGenerated(planet.surfaces[i]);
		}
	}

	/// <summary>
	/// Clears the generated objects.
	/// </summary>
	public void ClearGeneratedObjects() {
		if(surfaceObjects != null) {
			for(int i = 0; i < surfaceObjects.Count; i++) {
				if(surfaceObjects[i] != null)
					DestroyImmediate(surfaceObjects[i]);
			}
			surfaceObjects.Clear();
		}
	}

	/// <summary>
	/// Places one object at given position using the static function
	/// </summary>
	public void PlaceObject(Transform parent, Vector3 position, Vector3 up) {
		Transform t = PlaceObject(objectToPlace, parent, position, up, scale, rotation, scaleVariation, rotationVariation);
		objects.Add(t);
	}

	/// <summary>
	/// Places one object at given position
	/// </summary>
	public static Transform PlaceObject(Transform objectToPlace, Transform parent, Vector3 position, Vector3 up, Vector3 scale, Vector3 rotation, Vector3 scaleVariation, Vector3 rotationVariation) {
		Vector3 currentScale = scale + scaleVariation * Random.Range(-1f, 1f);

		Vector3 currentRotation = rotation;
		currentRotation.x += rotationVariation.x * Random.Range(-1f, 1f);
		currentRotation.y += rotationVariation.y * Random.Range(-1f, 1f);
		currentRotation.z += rotationVariation.z * Random.Range(-1f, 1f);

		Transform t = (Transform)Instantiate(objectToPlace);
		if(parent != null)
			t.parent = parent;
		t.localScale = currentScale;
		t.position = position;
		t.rotation = Quaternion.LookRotation(up);
		t.Rotate(currentRotation);

		return t;
	}

	/// <summary>
	/// Event raised by the planet when a new surface is generated.
	/// If enabled, adds SurfaceObjects component to the Surface and populates it with objects
	/// </summary>
	public void OnSurfaceGenerated(Surface s) {
		if(automaticPerVertexPlacement) {
			if(s.lodLevel == lodLevel) {
				SurfaceObjects so = s.gameObject.AddComponent<SurfaceObjects>();

				so.minHeight = minHeight;
				so.maxHeight = maxHeight;
				so.minPolarity = minPolarity;
				so.maxPolarity = maxPolarity;
				so.objectChance = objectChance;
				so.scale = scale;
				so.rotation = rotation;
				so.scaleVariation = scaleVariation;
				so.rotationVariation = rotationVariation;

				so.Populate(s, objectToPlace);
				surfaceObjects.Add(so);
			}
		}
	}
}

}
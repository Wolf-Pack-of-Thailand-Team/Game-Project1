using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Planetary {

[CustomEditor(typeof(PlacementHelper))]
public class PlacementHelperEditor : Editor {

	private bool placeObjects = false;
	private PlacementHelper placementHelper;

	private Vector3 lastPoint, lastNormal;

	void OnEnable() {
		placementHelper = (PlacementHelper)target;
	}

	override public void OnInspectorGUI() {

		placementHelper.objectToPlace = (Transform)EditorGUILayout.ObjectField(placementHelper.objectToPlace, typeof(Transform), true);
		placementHelper.rotation = EditorGUILayout.Vector3Field("Rotation", placementHelper.rotation);
		placementHelper.scale = EditorGUILayout.Vector3Field("Scale", placementHelper.scale);

		placementHelper.rotationVariation = EditorGUILayout.Vector3Field("Rotation variation", placementHelper.rotationVariation);
		placementHelper.scaleVariation = EditorGUILayout.Vector3Field("Scale variation", placementHelper.scaleVariation);

		if(placeObjects) {
			if(GUILayout.Button("Stop placing")) {
				placeObjects = false;
			}
		}
		else {
			if(GUILayout.Button("Place objects")) {
				placeObjects = true;
			}
		}
		if(placementHelper.objects.Count > 0) {
			if(GUILayout.Button("Clear objects")) {
				placementHelper.ClearObjects();
			}
		}

		// automatic
		placementHelper.automaticPerVertexPlacement = EditorGUILayout.Toggle("Automatic placement", placementHelper.automaticPerVertexPlacement);
		if(placementHelper.automaticPerVertexPlacement) {
			EditorGUILayout.HelpBox("Automatic placement adds objects to newly generated surfaces at given lod level. The objects are added to each vertex position.", MessageType.Info);
			placementHelper.lodLevel = EditorGUILayout.IntField("Lod Level", placementHelper.lodLevel);
			placementHelper.objectChance = EditorGUILayout.FloatField("Object Chance", placementHelper.objectChance);
			placementHelper.minHeight = EditorGUILayout.FloatField("Min Height", placementHelper.minHeight);
			placementHelper.maxHeight = EditorGUILayout.FloatField("Max Height", placementHelper.maxHeight);
			placementHelper.minPolarity = EditorGUILayout.FloatField("Min Polarity", placementHelper.minPolarity);
			placementHelper.maxPolarity = EditorGUILayout.FloatField("Max Polarity", placementHelper.maxPolarity);

			if(GUILayout.Button("Generate Objects")) {
				placementHelper.GenerateObjects();
			}
		}

		if(placementHelper.surfaceObjects.Count > 0) {
			if(GUILayout.Button("Clear generated objects")) {
				placementHelper.ClearGeneratedObjects();
			}
		}
	}

	public void OnSceneGUI() {
		if(placeObjects) {
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

			if(Event.current.type == EventType.MouseMove) {
				Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				RaycastHit hit;
				
				if(Physics.Raycast(ray, out hit)) {
					lastPoint = hit.point;
					lastNormal = hit.normal;
					
					SceneView.RepaintAll();
				}
			}

			if(Event.current.type == EventType.MouseDown){
				if(Event.current.button == 0) {
					Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
					RaycastHit hit = new RaycastHit();

					if(Physics.Raycast(ray, out hit)) {
						Surface s = hit.collider.gameObject.GetComponent<Surface>();
						if(s != null) {
							placementHelper.PlaceObject(null, hit.point, hit.normal);
						}
					}

					Event.current.Use();
				}
			}

			Handles.DrawWireDisc(lastPoint, lastNormal, placementHelper.scale.magnitude);
		}
	}
}

}
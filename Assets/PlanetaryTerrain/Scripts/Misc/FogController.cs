using UnityEngine;
using System.Collections;

namespace Planetary {

public class FogController : MonoBehaviour {

	public new Camera camera;
	
	public float fogDensityAtGroundLevel = 0.1f;
	public float groundLevelDistanceFromPlanet = 50000;
	public float spaceStartDistanceFromPlanet = 52000;
	
	void Update () {
		if(camera == null)
			return;
		
		float distance = Vector3.Distance(camera.transform.position, transform.position);
		
		if(distance < groundLevelDistanceFromPlanet) {
			RenderSettings.fogDensity = fogDensityAtGroundLevel;
		}
		else {
			if(distance < spaceStartDistanceFromPlanet) {
				float fogRange = spaceStartDistanceFromPlanet - groundLevelDistanceFromPlanet;
					RenderSettings.fogDensity = Mathf.Lerp(fogDensityAtGroundLevel, 0f, (distance - groundLevelDistanceFromPlanet) / fogRange);
			}
			else {
				// dont affect fog
			}
		}
	}
}

}
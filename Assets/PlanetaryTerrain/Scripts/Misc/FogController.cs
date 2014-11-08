using UnityEngine;
using System.Collections;

public class FogController : MonoBehaviour {

	public new Camera camera;
	
	public bool atmosphericFog = true;
	public FogMode atmospheriFogMode = FogMode.ExponentialSquared;
	public Color atmospheriFogColor = Color.white;
	public float atmospheriFogDensity = 0.05f;
	public float atmospheriFogStartDistance = 1500f;
	public AnimationCurve fogCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
	
	public Renderer atmosphere;
	private float startDensity;
	public float densityRange = 1f;
	public AnimationCurve densityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
	
	void Start() {
		RenderSettings.fog = atmosphericFog;
		RenderSettings.fogMode = atmospheriFogMode;
		RenderSettings.fogColor = atmospheriFogColor;
		RenderSettings.fogDensity = 0f;
		
		if(atmosphere != null) {
			startDensity = atmosphere.sharedMaterial.GetFloat("_EdgeDensity");
		}
	}
	
	void OnDisable() {
		atmosphere.sharedMaterial.SetFloat("_EdgeDensity", startDensity);
	}
	
	void Update () {
		if(camera == null)
			return;
		
		float distance = Vector3.Distance(camera.transform.position, transform.position);
		
		if(atmosphere != null) {
			if(distance <= atmospheriFogStartDistance) {
				atmosphere.sharedMaterial.SetFloat("_EdgeDensity", startDensity + densityCurve.Evaluate(1f - Mathf.Clamp01(distance / atmospheriFogStartDistance)) * densityRange);
			}
		}
		
		if(distance < atmospheriFogStartDistance) {
			float d = 1f - Mathf.Clamp01(distance / atmospheriFogStartDistance);
			
			if(atmosphericFog) {
				if(RenderSettings.fogColor != atmospheriFogColor)
					RenderSettings.fogColor = atmospheriFogColor;
				if(RenderSettings.fogMode != atmospheriFogMode)
					RenderSettings.fogMode = atmospheriFogMode;
				
				RenderSettings.fogDensity = fogCurve.Evaluate(d) * atmospheriFogDensity;
			}
			
			if(atmosphere != null) {
				atmosphere.sharedMaterial.SetFloat("_EdgeDensity", startDensity + densityCurve.Evaluate(d) * densityRange);
			}
		}
		else {
			RenderSettings.fogDensity = 0f;
		}
	}
}

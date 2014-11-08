using UnityEngine;
using System.Collections;

public class PlanetaryTerrain {
	
	public static Planet CreatePlanet() {
		return CreatePlanet("Planet");
	}
	
	public static Planet CreatePlanet(string name) {
		GameObject go = new GameObject(name);
		Planet planet = go.AddComponent<Planet>();
		SurfaceColor surfaceColor = go.AddComponent<SurfaceColor>();
		surfaceColor.shader = Shader.Find("TexturedPlanet");
		
		return planet;
	}
	
	public static GameObject CreateAtmosphere(float radius) {
		GameObject go = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Atmosphere"));
		go.transform.localScale = Vector3.one * radius;
		
		return go;
	}

	public static GameObject CreateHydrosphere(float radius) {
		GameObject go = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Hydrosphere"));
		go.transform.localScale = Vector3.one * radius;
		
		return go;
	}
}

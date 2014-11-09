using UnityEngine;
using System.Collections.Generic;

namespace Planetary {

[ExecuteInEditMode()]
public class SurfaceColor : MonoBehaviour {
	
	public Planet planet;
	public Renderer atmosphere;
	
	public Material material;
	public Shader shader;
	
	public float value1 = .33f, value2 = .66f, value3 = 1f;
	public float brightness = 1f;
	
	public Color color1 = new Color(97f /255f, 94f /255f, 66f /255f);
	public Color color2 = new Color(194f /255f, 188f /255f, 132f /255f);
	public Color color3 = new Color(115f /255f, 183f /255f, 101f /255f);
	public Color color4 = new Color(255f /255f, 244f /255f, 215f /255f);
	
	public bool useTextures = true;
	public Texture2D texture1, texture2, texture3, texture4;
	public Vector4 textureScales = new Vector4(1f, 1f, 1f, 1f);
	
	public bool useBumpMaps = false;
	public Texture2D normal1, normal2, normal3, normal4;
	public Vector4 normalScales = new Vector4(1f, 1f, 1f, 1f);
	
	public float polarity = .8f, polarStrenght = 1;
	public Color polarColor = Color.white;
	
	public float equatorWidth = .2f, equatorStrenght = 1, equatorHeight = 0f;
	public Color equatorColor = new Color(200f /255f, 85f /255f, 0f /255f);
	
	public void Awake() {
		if(planet == null)
			planet = GetComponent<Planet>();

		if(shader == null)
			shader = Shader.Find("TexturedPlanet");

		if(material == null)
			material = new Material(shader);
	}

	public void OnEnable() {
		planet.SurfaceGenerated += ApplyToSurface;
	}
	
	[ContextMenu("Apply")]
	public void Apply() {
		if(planet == null)
			planet = GetComponent<Planet>();
		if(planet == null)
			return;
		
		if(material == null)
			material = new Material(shader);
		
		List<Surface> surfaces = planet.surfaces;
		if(surfaces == null) {
			Surface[] childSurfaces = (Surface[])planet.GetComponentsInChildren<Surface>();
			if(childSurfaces.Length == 0)
				return;
			surfaces = new List<Surface>();
			surfaces.AddRange(childSurfaces);
		}
		
		for(int i = 0; i < surfaces.Count; i++) {
			ApplyToSurface(surfaces[i]);
		}
	}
	
	public void ApplyToSurface(Surface surface) {
		if(surface == null)
			return;

		//material = new Material(Shader.Find("Diffuse"));
		
		//surface.renderer.sharedMaterial = material;
		//surface.renderer.sharedMaterial.SetTexture("_MainTex", surface.textures[0]);
		
		if(material == null)
			material = new Material(shader);
		
		surface.renderer.sharedMaterial = material;
		
		/*surface.renderer.sharedMaterial.SetFloat("_Value1", value1);
		surface.renderer.sharedMaterial.SetFloat("_Value2", value2);
		surface.renderer.sharedMaterial.SetFloat("_Value3", value3);
			
		surface.renderer.sharedMaterial.SetFloat("_Brightness", brightness);
			
		surface.renderer.sharedMaterial.SetColor("_Color1", color1);
		surface.renderer.sharedMaterial.SetColor("_Color2", color2);
		surface.renderer.sharedMaterial.SetColor("_Color3", color3);
		surface.renderer.sharedMaterial.SetColor("_Color4", color4);

		if(useTextures) {
			surface.renderer.sharedMaterial.SetTexture("_Texture1", texture1);
			surface.renderer.sharedMaterial.SetTexture("_Texture2", texture2);
			surface.renderer.sharedMaterial.SetTexture("_Texture3", texture3);
			surface.renderer.sharedMaterial.SetTexture("_Texture4", texture4);

			surface.renderer.sharedMaterial.SetVector("_UvScale", textureScales);
		}

		if(useBumpMaps) {
			surface.renderer.sharedMaterial.SetTexture("_Normal1", normal1);
			surface.renderer.sharedMaterial.SetTexture("_Normal2", normal2);
			surface.renderer.sharedMaterial.SetTexture("_Normal3", normal3);
			surface.renderer.sharedMaterial.SetTexture("_Normal4", normal4);

			surface.renderer.sharedMaterial.SetVector("_NormalUvScale", normalScales);
		}

		// poles
		surface.renderer.sharedMaterial.SetColor("_PolarColor", polarColor);
		surface.renderer.sharedMaterial.SetFloat("_Polarity", polarity);
		surface.renderer.sharedMaterial.SetFloat("_PolarStrenght", polarStrenght);
			
		// equator
		surface.renderer.sharedMaterial.SetColor("_EquatorColor", equatorColor);
		surface.renderer.sharedMaterial.SetVector("_Equator", new Vector4(equatorWidth, equatorStrenght, equatorHeight));*/
	}
}

}
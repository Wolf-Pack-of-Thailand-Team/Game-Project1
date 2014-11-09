using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Planetary {

public class FoliageRenderer : MonoBehaviour {

	public Planet planet;

	public Transform lodTarget;
	public Camera[] cameras;

	public TextAsset foliageModule;
	private TerrainModule terrainModule;
	
	public FoliageType[] foliageTypes;
	
	#region Initialization

	/// <summary>
	/// Keeps connections to planet
	/// </summary>
	void OnEnable() {
		if(planet == null)
			planet = GetComponent<Planet>();
		
		if(planet != null)
			planet.SurfaceGenerated += OnSurfaceGenerated;

		terrainModule = TerrainModule.LoadTextAsset(foliageModule, false, 0f, 1f);

		GenerateImpostors();
	}

	[ContextMenu("Generate Impostors")]
	private void GenerateImpostors() {
		for(int i = 0; i < foliageTypes.Length; i++) {
			foliageTypes[i].squaredDrawDistance = foliageTypes[i].drawDistance * foliageTypes[i].drawDistance;
			foliageTypes[i].squaredImpostorDistance = foliageTypes[i].impostorDistance * foliageTypes[i].impostorDistance;
			
			if(foliageTypes[i].generateMesh) {
				if(foliageTypes[i].impostor == null) {
					foliageTypes[i].mesh = ImpostorGenerator.CreateMesh(foliageTypes[i].meshSize);
					//impostorMaterial = new Material(Shader.Find("Transparent/Diffuse"));
				}
			}	
			else {
				if(foliageTypes[i].impostor == null && foliageTypes[i].generateImpostor) {
					foliageTypes[i].impostor = ImpostorGenerator.GenerateImpostor(foliageTypes[i].impostorSize, foliageTypes[i].impostorTextureResolution, foliageTypes[i].impostorScaleOnTexture, foliageTypes[i].mesh, foliageTypes[i].materials, 7);
					//impostorMaterial = new Material(Shader.Find("Transparent/Diffuse"));
					foliageTypes[i].impostorMaterial.SetTexture("_MainTex", foliageTypes[i].impostor.texture);
				}
			}
		}
	}

	#endregion

	#region Rendering

	public void Update() {
		for(int f = 0; f < foliageTypes.Length; f++) {
			for(int i = 0; i < foliageTypes[f].meshLists.Count; i++) {

				// test whether to draw model or impostor
				for(int m = 0; m < foliageTypes[f].meshLists[i].meshInstances.Count; m++) {
					MeshInstance mi = foliageTypes[f].meshLists[i].meshInstances[m];

					if(foliageTypes[f].generateMesh) {
						float squaredDistance = 0f;
						if(foliageTypes[f].calculateDistance)
							squaredDistance = (lodTarget.position - mi.position).sqrMagnitude;

						if(squaredDistance < foliageTypes[f].squaredDrawDistance)
							for(int c = 0 ; c < cameras.Length; c++)
								Graphics.DrawMesh(foliageTypes[f].mesh, mi.matrix, foliageTypes[f].materials[0], mi.layer, cameras[c], 0, mi.mpb, false, false);
					} 
					else {
						float squaredDistance = (lodTarget.position - mi.position).sqrMagnitude;
						if(squaredDistance < foliageTypes[f].squaredDrawDistance) {
							Material submeshMaterial;
							for(int s = 0; s < mi.mesh.subMeshCount; s++) {
								if(s < foliageTypes[f].materials.Length)
									submeshMaterial = foliageTypes[f].materials[s];
								else
									submeshMaterial = foliageTypes[f].materials[foliageTypes[f].materials.Length-1];
								for(int c = 0 ; c < cameras.Length; c++)
									Graphics.DrawMesh(mi.mesh, mi.matrix, submeshMaterial, mi.layer, cameras[c], s, mi.mpb, mi.castShadows, mi.receiveShadows);
							}
						}
						else if(foliageTypes[f].generateImpostor && foliageTypes[f].impostor != null) {
							if(squaredDistance < foliageTypes[f].squaredImpostorDistance) {
								for(int c = 0 ; c < cameras.Length; c++)
									Graphics.DrawMesh(foliageTypes[f].impostor.mesh, mi.matrix, foliageTypes[f].impostorMaterial, mi.layer, cameras[c], 0, mi.mpb, false, false);
							}
						}
					}
				}
			}
		}
	}
	
	#endregion
	
	#region ObjectPlacement

	/// <summary>
	/// Event raised by the planet when a new surface is generated.
	/// If enabled, adds SurfaceObjects component to the Surface and populates it with objects
	/// </summary>
	public void OnSurfaceGenerated(Surface surface) {
		for(int f = 0; f < foliageTypes.Length; f++) {
			if(surface.lodLevel == foliageTypes[f].lodLevel) {
				// create new list
				MeshInstanceList mil = new MeshInstanceList();
				mil.meshInstances = new List<MeshInstance>();
	
				// save surface reference and listen for its destruction
				mil.surface = surface;
				surface.SurfaceDestroyed += SurfaceDestroyed;
	
				// get mesh
				Vector3[] vertices = surface.mesh.vertices;
				Vector3[] normals = surface.mesh.normals;
				Color[] colors = surface.mesh.colors; 

				Vector3 right = Vector3.zero, perpendicular = Vector3.one, normalized = Vector3.zero;
				float vertexDistance = 0f;	

				for(int i = 0; i < vertices.Length; i++) {
					if(i + 1 < vertices.Length) {
						right = vertices[i] - vertices[i+1];
						vertexDistance = right.magnitude;
						right.Normalize();
					}

					if(colors[i].r >= foliageTypes[f].minHeight && colors[i].r <= foliageTypes[f].maxHeight && 
					   colors[i].g >= foliageTypes[f].minPolarity && colors[i].g <= foliageTypes[f].maxPolarity &&
					   colors[i].b >= foliageTypes[f].minSlope && colors[i].b <= foliageTypes[f].maxSlope ) {
						float terrainValue = terrainModule.module.GetValue(vertices[i].normalized);
						if(terrainValue >= foliageTypes[f].minNoiseValue && terrainValue <= foliageTypes[f].maxNoiseValue) {
							MeshInstance newInstance = new MeshInstance();

							Vector3 vertex = planet.transform.TransformPoint(vertices[i]);
							Vector3 normal = planet.transform.TransformDirection(normals[i]);
	
							/*if(foliageTypes[f].positionVariation != Vector3.zero) {
								normalized = vertices[i].normalized;
								perpendicular = Vector3.Cross(right, normalized);
								newInstance.position += right * vertexDistance * Random.Range(-foliageTypes[f].positionVariation.x, foliageTypes[f].positionVariation.x);
								newInstance.position += normalized * vertexDistance * Random.Range(-foliageTypes[f].positionVariation.y, foliageTypes[f].positionVariation.y);
								newInstance.position += perpendicular * vertexDistance * Random.Range(-foliageTypes[f].positionVariation.z, foliageTypes[f].positionVariation.z);
							}*/
							
							Quaternion adjustedRotation;
							if(foliageTypes[f].useGroundNormalAsUp)
								adjustedRotation = Quaternion.LookRotation(normal);
							else
								adjustedRotation = Quaternion.LookRotation(vertex.normalized);
							adjustedRotation *= Quaternion.Euler(foliageTypes[f].rotation);
							adjustedRotation *= Quaternion.Euler(new Vector3(Random.Range(-foliageTypes[f].rotationVariation.x, foliageTypes[f].rotationVariation.x),
							                                                 Random.Range(-foliageTypes[f].rotationVariation.y, foliageTypes[f].rotationVariation.y),
							                                                 Random.Range(-foliageTypes[f].rotationVariation.z, foliageTypes[f].rotationVariation.z)));

							Vector3 adjustedScale = foliageTypes[f].scale * (1f + foliageTypes[f].scaleVariation * Random.Range(0f, 1f));
							
							newInstance.position = vertex;
							newInstance.scale = adjustedScale;
							
							newInstance.matrix = new Matrix4x4();
							newInstance.matrix.SetTRS(newInstance.position, adjustedRotation, adjustedScale);
	
							newInstance.mesh = foliageTypes[f].mesh;
							newInstance.materials = foliageTypes[f].materials;
							newInstance.layer = foliageTypes[f].meshLayer;
							newInstance.castShadows = foliageTypes[f].castShadows;
							newInstance.receiveShadows = foliageTypes[f].receiveShadows;
	
							mil.meshInstances.Add(newInstance);
						}
					}
				}
	
				// add list to other lists
				foliageTypes[f].meshLists.Add(mil);
			}
		}
	}

	/// <summary>
	/// Event raised by Surface when its being destroyed
	/// </summary>
	public void SurfaceDestroyed(Surface surface) {
		for(int f = 0; f < foliageTypes.Length; f++) {
			MeshInstanceList toBeRemoved = null;
			for(int i = 0; i < foliageTypes[f].meshLists.Count; i++) {
				if(foliageTypes[f].meshLists[i].surface == surface) {
					toBeRemoved = foliageTypes[f].meshLists[i];
					break;
				}
			}
			if(toBeRemoved != null)
				foliageTypes[f].meshLists.Remove(toBeRemoved);
		}
	}

	[System.Serializable()]
	public class FoliageType {
		public float drawDistance = 5f;
		public float impostorDistance = 15f;
		public float squaredDrawDistance;
		public float squaredImpostorDistance;
		
		public Mesh mesh;
		public Material[] materials;
		public LayerMask meshLayer;
		public bool castShadows = false, receiveShadows = false;
		
		public Vector3 positionVariation = Vector3.zero;
		public Vector3 scale = Vector3.one, rotation = Vector3.zero;
		public float scaleVariation = 0.1f;
		public Vector3 rotationVariation = Vector3.zero;
		public bool useGroundNormalAsUp = true;
		
		public int lodLevel = 5;
		public float minNoiseValue = -1f, maxNoiseValue = 1f;
		public float minHeight = -1f, maxHeight = 0f;
		public float minPolarity = 0f, maxPolarity = 1f;
		public float minSlope = 0f, maxSlope = .5f;

		public bool generateMesh = false;
		public bool calculateDistance = false;
		public float meshSize = 1f;
		
		public bool generateImpostor = false;
		public int impostorTextureResolution = 256;
		public float impostorScaleOnTexture = 0.035f, impostorSize = 30f;
		public ImpostorGenerator.Impostor impostor;
		public Material impostorMaterial;
		public Texture2D impostorTexture;

		public List<MeshInstanceList> meshLists = new List<MeshInstanceList>();

		public FoliageType() {

		}
	}

	public class MeshInstance {
		public Vector3 position, scale;
		public Matrix4x4 matrix;
		public Mesh mesh;
		public Material[] materials;
		public LayerMask layer;
		public MaterialPropertyBlock mpb;
		public bool castShadows = false;
		public bool receiveShadows = false;

		public MeshInstance() {
			mpb = new MaterialPropertyBlock();
		}
	}

	public class MeshInstanceList {
		public List<MeshInstance> meshInstances;
		public Surface surface;
	}

	#endregion
}

}

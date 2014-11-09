using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Planetary {

public class Surface : MonoBehaviour 
{
	// dimensions of the surface
	public Vector3 topleft, bottomright, topright, bottomleft; 

	// mesh
	public Mesh mesh;
	public bool generated = false;

	// collider
	private new MeshCollider collider;
	
	// reference to planet and parent surface
	public Planet planet;
	public Surface parent;
	public Planet.SurfacePosition surfacePosition;
	public int subX, subY;
	
	// LOD
	public int lodLevel = 0;
	public bool queuedForSubdivision = false;
	
	public delegate void SurfaceDelegate(Surface s);
	public event SurfaceDelegate GenerationComplete, SubdivisionComplete, SurfaceDestroyed;

	// modifier
	public float modifierStartX, modifierStartY;
	public float modifierResolution, modifierMultiplier;

	// uv
	public float uvStartX, uvStartY, uvResolution;

	#region Initialization
	
	/// <summary>
	/// Initializes the surface, called by the genetor to pass parameters
	/// </summary>
	public void Initialize(int level, Vector3 start, Vector3 end, Vector3 topRight, Vector3 bottomLeft, Planet planet, Planet.SurfacePosition sp, Surface parent, int sx, int sy) {
		// save reference
		this.planet = planet;
		this.surfacePosition = sp;
		this.parent = parent;
		this.subX = sx;
		this.subY = sy;

		// surface start and end points
		this.topleft = start;
		this.bottomright = end;
		this.topright = topRight;
		this.bottomleft = bottomLeft;

		// lod subdivision level
		lodLevel = level;

		// modifier starting point 
		modifierStartX = 0;
		modifierStartY = 0;
		if(lodLevel == 0) {
			modifierResolution = planet.modifierResolution / (float)planet.subdivisions;
		}
		else {
			modifierResolution = parent.modifierResolution / 2f;
			modifierStartX += parent.modifierStartX;
			modifierStartY += parent.modifierStartY;
		}

		modifierStartX += subX * modifierResolution;
		modifierStartY += subY * modifierResolution;

		modifierMultiplier = modifierResolution / planet.meshResolution;

		// uv
		uvStartX = 0f;
		uvStartY = 0f;
		if(lodLevel == 0) {
			uvResolution = 1f / planet.subdivisions;
		}
		else {
			uvResolution = parent.uvResolution / 2f;
			uvStartX += parent.uvStartX;
			uvStartY += parent.uvStartY;
		}
		uvStartX += subX * uvResolution;
		uvStartY += subY * uvResolution;

		// corners
		subSurfaces = new List<Surface>();
		if(planet.useLod) {
			/*topLeftCorner = SperifyPoint(topleft) * planet.radius;
			bottomRightCorner = SperifyPoint(bottomright) * planet.radius;
			
			middlePoint = SperifyPoint((start + end) / 2f) * planet.radius;

			topRightCorner = SperifyPoint(topright) * planet.radius;
			bottomLeftCorner = SperifyPoint(bottomleft) * planet.radius;*/

			// TOP LEFT
			topLeftCorner = SperifyPoint(topleft);
			float displacement = planet.Terrain.module.GetValue(topLeftCorner);
			float rx = modifierStartX + 0 * modifierMultiplier;
			float cy = modifierStartY + 0 * modifierMultiplier;
			if(planet.useBicubicInterpolation)
				displacement += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
			else
				displacement += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);
			topLeftCorner += topLeftCorner * displacement * planet.heightVariation;
			topLeftCorner *= planet.radius;
			
			// BOTTOM RIGHT
			bottomRightCorner = SperifyPoint(bottomright);
			displacement = planet.Terrain.module.GetValue(bottomRightCorner);
			rx = modifierStartX + planet.meshResolution * modifierMultiplier;
			cy = modifierStartY + planet.meshResolution * modifierMultiplier;
			if(planet.useBicubicInterpolation)
				displacement += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
			else
				displacement += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);
			bottomRightCorner += bottomRightCorner * displacement * planet.heightVariation;
			bottomRightCorner *= planet.radius;
			
			// MIDDLE POINT
			middlePoint = SperifyPoint((start + end) / 2f);
			displacement = planet.Terrain.module.GetValue(middlePoint);
			rx = modifierStartX + (planet.meshResolution/2) * modifierMultiplier;
			cy = modifierStartY + (planet.meshResolution/2) * modifierMultiplier;
			if(planet.useBicubicInterpolation)
				displacement += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
			else
				displacement += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);
			middlePoint += middlePoint * displacement * planet.heightVariation;
			middlePoint *= planet.radius;
			
			// TOP RIGHT
			topRightCorner = SperifyPoint(topright);
			displacement = planet.Terrain.module.GetValue(topRightCorner);
			rx = modifierStartX + planet.meshResolution * modifierMultiplier;
			cy = modifierStartY + 0 * modifierMultiplier;
			if(planet.useBicubicInterpolation)
				displacement += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
			else
				displacement += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);
			topRightCorner += topRightCorner * displacement * planet.heightVariation;
			topRightCorner *= planet.radius;
			
			// BOTTOM LEFT
			bottomLeftCorner = SperifyPoint(bottomleft);
			displacement = planet.Terrain.module.GetValue(bottomLeftCorner);
			rx = modifierStartX + planet.meshResolution * modifierMultiplier;
			cy = modifierStartY + 0 * modifierMultiplier;
			if(planet.useBicubicInterpolation)
				displacement += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
			else
				displacement += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);
			bottomLeftCorner += bottomLeftCorner * displacement * planet.heightVariation;
			bottomLeftCorner *= planet.radius;

			/*Transform sphere = (Transform)Resources.Load("Sphere", typeof(Transform));
			Transform newSphere = (Transform)Instantiate(sphere, planet.transform.TransformPoint(topLeftCorner), Quaternion.identity);
			newSphere.parent = transform;
			newSphere = (Transform)Instantiate(sphere, bottomRightCorner, Quaternion.identity);
			newSphere.parent = transform;
			newSphere = (Transform)Instantiate(sphere, middlePoint, Quaternion.identity);
			newSphere.parent = transform;
			newSphere = (Transform)Instantiate(sphere, topRightCorner, Quaternion.identity);
			newSphere.parent = transform;
			newSphere = (Transform)Instantiate(sphere, bottomLeftCorner, Quaternion.identity);
			newSphere.parent = transform;*/
		}

		// mesh
		if(mesh == null) {
			// create mesh filter
			MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
			mesh = meshFilter.sharedMesh = new Mesh();
				
			// create mesh renderer
			gameObject.AddComponent(typeof(MeshRenderer));
			renderer.castShadows = true;
			renderer.receiveShadows = true;
			renderer.enabled = true;
		}
	}
	
	#endregion
	
	#region Generation

	public List<Texture2D> textures = null;

	/// <summary>
	/// Creates a mesh with the given amount of detail (vertices)
	/// </summary>
	/// <param name="detail">
	/// A <see cref="System.Int32"/>
	/// </param>
	public void GenerateMesh(int detail) {
		if(planet.Terrain.module == null)
			planet.LoadModule();

		// mesh data
		Vector3[] vertices = null;
		Color[] vertexColors = null;
		Vector2[] uv1s = null, uv2s = null;
		Vector3[] normals = null;
		int[] indexbuffer = null;

		// texture data
		int textureDetail = planet.textureResolution;
		int textureCount = 0;
		for(int i = 0; i < planet.textureNodeInstances.Length; i++)
			textureCount++;
		Color32[][] colors = null;
		
		// Action to be called when generation has finished, applies mesh data
		Action ApplyMesh = () => {
			if(this != null) {
				if(mesh != null) {
					// create mesh
					mesh.vertices = vertices;
					mesh.colors = vertexColors;
					mesh.triangles = indexbuffer;
					mesh.uv = uv1s;
					if(planet.uv2)
						mesh.uv2 = uv2s;
					mesh.normals = normals;
					//mesh.RecalculateNormals();
					mesh.RecalculateBounds();
					TangentSolverPT.Solve(mesh);
				}
				
				if(this != null) {
					transform.localPosition = Vector3.zero;
					transform.localRotation = Quaternion.identity;
					transform.position = transform.parent.position;
					transform.rotation = transform.parent.rotation;
				}
				
				// set mesh to collider
				if(planet.generateColliders[lodLevel]) {
					collider = (MeshCollider)gameObject.GetComponent(typeof(MeshCollider));
					if(collider == null)
						collider = (MeshCollider)gameObject.AddComponent(typeof(MeshCollider));
					collider.sharedMesh = mesh;	

					// listen to higher level surafces generating colliders
					planet.ColliderGenerated += OnColliderGenerated;
				}

				// report to planet
				planet.ReportGeneration(this);
				
				// event to parent surface
				if(GenerationComplete != null)
					GenerationComplete(this);

				generated = true;
			}
		};
		
		Action ApplyTextures = () => {
			if(this != null) {
				textures = new List<Texture2D>();
				int ii = 0;
				for(int i = 0; i < planet.textureNodeInstances.Length; i++) {
					if(planet.textureNodeInstances[i].generateSurfaceTextures) {
						Texture2D tex = new Texture2D(textureDetail, textureDetail);
						tex.wrapMode = TextureWrapMode.Clamp;
						textures.Add(tex);
						textures[ii].SetPixels32(colors[i]);
						textures[ii].Apply();
		
						if(Application.isPlaying)
							renderer.material.SetTexture(planet.textureNodeInstances[i].materialPropertyName, textures[ii]);
	
						ii++;
					}
				}
			}
		};
		
		if(planet.useLod && Application.isPlaying) {
		    // calculate on another thread
		    ThreadScheduler.RunOnThread(()=>{
				CalculateGeometry(detail, out vertices, out vertexColors, out uv1s, out uv2s, out normals, out indexbuffer);
				
				if(textureCount > 0)
					GenerateTextures(textureDetail, out colors);
				
				ThreadScheduler.RunOnMainThread(ApplyMesh);

				if(textureCount > 0)
					ThreadScheduler.RunOnMainThread(ApplyTextures);
		    });
		}
		else {
			// no LOD or in editor, generate in main thread
			CalculateGeometry(detail, out vertices, out vertexColors, out uv1s, out uv2s, out normals, out indexbuffer);
			if(textureCount > 0) {
				GenerateTextures(textureDetail, out colors);
				ApplyTextures();
			}
			ApplyMesh();
		}
	}
	
	/// <summary>
	/// Calculates the mesh vertices etc
	/// </summary>
	private void CalculateGeometry(int detail, out Vector3[] vertices, out Color[] vertexColors, out Vector2[] uv1s, out Vector2[] uv2s, out Vector3[] normals, out int[] indexbuffer) {
		
		// measure execution time
		System.DateTime startTime = System.DateTime.UtcNow;
		
		// downward facing vertices at border of the surface to prevent seams appearing
		int borders = 0;
		if(planet.createBorders)
			borders = detail * 4;
		
		// vertex array
		vertices = new Vector3[detail * detail + borders];
		
		// vertex colours, to store data in
		vertexColors = new Color[detail * detail + borders];
		
		// calculate interpolation between coordinates
		float stepX = (bottomright.x - topleft.x) / (detail-1);
		float stepY = (bottomright.y - topleft.y) / (detail-1);
		float stepZ = (bottomright.z - topleft.z) / (detail-1);
		
		// check which axis remains stationary
		bool staticX = false, staticY = false, staticZ = false;
		if(stepX == 0)
			staticX = true;
		if(stepY == 0)
			staticY = true;
		if(stepZ == 0)
			staticZ = true;
		
		// normals
		normals = new Vector3[detail * detail + borders];
		Vector3 line1 = Vector3.zero;
		Vector3 line2 = Vector3.zero;
		
		// uvs
		uv1s = new Vector2[detail * detail + borders];
		if(planet.uv2)
			uv2s = new Vector2[detail * detail + borders];
		else
			uv2s = null;
		
		// indices
		int indexCount = (detail-1) * (detail-1) * 6;
		if(planet.createBorders)
			indexCount += (borders-4) * 6;
		indexbuffer = new int[indexCount];
		int index = 0;
		
		// plot mesh geometry
		for(int col = 0; col < detail; col++) {
			for(int row = 0; row < detail; row++) {
				// set vertex position
				if(staticX)
					vertices[col * detail + row] = new Vector3(topleft.x, topleft.y + stepY * col, topleft.z + stepZ * row);
				if(staticY)
					vertices[col * detail + row] = new Vector3(topleft.x + stepX * row, topleft.y, topleft.z + stepZ * col);
				if(staticZ)
					vertices[col * detail + row] = new Vector3(topleft.x + stepX * row, topleft.y + stepY * col, topleft.z);
					
				// map the point on to the sphere
				vertices[col * detail + row] = SperifyPoint(vertices[col * detail + row]);

				// calculate noise displacement
				float displacement = planet.Terrain.module.GetValue(vertices[col * detail + row]);

				// take user painted values to account
				float rx = modifierStartX + row * modifierMultiplier;
				float cy = modifierStartY + col * modifierMultiplier;
				if(planet.useBicubicInterpolation)
					displacement += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
				else
					displacement += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);

				// displace vertex position
				vertices[col * detail + row] += vertices[col * detail + row] * displacement * planet.heightVariation;

				// calculate uv's
				switch(planet.uv1type) {
				case Planet.UV.SPHERICAL:
					// spherical
					uv1s[col * detail + row] = GetSphericalUv(detail, col, row, vertices[col * detail + row], staticX, staticY);
					break;
				case Planet.UV.CONTINUOUS:
					// continuous uv's
					uv1s[col * detail + row] = new Vector2(uvStartX + ((float)row / (detail-1)) * uvResolution, uvStartY + ((float)col / (detail-1)) * uvResolution);
					//uvs[col * detail + row] = new Vector2((float)row / detail, (float)col / detail);
					break;
				case Planet.UV.SURFACE:
					uv1s[col * detail + row] = new Vector2((float)row / detail, (float)col / detail);
					break;
				}
				if(planet.uv2) {
					switch(planet.uv2type) {
					case Planet.UV.SPHERICAL:
						// spherical
						uv2s[col * detail + row] = GetSphericalUv(detail, col, row, vertices[col * detail + row], staticX, staticY);
						break;
					case Planet.UV.CONTINUOUS:
						// continuous uv's
						uv2s[col * detail + row] = new Vector2(uvStartX + ((float)row / (detail-1)) * uvResolution, uvStartY + ((float)col / (detail-1)) * uvResolution);						//uvs[col * detail + row] = new Vector2((float)row / detail, (float)col / detail);
						break;
					case Planet.UV.SURFACE:
						uv2s[col * detail + row] = new Vector2((float)row / detail, (float)col / detail);
						break;
					}
				}

				// scale to planet radius
				vertices[col * detail + row] *= planet.radius;

				// calculate triangle indexes
				if(col < detail -1 && row < detail - 1) {		
					indexbuffer[index] = (col * detail + row);
					index++;
					indexbuffer[index] = (col + 1) * detail + row;
					index++;
					indexbuffer[index] = col * detail + row + 1;
					index++;
					
					indexbuffer[index] = (col + 1) * detail + row;
					index++;
					indexbuffer[index] = (col + 1) * detail + row + 1;
					index++;
					indexbuffer[index] = col * detail + row + 1;
					index++;
				}

				// CALCULATE NORMALS
				int previousCol = col - 1;
				int previousRow = row - 1;
				if(previousCol >= 0) {
					line1.x = vertices[col * detail + row].x - vertices[previousCol * detail + row].x;
					line1.y = vertices[col * detail + row].y - vertices[previousCol * detail + row].y;
					line1.z = vertices[col * detail + row].z - vertices[previousCol * detail + row].z;
				}
				else {
					Vector3 previous = Vector3.zero;
					
					if(staticX)
						previous = new Vector3(topleft.x, topleft.y - stepY, topleft.z + stepZ * row);
					if(staticY)
						previous = new Vector3(topleft.x + stepX * row, topleft.y, topleft.z - stepZ);
					if(staticZ)
						previous = new Vector3(topleft.x + stepX * row, topleft.y - stepY, topleft.z);
				
					previous = SperifyPoint(previous);
					float disp = planet.Terrain.module.GetValue(previous);

					rx = modifierStartX + row * modifierMultiplier;
					cy = modifierStartY + previousCol * modifierMultiplier;
					if(planet.useBicubicInterpolation)
						disp += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
					else
						disp += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);

					previous += previous * planet.heightVariation * disp;
					previous *= planet.radius;
					
					line1.x = vertices[col * detail + row].x - previous.x;
					line1.y = vertices[col * detail + row].y - previous.y;
					line1.z = vertices[col * detail + row].z - previous.z;
				}
				
				if(previousRow >= 0) {
					line2.x = vertices[col * detail + row].x - vertices[col * detail + previousRow].x;
					line2.y = vertices[col * detail + row].y - vertices[col * detail + previousRow].y;
					line2.z = vertices[col * detail + row].z - vertices[col * detail + previousRow].z;
				}
				else {
					Vector3 previous = Vector3.zero;
					
					if(staticX)
						previous = new Vector3(topleft.x, topleft.y + stepY * col, topleft.z - stepZ);
					if(staticY)
						previous = new Vector3(topleft.x - stepX, topleft.y, topleft.z + stepZ * col);
					if(staticZ)
						previous = new Vector3(topleft.x - stepX, topleft.y + stepY * col, topleft.z);
					
					previous = SperifyPoint(previous);
					float disp = planet.Terrain.module.GetValue(previous);

					rx = modifierStartX + previousRow * modifierMultiplier;
					cy = modifierStartY + col * modifierMultiplier;
					if(planet.useBicubicInterpolation)
						disp += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
					else
						disp += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);

					previous += previous * planet.heightVariation * disp;
					previous *= planet.radius;
					
					line2.x = vertices[col * detail + row].x - previous.x;
					line2.y = vertices[col * detail + row].y - previous.y;
					line2.z = vertices[col * detail + row].z - previous.z;
				}

				normals[col * detail + row] = Vector3.Cross(line1, line2);
				normals[col * detail + row].Normalize();

				// calculate slope
				float slope = Vector3.Dot(normals[col * detail + row], -vertices[col * detail + row].normalized) + 1.0f;
				
				// store data to vertex color:, r = height, g = polar, b = slope, a = unused
				vertexColors[col * detail + row] = new Color((displacement + 1f) / 2f, Mathf.Abs(vertices[col * detail + row].normalized.y), slope, 0f);
			}
		}

		// borders
		if(planet.createBorders) {
			float borderDepth = -0.025f;
			int vertexCount = detail * detail;
			for(int col = 0; col < detail; col++) {
				int row = 0;
				vertices[vertexCount] = vertices[col * detail + row] + vertices[col * detail + row].normalized * planet.radius * borderDepth;
				normals[vertexCount] = normals[col * detail + row];
				uv1s[vertexCount] = uv1s[col * detail + row];
				if(planet.uv2)
					uv2s[vertexCount] = uv2s[col * detail + row];
				vertexColors[vertexCount] = vertexColors[col * detail + row];
				
				// calculate triangle indexes
				if(col < detail-1) {		
					indexbuffer[index] = (col * detail + row);
					index++;
					indexbuffer[index] = vertexCount;
					index++;
					indexbuffer[index] = (col + 1) * detail + row;
					index++;
						
					indexbuffer[index] = (col + 1) * detail + row;
					index++;
					indexbuffer[index] = vertexCount;
					index++;
					indexbuffer[index] = vertexCount + 1;
					index++;
				}
				
				vertexCount++;
			}
			for(int row = 0; row < detail; row++) {
				int col = 0;
				vertices[vertexCount] = vertices[col * detail + row] + vertices[col * detail + row].normalized * planet.radius * borderDepth;
				normals[vertexCount] = normals[col * detail + row];
				uv1s[vertexCount] = uv1s[col * detail + row];
				if(planet.uv2)
					uv2s[vertexCount] = uv2s[col * detail + row];
				vertexColors[vertexCount] = vertexColors[col * detail + row];
	
				// calculate triangle indexes
				if(row < detail-1) {		
					indexbuffer[index] = (col * detail + row);
					index++;
					indexbuffer[index] = col * detail + row + 1;
					index++;
					indexbuffer[index] = vertexCount;
					index++;
						
					indexbuffer[index] = vertexCount;
					index++;
					indexbuffer[index] = col * detail + row + 1;
					index++;
					indexbuffer[index] = vertexCount + 1;
					index++;
				}
				
				vertexCount++;
			}
			
			for(int col = 0; col < detail; col++) {
				int row = detail-1;
				vertices[vertexCount] = vertices[col * detail + row] + vertices[col * detail + row].normalized * planet.radius * borderDepth;
				normals[vertexCount] = normals[col * detail + row];
				uv1s[vertexCount] = uv1s[col * detail + row];
				if(planet.uv2)
					uv2s[vertexCount] = uv2s[col * detail + row];
				vertexColors[vertexCount] = vertexColors[col * detail + row];
	
				// calculate triangle indexes
				if(col < detail-1) {		
					indexbuffer[index] = (col * detail + row);
					index++;
					indexbuffer[index] = (col + 1) * detail + row;
					index++;
					indexbuffer[index] = vertexCount;
					index++;
						
					indexbuffer[index] = (col + 1) * detail + row;
					index++;
					indexbuffer[index] = vertexCount + 1;
					index++;
					indexbuffer[index] = vertexCount;
					index++;
				}
				
				vertexCount++;
			}
			for(int row = 0; row < detail; row++) {
				int col = detail-1;
				vertices[vertexCount] = vertices[col * detail + row] + vertices[col * detail + row].normalized * planet.radius * borderDepth;
				normals[vertexCount] = normals[col * detail + row];
				uv1s[vertexCount] = uv1s[col * detail + row];
				if(planet.uv2)
					uv2s[vertexCount] = uv2s[col * detail + row];
				vertexColors[vertexCount] = vertexColors[col * detail + row];
				
				// calculate triangle indexes
				if(row < detail-1) {		
					indexbuffer[index] = (col * detail + row);
					index++;
					indexbuffer[index] = vertexCount;
					index++;
					indexbuffer[index] = col * detail + row + 1;
					index++;
						
					indexbuffer[index] = vertexCount;
					index++;
					indexbuffer[index] = vertexCount + 1;
					index++;
					indexbuffer[index] = col * detail + row + 1;
					index++;
				}
				
				vertexCount++;
			}
		}
		
		if(planet.logGenerationTimes)
			Debug.Log("Mesh calculated in " + (float)(System.DateTime.UtcNow - startTime).TotalSeconds * 1000f + "ms");
	}

	/// <summary>
	/// Generates the textures
	/// </summary>
	private void GenerateTextures(int detail, out Color32[][] colors) {
		
		// measure execution time
		System.DateTime startTime = System.DateTime.UtcNow;
		
		// texture & colors
		int textureCount = 0;
		for(int i = 0; i < planet.textureNodeInstances.Length; i++) {
			if(planet.textureNodeInstances[i].generateSurfaceTextures)
				textureCount++;
		}
		colors = new Color32[textureCount][];
		ModuleBase[] textureModules = new ModuleBase[textureCount];

		int ii = 0;
		for(int i = 0; i < planet.textureNodeInstances.Length; i++) {
			if(planet.textureNodeInstances[i].generateSurfaceTextures) {
				colors[ii] = new Color32[detail * detail];
				textureModules[ii] = planet.textureNodeInstances[i].node.GetModule();
				ii++;
			}
		}
		
		// calculate interpolation between coordinates
		float stepX = (bottomright.x - topleft.x) / (detail-1);
		float stepY = (bottomright.y - topleft.y) / (detail-1);
		float stepZ = (bottomright.z - topleft.z) / (detail-1);
		
		// check which axis remains stationary
		bool staticX = false, staticY = false, staticZ = false;
		if(stepX == 0)
			staticX = true;
		if(stepY == 0)
			staticY = true;
		if(stepZ == 0)
			staticZ = true;

		Vector3 position = Vector3.zero;
		
		// loop through pixels
		for(int col = 0; col < detail; col++) {
			for(int row = 0; row < detail; row++) {
				// set vertex position
				if(staticX)
					position = new Vector3(topleft.x, topleft.y + stepY * col, topleft.z + stepZ * row);
				if(staticY)
					position = new Vector3(topleft.x + stepX * row, topleft.y, topleft.z + stepZ * col);
				if(staticZ)
					position = new Vector3(topleft.x + stepX * row, topleft.y + stepY * col, topleft.z);
				
				// map the point on to the sphere
				position = SperifyPoint(position);
				
				// displace with noise values
				//float displacement = planet.Terrain.module.GetValue(position, -position);
				
				// take painted heights to account
				/*float rx = modifierStartX + row * modifierMultiplier;
				float cy = modifierStartY + col * modifierMultiplier;
				if(planet.useBicubicInterpolation)
					displacement += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
				else
					displacement += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);*/
				
				// get color for each texture
				ii = 0;
				for(int i = 0; i < planet.textureNodeInstances.Length; i++) {
					if(planet.textureNodeInstances[i].generateSurfaceTextures) {
						colors[ii][col * detail + row] = textureModules[i].GetColor(position);
						ii++;
					}
				}
			}
		}

		if(planet.logGenerationTimes)
			Debug.Log("Texture generated in " + (float)(System.DateTime.UtcNow - startTime).TotalSeconds * 1000f + "ms");
	}
	
	/// <summary>
	/// Takes a point in the cube and plots it in to a sphere.
	/// Coordinates must be in the range of -1 to 1
	/// </summary>
	public static Vector3 SperifyPoint(Vector3 point) {
		float dX2, dY2, dZ2;
		float dX2Half, dY2Half, dZ2Half;
		
		dX2 = point.x * point.x;
		dY2 = point.y * point.y;
		dZ2 = point.z * point.z;
		
		dX2Half = dX2 * 0.5f;
		dY2Half = dY2 * 0.5f;
		dZ2Half = dZ2 * 0.5f;
		
		point.x = point.x * Mathf.Sqrt(1f - dY2Half - dZ2Half + (dY2 * dZ2) * (1f / 3f));
		point.y = point.y * Mathf.Sqrt(1f - dZ2Half - dX2Half + (dZ2 * dX2) * (1f / 3f));
		point.z = point.z * Mathf.Sqrt(1f - dX2Half - dY2Half + (dX2 * dY2) * (1f / 3f));
		
		return point;
	}
	
	/// <summary>
	/// Destroys the generated mesh to free VBO's and then destroys the gameobject
	/// </summary>
	public void CleanAndDestroy() {
		if(this != null) {
			if(planet.generateColliders[lodLevel]) {
				// unsubscribe to collider generation event
				planet.ColliderGenerated -= OnColliderGenerated;
			}
			if(textures != null)
				for(int i = 0; i < textures.Count; i++)
					Destroy(textures[i]);

			if(this.mesh != null)
				DestroyImmediate(this.mesh, false);
			if(this.gameObject != null)
				DestroyImmediate(this.gameObject);
		}

		if(SurfaceDestroyed != null)
			SurfaceDestroyed(this);
	}

	private Vector2 GetSphericalUv(int detail, int col, int row, Vector3 vertex, bool staticX, bool staticY) {
		Vector2 uv = new Vector2();//new Vector2(Mathf.Atan2(vertex.z, vertex.x)/Mathf.PI*0.5f+0.5f, vertex.y*0.5f+0.5f);

		uv.x = -Mathf.Atan2(vertex.x, vertex.z) / (2f * Mathf.PI) + 0.5f;
		uv.y = Mathf.Asin(vertex.y) / Mathf.PI + .5f;

		// take care of edge wrap
		if (staticX) {
			if ( vertex.x<0 ) {
				if ( (row==detail-1) && vertex.z>-0.01f && vertex.z<0.01f) uv.x=0;
				if ( (row==0) && vertex.z>-0.01f && vertex.z<0.01f) uv.x=1;
			}
		} else if (staticY) {
			if ( vertex.y>0 ) {
				if ( (col==detail-1) && vertex.x<0 && vertex.z>-0.01f && vertex.z<0.01f ) uv.x=0;
				if ( (col==0) && vertex.x<0 && vertex.z>-0.01f && vertex.z<0.01f ) uv.x=1;
			} else {
				if ( (col==detail-1) && vertex.x<0 && vertex.z>-0.01f && vertex.z<0.01f ) uv.x=1;
				if ( (col==0) && vertex.x<0 && vertex.z<0.01f && vertex.z>-0.01f ) uv.x=0;
			}
		}
		return uv;
	}
	
	#endregion
	
	#region LOD

	// distance and angle from camera
	public float distance = 0f, angle = 0f, dot = 0f;
	
	// closest corner for culling
	public Vector3 closestCorner;
	public Vector3 topLeftCorner, topRightCorner, bottomLeftCorner, bottomRightCorner, middlePoint;
	
	// list of child surfaces
	public List<Surface> subSurfaces;
	public bool hasSubsurfaces = false;
	private int generatedCount = 0;

	// generation priority value
	public float priorityPenalty = 0f;
	
	/// <summary>
	/// Update
	/// </summary>
	public void UpdateLOD() {
		if(lodLevel < planet.maxLodLevel) {
			distance = GetClosestDistance();
			//distance = distance * (3f - (dot + 1f)) / 2f;
			CalculatePriority();
			
			if(!queuedForSubdivision) {
				if(!hasSubsurfaces && generated) {
					if(planet.useAngleCulling) {
						angle = Vector3.Angle((planet.transform.position - planet.lodTarget.position).normalized, (planet.transform.position - closestCorner).normalized);
						if(angle > planet.rendererCullingAngle)
							renderer.enabled = false;
						else
							renderer.enabled = true;
					}
					
					float lodDistance = planet.lodDistances[lodLevel];
					if(distance < lodDistance && dot >= planet.lodDots[lodLevel]) {
						planet.QueueForSubdivision(this);
						queuedForSubdivision = true;
					}
				}
				else {
					float lodDistance = planet.lodDistances[lodLevel] * 2f;
					if(distance > lodDistance || dot < planet.lodDots[lodLevel]) {
						UnloadSubsurfaces();
					}
					else {
						for(int i = 0; i < subSurfaces.Count; i++) {
							subSurfaces[i].UpdateLOD();
						}
					}
				}
			}
			else {
				// test if should be removed from queue
				if(distance > planet.lodDistances[lodLevel] || dot < planet.lodDots[lodLevel]) {
					planet.RemoveFromQueue(this);
					queuedForSubdivision = false;
				}
			}
		}
	}
	
	/// <summary>
	/// Calculates the priority penalty to be used in generation queue
	/// </summary>
	public void CalculatePriority() {
		if(distance == 0f)
			distance = GetClosestDistance();

		//priorityPenalty = 1f - distance / planet.lodDistances[lodLevel];
		//priorityPenalty += -dot; // dot
		//priorityPenalty -= (float)lodLevel / planet.maxLodLevel;
		//priorityPenalty += (1f - Vector3.Angle(planet.lodTarget.forward, (closestCorner - planet.lodTarget.position).normalized) / 180f) * .5f;
		//priorityPenalty -= lodLevel;

		priorityPenalty = distance;

		// view direction dot
		float viewDot = -Vector3.Dot(planet.lodTarget.forward, (closestCorner - planet.lodTarget.position).normalized);
		priorityPenalty += viewDot * distance;
	}
	
	/// <summary>
	/// Gets the closest distance.
	/// </summary>
	public float GetClosestDistance() {
		float closestDistance = Mathf.Infinity;
		float d = Vector3.Distance(planet.lodTarget.position, transform.TransformPoint(topLeftCorner));
		if(d < closestDistance) {
			closestDistance = d;
			closestCorner = transform.TransformPoint(topLeftCorner);
		}
		
		d = Vector3.Distance(planet.lodTarget.position, transform.TransformPoint(topRightCorner));
		if(d < closestDistance) {
			closestDistance = d;
			closestCorner = transform.TransformPoint(topRightCorner);
		}
		
		d = Vector3.Distance(planet.lodTarget.position, transform.TransformPoint(middlePoint));
		if(d < closestDistance) {
			closestDistance = d;
			closestCorner = transform.TransformPoint(middlePoint);
		}
		
		d = Vector3.Distance(planet.lodTarget.position, transform.TransformPoint(bottomLeftCorner));
		if(d < closestDistance) {
			closestDistance = d;
			closestCorner = transform.TransformPoint(bottomLeftCorner);
		}
		
		d = Vector3.Distance(planet.lodTarget.position, transform.TransformPoint(bottomRightCorner));
		if(d < closestDistance) {
			closestDistance = d;
			closestCorner = transform.TransformPoint(bottomRightCorner);
		}

		dot = Vector3.Dot((planet.lodTarget.position - planet.transform.position).normalized, (closestCorner - planet.transform.position).normalized);
		
		return closestDistance;
	}

	/// <summary>
	/// Creates the subsurfaces
	/// </summary>
	[ContextMenu("Create subsurfaces")]
	public void CreateSubsurfaces() {
		if(hasSubsurfaces)
			return;
		hasSubsurfaces = true;
		generatedCount = 0;
		
		// disable collider
		/*if(planet.disableCollidersOnSubdivision[lodLevel]) {
			DisableCollider();
		}*/
		
		// plot vertex positions for new surfaces
		int subdivisions = 2;
		Vector3 size = bottomright - topleft;
		Vector3 step = size / subdivisions;
		
		// check which axis remains stationary
		bool staticX = false, staticY = false, staticZ = false;
		if(step.x == 0)
			staticX = true;
		if(step.y == 0)
			staticY = true;
		if(step.z == 0)
			staticZ = true;
		
		for(int sY = 0; sY < subdivisions; sY++) {
			for(int sX = 0; sX < subdivisions; sX++) {
				// calculate start and end positions
				Vector3 subStart = Vector3.zero, subEnd = Vector3.zero;
				Vector3 subTopRight = Vector3.zero, subBottomLeft = Vector3.zero;
				
				if(staticX) {
					subStart = new Vector3(topleft.x, topleft.y + step.y * sY, topleft.z + step.z * sX);
					subEnd = new Vector3(topleft.x, topleft.y + step.y * (sY+1), topleft.z + step.z * (sX+1));
					
					subTopRight = new Vector3(topleft.x, topleft.y + step.y * sY, topleft.z + step.z * (sX+1));
					subBottomLeft = new Vector3(topleft.x, topleft.y + step.y * (sY+1), topleft.z + step.z * sX);
				}
				if(staticY) {
					subStart = new Vector3(topleft.x + step.x * sX, topleft.y, topleft.z + step.z * sY);
					subEnd = new Vector3(topleft.x + step.x * (sX+1), topleft.y, topleft.z + step.z * (sY+1));
					
					subTopRight = new Vector3(topleft.x + step.x * (sX+1), topleft.y, topleft.z + step.z * sY);
					subBottomLeft = new Vector3(topleft.x + step.x * sX, topleft.y, topleft.z + step.z * (sY+1));
				}
				if(staticZ) {
					subStart = new Vector3(topleft.x + step.x * sX, topleft.y + step.y * sY, topleft.z);
					subEnd = new Vector3(topleft.x + step.x * (sX+1), topleft.y + step.y * (sY+1), topleft.z);
					
					subTopRight = new Vector3(topleft.x + step.x * (sX+1), topleft.y + step.y * sY, topleft.z);
					subBottomLeft = new Vector3(topleft.x + step.x * sX, topleft.y + step.y * (sY+1), topleft.z);
				}
				
				// instantiate new surface
				GameObject t = new GameObject("SubSurf_" + "_Lod" + (lodLevel+1).ToString() + " " + surfacePosition.ToString());
				t.layer = gameObject.layer;
				t.tag = gameObject.tag;
				t.transform.parent = this.transform;
				Surface surface = t.AddComponent<Surface>();
				surface.Initialize(lodLevel + 1, subStart, subEnd, subTopRight, subBottomLeft, this.planet, this.surfacePosition, this, sX, sY);
				surface.GenerationComplete += SubsurfaceGenerationComplete;
				surface.GenerateMesh(planet.meshResolution);
				subSurfaces.Add(surface);
			}
		}

		// if not playing, hide this instantly
		if(!Application.isPlaying) {
			renderer.enabled = false;
		}
	}
	
	/// <summary>
	/// Unloads the subsurfaces
	/// </summary>
	public void UnloadSubsurfaces() {
		if(subSurfaces.Count > 0) {
			// exit if subsurfaces also have subsurfaces
			if(subSurfaces[0].hasSubsurfaces)
				return;

			// destroy subsurfaces
			for(int i = 0; i < subSurfaces.Count; i++) {
				subSurfaces[i].CleanAndDestroy();
			}
			subSurfaces.Clear();
			
			hasSubsurfaces = false;
			
			// enable collider
			if(planet.generateColliders[lodLevel]) {
				MeshCollider collider = GetComponent<MeshCollider>();
				if(collider != null)
					collider.enabled = true;
			}

			// enable renderer
			renderer.enabled = true;
		}
	}
	
	/// <summary>
	/// Event handler called by each subsurface when completed
	/// </summary>
	private void SubsurfaceGenerationComplete(Surface s) {
		if(this == null)
			return;
		
		generatedCount++;
		if(generatedCount == subSurfaces.Count) {
			renderer.enabled = false;
			
			if(SubdivisionComplete != null)
				SubdivisionComplete(this);
			
			queuedForSubdivision = false;
		}
	}

	/// <summary>
	/// Event handler for when other colliders are generated
	/// </summary>
	private void OnColliderGenerated(int level) {
		if(level > this.lodLevel && hasSubsurfaces) {
			DisableCollider();
		}
	}

	private void DisableCollider() {
		if(this != null) {
			if(collider == null)
				collider = GetComponent<MeshCollider>();
			if(collider != null)
				collider.enabled = false;
		}
	}
	
	#endregion
}

}
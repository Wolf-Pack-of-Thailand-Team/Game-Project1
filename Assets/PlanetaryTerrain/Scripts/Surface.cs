using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Surface : MonoBehaviour 
{
	// dimensions of the surface
	public Vector3 topleft, bottomright, topright, bottomleft; 

	// mesh
	public Mesh mesh;
	
	// reference to planet and parent surface
	public Planet planet;
	public Surface parent;
	public Planet.SurfacePosition surfacePosition;
	public int subX, subY;
	
	// LOD
	public int lodLevel = 0;
	public bool queuedForSubdivision = false;
	
	public delegate void SurfaceDelegate(Surface s);
	public event SurfaceDelegate GenerationComplete, SubdivisionComplete;

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
			topLeftCorner = SperifyPoint(topleft) * planet.radius;
			bottomRightCorner = SperifyPoint(bottomright) * planet.radius;
			
			middlePoint = SperifyPoint(((start + end) / 2f)) * planet.radius;

			topRightCorner = SperifyPoint(topRight) * planet.radius;
			bottomLeftCorner = SperifyPoint(bottomLeft) * planet.radius;
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
		Vector2[] uvs = null;
		Vector3[] normals = null;
		int[] indexbuffer = null;
		
		// Action to be called when generation has finished, applies mesh data
		Action ApplyMesh = () => {
			if(mesh != null) {
				// create mesh
				mesh.vertices = vertices;
				mesh.colors = vertexColors;
				mesh.triangles = indexbuffer;
				mesh.uv = uvs;
				mesh.normals = normals;
				//mesh.RecalculateNormals();
				mesh.RecalculateBounds();
			}
			
			if(this != null) {
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				transform.position = transform.parent.position;
				transform.rotation = transform.parent.rotation;
			}
			
			// set mesh to collider
			if(planet.generateColliders[lodLevel]) {
				MeshCollider collider = (MeshCollider)gameObject.GetComponent(typeof(MeshCollider));
				if(collider == null)
					collider = (MeshCollider)gameObject.AddComponent(typeof(MeshCollider));
				collider.sharedMesh = mesh;	
			}

			// report to planet
			planet.ReportGeneration(this);
			
			// event to parent surface
			if(GenerationComplete != null)
				GenerationComplete(this);
		};
		
		if(planet.useLod && Application.isPlaying) {
		    // calculate on another thread
		    ThreadScheduler.RunOnThread(()=>{
				CalculateGeometry(detail, out vertices, out vertexColors, out uvs, out normals, out indexbuffer);
		        ThreadScheduler.RunOnMainThread(ApplyMesh);
		    });
		}
		else {
			// no LOD or in editor, generate in main thread
			CalculateGeometry(detail, out vertices, out vertexColors, out uvs, out normals, out indexbuffer);
			ApplyMesh();
		}
	}
	
	/// <summary>
	/// Calculates the mesh vertices etc
	/// </summary>
	private void CalculateGeometry(int detail, out Vector3[] vertices, out Color[] vertexColors, out Vector2[] uvs, out Vector3[] normals, out int[] indexbuffer) {
		
		// measure execution time
		//System.DateTime startTime = System.DateTime.UtcNow;
		
		// downward facing vertices at border of the surface to prevent seams appearing
		int borders = detail * 4;
		
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
		uvs = new Vector2[detail * detail + borders];
		
		// indices
		indexbuffer = new int[(detail-1) * (detail-1) * 6 + (borders-4) * 6];
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

					
				// displace with noise values
				float displacement = planet.Terrain.module.GetValue(vertices[col * detail + row]);

				float rx = modifierStartX + row * modifierMultiplier;
				float cy = modifierStartY + col * modifierMultiplier;
				if(planet.useBicubicInterpolation)
					displacement += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
				else
					displacement += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);

				// displace vertex position
				vertices[col * detail + row] += vertices[col * detail + row] * displacement * planet.heightVariation;

				// store data to vertex color:, r = height, g = polar, b = unused
				vertexColors[col * detail + row] = new Color(displacement, Mathf.Abs(vertices[col * detail + row].y), 0f);
	
				// scale to planet radius
				vertices[col * detail + row] *= planet.radius;
				
				// calculate uv's
				uvs[col * detail + row] = new Vector2(uvStartX + ((float)(row+1) / detail) * uvResolution, uvStartY + ((float)(col+1) / detail) * uvResolution);

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
			}
		}

		// ALTERNATE CALCULATE NORMALS
		/*Vector3 A, B, C, D, E;
		Vector3 CA, CB, CD, CE;
		Vector3 normalACB, normalBCE, normalECD, normalDCA;
		Vector3 averageNormal;
		
		for(int col = 0; col < detail; col++) {
			for(int row = 0; row < detail; row++) {
				int previousCol = col - 1;
				int previousRow = row - 1;
				int nextCol = col + 1;
				int nextRow = row + 1;
				float rx, cy;
				
				// C
				C = vertices[col * detail + row];
				
				// A
				if(previousCol >= 0) {
					A = vertices[previousCol * detail + row];
				}
				else {
					A = Vector3.zero;
					
					if(staticX)
						A = new Vector3(topleft.x, topleft.y - stepY, topleft.z + stepZ * row);
					if(staticY)
						A = new Vector3(topleft.x + stepX * row, topleft.y, topleft.z - stepZ);
					if(staticZ)
						A = new Vector3(topleft.x + stepX * row, topleft.y - stepY, topleft.z);
					
					A = SperifyPoint(A);
					float disp = planet.Terrain.module.GetValue(A);
					
					rx = modifierStartX + row * modifierMultiplier;
					cy = modifierStartY + previousCol * modifierMultiplier;
					if(planet.useBicubicInterpolation)
						disp += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
					else
						disp += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);
					
					A += A * planet.heightVariation * disp;
					A *= planet.radius;
				}
				
				// B
				if(previousRow >= 0) {
					B = vertices[col * detail + previousRow];
				}
				else {
					B = Vector3.zero;
					
					if(staticX)
						B = new Vector3(topleft.x, topleft.y + stepY * col, topleft.z - stepZ);
					if(staticY)
						B = new Vector3(topleft.x - stepX, topleft.y, topleft.z + stepZ * col);
					if(staticZ)
						B = new Vector3(topleft.x - stepX, topleft.y + stepY * col, topleft.z);
					
					B = SperifyPoint(B);
					float disp = planet.Terrain.module.GetValue(B);
					
					rx = modifierStartX + previousRow * modifierMultiplier;
					cy = modifierStartY + col * modifierMultiplier;
					if(planet.useBicubicInterpolation)
						disp += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
					else
						disp += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);
					
					B += B * planet.heightVariation * disp;
					B *= planet.radius;
				}
				
				if(nextRow < detail) {
					D = vertices[col * detail + nextRow];
				}
				else {
					D = Vector3.zero;
					
					if(staticX)
						D = new Vector3(topleft.x, topleft.y + stepY * col, topleft.z - stepZ);
					if(staticY)
						D = new Vector3(topleft.x - stepX, topleft.y, topleft.z + stepZ * col);
					if(staticZ)
						D = new Vector3(topleft.x - stepX, topleft.y + stepY * col, topleft.z);
					
					D = SperifyPoint(D);
					float disp = planet.Terrain.module.GetValue(D);
					
					rx = modifierStartX + nextRow * modifierMultiplier;
					cy = modifierStartY + col * modifierMultiplier;
					if(planet.useBicubicInterpolation)
						disp += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
					else
						disp += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);
					
					D += D * planet.heightVariation * disp;
					D *= planet.radius;
				}
				
				if(nextCol < detail) {
					E = vertices[nextCol * detail + row];
				}
				else {
					E = Vector3.zero;
					
					if(staticX)
						E = new Vector3(topleft.x, topleft.y - stepY, topleft.z + stepZ * row);
					if(staticY)
						E = new Vector3(topleft.x + stepX * row, topleft.y, topleft.z - stepZ);
					if(staticZ)
						E = new Vector3(topleft.x + stepX * row, topleft.y - stepY, topleft.z);
					
					E = SperifyPoint(E);
					float disp = planet.Terrain.module.GetValue(E);
					
					rx = modifierStartX + row * modifierMultiplier;
					cy = modifierStartY + nextCol * modifierMultiplier;
					if(planet.useBicubicInterpolation)
						disp += planet.GetBicubicInterpolatedModifierAt(rx, cy, surfacePosition);
					else
						disp += planet.GetBilinearInterpolatedModifierAt(rx, cy, surfacePosition);
					
					E += E * planet.heightVariation * disp;
					E *= planet.radius;
				}
				
				// SUBSTRACT
				CA = A - C;
				CB = B - C;
				CD = D - C;
				CE = E - C;
				
				// CROSS
				normalACB = Vector3.Cross(CA, CB);
				normalBCE = Vector3.Cross(CB, CE);
				normalECD = Vector3.Cross(CE, CD);
				normalDCA = Vector3.Cross(CD, CA);

				normalACB.Normalize();
				normalBCE.Normalize();
				normalECD.Normalize();
				normalDCA.Normalize();
				
				// AVERAGE
				normals[col * detail + row] = (normalACB + normalBCE + normalECD + normalDCA) / 4f;
			}
		}*/

		// borders
		float borderDepth = -0.025f;
		int vertexCount = detail * detail;
		for(int col = 0; col < detail; col++) {
			int row = 0;
			vertices[vertexCount] = vertices[col * detail + row] + vertices[col * detail + row].normalized * planet.radius * borderDepth;
			normals[vertexCount] = normals[col * detail + row];
			uvs[vertexCount] = uvs[col * detail + row];
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
			uvs[vertexCount] = uvs[col * detail + row];
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
			uvs[vertexCount] = uvs[col * detail + row];
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
			uvs[vertexCount] = uvs[col * detail + row];
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
		
		//Debug.Log("Mesh calculated in " + (float)(System.DateTime.UtcNow - startTime).TotalSeconds * 1000f + "ms");
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
			if(this.mesh != null)
				DestroyImmediate(this.mesh, false);
			if(this.gameObject != null)
				DestroyImmediate(this.gameObject);
		}
	}
	
	#endregion
	
	#region LOD

	// distance and angle from camera
	public float distance = 0f, angle = 0f;
	
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
			
			if(!queuedForSubdivision) {
				if(!hasSubsurfaces) {
					if(planet.useAngleCulling) {
						angle = Vector3.Angle((planet.transform.position - planet.lodTarget.position).normalized, (planet.transform.position - closestCorner).normalized);
						if(angle > planet.rendererCullingAngle)
							renderer.enabled = false;
						else
							renderer.enabled = true;
					}
					
					float lodDistance = planet.lodDistances[lodLevel];
					if(distance < lodDistance) {
						planet.QueueForSubdivision(this);
						queuedForSubdivision = true;
					}
				}
				else {
					float lodDistance = planet.lodDistances[lodLevel] * 2f;
					if(distance > lodDistance) {
						UnloadSubsurfaces();
						renderer.enabled = true;
					}
					else {
						for(int i = 0; i < subSurfaces.Count; i++) {
							subSurfaces[i].UpdateLOD();
						}
					}
				}
			}
		}
	}
	
	/// <summary>
	/// Calculates the priority penalty to be used in generation queue
	/// </summary>
	public void CalculatePriority() {
		//float distance = GetClosestDistance();
		//priorityPenalty = distance / planet.lodDistances[lodLevel];
		//priorityPenalty += (1f - Vector3.Angle(planet.lodTarget.forward, (closestCorner - planet.lodTarget.position).normalized) / 180f) * .5f;
		//priorityPenalty += (1f - lodLevel / planet.maxLodLevel);
		priorityPenalty = 0f;
	}
	
	/// <summary>
	/// Gets the closest distance.
	/// </summary>
	private float GetClosestDistance() {
		float closestDistance = Mathf.Infinity;
		float distance = Vector3.Distance(planet.lodTarget.position, transform.TransformPoint(topLeftCorner));
		if(distance < closestDistance) {
			closestDistance = distance;
			closestCorner = transform.TransformPoint(topLeftCorner);
		}
		
		distance = Vector3.Distance(planet.lodTarget.position, transform.TransformPoint(topRightCorner));
		if(distance < closestDistance) {
			closestDistance = distance;
			closestCorner = transform.TransformPoint(topRightCorner);
		}
		
		distance = Vector3.Distance(planet.lodTarget.position, transform.TransformPoint(middlePoint));
		if(distance < closestDistance) {
			closestDistance = distance;
			closestCorner = transform.TransformPoint(middlePoint);
		}
		
		distance = Vector3.Distance(planet.lodTarget.position, transform.TransformPoint(bottomLeftCorner));
		if(distance < closestDistance) {
			closestDistance = distance;
			closestCorner = transform.TransformPoint(bottomLeftCorner);
		}
		
		distance = Vector3.Distance(planet.lodTarget.position, transform.TransformPoint(bottomRightCorner));
		if(distance < closestDistance) {
			closestDistance = distance;
			closestCorner = transform.TransformPoint(bottomRightCorner);
		}
		
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
		if(planet.disableCollidersOnSubdivision[lodLevel]) {
			MeshCollider collider = GetComponent<MeshCollider>();
			if(collider != null)
				collider.enabled = false;
		}
		
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
				GameObject t = new GameObject("Subsurface");
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
			for(int i = 0; i < subSurfaces.Count; i++) {
				subSurfaces[i].CleanAndDestroy();
			}
			subSurfaces.Clear();
			
			hasSubsurfaces = false;
			
			// enable collider
			if(planet.disableCollidersOnSubdivision[lodLevel]) {
				MeshCollider collider = GetComponent<MeshCollider>();
				if(collider != null)
					collider.enabled = true;
			}
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
	
	#endregion
}
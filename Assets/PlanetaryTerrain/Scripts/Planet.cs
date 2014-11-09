using UnityEngine;
using System.Collections.Generic;
using System;

namespace Planetary {

public class Planet : MonoBehaviour 
{
	public TextAsset terrainAsset;
	private TerrainModule terrain;
	public TerrainModule Terrain {
		set {
			terrain = value;
		}
		get {
			if(terrain == null)
				LoadModule();
			return this.terrain;
		}
	}
	
	// terrain settings
	public float radius = 10f;
	public float heightVariation = 0.0075f;
	public float frequencyScale = 1f;

	public bool generateOnStart = false;
	public bool randomizeSeeds = false;
	public float seed = 0;

	// mesh settings
	public int subdivisions = 1;
	public int meshResolution = 64;
	public bool createBorders = true;
	public bool logGenerationTimes = false;

	public bool uv2 = false;
	public UV uv1type, uv2type;
	public enum UV {
		CONTINUOUS, SPHERICAL, SURFACE
	}

	// texture settings
	public int textureResolution = 32;
	
	// list of terrain surfaces
	public List<Surface> surfaces = new List<Surface>();
	private int id = 0;

	// surface generation event
	public event Surface.SurfaceDelegate SurfaceGenerated;

	// event for when surface with collider has been generated
	public delegate void ColliderGenerationDelegate(int lodLevel);
	public event ColliderGenerationDelegate ColliderGenerated;
	
	// LOD
	[SerializeField] private bool _useLod = false;
	public bool useLod {
		get { return _useLod; }
		set { 
			_useLod = value;
			if(lodDistances == null)
				lodDistances = new float[_maxLodLevel];
			if(generateColliders == null)
				generateColliders = new bool[_maxLodLevel+1];
			//if(disableCollidersOnSubdivision == null)
			//	disableCollidersOnSubdivision = new bool[_maxLodLevel+1];
		}
	}
	
	public Transform lodTarget;
	
	[SerializeField] private int _maxLodLevel = 1;
	public int maxLodLevel {
		get { return _maxLodLevel; }
		set { 
			if(value != _maxLodLevel && value > 0) {
				_maxLodLevel = value;
						
				float[] newLodDistances = new float[_maxLodLevel];
				if(lodDistances != null) {
					if(lodDistances.Length != 0) {
						for(int i = 0; i < newLodDistances.Length; i++) {
							if(i < lodDistances.Length) {
								newLodDistances[i] = lodDistances[i];
							}
							else {
								newLodDistances[i] = lodDistances[lodDistances.Length - 1] * Mathf.Pow(.5f, i - lodDistances.Length+1);
							}
						}
					}
				}
				lodDistances = newLodDistances;

				float[] newLodDots = new float[_maxLodLevel];
				if(lodDots != null) {
					if(lodDots.Length != 0) {
						for(int i = 0; i < newLodDots.Length; i++) {
							if(i < lodDots.Length) {
								newLodDots[i] = lodDots[i];
							}
							else {
								newLodDots[i] = 0.5f + .49f * ((float)i / value);//lodDots[lodDots.Length - 1] * Mathf.Pow(.5f, i - lodDots.Length+1);
							}
						}
					}
				}
				lodDots = newLodDots;
				
				bool[] newGenerateColliders = new bool[_maxLodLevel+1];
				if(generateColliders != null) {
					if(generateColliders.Length != 0) {
						for(int i = 0; i < newLodDistances.Length; i++) {
							if(i < generateColliders.Length) {
								newGenerateColliders[i] = generateColliders[i];
							}
							else {
								newGenerateColliders[i] = false;
							}
						}
					}
				}
				generateColliders = newGenerateColliders;

				/*bool[] newDisableCollidersOnSubdivision = new bool[_maxLodLevel+1];
				if(disableCollidersOnSubdivision != null) {
					if(disableCollidersOnSubdivision.Length != 0) {
						for(int i = 0; i < newLodDistances.Length; i++) {
							if(i < disableCollidersOnSubdivision.Length) {
								newDisableCollidersOnSubdivision[i] = disableCollidersOnSubdivision[i];
							}
							else {
								newDisableCollidersOnSubdivision[i] = false;
							}
						}
					}
				}
				disableCollidersOnSubdivision = newDisableCollidersOnSubdivision;*/
			}
		}
	}
	
	public float[] lodDistances = new float[1];
	public float[] lodDots = new float[1];
	public bool[] generateColliders = new bool[2];
	//public bool[] disableCollidersOnSubdivision = new bool[2];

	// materials & textures
	public Material terrainMaterial;

	/// <summary>
	/// Start 
	/// </summary>
	public void Start() {		
		
		subdivisionQueue = new List<Surface>();
		generationQueue = new List<Surface>();
		if(useLod && lodTarget == null) {
			lodTarget = Camera.main.transform;
		}
		
		if(generateOnStart)
			Generate(null);
	}

	#region SurfaceGeneration
	
	/// <summary>
	/// Generates the surfaces which in turn create the meshes
	/// </summary>
	/// <param name='OnIteration'>
	/// Delegate for showing progress bar on each iteration in the editor
	/// </param>
	public void Generate(Action OnIteration, bool loadModule) {
		if(loadModule)
			LoadModule();
		
		if(terrain != null) {
			// global maps
			if(textureNodeInstances == null)
				textureNodeInstances = new Planet.TextureNodeInstance[0];
			
			if(textureNodeInstances.Length != terrain.textureNodes.Count) {
				Planet.TextureNodeInstance[] newInstances = new Planet.TextureNodeInstance[terrain.textureNodes.Count];
				for(int i = 0; i < terrain.textureNodes.Count; i++) {
					bool createNewInstance = true;
					if(i < textureNodeInstances.Length) {
						if(textureNodeInstances[i] != null) {
							newInstances[i] = textureNodeInstances[i];
							createNewInstance = false;
						}
					}
					if(createNewInstance) {
						newInstances[i] = new Planet.TextureNodeInstance(terrain.textureNodes[i]);
					}
				}
				textureNodeInstances = newInstances;
			}

			for(int i = 0; i < textureNodeInstances.Length; i++) {
				textureNodeInstances[i].node = terrain.textureNodes[i];
				if(textureNodeInstances[i].generateGlobalMap)
					GenerateGlobalMaps(textureNodeInstances[i]);
			}

			// initialize modifiers if not existing already
			if(!modifiersInitialized){
				modifiersInitialized = true;
				ClearModifiers();
			}

			// destroy already existing surfaces before creating new ones
			ClearSurfaces();

			// plot vertex positions for all surfaces
			float step = 2f / subdivisions;
			float half = subdivisions / 2f * step;
			
			id = 0;
			
			if(subdivisions > 0) {
				for(int sY = 0; sY < subdivisions; sY++) {
					float y = sY * step;
					float ny = (sY+1) * step;
					
					for(int sX = 0; sX < subdivisions; sX++) {
						float x = sX * step;
						float nx = (sX+1) * step;
		
						for(int i = 0; i < 6; i++) {
							CreateSurface(x, half, y, nx, ny, (SurfacePosition)i, sX, sY);
							if(OnIteration != null)
								OnIteration();
						}
					}
				}
			}
		}
	}
	
	/// <summary>
	/// Generates without the Action parameter
	/// </summary>
	public void Generate() {
		Generate(null, true);
	}
	public void Generate(Action OnIteration) {
		Generate(OnIteration, true);
	}
	
	/// <summary>
	/// Loads settings from the file 
	/// </summary>
	public void LoadModule() {
		terrain = TerrainModule.LoadTextAsset(terrainAsset, randomizeSeeds, seed, frequencyScale);
		if(terrain == null) {
			Debug.Log("TerrainModule file not found.");
			return;
		}
	}
	
	/// <summary>
	/// Surface position enumerator for indicating surface positions around the planet
	/// </summary>
	public enum SurfacePosition {
		 TOP, FRONT, BOTTOM, BACK, LEFT, RIGHT
	}
	
	/// <summary>
	/// Single iteration generates one surface. To be used to show progress on the editor script
	/// </summary>
	public void CreateSurface(float x, float half, float y, float nx, float ny, SurfacePosition pos, int sx, int sy)
	{
		Vector3 start = Vector3.zero, end = Vector3.zero;
		Vector3 topRight = Vector3.zero, bottomLeft = Vector3.zero;
		
		switch(pos) {
		case SurfacePosition.TOP:
			start = new Vector3(x - half, half, y - half);
			end = new Vector3(nx - half, half, ny - half);
			
			topRight = new Vector3(nx - half, half, y - half);
			bottomLeft = new Vector3(x - half, half, ny - half);
			break;
		case SurfacePosition.FRONT:
			start = new Vector3(x - half , half - y , half);
			end = new Vector3(nx - half , half - ny , half);
			
			topRight = new Vector3(nx - half , half - y , half);
			bottomLeft = new Vector3(x - half , half - ny , half);
			break;
		case SurfacePosition.BOTTOM:
			start = new Vector3(x - half , -half , half - y);
			end = new Vector3(nx - half , -half , half - ny);
			
			topRight = new Vector3(nx - half , -half , half - y);
			bottomLeft = new Vector3(x - half , -half , half - ny);
			break;
		case SurfacePosition.BACK:
			start = new Vector3(half - x , half - y , -half);
			end = new Vector3(half - nx , half - ny , -half);
			
			topRight = new Vector3(half - nx , half - y , -half);
			bottomLeft = new Vector3(half - x , half - ny , -half);
			break;
		case SurfacePosition.LEFT:
			start = new Vector3(-half , half - y , -half + x);
			end = new Vector3(-half , half - ny , -half + nx);
			
			topRight = new Vector3(-half , half - y , -half + nx);
			bottomLeft = new Vector3(-half , half - ny , -half + x);
			break;
		case SurfacePosition.RIGHT:
			start = new Vector3(half , half - y , half - x);
			end = new Vector3(half , half - ny , half - nx);
			
			topRight = new Vector3(half , half - y , half - nx);
			bottomLeft = new Vector3(half , half - ny , half - x);
			break;
		}
		
		InstantiateSurface(id, start, end, topRight, bottomLeft, pos, sx, sy);
		id++;
	}
	
	/// <summary>
	/// Instantiates a surface and adds it to the list. Virtual so other planet types can override it to use different surfaces
	/// </summary>
	private void InstantiateSurface(int id, Vector3 start, Vector3 end, Vector3 topRight, Vector3 bottomLeft, SurfacePosition sp, int sx, int sy) {
		GameObject t = new GameObject("Surface" + id);
		t.layer = gameObject.layer;
		t.tag = gameObject.tag;
		t.transform.parent = this.transform;
		t.transform.position = transform.position;
		Surface surface = t.AddComponent<Surface>();
		surface.Initialize(0, start, end, topRight, bottomLeft, this, sp, null, sx, sy);
		surface.GenerateMesh(meshResolution);
		surfaces.Add(surface);
	}
	
	/// <summary>
	/// Destroys all currently existing surfaces 
	/// </summary>
	public void ClearSurfaces ()
	{
		Component[] childComponents = GetComponentsInChildren(typeof(Surface)) as Component[];
		if(childComponents.Length > 0) {
			foreach(Component c in childComponents) {
				Surface s = (Surface)c;
				if(s != null) {
					// if clearing in editor, dont destroy meshes so undo is possible
					if(Application.isEditor && !Application.isPlaying)
						DestroyImmediate(s.gameObject);
					else
						s.CleanAndDestroy();
				}
			}
		}
		
		surfaces.Clear();
	}

	/// <summary>
	/// Should only be called by Surfaces to notify the planet and generate the event
	/// </summary>
	public void ReportGeneration(Surface s) {
		s.renderer.sharedMaterial = terrainMaterial;

		if(SurfaceGenerated != null)
			SurfaceGenerated(s);

		if(generateColliders[s.lodLevel])
			ColliderGenerated(s.lodLevel);
	}

	/// <summary>
	/// Returns suraface point at position.
	/// </summary>
	/// <returns>The <see cref="UnityEngine.Vector3"/>.</returns>
	/// <param name="position">Position.</param>
	public Vector3 SurfacePointAt(Vector3 position) {
		position /= radius;
		
		position += position * terrain.module.GetValue(position) * heightVariation;
		position *= radius;
		
		return position;
	}

	#endregion

	#region LodGenerationQueue
	
	private List<Surface> subdivisionQueue, generationQueue;
	private SurfacePriorityComparer surfaceComparer;

	// how many surfaces can be subdivided at the same time
	public int simultaneousSubdivisions = 1;

	// renderer culling by angle
	public bool useAngleCulling = true;
	public float rendererCullingAngle = 70f;

	// how often lod is updated in seconds
	public float lodUpdateInterval = 0f;
	private float lastUpdateTime = 0f;
	
	/// <summary>
	/// Surfaces call this method when needing to be subdivided. Adds it to queue.
	/// </summary>
	public void QueueForSubdivision(Surface s) {
		if(!subdivisionQueue.Contains(s)) {
			subdivisionQueue.Add(s);
		}

		s.CalculatePriority();

		// sort queue to find highest priority surfaces
		if(surfaceComparer == null)
			surfaceComparer = new SurfacePriorityComparer();
		subdivisionQueue.Sort(surfaceComparer);
	}

	/// <summary>
	/// Removes surface from queue
	/// </summary>
	public void RemoveFromQueue(Surface s) {
		if(subdivisionQueue.Contains(s) && !generationQueue.Contains(s)) {
			subdivisionQueue.Remove(s);
			//Debug.Log("Surface removed from queue");
		}
	}

	/// <summary>
	/// Event handler called by the surface when all subsurfaces have been generated.
	/// </summary>
	public void RemoveWhenSubdivided(Surface s) {
		generationQueue.Remove(s);
		s.SubdivisionComplete -= RemoveWhenSubdivided;
	}
	
	/// <summary>
	/// LOD generation queue
	/// </summary>
	private void Update() {
		if(useLod) {
			// update surfaces
			if(Time.time > lastUpdateTime + lodUpdateInterval) {
				lastUpdateTime = Time.time;
				for(int i = 0; i < surfaces.Count; i++) {
					surfaces[i].UpdateLOD();
				}
			}

			// clean up subdivision queue
			subdivisionQueue.RemoveAll(item => item == null);
				
			// clean up generation queue
			generationQueue.RemoveAll(item => item == null);
			
			//Debug.Log("queue: " + subdivisionQueue.Count + ", generating: " + generationQueue.Count);
			for(int i = 0; i < simultaneousSubdivisions; i++) {
				// add surfaces to generation queue if free
				if(generationQueue.Count < simultaneousSubdivisions && subdivisionQueue.Count > 0 && subdivisionQueue[0] != null) {
					generationQueue.Add(subdivisionQueue[0]);
					subdivisionQueue[0].SubdivisionComplete += RemoveWhenSubdivided;
					subdivisionQueue[0].CreateSubsurfaces();
					subdivisionQueue.RemoveAt(0);
				}
				else
					break;
			}
		}
	}
	
	/// <summary>
	/// Helper class, sorts surface list to ascending order by prioritypenalty value.
	/// </summary>
	public class SurfacePriorityComparer : IComparer<Surface> {
		public int Compare(Surface x, Surface y) {
			if (x.priorityPenalty > y.priorityPenalty)
               return 1;
            if (x.priorityPenalty < y.priorityPenalty)
               return -1;
            else
               return 0;
		}
	}
	
	#endregion

	#region HeightPainting

	public bool useBrushTexture = false;
	public bool useBrushAlpha = true;
	public Texture2D brushTexture;

	public enum PaintMode  {
		COLOR, HEIGHT
	}
	public PaintMode paintMode = PaintMode.COLOR;

	public enum BrushMode  {
		ADD, SUBSTRACT, SET
	}
	public BrushMode brushMode = BrushMode.ADD;
	
	public float brushSize = 0.1f, brushStrength = 0.1f, brushSetValue = 0f;
	public bool brushFalloff = true;
	public AnimationCurve falloff = AnimationCurve.Linear(0f, 0f, 1f, 1f);
	
	public float lowLimit = -1f, highLimit = 1f;

	// modifier arrays
	public float[] topModifier, bottomModifier, leftModifier, rightModifier, frontModifier, backModifier;
	public int modifierResolution = 128;
	public bool useBicubicInterpolation = true;
	public bool modifiersInitialized = false;

	// color arrays
	//public Color[] topColor, bottomColor, leftColor, rightColor, frontColor, backColor;

	/// <summary>
	/// Clears the modifiers.
	/// </summary>
	public void ClearModifiers() {
		// create modifier arrays
		topModifier = new float[modifierResolution * modifierResolution];
		bottomModifier = new float[modifierResolution * modifierResolution];
		leftModifier = new float[modifierResolution * modifierResolution];
		rightModifier = new float[modifierResolution * modifierResolution];
		frontModifier = new float[modifierResolution * modifierResolution];
		backModifier = new float[modifierResolution * modifierResolution];

		/*topColor = new Color[modifierResolution * modifierResolution];
		bottomColor = new Color[modifierResolution * modifierResolution];
		leftColor = new Color[modifierResolution * modifierResolution];
		rightColor = new Color[modifierResolution * modifierResolution];
		frontColor = new Color[modifierResolution * modifierResolution];
		backColor = new Color[modifierResolution * modifierResolution];*/
		
		for(int x = 0; x < modifierResolution; x++) {
			for(int y = 0; y < modifierResolution; y++) {
				topModifier[x * modifierResolution + y] = 0f;
				bottomModifier[x * modifierResolution + y] = 0f;
				leftModifier[x * modifierResolution + y] = 0f;
				rightModifier[x * modifierResolution + y] = 0f;
				frontModifier[x * modifierResolution + y] = 0f;
				backModifier[x * modifierResolution + y] = 0f;

				/*topColor[x * modifierResolution + y] = Color.black;
				bottomColor[x * modifierResolution + y] = Color.black;
				leftColor[x * modifierResolution + y] = Color.black;
				rightColor[x * modifierResolution + y] = Color.black;
				frontColor[x * modifierResolution + y] = Color.black;
				backColor[x * modifierResolution + y] = Color.black;*/
			}
		}
	}

	public float[] GetModifierArray(SurfacePosition surfacePosition) {
		switch(surfacePosition) {
		case SurfacePosition.TOP:
			return topModifier;
		case SurfacePosition.BOTTOM:
			return bottomModifier;
		case SurfacePosition.LEFT:
			return leftModifier;
		case SurfacePosition.RIGHT:
			return rightModifier;
		case SurfacePosition.FRONT:
			return frontModifier;
		case SurfacePosition.BACK:
			return backModifier;
		}
		return null;
	}

	/*public Color[] GetColorArray(SurfacePosition surfacePosition) {
		switch(surfacePosition) {
		case SurfacePosition.TOP:
			return topColor;
		case SurfacePosition.BOTTOM:
			return bottomColor;
		case SurfacePosition.LEFT:
			return leftColor;
		case SurfacePosition.RIGHT:
			return rightColor;
		case SurfacePosition.FRONT:
			return frontColor;
		case SurfacePosition.BACK:
			return backColor;
		}
		return null;
	}*/
	
	/// <summary>
	/// Returns the correct surface position when out of bounds of the current one
	/// </summary>
	public SurfacePosition GetCorrectSurfacePosition(ref int row, ref int col, SurfacePosition sp) {
		SurfacePosition correctSP = sp;

		switch(sp) {
		case SurfacePosition.TOP:
			if(row >= 0 && row < modifierResolution && col >= 0 && col < modifierResolution)
				correctSP = SurfacePosition.TOP;

			else if(row < 0) {
				correctSP = SurfacePosition.LEFT;
				int oldCol = col;
				col = Mathf.Abs(row+1);
				row = oldCol;
			}
			else if(row >= modifierResolution) {
				correctSP = SurfacePosition.RIGHT;
				int oldCol = col;
				col = row - modifierResolution;
				row = (modifierResolution-1) - oldCol;
			}
			else if(col < 0) {
				correctSP = SurfacePosition.BACK;
				col = Mathf.Abs(col+1);
				row = (modifierResolution-1) - row;
			}
			else if(col >= modifierResolution) {
				correctSP = SurfacePosition.FRONT;
				col -= modifierResolution;
			}
			break;
		case SurfacePosition.BOTTOM:
			if(row >= 0 && row < modifierResolution && col >= 0 && col < modifierResolution)
				correctSP = SurfacePosition.BOTTOM;
			
			else if(row < 0) {
				correctSP = SurfacePosition.LEFT;
				int oldCol = col;
				col = modifierResolution + row;
				row = (modifierResolution-1) - oldCol;
			}
			else if(row >= modifierResolution) {
				correctSP = SurfacePosition.RIGHT;
				int oldCol = col;
				col = modifierResolution - (int)Mathf.Abs((modifierResolution-1) - row);
				row = oldCol;
			}
			else if(col < 0) {
				correctSP = SurfacePosition.FRONT;
				col = modifierResolution + col;
			}
			else if(col >= modifierResolution) {
				correctSP = SurfacePosition.BACK;
				col = modifierResolution - (int)Mathf.Abs((modifierResolution-1) - col);
				row = (modifierResolution-1) - row;
			}
			break;
		case SurfacePosition.LEFT:
			if(row >= 0 && row < modifierResolution && col >= 0 && col < modifierResolution)
				correctSP = SurfacePosition.LEFT;

			else if(row < 0) {
				correctSP = SurfacePosition.BACK;
				row = modifierResolution + row;
			}
			else if(row >= modifierResolution) {
				correctSP = SurfacePosition.FRONT;
				row -= modifierResolution;
			}
			else if(col < 0) {
				correctSP = SurfacePosition.TOP;
				int oldRow = row;
				row = Mathf.Abs(col+1);
				col = oldRow;
			}
			else if(col >= modifierResolution) {
				correctSP = SurfacePosition.BOTTOM;
				int oldRow = row;
				row = col - modifierResolution;
				col = (modifierResolution-1) - oldRow;
			}
			break;
		case SurfacePosition.RIGHT:
			if(row >= 0 && row < modifierResolution && col >= 0 && col < modifierResolution)
				correctSP = SurfacePosition.RIGHT;
			else if(row < 0) {
				correctSP = SurfacePosition.FRONT;
				row = modifierResolution + row;
			}
			else if(row >= modifierResolution) {
				correctSP = SurfacePosition.BACK;
				row -= modifierResolution;
			}
			else if(col < 0) {
				correctSP = SurfacePosition.TOP;
				int oldRow = row;
				row = modifierResolution + col;
				col = (modifierResolution-1) - oldRow;
			}
			else if(col >= modifierResolution) {
				correctSP = SurfacePosition.BOTTOM;
				int oldRow = row;
				row = modifierResolution - (int)Mathf.Abs((modifierResolution-1) - col);
				col = oldRow;
			}
			break;
		case SurfacePosition.FRONT:
			if(row >= 0 && row < modifierResolution && col >= 0 && col < modifierResolution)
				correctSP = SurfacePosition.FRONT;
			else if(row < 0) {
				correctSP = SurfacePosition.LEFT;
				row = modifierResolution + row;
			}
			else if(row >= modifierResolution) {
				correctSP = SurfacePosition.RIGHT;
				row -= modifierResolution;
			}
			else if(col < 0) {
				correctSP = SurfacePosition.TOP;
				col = modifierResolution + col;
			}
			else if(col >= modifierResolution) {
				correctSP = SurfacePosition.BOTTOM;
				col -= modifierResolution;
			}
			break;
		case SurfacePosition.BACK:
			if(row >= 0 && row < modifierResolution && col >= 0 && col < modifierResolution)
				correctSP = SurfacePosition.BACK;
			else if(row < 0) {
				correctSP = SurfacePosition.RIGHT;
				row = modifierResolution + row;
			}
			else if(row >= modifierResolution) {
				correctSP = SurfacePosition.LEFT;
				row -= modifierResolution;
			}
			else if(col < 0) {
				correctSP = SurfacePosition.TOP;
				col = Mathf.Abs(col+1);
				row = (modifierResolution-1) - row;
			}
			else if(col >= modifierResolution) {
				correctSP = SurfacePosition.BOTTOM;
				col = modifierResolution - (int)Mathf.Abs((modifierResolution-1) - col);
				row = modifierResolution - row;
			}
			break;
		}

		return correctSP;
	}

	/// <summary>
	/// Gets the nearest modifier at position
	/// </summary>
	public float GetModifierAt(int row, int col, SurfacePosition sp) {
		int x = row, y = col;

		SurfacePosition correctSP = GetCorrectSurfacePosition(ref x, ref y, sp);
		//if(x < 0 || x > modifierResolution-1 || y < 0 || y > modifierResolution-1)
		//	Debug.Log("row: " + row + " col: " + col + " sp: " + sp + " | " + " x: " + x + " y: " + y + " correct:" + correctSP.ToString());

		if(x < 0)
			x = 0;
		if(x > modifierResolution-1)
			x = modifierResolution-1;
		if(y < 0)
			y = 0;
		if(y > modifierResolution-1)
			y = modifierResolution-1;
		return GetModifierArray(correctSP)[y * modifierResolution + x];
	}

	/// <summary>
	/// Gets the bilinearly interpolated modifier at given position. Called by the surface to get modifier height for each point.
	/// </summary>
	public float GetBilinearInterpolatedModifierAt(float row, float col, SurfacePosition sp) {
		int row1 = Mathf.FloorToInt(row);
		int col1 = Mathf.FloorToInt(col);
		float interX = row - row1;
		float interY = col - col1;

		float a = GetModifierAt(row1, col1, sp);
		float b = GetModifierAt(row1+1, col1, sp);
		float c = GetModifierAt(row1, col1+1, sp);
		float d = GetModifierAt(row1+1, col1+1, sp);

		return Mathf.Lerp(Mathf.Lerp(a, b, interX), Mathf.Lerp(c, d, interX), interY);
	}

	/// <summary>
	/// Gets the bicubicly interpolated modifier at given position. Called by the surface to get modifier height for each point.
	/// </summary>
	public float GetBicubicInterpolatedModifierAt(float row, float col, SurfacePosition sp) {
		int row1 = Mathf.FloorToInt(row);
		int col1 = Mathf.FloorToInt(col);
		float interX = row - row1;
		float interY = col - col1;
		
		float[][] array = new float[4][] { new float[4], new float[4], new float[4], new float[4]};

		array[0][0] = GetModifierAt(row1-1, col1-1, sp);
		array[0][1] = GetModifierAt(row1-1, col1, sp);
		array[0][2] = GetModifierAt(row1-1, col1+1, sp);
		array[0][3] = GetModifierAt(row1-1, col1+2, sp);
		
		array[1][0] = GetModifierAt(row1, col1-1, sp);
		array[1][1] = GetModifierAt(row1, col1, sp);
		array[1][2] = GetModifierAt(row1, col1+1, sp);
		array[1][3] = GetModifierAt(row1, col1+2, sp);
		
		array[2][0] = GetModifierAt(row1+1, col1-1, sp);
		array[2][1] = GetModifierAt(row1+1, col1, sp);
		array[2][2] = GetModifierAt(row1+1, col1+1, sp);
		array[2][3] = GetModifierAt(row1+1, col1+2, sp);
		
		array[3][0] = GetModifierAt(row1+2, col1-1, sp);
		array[3][1] = GetModifierAt(row1+2, col1, sp);
		array[3][2] = GetModifierAt(row1+2, col1+1, sp);
		array[3][3] = GetModifierAt(row1+2, col1+2, sp);
		
		return GetBiCubicValue(array, interX, interY);
	}
	
	public static float GetCubicValue (float[] p, float x) {
		return p[1] + 0.5f * x*(p[2] - p[0] + x*(2.0f*p[0] - 5.0f*p[1] + 4.0f*p[2] - p[3] + x*(3.0f*(p[1] - p[2]) + p[3] - p[0])));
	}
	
	public float GetBiCubicValue (float[][] p, float x, float y) {
		float[] arr = new float[4];
		arr[0] = GetCubicValue(p[0], y);
		arr[1] = GetCubicValue(p[1], y);
		arr[2] = GetCubicValue(p[2], y);
		arr[3] = GetCubicValue(p[3], y);
		return GetCubicValue(arr, x);
	}

	/// <summary>
	/// Paints heights on the planet surface
	/// </summary>
	public void Paint(Surface surface, Vector2 pos) {
		int res = modifierResolution;
		float increment = 1f / res;
		int area = Mathf.RoundToInt(brushSize / increment);

		SurfacePosition sp = surface.surfacePosition;
		List<Surface> positions = new List<Surface>();
		
		for(int y = -area / 2; y < area / 2; y++) {
			for(int x = -area / 2; x < area / 2; x++) {
				int xx = Mathf.RoundToInt((pos.x + x * increment) * res);
				int yy = Mathf.RoundToInt((pos.y + y * increment) * res);
				Vector2 brushPosition = new Vector2(xx, yy);

				Color pixel = Color.black;
				if(useBrushTexture && brushTexture != null)
					pixel = brushTexture.GetPixelBilinear((x + area / 2f) / area, (y + area / 2f) / area);

				// paint first to this position if known duplicate position (shared between surfaces) 
				/*if(xx == 0 || xx == modifierResolution-1 || yy == 0 || yy == modifierResolution-1) {
					if(!positions.Contains(sp))
						positions.Add(sp);
					
					Add(sp, pos, brushPosition, brushPosition, pixel);
				}*/

				SurfacePosition correctSP = GetCorrectSurfacePosition(ref xx, ref yy, sp);
				for(int i = 0; i < surfaces.Count; i++) {
					if(surfaces[i].surfacePosition == correctSP) {
						if(xx >= surfaces[i].modifierStartX && xx < surfaces[i].modifierStartX + surfaces[i].modifierResolution && 
						   yy >= surfaces[i].modifierStartY && yy < surfaces[i].modifierStartY + surfaces[i].modifierResolution) {
							if(!positions.Contains(surfaces[i]))
								positions.Add(surfaces[i]);
							break;
						}
					}
				}

				Vector2 newPos = new Vector2(xx, yy);
				Add(correctSP, pos, newPos, brushPosition, pixel);
			}
		}
	
		for(int i = 0; i < surfaces.Count; i++) {
			if(positions.Contains(surfaces[i])) {
				surfaces[i].GenerateMesh(meshResolution);
			}
		}
		positions.Clear();
	}

	/// <summary>
	/// Rounds the point to integer
	/// </summary>
	private Vector2 RoundPoint(Vector2 pos, int res) {
		int x = Mathf.RoundToInt(pos.x * res);
		int y = Mathf.RoundToInt(pos.y * res);
		return new Vector2(x, y);
	}

	/// <summary>
	/// Add height value to surface with selected brush mode
	/// </summary>
	private void Add(SurfacePosition sp, Vector2 brushCenter, Vector2 position, Vector2 brushPosition, Color brushPixel) {
		// make sure position is in array range
		if(position.x < 0 || position.x > modifierResolution-1 || position.y < 0 || position.y > modifierResolution-1) {
			//Debug.Log("Planet tries to paint outside of array range");
			return;
		}

		float[] modifier = GetModifierArray(sp);
		float value = modifier[(int)position.x + (int)position.y * modifierResolution];

		float falloffValue = 1f;
		if(brushFalloff)
			falloffValue = 1f - falloff.Evaluate(Vector2.Distance(brushCenter, brushPosition / modifierResolution) / (brushSize * 0.5f));

		float strength = brushStrength;
		if(useBrushTexture) {
			if(useBrushAlpha)
				strength *= brushPixel.a;
			else
				strength *= brushPixel.grayscale;
		}

		switch(brushMode) {
		case BrushMode.ADD:
			value += strength * falloffValue;
			break;
		case BrushMode.SUBSTRACT:
			value -= strength * falloffValue;
			break;
		case BrushMode.SET:
			value = Mathf.Lerp(value, brushSetValue, falloffValue * strength);
			break;
		}

		if(value > highLimit)
			value = highLimit;
		if(value < lowLimit)
			value = lowLimit;
		modifier[(int)position.x + (int)position.y * modifierResolution] = value;
	}

	#endregion
	
	#region Debug
	
	public bool showDebugInfo = false;
	bool showDistance = true;
	Vector3 screenPoint;
	bool[] showLodLevel;
	
	void OnGUI() {
		if(showDebugInfo) {
			if(showLodLevel == null) {
				showLodLevel = new bool[maxLodLevel+1];
				for(int i = 0; i < maxLodLevel+1; i++)
					showLodLevel[i] = true;
			}
			
			for(int i = 0; i < showLodLevel.Length; i++) {
				showLodLevel[i] = GUI.Toggle(new Rect(0, i * 25, 100, 20), showLodLevel[i], "Show LOD " + i);
			}

			for(int i = 0; i < surfaces.Count; i++) {
				ShowSubsurfaces(surfaces[i]);
				if(!surfaces[i].hasSubsurfaces)
					ShowDebugGUI(surfaces[i]);
			}
		}
	}

	void ShowSubsurfaces(Surface s) {
		if(s.hasSubsurfaces) {
			for(int i = 0; i < s.subSurfaces.Count; i++) {
				ShowSubsurfaces(s.subSurfaces[i]);
				if(!s.subSurfaces[i].hasSubsurfaces)
					ShowDebugGUI(s.subSurfaces[i]);
			}
		}
	}
	
	void ShowDebugGUI(Surface s) {
		if(s.renderer.enabled && !s.hasSubsurfaces && showLodLevel[s.lodLevel]) {
			Vector3 transformedPoint = transform.TransformPoint(s.closestCorner);
			screenPoint = Camera.main.WorldToScreenPoint(transformedPoint);
			screenPoint.y = Screen.height - screenPoint.y;
			
			Vector3 heading = transformedPoint - Camera.main.transform.position;

			if(Vector3.Dot(Camera.main.transform.forward, heading) > 0f) {
				if(screenPoint.x > 0f && screenPoint.x < Screen.width && screenPoint.y > 0f && screenPoint.y < Screen.height) {
					GUI.color = Color.white;
					
					if(showDistance)
						GUI.Box(new Rect(screenPoint.x, screenPoint.y, 80, 20), s.lodLevel.ToString() + ": " + s.distance.ToString("0"));
					else
						GUI.Box(new Rect(screenPoint.x, screenPoint.y, 80, 20), "Level " + s.lodLevel.ToString());
				}
			}
		}
	}
	
	#endregion

	#region GlobalMaps

	public TextureNodeInstance[] textureNodeInstances;

	public void GenerateGlobalMaps(TextureNodeInstance tni) {
		Texture2D globalMap = GlobalMapUtility.Generate(tni.globalMapSize, tni.node.GetModule());
		terrainMaterial.SetTexture(tni.materialPropertyName, globalMap);
	}

	[System.Serializable()]
	public class TextureNodeInstance {
		public TextureNode node;
		public int globalMapSize = 512;
		public bool generateGlobalMap = false, generateSurfaceTextures = false;
		public string materialPropertyName = "";

		public TextureNodeInstance(TextureNode tn) {
			this.node = tn;
			this.materialPropertyName = tn.textureId;
		}
	}
	
	#endregion
}

}
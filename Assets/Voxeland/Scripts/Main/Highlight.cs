
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Voxeland {

public class Highlight : MonoBehaviour 
{
	public VoxelandTerrain land;
	public MeshFilter filter;
	
	public Vector3[] verts = new Vector3[0];
	public Vector2[] uvs = new Vector2[0];
	public int[] tris = new int[0];
	
	int oldIndex = -1; //to determine highlight change
	int newIndex = -2;

	public void DrawFace (Chunk.VoxelandFace face) 
	{ 
		//get change
		newIndex = face.block.x*10000 + face.block.y*100 + face.block.z + face.block.chunk.offsetX*10000 + face.block.chunk.offsetZ + face.dir;
		if (newIndex == oldIndex) return;
		oldIndex = newIndex;
		
		if (verts.Length != 9)
		{
			verts = new Vector3[9];
			uvs = faceUvs;
			tris = faceTris;
		}
		
		Vector3 addVector= new Vector3(face.block.chunk.offsetX, 0, face.block.chunk.offsetZ);
		for (int v=0; v<9; v++) verts[v] = (face.verts[v].pos + addVector);
		
		Apply();
	}
	
	public void DrawBox (Vector3 center, Vector3 size) 
	{ 
		if (verts.Length != cubeVerts.Length)
		{
			verts = new Vector3[cubeVerts.Length];
			uvs = new Vector2[cubeVerts.Length];
			tris = cubeTris;
		}
		
		for (int i=0; i<cubeVerts.Length; i++) verts[i] = Vector3.Scale(cubeVerts[i],size) + center;
		
		Apply();
	}
	
	public void DrawSphere (Vector3 center, float radius) 
	{ 
		if (verts.Length != sphereVerts.Length)
		{
			verts = new Vector3[sphereVerts.Length];
			uvs = new Vector2[sphereVerts.Length];
			tris = sphereTris;
		}
		
		for (int i=0; i<sphereVerts.Length; i++) verts[i] = sphereVerts[i]*radius + center;
		
		Apply();
	}
	
	public void DrawPoly (Chunk chunk, Mesh mesh, int index) 
	{ 
		newIndex = index + chunk.offsetX + chunk.offsetZ;
		if (newIndex == oldIndex) return;
		oldIndex = newIndex;
		
		if (verts.Length != sphereVerts.Length)
		{
			verts = new Vector3[6];
			uvs = polyUvs;
			tris = polyTris;
		}

		Vector3 addVector= new Vector3(chunk.offsetX, 0, chunk.offsetZ);
		for (int i=0; i<6; i++) verts[i] = mesh.vertices[ mesh.triangles[index*6+i] ] + addVector;
		
		Apply();
	}
	
	public void Apply ()
	{
		if (!filter.sharedMesh) filter.sharedMesh = new Mesh();
		filter.sharedMesh.Clear();
		
		filter.sharedMesh.vertices = verts;
		filter.sharedMesh.triangles = tris;
		filter.sharedMesh.uv = uvs;
		
		filter.sharedMesh.RecalculateBounds();
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
		#endif
	}


	static public Highlight Create (VoxelandTerrain land)
	{
		GameObject hlObj = new GameObject("Highlight");
		hlObj.transform.parent = land.transform;
		//hlObj.transform.hideFlags=HideFlags.HideInHierarchy;
		hlObj.transform.localPosition = new Vector3(0,0,0);
		hlObj.transform.localScale = new Vector3(1,1,1);
		
		Highlight highlight = hlObj.AddComponent<Highlight>();
		
		highlight.land = land;
		highlight.filter = hlObj.gameObject.AddComponent<MeshFilter>();	
		hlObj.AddComponent<MeshRenderer>();
		highlight.renderer.castShadows = false;
		highlight.renderer.receiveShadows = false;
		if (land.hideChunks) highlight.transform.hideFlags = HideFlags.HideInHierarchy;
		
		highlight.filter.sharedMesh = new Mesh();
		highlight.filter.sharedMesh.hideFlags=HideFlags.HideAndDontSave;
		
		//auto-setting highlight material if is does not exists
		if (!land.highlightMaterial || land.highlightMaterial.name=="Voxeland/Hightlight") //the last one for backwards compatability
		{
			land.highlightMaterial = new Material( Shader.Find("Voxeland/Highlight") );
			land.highlightMaterial.color = new Color(0.6f, 0.73f, 1, 0.353f);
		}
		
		highlight.renderer.sharedMaterial = land.highlightMaterial; //setting material
		
		//hiding wireframe
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetSelectedWireframeHidden(highlight.renderer, true);
		#endif
		
		return highlight;
	}
	
	public void  Clear ()
	{
		if (!filter.sharedMesh) filter.sharedMesh = new Mesh();
		filter.sharedMesh.Clear();
	}
	
	static private Vector2[] polyUvs = {new Vector2(0.25f,0), new Vector2(0.125f,0), new Vector2(0,0), new Vector2(0,0.125f), new Vector2(0,0.25f), new Vector2(0.125f,0.25f)};
	static private int[] polyTris = {0,1,2,3,4,5};
	

	static private Vector2[] faceUvs = {new Vector2(0.25f,0), new Vector2(0.125f,0), new Vector2(0,0), new Vector2(0,0.125f), new Vector2(0,0.25f),
						new Vector2(0.125f,0.25f), new Vector2(0.25f,0.25f), new Vector2(0.25f,0.125f), new Vector2(0.125f,0.125f)};
	static private int[] faceTris = {7,0,1,1,8,7,8,1,2,2,3,8,5,8,3,3,4,5,6,7,8,8,5,6};
	
	
	static private Vector3[] cubeVerts = {new Vector3(-1.0f,-1.0f,-1.0f), new Vector3(1.0f,-1.0f,-1.0f), new Vector3(-1.0f,-1.0f,1.0f), new Vector3(1.0f,-1.0f,1.0f), new Vector3(-1.0f,1.0f,-1.0f), new Vector3(1.0f,1.0f,-1.0f), new Vector3(-1.0f,1.0f,1.0f), new Vector3(1.0f,1.0f,1.0f), new Vector3(-1.0f,-1.0f,-1.0f), new Vector3(-1.0f,-1.0f,-1.0f), new Vector3(1.0f,-1.0f,-1.0f), new Vector3(1.0f,-1.0f,-1.0f), new Vector3(-1.0f,-1.0f,1.0f), new Vector3(-1.0f,-1.0f,1.0f), new Vector3(1.0f,-1.0f,1.0f), new Vector3(1.0f,-1.0f,1.0f), new Vector3(-1.0f,1.0f,-1.0f), new Vector3(-1.0f,1.0f,-1.0f), new Vector3(1.0f,1.0f,-1.0f), 
	                       new Vector3(1.0f,1.0f,-1.0f), new Vector3(-1.0f,1.0f,1.0f), new Vector3(-1.0f,1.0f,1.0f), new Vector3(1.0f,1.0f,1.0f), new Vector3(1.0f,1.0f,1.0f)};
	static private int[] cubeTris = {0,1,3,3,2,0,4,6,7,7,5,4,8,16,18,18,10,8,11,19,22,22,14,11,15,23,20,20,12,15,13,21,17,17,9,13};
	
	
	static private Vector3[] sphereVerts = {new Vector3(0.0f,1.0f,0.0f), new Vector3(0.894427f,0.447214f,0.0f), new Vector3(0.276393f,0.447214f,0.850651f), new Vector3(-0.723607f,0.447214f,0.525731f), new Vector3(-0.723607f,0.447214f,-0.525731f), new Vector3(0.276393f,0.447214f,-0.850651f), new Vector3(0.723607f,-0.447214f,0.525731f), new Vector3(-0.276393f,-0.447214f,0.850651f), new Vector3(-0.894427f,-0.447214f,0.0f), new Vector3(-0.276393f,-0.447214f,-0.850651f), new Vector3(0.723607f,-0.447214f,-0.525731f), new Vector3(0.0f,-1.0f,0.0f), new Vector3(0.360729f,0.932671f,0.0f), 
	                         new Vector3(0.672883f,0.739749f,0.0f), new Vector3(0.111471f,0.932671f,0.343074f), new Vector3(0.207932f,0.739749f,0.63995f), new Vector3(-0.291836f,0.932671f,0.212031f), new Vector3(-0.544374f,0.739749f,0.395511f), new Vector3(-0.291836f,0.932671f,-0.212031f), new Vector3(-0.544374f,0.739749f,-0.395511f), new Vector3(0.111471f,0.932671f,-0.343074f), new Vector3(0.207932f,0.739749f,-0.63995f), new Vector3(0.784354f,0.516806f,0.343074f), new Vector3(0.568661f,0.516806f,0.63995f), new Vector3(-0.0839038f,0.516806f,0.851981f), new Vector3(-0.432902f,0.516806f,0.738584f), 
	                         new Vector3(-0.83621f,0.516806f,0.183479f), new Vector3(-0.83621f,0.516806f,-0.183479f), new Vector3(-0.432902f,0.516806f,-0.738584f), new Vector3(-0.0839036f,0.516806f,-0.851981f), new Vector3(0.568661f,0.516806f,-0.63995f), new Vector3(0.784354f,0.516806f,-0.343074f), new Vector3(0.964719f,0.156077f,0.212031f), new Vector3(0.905103f,-0.156077f,0.395511f), new Vector3(0.0964608f,0.156077f,0.983023f), new Vector3(-0.0964609f,-0.156077f,0.983024f), new Vector3(-0.905103f,0.156077f,0.395511f), new Vector3(-0.964719f,-0.156077f,0.212031f), new Vector3(-0.655845f,0.156077f,-0.738585f), 
	                         new Vector3(-0.499768f,-0.156077f,-0.851981f), new Vector3(0.499768f,0.156077f,-0.851981f), new Vector3(0.655845f,-0.156077f,-0.738584f), new Vector3(0.964719f,0.156077f,-0.212031f), new Vector3(0.905103f,-0.156077f,-0.395511f), new Vector3(0.499768f,0.156077f,0.851981f), new Vector3(0.655845f,-0.156077f,0.738584f), new Vector3(-0.655845f,0.156077f,0.738584f), new Vector3(-0.499768f,-0.156077f,0.851981f), new Vector3(-0.905103f,0.156077f,-0.395511f), new Vector3(-0.964719f,-0.156077f,-0.212031f), new Vector3(0.0964611f,0.156077f,-0.983024f), new Vector3(-0.0964605f,-0.156077f,-0.983023f), 
	                         new Vector3(0.432902f,-0.516806f,0.738584f), new Vector3(0.0839037f,-0.516806f,0.851981f), new Vector3(-0.568661f,-0.516806f,0.63995f), new Vector3(-0.784354f,-0.516806f,0.343074f), new Vector3(-0.784354f,-0.516806f,-0.343074f), new Vector3(-0.568661f,-0.516806f,-0.63995f), new Vector3(0.083904f,-0.516806f,-0.851981f), new Vector3(0.432902f,-0.516806f,-0.738584f), new Vector3(0.83621f,-0.516806f,-0.183479f), new Vector3(0.83621f,-0.516806f,0.183479f), new Vector3(0.291836f,-0.932671f,0.212031f), new Vector3(0.544374f,-0.739749f,0.395511f), new Vector3(-0.111471f,-0.932671f,0.343074f), 
	                         new Vector3(-0.207932f,-0.739749f,0.63995f), new Vector3(-0.360729f,-0.932671f,0.0f), new Vector3(-0.672883f,-0.739749f,0.0f), new Vector3(-0.111471f,-0.932671f,-0.343074f), new Vector3(-0.207932f,-0.739749f,-0.63995f), new Vector3(0.291836f,-0.932671f,-0.212031f), new Vector3(0.544374f,-0.739749f,-0.395511f), new Vector3(0.479506f,0.805422f,0.348381f), new Vector3(-0.183155f,0.805422f,0.563693f), new Vector3(-0.592702f,0.805422f,0.0f), new Vector3(-0.183155f,0.805422f,-0.563693f), new Vector3(0.479506f,0.805422f,-0.348381f), new Vector3(0.985456f,-0.169933f,0.0f), 
	                         new Vector3(0.304522f,-0.169933f,0.937224f), new Vector3(-0.79725f,-0.169933f,0.579236f), new Vector3(-0.79725f,-0.169933f,-0.579236f), new Vector3(0.304523f,-0.169933f,-0.937224f), new Vector3(0.79725f,0.169933f,0.579236f), new Vector3(-0.304523f,0.169933f,0.937224f), new Vector3(-0.985456f,0.169933f,0.0f), new Vector3(-0.304522f,0.169933f,-0.937224f), new Vector3(0.79725f,0.169933f,-0.579236f), new Vector3(0.183155f,-0.805422f,0.563693f), new Vector3(-0.479506f,-0.805422f,0.348381f), new Vector3(-0.479506f,-0.805422f,-0.348381f), new Vector3(0.183155f,-0.805422f,-0.563693f), new Vector3(0.592702f,-0.805422f,0.0f)};
	static private int[] sphereTris = {14,12,0,72,13,12,14,72,12,15,72,14,22,1,13,72,22,13,23,22,72,15,23,72,2,23,15,16,14,0,73,15,14,16,73,14,17,73,16,24,2,15,73,24,15,25,24,73,17,25,73,3,25,17,18,16,0,74,17,16,18,74,16,19,74,18,26,3,17,74,26,17,27,26,74,19,27,74,4,27,19,20,18,0,75,19,18,20,75,18,21,75,20,28,4,19,75,28,19,29,28,75,21,29,75,5,29,21,12,20,0,76,21,20,12,76,20,13,76,12,30,5,21,76,30,21,31,30,76,13,31,76,1,31,13,32,42,1,77,43,42,32,77,42,33,77,32,60,10,43,77,60,43,61,60,77,33,61,77,6,61,33,34,44,2,78,45,44,34,78,44,35,78,
	                    34,52,6,45,78,52,45,53,52,78,35,53,78,7,53,35,36,46,3,79,47,46,36,79,46,37,79,36,54,7,47,79,54,47,55,54,79,37,55,79,8,55,37,38,48,4,80,49,48,38,80,48,39,80,38,56,8,49,80,56,49,57,56,80,39,57,80,9,57,39,40,50,5,81,51,50,40,81,50,41,81,40,58,9,51,81,58,51,59,58,81,41,59,81,10,59,41,33,45,6,82,44,45,33,82,45,32,82,33,23,2,44,82,23,44,22,23,82,32,22,82,1,22,32,35,47,7,83,46,47,35,83,47,34,83,35,25,3,46,83,25,46,24,25,83,34,24,83,2,24,34,37,49,8,84,48,49,37,84,49,36,84,37,27,4,48,84,27,48,26,27,84,36,26,84,3,26,36,39,51,9,85,50,
	                    51,39,85,51,38,85,39,29,5,50,85,29,50,28,29,85,38,28,85,4,28,38,41,43,10,86,42,43,41,86,43,40,86,41,31,1,42,86,31,42,30,31,86,40,30,86,5,30,40,62,64,11,87,65,64,62,87,64,63,87,62,53,7,65,87,53,65,52,53,87,63,52,87,6,52,63,64,66,11,88,67,66,64,88,66,65,88,64,55,8,67,88,55,67,54,55,88,65,54,88,7,54,65,66,68,11,89,69,68,66,89,68,67,89,66,57,9,69,89,57,69,56,57,89,67,56,89,8,56,67,68,70,11,90,71,70,68,90,70,69,90,68,59,10,71,90,59,71,58,59,90,69,58,90,9,58,69,70,62,11,91,63,62,70,91,62,71,91,70,61,6,63,91,61,63,60,61,91,71,60,91,10,60,71};
}

} //namespace
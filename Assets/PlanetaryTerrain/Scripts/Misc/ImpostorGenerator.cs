using UnityEngine;
using System.Collections;

namespace Planetary {

public class ImpostorGenerator {

	public static Texture2D GenerateTexture(int textureSize, float textureScale, Mesh mesh, Material[] materials, int layer) {
		// create temporary camera and render texture
		GameObject cameraObject = new GameObject("Impostor camera");
		Camera camera = cameraObject.AddComponent<Camera>();
		RenderTexture rt = RenderTexture.GetTemporary(textureSize, textureSize, 16);

		camera.cullingMask = (1 << layer);
		camera.targetTexture = rt;
		camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		camera.clearFlags = CameraClearFlags.Color;

		Matrix4x4 matrix = new Matrix4x4();
		matrix.SetTRS(Vector3.zero, Quaternion.identity, textureScale * Vector3.one);

		Material submeshMaterial;

		RenderTexture.active = rt;
		if(!RenderTexture.active.IsCreated())
			RenderTexture.active.Create();

		// front pass
		cameraObject.transform.position = -Vector3.forward + Vector3.up * .5f;
		cameraObject.transform.rotation = Quaternion.identity;

		for(int s = 0; s < mesh.subMeshCount; s++) {
			if(s < materials.Length)
				submeshMaterial = materials[s];
			else
				submeshMaterial = materials[materials.Length-1];
				
			Graphics.DrawMesh(mesh, matrix, submeshMaterial, layer, camera, s);
		}
		camera.Render();

		// read pixels from render texture
		Texture2D frontTexture = new Texture2D(textureSize, textureSize);
		frontTexture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
		frontTexture.Apply();
		
		// release RT
		camera.targetTexture = null;
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary(rt);

		// destroy camera		
		if(!Application.isPlaying)
			GameObject.DestroyImmediate(cameraObject);
		else
			GameObject.Destroy(cameraObject);
		
		return frontTexture;
	}

	public static Impostor GenerateImpostor(float size, int textureSize, float textureScale, Mesh mesh, Material[] materials, int layer) {
		Texture2D frontTexture = GenerateTexture(textureSize, textureScale, mesh, materials, layer);

		// create impostor
		Impostor impostor = new Impostor();
		impostor.mesh = CreateMesh(size);
		impostor.texture = frontTexture;
		
		return impostor;
	}

	public static Mesh CreateMesh(float size) {
		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[16];
		Vector2[] uvs = new Vector2[16];
		int[] triangles = new int[(vertices.Length / 2) * 3];

		float halfSize = size / 2f;
		
		// front plane
		vertices[0] = new Vector3(-halfSize, size, 0f);
		vertices[1] = new Vector3(halfSize, size, 0f);
		vertices[2] = new Vector3(-halfSize, 0f, 0f);
		vertices[3] = new Vector3(halfSize, 0f, 0f);
		uvs[0] = new Vector2(0f, 1f);
		uvs[1] = new Vector2(1f, 1f);
		uvs[2] = new Vector2(0f, 0f);
		uvs[3] = new Vector2(1f, 0f);
		triangles[0] = 0;
		triangles[1] = 2;
		triangles[2] = 1;
		triangles[3] = 1;
		triangles[4] = 2;
		triangles[5] = 3;

		// back plane
		int vertexCount = 4;
		int triangleCount = (vertexCount / 2) * 3;
		vertices[vertexCount + 0] = new Vector3(-halfSize, size, 0f);
		vertices[vertexCount + 1] = new Vector3(halfSize, size, 0f);
		vertices[vertexCount + 2] = new Vector3(-halfSize, 0f, 0f);
		vertices[vertexCount + 3] = new Vector3(halfSize, 0f, 0f);
		uvs[vertexCount + 0] = new Vector2(0f, 1f);
		uvs[vertexCount + 1] = new Vector2(1f, 1f);
		uvs[vertexCount + 2] = new Vector2(0f, 0f);
		uvs[vertexCount + 3] = new Vector2(1f, 0f);
		triangles[triangleCount + 0] = vertexCount + 0;
		triangles[triangleCount + 1] = vertexCount + 1;
		triangles[triangleCount + 2] = vertexCount + 2;
		triangles[triangleCount + 3] = vertexCount + 1;
		triangles[triangleCount + 4] = vertexCount + 3;
		triangles[triangleCount + 5] = vertexCount + 2;

		// left plane
		vertexCount = 8;
		triangleCount = (vertexCount / 2) * 3;
		vertices[vertexCount + 0] = new Vector3(0f, size, -halfSize);
		vertices[vertexCount + 1] = new Vector3(0f, size, halfSize);
		vertices[vertexCount + 2] = new Vector3(0f, 0f, -halfSize);
		vertices[vertexCount + 3] = new Vector3(0f, 0f, halfSize);
		uvs[vertexCount + 0] = new Vector2(0f, 1f);
		uvs[vertexCount + 1] = new Vector2(1f, 1f);
		uvs[vertexCount + 2] = new Vector2(0f, 0f);
		uvs[vertexCount + 3] = new Vector2(1f, 0f);
		triangles[triangleCount + 0] = vertexCount + 0;
		triangles[triangleCount + 1] = vertexCount + 2;
		triangles[triangleCount + 2] = vertexCount + 1;
		triangles[triangleCount + 3] = vertexCount + 1;
		triangles[triangleCount + 4] = vertexCount + 2;
		triangles[triangleCount + 5] = vertexCount + 3;

		// right plane
		vertexCount = 12;
		triangleCount = (vertexCount / 2) * 3;
		vertices[vertexCount + 0] = new Vector3(0f, size, -halfSize);
		vertices[vertexCount + 1] = new Vector3(0f, size, halfSize);
		vertices[vertexCount + 2] = new Vector3(0f, 0f, -halfSize);
		vertices[vertexCount + 3] = new Vector3(0f, 0f, halfSize);
		uvs[vertexCount + 0] = new Vector2(0f, 1f);
		uvs[vertexCount + 1] = new Vector2(1f, 1f);
		uvs[vertexCount + 2] = new Vector2(0f, 0f);
		uvs[vertexCount + 3] = new Vector2(1f, 0f);
		triangles[triangleCount + 0] = vertexCount + 0;
		triangles[triangleCount + 1] = vertexCount + 1;
		triangles[triangleCount + 2] = vertexCount + 2;
		triangles[triangleCount + 3] = vertexCount + 1;
		triangles[triangleCount + 4] = vertexCount + 3;
		triangles[triangleCount + 5] = vertexCount + 2;

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		TangentSolverPT.Solve(mesh);
		return mesh;
	}

	public class Impostor {
		public Mesh mesh;
		public Texture2D texture;
	}
}

}
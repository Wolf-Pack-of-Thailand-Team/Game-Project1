using UnityEngine;
using System.Collections;

namespace Planetary {

[System.Serializable()]
public class TextureSource : ModuleBase {

	private Color[] colors;
	private int width, height;

	public TextureSource(Texture2D texture) {
		colorOutput = true;

		if(texture != null) {
			width = texture.width;
			height = texture.height;
			colors = texture.GetPixels(0, 0, texture.width, texture.height);
		}
	}

	public override float GetValue(Vector3 position) {
		if(colors != null) {
			float u = -Mathf.Atan2(position.x, position.z) / (2f * Mathf.PI) + 0.5f;
			float v = Mathf.Asin(position.y) / Mathf.PI + 0.5f;
			Color pixel = GetBicubicColor(u * width, v * height);
			return pixel.grayscale;
		}
		return 0f;
	}

	public override Color32 GetColor(Vector3 position) {
		if(colors != null) {
			float u = -Mathf.Atan2(position.x, position.z) / (2f * Mathf.PI) + 0.5f;
			float v = Mathf.Asin(position.y) / Mathf.PI + .5f;
			Color pixel = GetBicubicColor(u * width, v * height);
			return pixel;
		}
		return Color.cyan;
	}

	#region Interpolation

	/// <summary>
	/// Gets the bicubicly interpolated modifier at given position. Called by the surface to get modifier height for each point.
	/// </summary>
	public Color GetBicubicColor(float row, float col) {
		int row1 = Mathf.FloorToInt(row);
		row1 = row1 < width-1 ? row1 : width-1;
		row1 = row1 < 0 ? 0 : row1;
		int col1 = Mathf.FloorToInt(col);
		col1 = col1 < height-1 ? col1 : height-1;
		col1 = col1 < 0 ? 0 : col1;
		float interX = row - row1;
		float interY = col - col1;
		
		Color[][] array = new Color[4][] { new Color[4], new Color[4], new Color[4], new Color[4]};
		
		array[0][0] = colors[WrapCoordinates(col1-1, row1-1)];
		array[0][1] = colors[WrapCoordinates(col1, row1-1)];
		array[0][2] = colors[WrapCoordinates(col1+1, row1-1)];
		array[0][3] = colors[WrapCoordinates(col1+2, row1-1)];
		
		array[1][0] = colors[WrapCoordinates(col1-1, row1)];
		array[1][1] = colors[WrapCoordinates(col1, row1)];
		array[1][2] = colors[WrapCoordinates(col1+1, row1)];
		array[1][3] = colors[WrapCoordinates(col1+2, row1)];
		
		array[2][0] = colors[WrapCoordinates(col1-1, row1+1)];
		array[2][1] = colors[WrapCoordinates(col1, row1+1)];
		array[2][2] = colors[WrapCoordinates(col1+1, row1+1)];
		array[2][3] = colors[WrapCoordinates(col1+2, row1+1)];
		
		array[3][0] = colors[WrapCoordinates(col1-1, row1+2)];
		array[3][1] = colors[WrapCoordinates(col1, row1+2)];
		array[3][2] = colors[WrapCoordinates(col1+1, row1+2)];
		array[3][3] = colors[WrapCoordinates(col1+2, row1+2)];
		
		return GetBiCubicValue(array, interX, interY);
	}

	private int WrapCoordinates(int col, int row) {
		row = row < width-1 ? row : width-1;
		row = row < 0 ? 0 : row;
		col = col < height-1 ? col : height-1;
		col = col < 0 ? 0 : col;

		return col * height + row;
	}
	
	public Color GetBiCubicValue (Color[][] p, float x, float y) {
		Color[] arr = new Color[4];
		arr[0] = GetCubicValue(p[0], y);
		arr[1] = GetCubicValue(p[1], y);
		arr[2] = GetCubicValue(p[2], y);
		arr[3] = GetCubicValue(p[3], y);
		return GetCubicValue(arr, x);
	}

	public static Color GetCubicValue (Color[] p, float x) {
		return p[1] + 0.5f * x*(p[2] - p[0] + x*(2.0f*p[0] - 5.0f*p[1] + 4.0f*p[2] - p[3] + x*(3.0f*(p[1] - p[2]) + p[3] - p[0])));
	}

	#endregion

	#region FileLoading

	public static Texture2D LoadTexture(string filename) {
		string relativePath = filename.Substring(filename.IndexOf("Resources/"));
		relativePath = relativePath.Substring(10, relativePath.Length - 14);
		
		Texture2D tex = Resources.Load(relativePath) as Texture2D;
		if(tex == null)
			Debug.LogError("Texture source node could not read texture at: " + filename);
		return tex;
	}

	#endregion
}

}
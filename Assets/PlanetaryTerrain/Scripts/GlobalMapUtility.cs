
using UnityEngine;

namespace Planetary {

/// <summary>
/// Global map utility, thanks to exiguous for original the code!
/// </summary>
public class GlobalMapUtility {

	public static Texture2D Generate(int texwidth, ModuleBase module) {

		// measure execution time
		System.DateTime startTime = System.DateTime.UtcNow;

		// creates a rectangular heightmap with "distorted" poles from the planet, texture height is calculated from width
		int texheight = texwidth / 2, ty, tx;
		Texture2D tex = new Texture2D(texwidth, texheight, TextureFormat.ARGB32, false);
		Color32[] colors = new Color32[texwidth * texheight];
		float x, y, z, polar, azimut, onebyty, onebytx, halfx, halfy, sinpolar, cospolar;
			
		onebytx = 1.0f / (float)texwidth;   // onebytx/y is an optimization to prevent recurring divisions
		onebyty = 1.0f / (float)texheight;
			
		halfx = onebytx * 0.5f;             // halfx/y is the width/height of a half pixel to calculate the midpoint of a pixel
		halfy = onebyty * 0.5f;
			
		for (ty = 0; ty < texheight; ++ty) {// calculate the 3d position of each pixel on the surface of a normalized sphere (radius 1)
				
			// description of spherical coordinate system (polar + azimut) can be found here: http://upload.wikimedia.org/wikipedia/commons/8/82/Sphericalcoordinates.svg and here http://en.wikipedia.org/wiki/Spherical_coordinate_system
			polar = ( (float)ty * onebyty + halfy ) * Mathf.PI;     // calculate the polar angle (between positive y in unity (northpole) and the line to the point in space), required in radians 0 to pi
				
			sinpolar = Mathf.Sin ( polar ); // cache these values as they are the same for each column in the next line
			cospolar = Mathf.Cos ( polar );
			
			for (tx = 0; tx < texwidth; ++tx) {
				azimut = ( ( ( (float)(tx+texwidth/4) * onebytx + halfx ) * 2.0f ) - 1.0f ) * Mathf.PI;      // calculate the azimut angle (between positive x axis and the line to the point in space), required in radians -pi to +pi,
				x = sinpolar * Mathf.Cos ( azimut );
				z = sinpolar * Mathf.Sin ( azimut );// this is y in the wikipedia formula but because unitys axis are differerent (y and z swapped) its changed here
				y = cospolar;// this is z in the wikipedia formula but because unitys axis are differerent (y and z swapped) its changed here
				Vector3 position = new Vector3(x, y, z);				

				Color32 color;
				if(module.colorOutput)
					color = module.GetColor(position);  // retrieve the height on the actual planet surface by passing the normalized position to the terrain module
				else {
					float value = (module.GetValue(position) + 1f) / 2f;
					color = new Color(value, value, value);
				}
				
				colors[(texheight - ty - 1) * texwidth + tx] = color;
				//tex.SetPixel(tx, texheight - ty - 1,new Color(height,height,height));    // the y direction needs to be reversed to match unity up (+y) equals texture up, otherwise the texture is y-reversed compared with the planet (maybe due to v-definition)
			}
		}
		tex.SetPixels32(colors);
		tex.Apply(); // apply the texture to make the changes persistent

		Debug.Log("Global map generated in " + (float)(System.DateTime.UtcNow - startTime).TotalSeconds * 1000f + "ms");

		return tex;
		
		/*string fileName = "Planetary_MiniMap.png";
		byte[] bytes = tex.EncodeToPNG();
		System.IO.FileStream   fs = new System.IO.FileStream(fileName,  System.IO.FileMode.Create);
		System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs);
		bw.Write(bytes);
		bw.Close();
		fs.Close();*/

		// exiguous ***************************************************************************************************
		// exiguous ***************************************************************************************************
	}
}

}
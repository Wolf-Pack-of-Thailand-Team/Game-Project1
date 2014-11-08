using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SurfaceColor))]
public class SurfaceColorEditor : Editor {
	
	override public void OnInspectorGUI() {
		SurfaceColor surfaceColor = (SurfaceColor)target;

		EditorGUILayout.LabelField("Shader:");
		surfaceColor.shader = (Shader)EditorGUILayout.ObjectField(surfaceColor.shader, typeof(Shader), false);
		
		EditorGUILayout.LabelField("Height Coloring");
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Threshold 1:");
		surfaceColor.value1 = EditorGUILayout.Slider(surfaceColor.value1, .0f, .99f);
	    EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Threshold 2:");
		surfaceColor.value2 = EditorGUILayout.Slider(surfaceColor.value2, .0f, .99f);
	    EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Threshold 3:");
		surfaceColor.value3 = EditorGUILayout.Slider(surfaceColor.value3, .0f, 2f);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Color 1:");
		surfaceColor.color1 = EditorGUILayout.ColorField(surfaceColor.color1);
	    EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Color 2:");
		surfaceColor.color2 = EditorGUILayout.ColorField(surfaceColor.color2);
	    EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Color 3:");
		surfaceColor.color3 = EditorGUILayout.ColorField(surfaceColor.color3);
	    EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Color 4:");
		surfaceColor.color4 = EditorGUILayout.ColorField(surfaceColor.color4);
	    EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Brightness Modifier:");
		surfaceColor.brightness = EditorGUILayout.FloatField(surfaceColor.brightness);
	    EditorGUILayout.EndHorizontal();
		
		
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Polar Regions");
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Polarity:");
		surfaceColor.polarity = EditorGUILayout.Slider(surfaceColor.polarity, 0f, 1f);
	    EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Polar Color:");
		surfaceColor.polarColor = EditorGUILayout.ColorField(surfaceColor.polarColor);
	    EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Polar Strength:");
		surfaceColor.polarStrenght = EditorGUILayout.FloatField(surfaceColor.polarStrenght);
	    EditorGUILayout.EndHorizontal();
		
		
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Equator");
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Equator Width:");
		surfaceColor.equatorWidth = EditorGUILayout.Slider(surfaceColor.equatorWidth, 0f, 1f);
	    EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Equator Height:");
		surfaceColor.equatorHeight = EditorGUILayout.Slider(surfaceColor.equatorHeight, -1f, 1f);
	    EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Equator Color:");
		surfaceColor.equatorColor = EditorGUILayout.ColorField(surfaceColor.equatorColor);
	    EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Equator Strength:");
		surfaceColor.equatorStrenght = EditorGUILayout.FloatField(surfaceColor.equatorStrenght);
	    EditorGUILayout.EndHorizontal();
		
		
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Textures");
		
		surfaceColor.useTextures= EditorGUILayout.Toggle("Use textures:", surfaceColor.useTextures);
		if(surfaceColor.useTextures) {
			EditorGUILayout.BeginHorizontal();
			surfaceColor.texture1 = (Texture2D)EditorGUILayout.ObjectField(surfaceColor.texture1, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));

			surfaceColor.texture2 = (Texture2D)EditorGUILayout.ObjectField(surfaceColor.texture2, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));

			surfaceColor.texture3 = (Texture2D)EditorGUILayout.ObjectField(surfaceColor.texture3, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));

			surfaceColor.texture4 = (Texture2D)EditorGUILayout.ObjectField(surfaceColor.texture4, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
			EditorGUILayout.EndHorizontal();
			
			surfaceColor.textureScales = EditorGUILayout.Vector4Field("Texture Scales:", surfaceColor.textureScales);
		}
		
		
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Bump Maps");
		
		surfaceColor.useBumpMaps= EditorGUILayout.Toggle("Use Bump Maps:", surfaceColor.useBumpMaps);
		if(surfaceColor.useBumpMaps) {
			EditorGUILayout.BeginHorizontal();
			surfaceColor.normal1 = (Texture2D)EditorGUILayout.ObjectField(surfaceColor.normal1, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));

			surfaceColor.normal2 = (Texture2D)EditorGUILayout.ObjectField(surfaceColor.normal2, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));

			surfaceColor.normal3 = (Texture2D)EditorGUILayout.ObjectField(surfaceColor.normal3, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));

			surfaceColor.normal4 = (Texture2D)EditorGUILayout.ObjectField(surfaceColor.normal4, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
			EditorGUILayout.EndHorizontal();
			
			surfaceColor.normalScales = EditorGUILayout.Vector4Field("Bump Map Scales:", surfaceColor.normalScales);
		}
		
		if(GUI.changed) {
			surfaceColor.Apply();
		}
	}
}

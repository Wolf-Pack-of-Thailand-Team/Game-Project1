using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System;

namespace Planetary {

[System.Serializable()]
public class TerrainModule
{
	[System.NonSerialized] public List<Node> nodes = new List<Node>();
	
	public Node[] serializedNodes;
	public OutputNode output;
	public List<TextureNode> textureNodes;
	
	public bool randomizeSeeds = false;
	public float seed;
	public int[] seeds;
	public float frequencyScale = 1f;
	
	public float alphaScale = 1.0f;
	
	public float color1pos, color2pos, color3pos, color4pos;
	
	[System.NonSerialized] public ModuleBase module;
	
	public void ReloadModules() {
		// recreate noise modules
		nodes = new List<Node>(serializedNodes);
		
		SetSeeds();
		SetFrequencyScale();
		
		FindOutputNode();
		module = output.GetModule();
	}
	
	public void SetSeeds() {
		if(randomizeSeeds)
			seed = UnityEngine.Random.Range(-1000f, 1000f);
		
		// find generators that have seeds
		for(int i = 0; i < nodes.Count; i++) {
			Node n = nodes[i];
			if(n is GeneratorNode) {
				GeneratorNode g = (GeneratorNode)n;
				g.seed = seed + i;
			}
		}
	}
	
	public void SetFrequencyScale() {
		// find generators that have frequency
		foreach(Node n in nodes) {
			if(n is GeneratorNode) {
				GeneratorNode g = (GeneratorNode)n;
				g.frequency *= frequencyScale;
			}
			if(n is MacroNode) {
				MacroNode m = (MacroNode)n;
				m.frequencyScale = frequencyScale;
			}
		}
	}
	
	/// <summary>
	/// Save to file 
	/// </summary>
	/// <param name="filename">
	/// A <see cref="System.String"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.Boolean"/>
	/// </returns>
	public bool Save(string filename) {
		// make sure there is output node
		if(!FindOutputNode()) {
			return false;
		}
		
		// find generators that have seeds
		List<GeneratorNode> generators = new List<GeneratorNode>();
		foreach(Node n in nodes) {
			if(n is GeneratorNode) {
				GeneratorNode g = (GeneratorNode)n;
				generators.Add(g);
			}
		}
		seeds = new int[generators.Count];
		
		// convert nodes to array for serialization
		serializedNodes = nodes.ToArray();
		
		// bool to track file writing success
		bool success;
		
		// save to file
		FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
		try {
			
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(fs, this);
			success = true;
		}
		catch(IOException e) {
			Debug.LogError(e.ToString());
			success = false;
		}
		finally {
			fs.Close();
		}
		
		return success;
	}
	
	/// <summary>
	/// Load from file 
	/// </summary>
	/// <param name="filename">
	/// A <see cref="System.String"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.Boolean"/>
	/// </returns>
	public static TerrainModule Load(string filename, bool randomize, float seed, float frequencyScale) {
		string relativePath = filename.Substring(filename.IndexOf("Resources/"));
		relativePath = relativePath.Substring(10, relativePath.Length - 14);
			
	    TextAsset ta = Resources.Load(relativePath) as TextAsset;

		return LoadTextAsset(ta, randomize, seed, frequencyScale);
	}
	
	/// <summary>
	/// Loads from text asset.
	/// </summary>
	public static TerrainModule LoadTextAsset(TextAsset ta, bool randomize, float seed, float frequencyScale) {
		TerrainModule ps = null;
		
		try {
	    	Stream s = new MemoryStream(ta.bytes);
	   	 	BinaryFormatter formatter = new BinaryFormatter();
	   	 	ps = (TerrainModule)formatter.Deserialize(s);
	    	s.Close();
			
			// retrieve node list
			ps.randomizeSeeds = randomize;
			ps.seed = seed;
			ps.frequencyScale = frequencyScale;
			ps.ReloadModules();
	    } catch (Exception e){
	    	Debug.Log("Terrain Module file could not be loaded. " + e.Message);
	    }

		return ps;
	}

	private bool FindOutputNode() {
		// find outputnode and texture nodes
		output = null;
		textureNodes = new List<TextureNode>();
		for(int i = 0; i < nodes.Count; i++) {
			Node n = nodes[i];
			if(n is OutputNode) {
				output = (OutputNode)n;
			}
			if(n is TextureNode) {
				textureNodes.Add((TextureNode)n);
			}
		}
		if(output == null) {
			Debug.LogError("OutputNode not found");
			return false;
		}
		return true;
	}
	
	// Deserialization binder
	public sealed class VersionDeserializationBinder : SerializationBinder
	{
	    public override System.Type BindToType( string assemblyName, string typeName )
	    {
	        if ( !string.IsNullOrEmpty( assemblyName ) && !string.IsNullOrEmpty( typeName ) )
	        {
	            System.Type typeToDeserialize = null;
	
	            assemblyName = Assembly.GetExecutingAssembly().FullName;
	
	            // The following line of code returns the type.
	            typeToDeserialize = System.Type.GetType( string.Format( "{0}, {1}", typeName, assemblyName ) );
	
	            return typeToDeserialize;
	        }
	
	        return null;
	    }
	}
}

}
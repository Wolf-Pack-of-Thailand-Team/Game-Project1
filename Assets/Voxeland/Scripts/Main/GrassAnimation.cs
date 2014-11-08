using UnityEngine;
using System.Collections;

namespace Voxeland 
{
	public class GrassAnimation : MonoBehaviour 
	{
		public float grassAnimState = 0.5f;
		public float grassAnimSpeed = 0.75f;
		
		void Update () 
		{
			grassAnimState += Time.deltaTime * grassAnimSpeed;
			grassAnimState = Mathf.Repeat(grassAnimState, 6.283185307179586476925286766559f);
			Shader.SetGlobalFloat("_GrassAnimState", grassAnimState);
		}
	}

}

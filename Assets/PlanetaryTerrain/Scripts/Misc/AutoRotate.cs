using UnityEngine;
using System.Collections;

namespace Planetary {

public class AutoRotate : MonoBehaviour {

	public Vector3 rotation = new Vector3(0,0,0);
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(rotation * Time.deltaTime);
	}
}

}
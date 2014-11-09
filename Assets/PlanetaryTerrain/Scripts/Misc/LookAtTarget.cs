using UnityEngine;
using System.Collections;

namespace Planetary {

public class LookAtTarget : MonoBehaviour {
	
	public Transform target;

	void Update () {
		transform.LookAt(target);
	}
}

}
using UnityEngine;
using System.Collections;

namespace Planetary {

public class Jetpack : MonoBehaviour {

	public float force = 2f;
	public Transform planet;

	void Update () {
		Vector3 gravityVector = (planet.position - transform.position).normalized;

		if(Input.GetKey(KeyCode.Q))
			rigidbody.AddForce(force * gravityVector, ForceMode.Impulse);
		if(Input.GetKey(KeyCode.E))
			rigidbody.AddForce(-force * gravityVector, ForceMode.Impulse);
	}
}

}
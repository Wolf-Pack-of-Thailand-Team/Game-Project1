using UnityEngine;
using System.Collections;

namespace Planetary {

public class Gravity : MonoBehaviour {
	
	public Transform planet;
	public float gravity = 9.81f;
	
	void FixedUpdate () {
		Vector3 direction = planet.position - transform.position;
		rigidbody.AddForce(direction.normalized * gravity);
	}
}

}
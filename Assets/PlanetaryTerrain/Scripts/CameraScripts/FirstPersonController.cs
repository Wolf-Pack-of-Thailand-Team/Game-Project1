using UnityEngine;
using System.Collections;

namespace Planetary {

public class FirstPersonController : MonoBehaviour {

	public Transform planet;
	public new Transform camera;

	public float speed = 10f, speedBoostMultiplier = 2f;
	public float gravity = 9.81f;

	public float sensitivityX = 15f;
	public float sensitivityY = 15f;
	public float minimumY = -90f;
	public float maximumY = 90f;
	private float rotationY = 0f;
		
	private Vector3 targetVelocity = Vector3.zero, currentVelocity = Vector3.zero, velocityChange = Vector3.zero;
	private Quaternion targetRotation;

	void Update () {
		Vector3 gravityVector = -transform.up;
		if(planet != null) {
			// align to planet surface
			gravityVector = (planet.position - transform.position).normalized;
			targetRotation = Quaternion.LookRotation(transform.forward, -gravityVector);
			transform.rotation = targetRotation;
		}

		// movement
		targetVelocity.x = Input.GetAxis("Horizontal");
		targetVelocity.y = 0f;
		targetVelocity.z = Input.GetAxis("Vertical");
	
		// shift: speed boost
		if(Input.GetKey(KeyCode.LeftShift))
			targetVelocity *= speed * speedBoostMultiplier;
		else
			targetVelocity *= speed;
	
		// movement forces
		currentVelocity = transform.InverseTransformDirection(rigidbody.velocity);
		currentVelocity.y = 0f;
		velocityChange = transform.TransformDirection((targetVelocity - currentVelocity));
		rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

		// gravity
		rigidbody.AddForce(gravity * gravityVector);
		
		// mouse look
		// rotate whole controller according to mouse X
		transform.Rotate(0f, Input.GetAxis("Mouse X") * sensitivityX, 0f);

		// rotate only the camera according to mouse Y
		rotationY += -Input.GetAxis("Mouse Y") * sensitivityY;
		rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
		camera.localEulerAngles = new Vector3(rotationY, 0f, 0f);
	}
}

}
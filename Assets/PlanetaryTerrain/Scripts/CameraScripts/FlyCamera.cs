using UnityEngine;
using System.Collections;

namespace Planetary {

public class FlyCamera : MonoBehaviour 
{
	public float speed = .1f;
	public float rotationSpeed = 3f;
	public float alignDistance = 200f;
	public Transform planet;
	
	private float currentSpeed;
	private Vector3 velocity = Vector3.zero;
	private Quaternion targetRotation = Quaternion.identity;
	
	void FixedUpdate () {
		// turn camera towards mouse
		if(Input.GetMouseButton(0)){
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			targetRotation = Quaternion.LookRotation((ray.origin + ray.direction * 10f) - transform.position, transform.up);
			transform.rotation = Quaternion.Slerp(transform.rotation,targetRotation, Time.fixedDeltaTime * rotationSpeed);
		}
		
		// align to planet surface
		if(planet != null) {
			float distanceToPlanetCore = Vector3.Distance(transform.position, planet.position);
			//Debug.Log(distanceToPlanetCore);
			if(distanceToPlanetCore < alignDistance) {
				Vector3 gravityVector = planet.position - transform.position;
				targetRotation = Quaternion.LookRotation(transform.forward, -gravityVector);
				transform.rotation = Quaternion.Slerp(transform.rotation,targetRotation, Time.fixedDeltaTime * rotationSpeed * 3f);
			}
		}
		
		velocity.z = Input.GetAxis("Vertical");
		velocity.x = Input.GetAxis("Horizontal");
		
		currentSpeed = speed;
		if(Input.GetKey(KeyCode.LeftShift))
			currentSpeed = speed * 10f;
		if(Input.GetKey(KeyCode.LeftAlt))
			currentSpeed = speed * 100f;
		
		transform.Translate(velocity * currentSpeed);
	}
}

}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VoxelandDemo
{
	public class HeroController : MonoBehaviour 
	{
		public Transform hero;
		
		public bool cameraSpace = true;
		public float moveSpeed = 10f;
		public float shiftAcceleration = 4f;
		public float smooth = 0.05f;
		
		private Vector3 smoothMoveVelocity = new Vector3(0,0,0);
		Vector3 oldHeroPosition = new Vector3(0,0,0);
		
		Vector3 rigidbodyPos = new Vector3(0,0,0);
	
		
		public void Start ()
		{
			if (hero==null) hero = transform;
			
			rigidbodyPos = hero.transform.position;
			oldHeroPosition = hero.transform.position;
		}
		
		public void FixedUpdate ()
		{
			//adding rigidbody
			if (hero.rigidbody==null) 
			{ 
				hero.gameObject.AddComponent<Rigidbody>();
				hero.rigidbody.useGravity = false;
				hero.rigidbody.freezeRotation = true;
			}
			
			Vector3 velocity = new Vector3(0,0,0);
			
			//determining look direction
			Vector3 lookDir;
			if (cameraSpace) lookDir = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
			else lookDir = hero.transform.forward;
			
			lookDir = lookDir.normalized;
			Vector3 strafeDir= Vector3.Cross(Vector3.up, lookDir);
	
			//moving
			float shiftMod = 1f;
			if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) shiftMod = shiftAcceleration; 
	
			if (Input.GetKey (KeyCode.W)) {velocity += lookDir; hero.rigidbody.useGravity = true;}
			if (Input.GetKey (KeyCode.S)) {velocity -= lookDir; hero.rigidbody.useGravity = true;}
			if (Input.GetKey (KeyCode.D)) {velocity += strafeDir; hero.rigidbody.useGravity = true;}
			if (Input.GetKey (KeyCode.A)) {velocity -= strafeDir; hero.rigidbody.useGravity = true;}
			
			velocity = Vector3.ClampMagnitude(velocity, 1f);
			velocity = velocity*moveSpeed*shiftMod; //no need for *Time.deltaTime
			
			velocity = new Vector3(velocity.x, hero.rigidbody.velocity.y, velocity.z);
			
			hero.rigidbody.velocity = velocity;
		}
		
		public void Update ()
		{
			rigidbodyPos = hero.transform.position;
	
			hero.transform.position = Vector3.SmoothDamp(oldHeroPosition, rigidbodyPos, ref smoothMoveVelocity, smooth);
			oldHeroPosition = hero.transform.position;
		}
		
		//returning rigidbody position after render
		public void OnRenderObject () 
		{
			hero.transform.position = rigidbodyPos; 
		}
		
	}
}
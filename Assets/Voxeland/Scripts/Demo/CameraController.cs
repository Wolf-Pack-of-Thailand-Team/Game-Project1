using UnityEngine;
using System.Collections;

namespace VoxelandDemo
{

	public class CameraController : MonoBehaviour 
	{
		public Camera cam;
		public Transform hero;
		
		public bool movable;
		public bool follow;
		
		private Vector3 pivot = new Vector3(0,0,0);
		
		public bool lockCursor = false; //no mouse 1 reqired
		public float elevation = 1.5f;
		public float sensitivity = 1f;

		private float rotationX = 0;
		private float rotationY = 190;
	
	
		public void Start ()
		{
			if (cam==null) cam = Camera.main;
			if (hero==null) hero = ((HeroController)FindObjectOfType(typeof(HeroController))).transform;
			pivot = cam.transform.position;
		}
		
		public void LateUpdate () //updating after hero is moved and all other scene changes made
		{
			//locking cursor
			Screen.lockCursor = lockCursor;
			
			//reading controls
			if (Input.GetMouseButton(1) || lockCursor)
			{
				rotationY += Input.GetAxis("Mouse X")*sensitivity; //note that axises from screen-space to world-space are swept!
				rotationX -= Input.GetAxis("Mouse Y")*sensitivity;
				rotationX = Mathf.Min(rotationX, 89.9f);
			}
			
			//setting cam
			pivot = hero.position + new Vector3(0, elevation, 0);
			cam.transform.localEulerAngles = new Vector3(rotationX, rotationY, 0); //note that this is never smoothed
			cam.transform.position = pivot;
		}
	}

}

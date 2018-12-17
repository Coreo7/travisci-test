using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollyZoome : MonoBehaviour {
	public Transform target;

	Camera cam;
	float distance=0f;
	float fov = 60;

	float viewWidth = 10f; 

	void Start () {
		cam = Camera.main;
	}
	
	void Update () 
	{
		Vector3 pos = cam.transform.position;

		fov = cam.fieldOfView;
		distance = viewWidth / ( 2f*Mathf.Tan(0.5f*fov*Mathf.Deg2Rad) );

		pos.z = -Mathf.Abs(distance);
		cam.transform.position = pos;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour {

	public Transform PlayerAnchor, LowerUI;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdateTracking();
	}

	public void UpdateTracking()
	{
		transform.position = PlayerAnchor.position;
		transform.eulerAngles = new Vector3(0, PlayerAnchor.eulerAngles.y, 0);
		LowerUI.LookAt(PlayerAnchor.position, Vector3.up);
	}
}

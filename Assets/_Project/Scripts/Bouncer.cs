using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bouncer : MonoBehaviour {

	public List<Transform> transforms;
	public float MoveDelay = 1f, MoveTime = 2f;

	Sequence moveSequence;

	// Use this for initialization
	void Start () 
	{
		moveSequence = DOTween.Sequence();
		CreateSequence();
		StartBouncer();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void CreateSequence()
	{
		foreach(Transform trans in transforms)
		{
			moveSequence.Append(transform.DOMove(new Vector3(trans.position.x,trans.position.y,trans.position.z), MoveTime, false));
		}
	}

	void StartBouncer()
	{
		moveSequence.SetLoops(-1).Play();
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using DG.Tweening;

public class CamDirector : MonoBehaviour {

	public List<Transform> camPosition;
	public float SwapTimer = 10f, TransitionSpeed = 1f;

	public int CurrentCam = 0;
	public GameObject Player;
	public Transform CurrentTarget;
	public Transform Center, HomeHoop, HomeR, HomeL, AwayHoop, AwayR, AwayL, Gate;
	public bool ShouldLockTarget = false;
	public Colorful.GrainyBlur GrainyBlur;

	float swapTimeRemaining = 0;

	// Use this for initialization
	void Start () {
		camPosition = new List<Transform>();
		
		foreach(Transform child in GetComponentsInChildren<Transform>())
		{
			camPosition.Add(child);
		}

		CurrentTarget = Center;

		swapTimeRemaining = SwapTimer;
		Debug.Log(XRDevice.fovZoomFactor);
		Debug.Log(XRDevice.isPresent);
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetKeyUp(KeyCode.Period))
		{
			SwapCamForward();
		}

		if(Input.GetKeyUp(KeyCode.Comma))
		{
			XRDevice.fovZoomFactor -= .1f;
			//ShouldLockTarget = !ShouldLockTarget;
		}

		if(Input.GetKeyUp(KeyCode.Keypad1))
		{
			CurrentTarget = HomeL;
		}
		if(Input.GetKeyUp(KeyCode.Keypad2))
		{
			CurrentTarget = HomeHoop;
		}
		if(Input.GetKeyUp(KeyCode.Keypad3))
		{
			CurrentTarget = HomeR;
		}
		if(Input.GetKeyUp(KeyCode.Keypad5))
		{
			CurrentTarget = Center;
		}
		if(Input.GetKeyUp(KeyCode.Keypad7))
		{
			CurrentTarget = AwayL;
		}
		if(Input.GetKeyUp(KeyCode.Keypad8))
		{
			CurrentTarget = AwayHoop;
		}
		if(Input.GetKeyUp(KeyCode.Keypad9))
		{
			CurrentTarget = AwayR;
		}
		if(Input.GetKeyUp(KeyCode.KeypadMinus))
		{
			CurrentTarget = Gate;
		}

		if(swapTimeRemaining <= 0)
		{
			SwapCamForward();
			swapTimeRemaining = SwapTimer;
		}
		else
		{
			swapTimeRemaining -= Time.deltaTime;
		}
	}

	void SwapCamForward()
	{
		StartCoroutine(SwapIt());
	}

	IEnumerator SwapIt()
	{
		if(TransitionSpeed != 0)
		{
			Sequence transSeq = DOTween.Sequence();

			transSeq.Append(DOTween.To(x => GrainyBlur.Radius = x, 0, 19, TransitionSpeed));
			transSeq.Append(DOTween.To(x => GrainyBlur.Radius = x, 19, 0, TransitionSpeed));

			transSeq.Play();
		}

		yield return new WaitForSeconds(TransitionSpeed);
		
		CurrentCam++;
		if(CurrentCam >= camPosition.Count)
		{
			CurrentCam = 0;
		}

		Player.transform.position = camPosition[CurrentCam].position;

		if(ShouldLockTarget)
		{
			SetViewToTarget(CurrentTarget);
		}
	}

	void SetViewToTarget(Transform _target)
	{
		var lookpos = camPosition[CurrentCam].position - _target.position;


		var rotation = Quaternion.LookRotation(-lookpos);

		//camPosition[CurrentCam].LookAt(Target);

		Player.transform.rotation = rotation;
		Player.transform.localEulerAngles = new Vector3(0, Player.transform.localEulerAngles.y);

	}
}

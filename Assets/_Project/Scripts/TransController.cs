using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TransController : MonoBehaviour {

	public Material FlatVid, Curved, Sphere, SpherePatch, FlatSport, CurvedSport;

	public float PreTransitionWaitTime = 2, TransitionDelay = 0, SwapTimer = 10;

	public UIController MyUIController;

	public AnimationCurve BounceCurve;

	public CanvasGroup LowerUIAlpha;

	public Image RadialLoader;

	[SerializeField]
	int CurrentContent = 0;

	[SerializeField]
	float PopupDist = 10, PopupTime = 1;

	float timePassed = 0, padding, timeToSwap = 0;
	bool shouldFillLoader = false;

	// Use this for initialization
	void Start () 
	{
		Curved.DOFade(0, 0);
		FlatVid.DOFade(0, 0);
		Sphere.DOFade(0,0);

        //m_mediaPlayerComponent.m_StereoPacking = StereoPacking.TopBottom;
    	//m_application.m_videoSphere.GetComponent<UpdateStereoMaterial>()._camera = m_application.HMD;
            
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(shouldFillLoader)
		{
			timePassed += Time.deltaTime;
			RadialLoader.fillAmount = timePassed / PreTransitionWaitTime;
		}

		if(Input.GetKeyDown(KeyCode.Period))
		{
			if(padding > (PopupTime + PreTransitionWaitTime))
			{
				StartCoroutine(BeginTransition());
			}
		}

		padding += Time.deltaTime;

		if(timeToSwap < SwapTimer)
		{
			timeToSwap += Time.deltaTime;
		}
		else
		{
			timeToSwap = 0;
			
			if(padding > (PopupTime + PreTransitionWaitTime))
			{
				StartCoroutine(BeginTransition());
			}
		}
	}

	IEnumerator BeginTransition()
	{
		shouldFillLoader = false;
		RadialLoader.fillAmount = 0;
		padding = 0;
		timePassed = 0;

		PopupLoadingIcon();
		yield return new WaitForSeconds(PopupTime);
		shouldFillLoader = true;
		yield return new WaitForSeconds(PreTransitionWaitTime);

		switch(CurrentContent)
		{
			case 0:
			{
				TransitionMaterial(FlatVid, Sphere);
				StartCoroutine(fadeLogo(FlatVid, FlatSport));
				CurrentContent++;
				break;
			}
			case 1:
			{
				TransitionMaterial(Curved, FlatSport);
				StartCoroutine(fadeLogo(Curved, CurvedSport));
				CurrentContent++;
				break;
			}
			case 2:
			{
				TransitionMaterial(Sphere, CurvedSport);
				CurrentContent = 0;
				break;
			}
		}
		LowerUIAlpha.DOFade(0,1);
	}

	IEnumerator fadeLogo(Material toFade, Material toReveal)
	{
		yield return new WaitForSeconds(2);
		toReveal.DOFade(1,0);
		toFade.DOFade(0, 1);
	}

	void PopupLoadingIcon()
	{
		LowerUIAlpha.alpha = 0;
		LowerUIAlpha.DOFade(1,PopupTime);
		var curPos = MyUIController.LowerUI;
		MyUIController.LowerUI.GetComponent<RectTransform>().localScale = new Vector3(0,0,0);
		MyUIController.LowerUI.position = new Vector3(curPos.position.x, curPos.position.y - PopupDist, curPos.position.z);
		MyUIController.LowerUI.DOMove(new Vector3(curPos.position.x, curPos.position.y + PopupDist, curPos.position.z), PopupTime, false).SetEase(BounceCurve);
		MyUIController.LowerUI.GetComponent<RectTransform>().DOScale(new Vector3(.01f,.01f,1), PopupTime).SetEase(BounceCurve);
	}
	void TransitionMaterial(Material InMat, Material OutMat)
	{
		if(InMat != Sphere && OutMat != Sphere)
		{
			Sequence transSeq = DOTween.Sequence();

			transSeq.Append(OutMat.DOFade(0, 1f));
			transSeq.Insert(TransitionDelay, InMat.DOFade(1, 1f));

			transSeq.Play();
		}
		else if (InMat == Sphere)
		{
			Sequence transSeq = DOTween.Sequence();

			transSeq.Append(OutMat.DOFade(0, 1f));
			transSeq.Insert(TransitionDelay, SpherePatch.DOFade(1, 1));
			transSeq.Insert(TransitionDelay, DOTween.ToAlpha(() => Sphere.color, x=> Sphere.color = x, 1, 1f));
			transSeq.Append( SpherePatch.DOFade(0, 1));


			transSeq.Play();
		}
		else if(OutMat == Sphere)
		{
			Sequence transSeq = DOTween.Sequence();

			transSeq.Append(DOTween.ToAlpha(() => Sphere.color, x=> Sphere.color = x, 0, 1f));
			transSeq.Insert(TransitionDelay, InMat.DOFade(1, 1f));

			transSeq.Play();
		}
	}
}

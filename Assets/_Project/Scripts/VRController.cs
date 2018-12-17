using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using DG.Tweening;
using UnityEngine.UI;

public class VRController : MonoBehaviour {

	GameObject RightController;
	
	public SteamVR_Input_Sources handType;
	public SteamVR_Action_Boolean grabAction;

	public Hand hand;

	public Transform ControllerTransform;

	public Animator MyAnimator, SecondScreen;

    public GameObject HoopSphere, CenterSphere;
	
	public float FadeStartTime = .25f, FadeDuration = .25f, animationSpeed = 2, MaxRayDistance = 500f;

    public Material HoopMat, MidMat, LeftMat, RightMat;

	public LineRenderer lineRenderer;

	public LayerMask myLayerMask;

    public CanvasGroup HoopCanvas, MidCanvas, LeftCanvas, RightCanvas;

    public float WaitTimer = 1.5f;

    float waitRemaining = 0;
    bool isClickable = true;

    [SerializeField]
    private GameObject previousbutton;

    
	SteamVR_TrackedObject trackedObj;
	
	// Use this for initialization
	void Start () {
		Setup();
	}

    public void MainToMid()
    {
        MoveTo(MidCanvas, HoopCanvas, 90, MidMat, -.25f);
    }

    public void MainToLeft()
    {
        MoveTo(LeftCanvas, HoopCanvas, 50, LeftMat, -.14f);
    }

    public void MainToRight()
    {
        MoveTo(RightCanvas, HoopCanvas, 130, RightMat, -.36f);
    }

    public void LeftToMid()
    {
        MoveTo(MidCanvas, LeftCanvas, 126, MidMat, -.352f);
    }

    public void LeftToHoop()
    {
        MoveTo(HoopCanvas, LeftCanvas, 230, HoopMat, .37f);
    }

    public void MidToHoop()
    {
        MoveTo(HoopCanvas, MidCanvas, -90, HoopMat, .25f);
    }

    public void MidToRight()
    {
        MoveTo(RightCanvas, MidCanvas, 230, RightMat, .37f);
    }

    public void MidToLeft()
    {
        MoveTo(LeftCanvas, MidCanvas, 0, LeftMat, 0);
    }

    public void RightToMid()
    {
        MoveTo(MidCanvas, RightCanvas, 50, MidMat, -.138f);
    }

    public void RightToHoop()
    {
        MoveTo(HoopCanvas, RightCanvas, -50, HoopMat, .14f);
    }

    void MoveTo(CanvasGroup _buttonoN, CanvasGroup _buttonOff, float _rotationAngle, Material _newMat, float _materialRotation)
    {
        _buttonOff.interactable = false;
        _buttonOff.blocksRaycasts = false;
        _buttonOff.DOFade(0, 1);

        CenterSphere.transform.eulerAngles = new Vector3(0, _rotationAngle);
        HoopSphere.transform.eulerAngles = new Vector3(0, _rotationAngle);

        HoopSphere.GetComponent<Renderer>().material = CenterSphere.GetComponent<Renderer>().material;
        CenterSphere.GetComponent<Renderer>().material = _newMat;

        CenterSphere.GetComponent<Renderer>().material.DOOffset(new Vector2(_materialRotation, 0), 0);
        HoopSphere.GetComponent<Renderer>().material.DOOffset(new Vector2(_materialRotation, 0), 0);

        MyAnimator.SetTrigger("WarpStretch");
        SecondScreen.SetTrigger("WarpStretch");

        MyAnimator.speed = animationSpeed;
        SecondScreen.speed = animationSpeed;

        PlayWarpAnim(CenterSphere, HoopSphere);

        _buttonoN.interactable = true;
        _buttonoN.blocksRaycasts = true;
        _buttonoN.DOFade(1, 0);
    }
	
	// Update is called once per frame
	void Update () {

		int layerMask = 1 << 8;

		layerMask = ~layerMask;

        if(waitRemaining <= 0 && isClickable == false)
        {
            isClickable = true;
        }
        else
        {
            waitRemaining -= Time.deltaTime;
        }

		if(SteamVR_Input._default.inActions.InteractUI.GetStateDown(hand.handType))
		{
            if(previousbutton != null && isClickable)
            {
                isClickable = false;
                waitRemaining = WaitTimer;

                previousbutton.GetComponent<Button>().onClick.Invoke();

                Sequence buttonAnim = DOTween.Sequence();

                buttonAnim.Append(previousbutton.GetComponent<RectTransform>().DOScale(1.25f, .125f));
                buttonAnim.Append(previousbutton.GetComponent<RectTransform>().DOScale(1.5f, .125f));

                buttonAnim.Play();

            }
        }

		if(Input.GetKeyUp(KeyCode.Period))
		{
			Setup();
		}

        Ray();
	}

	void PlayWarpAnim(GameObject _fadeInSphere, GameObject _fadeOutSphere)
	{
		Sequence mySequence = DOTween.Sequence();

        _fadeOutSphere.GetComponent<Renderer>().material.DOFade(1, 0);
        _fadeInSphere.GetComponent<Renderer>().material.DOFade(0, 0);


        Material outMat = _fadeOutSphere.GetComponent<Renderer>().material;
		Material inMat = _fadeInSphere.GetComponent<Renderer>().material;

		mySequence.Insert(FadeStartTime, outMat.DOFade(0, FadeDuration));
		mySequence.Insert(FadeStartTime, inMat.DOFade(1, FadeDuration));

		mySequence.Play();
	}

	void Setup()
	{
        HoopCanvas.interactable = true;
        HoopCanvas.blocksRaycasts = true;
        HoopCanvas.DOFade(1, 0);

        HoopSphere.GetComponent<Renderer>().material = HoopMat;

        CenterSphere.transform.eulerAngles = new Vector3(0, 90);
        HoopSphere.transform.eulerAngles = new Vector3(0, 90);

        CenterSphere.GetComponent<Renderer>().material.DOOffset(new Vector2(-.25f, 0), 0);
        HoopSphere.GetComponent<Renderer>().material.DOOffset(new Vector2(-.25f, 0), 0);

        HoopSphere.GetComponent<Renderer>().material.DOFade(1,0);
		CenterSphere.GetComponent<Renderer>().material.DOFade(0,0);

        setupLineRenderer();
	}

    void setupLineRenderer()
    {
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.widthMultiplier = 0.02f;

    }

    void Ray()
    {
        Ray laserPointer = new Ray(ControllerTransform.position, ControllerTransform.forward);

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, laserPointer.origin);
            lineRenderer.SetPosition(1, laserPointer.origin + laserPointer.direction * MaxRayDistance);
        }

        RaycastHit hit;

        if(Physics.Raycast (laserPointer, out hit, MaxRayDistance, myLayerMask))
        {
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(1, hit.point);

                if (hit.transform.gameObject.GetComponent<Button>() && previousbutton == null)
                {
                    hit.transform.GetComponent<RectTransform>().DOScale(1.5f, .25f);
                    previousbutton = hit.transform.gameObject;
                }
            }
        }
        else if (previousbutton != null)
        {
            previousbutton.GetComponent<RectTransform>().DOScale(1, .25f);
            previousbutton = null;
        }
        
    }
}

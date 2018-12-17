using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baller : MonoBehaviour {

	// Use this for initialization
       public Transform Goal;
       public Animator animator;
       public float FollowDelay = .5f;
       
       private Vector3 LastPosition, CurrentPosition;
       private float followTimer;


       void Start () {
         followTimer = FollowDelay;
         FollowDatBall();
       }
	
	// Update is called once per frame
	void Update () 
   {
		if(followTimer <= 0)
      {
         followTimer = FollowDelay;
         FollowDatBall();
      }
      else
      {
         followTimer -= Time.deltaTime;
      }

      CurrentPosition = transform.position;

      if(LastPosition != CurrentPosition)
      {
         LastPosition = CurrentPosition;
         animator.SetBool("isRunning", true);
      }
      else
      {
         animator.SetBool("isRunning", false);
      }

	}

   void FollowDatBall()
   {
      UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
      agent.destination = Goal.position; 
   }
}

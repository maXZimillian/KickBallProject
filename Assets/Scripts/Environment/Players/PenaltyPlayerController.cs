using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenaltyPlayerController : MonoBehaviour
{
    private Animator animator;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Start() 
    {
        GateMove ball = GameObject.FindObjectOfType<GateMove>();
        GameController gameController = GameObject.FindObjectOfType<GameController>();
        ball.OnSwiped +=StartAnimation;
        gameController.OnRestart += ResetPosition;
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void StartAnimation() 
    {
        animator.SetTrigger("ballHit");
    }

    private void ResetPosition()
    {

        transform.position = startPosition;
        transform.rotation = startRotation;
        animator.SetTrigger("restart");
    }
}

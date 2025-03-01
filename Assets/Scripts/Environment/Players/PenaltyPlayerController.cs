using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenaltyPlayerController : MonoBehaviour
{
    private Animator animator;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private GameController gameController;
    private WebGameManager webGameManager;

    private void Start() 
    {
        GateMove ball = FindObjectOfType<GateMove>();
        gameController = FindObjectOfType<GameController>();
        webGameManager = FindObjectOfType<WebGameManager>();
        animator = GetComponent<Animator>();
        Debug.Log("WebGameManager = " + webGameManager);
        webGameManager.OnKickStartReceived += OnMessageRecieved;
        //ball.OnSwiped += StartAnimation;
        webGameManager.OnGameRestart += ResetPosition;
        
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void OnMessageRecieved(KickStartMessage message)
    {
        StartAnimation(); 
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

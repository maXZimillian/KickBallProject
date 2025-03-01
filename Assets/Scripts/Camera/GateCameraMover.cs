using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class GateCameraMover : MonoBehaviour
{
    [SerializeField] private Transform targetBall;
    [SerializeField] private Transform targetGK;
    [SerializeField] private Transform startPointBall;
    [SerializeField] private Transform startPointGK;
    [SerializeField] private float smooth= 5.0f;
    [SerializeField] private GameObject[] followingListeners;
    private Coroutine followingCoroutine;
    private GameController gameController;
    bool a = false;

    void Start()
    {
        if (followingListeners.Length == 0)
        {
            followingListeners = new GameObject[1];
            followingListeners[0] = FindObjectOfType<Follow>().gameObject;
        }
        if (targetBall == null) targetBall = FindObjectOfType<BallController>().transform;
        if (targetGK == null) targetGK = FindObjectOfType<GoalkeeperPlayerControl>().transform;
        gameController = FindObjectOfType<GameController>();
        StartFollowing();
        gameController.OnWin += StopFollowing;
        gameController.OnGameOver += StopFollowing;
    }

    private void StartFollowing()
    {
        if (a == true)
         return;
         a = true;
            foreach(GameObject listener in followingListeners)
                ExecuteEvents.Execute<IFollowing>(listener,null,(x,y) => x.ChangeTarget());
            if(followingCoroutine!=null)
                StopCoroutine(followingCoroutine);
            followingCoroutine = StartCoroutine(FollowTarget());
    }

    private void StopFollowing(int points)
    {
        StartCoroutine(WaitAndStop());
    }

    private void StopFollowing()
    {
        StartCoroutine(WaitAndStop());
    }

    private IEnumerator WaitAndStop()
    {
        yield return new WaitForSeconds(1f);
        if(followingCoroutine!=null)
            StopCoroutine(followingCoroutine);
        followingCoroutine = null;
    }

    private IEnumerator FollowTarget()
    {
        while(true)
        {
            if(gameController.playerRole == PlayerTypes.kicker)
            {
                transform.position = Vector3.Lerp (transform.position, (targetBall.position-startPointBall.position)*0.4f+startPointBall.position, Time.deltaTime * smooth);
                transform.rotation = startPointBall.rotation;
            }
            if(gameController.playerRole == PlayerTypes.goalkeeper)
            {
                transform.position = Vector3.Lerp (transform.position, (targetGK.position-startPointGK.position)*0.4f+startPointGK.position, Time.deltaTime * smooth);
                transform.rotation = startPointGK.rotation;
            }
            yield return new WaitForEndOfFrame();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalkeeperAI : MonoBehaviour
{
    [SerializeField] GameObject checkpointPlayer;
    [SerializeField] float maxJumpDistance = 2.7f;
    [SerializeField] float minWaitTime = 1.5f;
    [SerializeField] float maxWaitTime = 3.5f;
    [SerializeField] List<Transform> positions;
    [SerializeField] float delay = 2.3f;
    [SerializeField] float accuracy = 2.3f;
    [SerializeField] Rigidbody player;
    [SerializeField] Vector3 colliderSize = new Vector3(1.5f, 2.7f, 1);

    private Animator animator;
    private BoxCollider col;
    private Coroutine movingCoroutine;
    private bool isMoving = false;

    private void Start() 
    {
        animator = checkpointPlayer.GetComponent<Animator>();
        col = checkpointPlayer.GetComponent<BoxCollider>();
        col.size = colliderSize;
        col.center = new Vector3(0, 1.3f, 0);
        if(player==null)player = FindObjectOfType<BallController>().gameObject.GetComponent<Rigidbody>();
        FindObjectOfType<GameController>().OnRestart += ResetTrigger;
        movingCoroutine = StartCoroutine(MoveBetweenPositions());
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StopCoroutine(movingCoroutine);
            movingCoroutine = null;
            animator.SetTrigger("BackToIdle");
            StartCoroutine(ChangeAnimation(other.gameObject));
            GetComponent<Collider>().enabled=false;
        }
    }

    private IEnumerator ChangeAnimation(GameObject other)
{
    Vector3 ballVelocity = other.GetComponent<Rigidbody>().velocity;
    Vector3 ballOffset = other.transform.position - checkpointPlayer.transform.position;

    // Проверка: движется ли мяч по Z
    if (Mathf.Approximately(ballVelocity.z, 0)) yield break;



    float timeToImpact = Mathf.Abs(ballOffset.z / ballVelocity.z); // Время до столкновения
    float g = Physics.gravity.y; // Гравитация в Unity (обычно -9.81)
    float gravityScaler = 0.5f; // Скорректированный коэффициент гравитации

    // Корректный расчет координат
    float predictedX = other.transform.position.x + ballVelocity.x * timeToImpact;
    float predictedY = other.transform.position.y + ballVelocity.y * timeToImpact + 0.5f * g * gravityScaler * Mathf.Pow(timeToImpact, 2);


    //GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere); // marker
    //Destroy(marker.GetComponent<Collider>());
    //marker.transform.position = new Vector3(predictedX, predictedY, checkpointPlayer.transform.position.z);
    //marker.transform.localScale = Vector3.one * 0.2f;
    //marker.GetComponent<Renderer>().material.color = Color.green;

    yield return new WaitForSeconds(timeToImpact * 0.12f);

    // Вычисление смещения для анимации
    Vector2 jumpOffset = new Vector2(predictedX - checkpointPlayer.transform.position.x, predictedY - checkpointPlayer.transform.position.y);

    // Ограничение максимального расстояния прыжка
    jumpOffset.x = Mathf.Clamp(jumpOffset.x, -maxJumpDistance, maxJumpDistance);

    Vector3 movement = new Vector3(jumpOffset.x * 2f, jumpOffset.y * 2f, 0.0f);
    BoxCollider col = checkpointPlayer.GetComponent<BoxCollider>();

    if (jumpOffset.y > 1.5f && Mathf.Abs(jumpOffset.x) < 1.1f)
    {
        animator.SetTrigger("Jump");
        checkpointPlayer.GetComponent<Rigidbody>().AddForce(movement, ForceMode.VelocityChange);
    }
    else if (jumpOffset.y > 1.5f && Mathf.Abs(jumpOffset.x) >= 1.1f)
    {
        col.size = new Vector3(col.size.y, col.size.x, col.size.z);
        movement = new Vector3(jumpOffset.x * 2.3f, jumpOffset.y * 2.1f, 0.0f);
        animator.SetTrigger(jumpOffset.x > 0 ? "RightDive" : "LeftDive");
        checkpointPlayer.GetComponent<Rigidbody>().AddForce(movement, ForceMode.VelocityChange);    
    }
    else if (jumpOffset.y <= 1.5f && Mathf.Abs(jumpOffset.x) > 0.0f)
    {
        col.size = new Vector3(col.size.y, col.size.x, col.size.z);
        col.center = new Vector3(col.center.x, 0.49f, col.center.z);
        animator.SetTrigger(jumpOffset.x > 0 ? "RightBlock" : "LeftBlock");
        StartCoroutine(WalkToBall(jumpOffset));
    }
    StartCoroutine(ResetCollider());
    if(movingCoroutine == null)
    movingCoroutine = StartCoroutine(MoveBetweenPositions());
}

    private IEnumerator WalkToBall(Vector3 distance)
    {
        float destPoint = checkpointPlayer.transform.position.x+distance.x;
        Vector3 movement = distance.x>0f? Vector3.right*4.2f:Vector3.left*4.2f;
        while(destPoint>checkpointPlayer.transform.position.x&&distance.x>0f||destPoint<checkpointPlayer.transform.position.x&&distance.x<0f)
        {
            checkpointPlayer.GetComponent<Rigidbody>().velocity = movement; 
            if(Mathf.Abs(destPoint-checkpointPlayer.transform.position.x)<=2f)
                animator.SetTrigger("EndBlock");
            yield return new WaitForFixedUpdate();
        }
        checkpointPlayer.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

private IEnumerator MoveBetweenPositions()
{
    while (true)
    {  
        yield return new WaitForSeconds(UnityEngine.Random.Range(minWaitTime, maxWaitTime));

        // Ждем, пока анимация не окажется в состоянии "Idle"
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            yield return null;
        }

        Transform targetPos;
        do
        {
            targetPos = positions[UnityEngine.Random.Range(0, positions.Count)];
        }
        while (Mathf.Abs(checkpointPlayer.transform.position.x - targetPos.position.x) <= 0.3f);

        Vector3 direction = (targetPos.position - checkpointPlayer.transform.position).normalized;
        string walkTrigger = direction.x > 0 ? "WalkRight" : "WalkLeft";

        animator.ResetTrigger("BackToIdle"); // Убираем возможные баги со сбросом анимации
        animator.SetTrigger(walkTrigger);

        float startX = checkpointPlayer.transform.position.x;
        float targetX = targetPos.position.x;
        
        while (Mathf.Abs(checkpointPlayer.transform.position.x - targetX) > 0.1f)
        {
            checkpointPlayer.transform.position = Vector3.MoveTowards(checkpointPlayer.transform.position, targetPos.position, 1.5f * Time.deltaTime);
            yield return null;
        }

        //yield return new WaitForSeconds(0.2f); // Даем небольшую задержку перед сбросом анимации
        animator.SetTrigger("BackToIdle");
    }
}

    private IEnumerator ResetCollider()
    {
        yield return new WaitForSeconds(1.5f);
        col.size = colliderSize;
        col.center = new Vector3(0, 1.3f, 0);
    }

    private void ResetTrigger()
    {
        GetComponent<Collider>().enabled=true;
    }
}


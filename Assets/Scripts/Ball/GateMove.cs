using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GateMove : MonoBehaviour
{
    public event Action OnSwiped;
    public TouchHandler touchHandler
    {
        get { return th; }
        set
        {
            th = value; AssignTouchHandler();
        }
    }
    [SerializeField] private float penaltyAwaiting = 1.7f;
    [SerializeField] private float forceMultiplier = 16f;
    [SerializeField] private float jumpForceF = 7f;
    [SerializeField] private float jumpForceK = 5f;
    private TouchHandler th;
    private bool assigned = false;
    private Rigidbody body;
    private Coroutine blockMovingCoroutine;
    private StrikeType strikeType;
    private GameController gameController;
    bool isMoving = false;
    

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        gameController = GameObject.FindObjectOfType<GameController>();
        gameController.OnEnterSwipeArea += StopMove;
        gameController.OnRestart += () => {isMoving = false;};
    }
    private void AssignTouchHandler()
    {
        if (!assigned)
        {
            th.OnSwipe += OnSwipe;
            assigned = true;
        }
    }

    private void StopMove()
    {
        if (blockMovingCoroutine != null)
            StopCoroutine(blockMovingCoroutine);
        blockMovingCoroutine = StartCoroutine(BlockMoving());
    }
    
    private IEnumerator BlockMoving()
    {
        while (true)
        {
            body.velocity = Vector3.zero;
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnSwipe(Vector2[] swipeCoords)
    {
        if(!isMoving)
        {
            isMoving = true;
            Vector3[] simpleCoords = SimplifySwipeLine(swipeCoords);
            if (simpleCoords != null)
            {
                strikeType = gameController.selectedStrikeType;
                StartCoroutine(MoveToGate(simpleCoords));
                if (blockMovingCoroutine != null)
                {
                    StopCoroutine(blockMovingCoroutine);
                    blockMovingCoroutine = null;
                }
            }
        }
    }

    private Vector3[] SimplifySwipeLine(Vector2[] swipeLine)
    {
        Vector3[] simplify = new Vector3[swipeLine.Length];
        for (int i = 0; i < swipeLine.Length; i++)
        {
            simplify[i] = new Vector3(swipeLine[i].x, swipeLine[i].y, 0f);
        }
        if (simplify.Length >= 5)
        {
            Vector3[] simpleCoords = new Vector3[5];
            simpleCoords[0] = simplify[0];
            simpleCoords[1] = simplify[(int)(simplify.Length * 0.4f)];
            simpleCoords[2] = simplify[(int)(simplify.Length * 0.5f)];
            simpleCoords[3] = simplify[(int)(simplify.Length * 0.8f)];
            simpleCoords[4] = simplify[simplify.Length - 1];
            return simpleCoords;
        }
        else
            return null;

    }

private IEnumerator MoveToGate(Vector3[] moveCoords)
{
    float distance = Vector2.Distance(new Vector2(moveCoords[0].x, moveCoords[0].y), 
                                      new Vector2(moveCoords[moveCoords.Length - 1].x, moveCoords[moveCoords.Length - 1].y));
    float jumpForce = (strikeType == StrikeType.kick ? jumpForceK : jumpForceF)*(distance < 0.4f ? 0.4f : distance);

    // Линейно интерполируем силу удара от 10 до 16, ограничивая диапазон расстояний
    forceMultiplier = Mathf.Lerp(8.0f, 17.0f, Mathf.InverseLerp(0.1f, 0.7f, distance));

    body.velocity = Vector3.zero;
    float startPosition = transform.position.z;

    if (moveCoords[4].y > moveCoords[0].y) // Если свайп был вверх
    {
        OnSwiped?.Invoke();
        if(strikeType == StrikeType.spin)
        {
            if(moveCoords[4].x>=0)
            {
                moveCoords[3].Set(moveCoords[3].x+0.2f,moveCoords[3].y,moveCoords[3].z);
                moveCoords[2].Set(moveCoords[2].x+0.3f,moveCoords[2].y,moveCoords[2].z);
                moveCoords[1].Set(moveCoords[1].x+0.1f,moveCoords[1].y,moveCoords[1].z);
            }
            else
            {
                moveCoords[3].Set(moveCoords[3].x-0.2f,moveCoords[3].y,moveCoords[3].z);
                moveCoords[2].Set(moveCoords[2].x-0.3f,moveCoords[2].y,moveCoords[2].z);
                moveCoords[1].Set(moveCoords[1].x-0.1f,moveCoords[1].y,moveCoords[1].z);
            }
        }
        body.velocity = Vector3.zero;
        yield return new WaitForSeconds(penaltyAwaiting);

        // Рассчитываем движение
        Vector3 movement = new Vector3((moveCoords[2].x*2f - moveCoords[0].x*2f), 0.0f, moveCoords[2].y*2.5f - moveCoords[0].y*2.5f);
        
        // Нормализуем вектор движения (чтобы не зависел от длины исходного вектора)
        movement = movement.normalized * forceMultiplier;

        // Добавляем силу прыжка
        movement.y = jumpForce;

        // Ограничиваем движение по X, если оно выходит за границы
        movement.x = Mathf.Clamp(movement.x, -6f, 6f);
        movement.z = Mathf.Clamp(movement.z, 5f, 17f); // Сила удара не может быть меньше 10

        StartCoroutine(SideSpinMove(moveCoords, startPosition));
        body.velocity = movement;
    }
    else
    {
        isMoving = false;
    }
}
    private IEnumerator SideSpinMove(Vector3[] moveCoords, float startPosition)
    {
        yield return new WaitForSeconds(0.5f);
        //float forceMultiplier = 100f;
        Vector3 movement = new Vector3(moveCoords[4].x-moveCoords[3].x,moveCoords[4].y-moveCoords[3].y,0.0f)*forceMultiplier;
        if(Mathf.Sqrt(movement.x*movement.x+movement.y*movement.y)>9f){
            movement*=9f/Mathf.Sqrt(movement.x*movement.x+movement.y*movement.y);
        }
        if(movement.y>1f)
            movement = new Vector3(movement.x,1f,movement.z);
        Vector3 tempVelocity = body.velocity;
        float addingX = (movement.x-tempVelocity.x)/30f;
        float addingY = (movement.y-tempVelocity.y)/30f;
        while(Mathf.Abs(tempVelocity.x-movement.x)>0.5f||Mathf.Abs(tempVelocity.y-movement.y)>0.5f){

            tempVelocity += new Vector3(addingX,addingY,0.0f);
            tempVelocity.z = body.velocity.z;
            body.velocity = tempVelocity;
            yield return new WaitForFixedUpdate();
        }
    }
}
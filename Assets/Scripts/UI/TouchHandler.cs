using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Vector2 pointerDownPosition;
    private Vector2[] swipeCoords;
    private int controlType = -1;
    private Coroutine swipeCoroutine;

    public event Action OnTouchEnd;
    public event Action<Vector2[]> OnSwipe;
    public event Action<Vector2> OnDragStart;
    public event Action OnDragEnd;
    public FixedJoystick joystick;

    private void Start() {
        StartCountdown downCounter = GameObject.FindObjectOfType<StartCountdown>();
        joystick = GameObject.FindObjectOfType<FixedJoystick>();
        joystick.OnJoystickReleased += GenerateSwipelineFromJoystick;
        GameController gameController = GameObject.FindObjectOfType<GameController>();
        gameController.OnEnterSwipeArea += ChangeControlToSwipe;
        gameController.OnSwipeExit += BlockControl;
        //downCounter.OnDelayEnd +=ChangeControlToDrag;
        ChangeControlToDrag();
    }

    private void GenerateSwipelineFromJoystick(Vector2 releaseCoords)
{
    if (controlType == 0)
    {
        // Генерация координат от начальной позиции до конечной
        Vector2 start = Vector2.zero; // Начальная позиция джойстика (ноль)
        Vector2 end = releaseCoords;  // Конечная позиция джойстика

        // Генерируем 5 точек по прямой от start до end
        swipeCoords = new Vector2[5];
        for (int i = 0; i < 5; i++)
        {
            // Рассчитываем пропорцию для каждой точки
            float t = i / 4f; // t будет от 0 до 1
            swipeCoords[i] = Vector2.Lerp(start, end, t);
        }

        OnSwipeEnd();
    }

    if (controlType == 1)
    {
        OnDragEnd?.Invoke();
    }

    OnTouchEnd?.Invoke();
}

    public void OnPointerDown(PointerEventData eventData)
    {

            pointerDownPosition = new Vector2(eventData.position.x, eventData.position.y);
            if(controlType==1)
            {
                OnDragStart?.Invoke(pointerDownPosition);
            }
            if(controlType==0)
            {
                if(swipeCoroutine!=null)
                    StopCoroutine(swipeCoroutine);
                //swipeCoroutine = StartCoroutine(BuildSwipeLine());
            }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(controlType==0){
            OnSwipeEnd();
            if(swipeCoroutine!=null)
                StopCoroutine(swipeCoroutine);
            swipeCoroutine = null;

        }
        if(controlType==1)
        {
            OnDragEnd?.Invoke();
        }
        OnTouchEnd?.Invoke();
    }

    private void BlockControl()
    {
        controlType = -1;
    }

    private void ChangeControlToSwipe()
    {
        controlType = 0;
    }

    private void ChangeControlToDrag()
    {
        controlType = 1;
    }

    private void OnSwipeEnd()
    {
        if(swipeCoords!=null)OnSwipe?.Invoke(swipeCoords);
    }
}



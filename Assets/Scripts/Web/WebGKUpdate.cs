using System.Collections;
using UnityEngine;

public class WebGKUpdate : MonoBehaviour
{
    [SerializeField] private GoalkeeperPlayerControl gk;
    private GameController gameController;
    private WebGameManager webGameManager;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 targetVelocity;
    private BoxCollider gkCollider;
    private Rigidbody body;
    
    private float interpolationSpeed = 10f; // Скорость интерполяции (можно настроить)
    private float sendInterval = 0.05f; // Частота отправки данных (раз в 0.05 секунды = 20 раз в секунду)
    private Coroutine sendGKStatsCoroutine; // Переменная для хранения корутины

    void Start()
    {
        body = GetComponent<Rigidbody>();
        gkCollider = GetComponent<BoxCollider>();
        gameController = FindObjectOfType<GameController>();
        webGameManager = FindObjectOfType<WebGameManager>();
        
        webGameManager.OnGKStatsReceived += HandleGKStatsReceived;
       
        sendGKStatsCoroutine = StartCoroutine(SendGKStatsCoroutine());
        

    }
    
    private void FixedUpdate()
    {
        if (gameController.playerRole == PlayerTypes.kicker)
        {
            Interpolate();
        }
    }

    private void Interpolate()
    {
        if (targetPosition != null)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.smoothDeltaTime * interpolationSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.smoothDeltaTime * interpolationSpeed);
        }
    }

    private IEnumerator SendGKStatsCoroutine()
    {
        while (true)
        {
            if (gameController.playerRole == PlayerTypes.goalkeeper)
            {
                webGameManager.SendGKStats(transform.position, transform.rotation, body.velocity, gkCollider.size, gk.currentAnimationTrigger);               
            }
            yield return new WaitForSeconds(sendInterval);
        }
    }

    private void HandleGKStatsReceived(GoalkeeperStatsMessage gkStatsMessage)
    {
        if (gameController.playerRole == PlayerTypes.kicker)
        {
            targetPosition = gkStatsMessage.position;
            targetRotation = gkStatsMessage.rotation;
            targetVelocity = gkStatsMessage.velocity;
            gkCollider.size = gkStatsMessage.colliderScale;
            gk.UpdateAnimationTrigger(gkStatsMessage.animation);
        }
    }

    private void OnDestroy()
    {
        if (webGameManager != null)
        {
            webGameManager.OnGKStatsReceived -= HandleGKStatsReceived;
        }
        if (sendGKStatsCoroutine != null)
        {
            StopCoroutine(sendGKStatsCoroutine);
        }
    }
}
using System.Collections;
using UnityEngine;

public class WebBallUpdate : MonoBehaviour
{
    private GameController gameController;
    private WebGameManager webGameManager;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 targetVelocity;
    private Rigidbody body;
    
    private float interpolationSpeed = 10f; // Скорость интерполяции (можно настроить)
    private float sendInterval = 0.05f; // Частота отправки данных (раз в 0.05 секунды = 20 раз в секунду)
    private Coroutine sendBallStatsCoroutine; // Переменная для хранения корутины

    void Start()
    {
        body = GetComponent<Rigidbody>();
        gameController = FindObjectOfType<GameController>();
        webGameManager = FindObjectOfType<WebGameManager>();
        
        // Подписываемся на события получения данных о мяче
        webGameManager.OnBallStatsReceived += HandleBallStatsReceived;
       
        sendBallStatsCoroutine = StartCoroutine(SendBallStatsCoroutine());
        

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

    private IEnumerator SendBallStatsCoroutine()
    {
        while (true)
        {
            if (gameController.playerRole == PlayerTypes.goalkeeper)
            {
                webGameManager.SendBallStats(transform.position, transform.rotation, body.velocity);               
            }
            yield return new WaitForSeconds(sendInterval);
        }
    }

    private void HandleBallStatsReceived(BallStatsMessage ballStatsMessage)
    {
        if (gameController.playerRole == PlayerTypes.kicker)
        {
            targetPosition = new Vector3(ballStatsMessage.position[0], ballStatsMessage.position[1], ballStatsMessage.position[2]);
            targetRotation = new Quaternion(ballStatsMessage.rotation[1], ballStatsMessage.rotation[2], ballStatsMessage.rotation[3], ballStatsMessage.rotation[0]);
            targetVelocity = new Vector3(ballStatsMessage.velocity[0], ballStatsMessage.velocity[1], ballStatsMessage.velocity[2]);
        }
    }

    private void OnDestroy()
    {
        if (webGameManager != null)
        {
            webGameManager.OnBallStatsReceived -= HandleBallStatsReceived;
        }
        if (sendBallStatsCoroutine != null)
        {
            StopCoroutine(sendBallStatsCoroutine);
        }
    }
}
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;
using System.Collections;
using UnityEngine.SceneManagement;

public class WebGameManager : MonoBehaviour
{
#region Public
    [SerializeField] private string serverUrl = /*"wss://userproject.evrobel.biz/ws/";*/ "wss://tma-game.ru/ws/";
    public string role = "";
    public string playerID = "";
    public bool gameStarted = false;
#endregion

#region Private
    WebSocket ws;
    bool isQuitting = false;
#endregion
    
#region Actions
    public event Action<string, string> OnGameStart;
    public event Action OnGameRestart;
    public event Action OnOpponentLost;
    public event Action<KickStartMessage> OnKickStartReceived;
    public event Action<KickLateMessage> OnKickLateReceived;
    public event Action<GoalMessage> OnGoalReceived;
    public event Action<BallStatsMessage> OnBallStatsReceived;
    public event Action<GoalkeeperStatsMessage> OnGKStatsReceived;
    public event Action<GoalkeeperAnimationMessage> OnGKAnimationReceived;
#endregion

    async void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        ws = new WebSocket(serverUrl);
        ws.OnOpen += async () =>
        {
            Debug.Log("WebSocket соединение установлено!");
            await SendMessage(new ConnectionMessage());
        };

        ws.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            //Debug.Log("Сообщение от сервера: " + message);
            HandleServerMessage(message); // Обработка сообщений от сервера
        };

        ws.OnError += (error) =>
        {
            Debug.LogError("Ошибка WebSocket: " + error);
        };

        ws.OnClose += async (error) =>
        {
            if (isQuitting) return; // Не переподключаемся, если приложение закрывается
            Debug.Log("Соединение закрыто, переподключение...");
            await Task.Delay(2000);
            await Connect();
        };

        await Connect();
        StartCoroutine(StartTimedMessageLoop());
    }

    void HandleServerMessage(string message)
    {
        try
        {
            var data = JsonUtility.FromJson<Message>(message);

            switch (data.type)
            {
                case "start":
                    var startMessage = JsonUtility.FromJson<StartMessage>(message);
                    OnGameStart?.Invoke(startMessage.role, startMessage.playerID);
                    role = startMessage.role;
                    playerID = startMessage.playerID;
                    gameStarted = true;
                    break;

                case "restart":
                    var restartMessage = JsonUtility.FromJson<RestartMessage>(message);
                    OnGameRestart?.Invoke();
                    //role = restartMessage.role;
                    gameStarted = true;
                    break;               

                case "ball_stats":
                    var ballStatsMessage = JsonUtility.FromJson<BallStatsMessage>(message);
                    OnBallStatsReceived?.Invoke(ballStatsMessage);
                    break;
                
                case "gk_stats":
                    var gkStatsMessage = JsonUtility.FromJson<GoalkeeperStatsMessage>(message);
                    OnGKStatsReceived?.Invoke(gkStatsMessage);
                    break;
                
                case "gk_animation":
                    var gkAnimMessage = JsonUtility.FromJson<GoalkeeperAnimationMessage>(message);
                    OnGKAnimationReceived?.Invoke(gkAnimMessage);
                    break;

                case "kick_start":
                    var kickStartMessage = JsonUtility.FromJson<KickStartMessage>(message);
                    Debug.Log("GOT MESSAGE");
                    OnKickStartReceived?.Invoke(kickStartMessage);
                    break;

                case "kick_late":
                    var kickLateMessage = JsonUtility.FromJson<KickLateMessage>(message);
                    OnKickLateReceived?.Invoke(kickLateMessage);
                    break;

                case "goal":
                    var goalMessage = JsonUtility.FromJson<GoalMessage>(message);
                    OnGoalReceived?.Invoke(goalMessage);
                    break;
                
                case "player_left":
                    OnOpponentLost?.Invoke();
                    Debug.Log("Игрок покинул комнату.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Ошибка при обработке сообщения: " + ex.Message);
        }
    }

    async Task Connect()
    {
        try
        {
            if (ws.State == WebSocketState.Connecting || ws.State == WebSocketState.Open)
                return;

            await ws.Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError("Ошибка подключения: " + ex.Message);
        }
    }

    public async Task SendMessage(object obj)
    {
        if (ws.State == WebSocketState.Open)
        {
            string json = JsonUtility.ToJson(obj);
            //Debug.Log(json);
            await ws.SendText(json);
        }
        else
        {
            Debug.LogWarning("WebSocket не открыт. Сообщение не отправлено.");
        }
    }


#region Message Senders
    public async Task SendBallStats(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        var message = new BallStatsMessage(position, rotation, velocity);
        await SendMessage(message);
    }

    public async Task SendGKStats(Vector3 position, Quaternion rotation, Vector3 velocity,
                                  Vector3 colliderScale, string animation)
    {
        var message = new GoalkeeperStatsMessage(position, rotation, velocity, colliderScale, animation);
        await SendMessage(message);
    }
#endregion

    async void OnApplicationQuit()
    {
        isQuitting = true;
        if (ws != null)
        {
            try
            {
                await ws.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Ошибка при закрытии WebSocket: " + ex.Message);
            }
        }
    }

    IEnumerator StartTimedMessageLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            SendMessage(new TimedMessage());
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            Destroy(gameObject);
        }
    }

    async void OnDestroy()
    {
        isQuitting = true;
        if (ws != null)
        {
            try
            {
                await ws.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Ошибка при закрытии WebSocket: " + ex.Message);
            }
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
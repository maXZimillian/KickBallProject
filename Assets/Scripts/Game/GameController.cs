using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameController : MonoBehaviour
{
#region Public
    [SerializeField] private float timeScale = 1f;
    public PlayerTypes playerRole = PlayerTypes.playerDefault;
    public string playerID = "";
    public int moneyCount { get; private set; } = 0;
    public StrikeType selectedStrikeType = StrikeType.force;
#endregion

#region Private
    private GameStates gameState = GameStates.game;
    private BallController ball;
    private WebGameManager webGameManager;
    private int attemptsPlayerMade = 0;
#endregion

#region Actions
    public event Action OnEnterSwipeArea;
    public event Action OnSwipeExit;
    public event Action<int> OnWin;
    public event Action OnGameOver;
    public event Action OnEscapePressed;
    public event Action OnBallCenterMove;
    public event Action OnRestart;
    public event Action<int> OnCollect;
#endregion
       

    private void Start()
    {
        Time.timeScale = timeScale;
        AssignParamsAndActions();
    }

    private void AssignParamsAndActions()
    {
        webGameManager = FindObjectOfType<WebGameManager>();
        if (webGameManager != null)
        {
            webGameManager.OnGameStart += HandleGameStart;
            webGameManager.OnBallStatsReceived += HandleBallStatsReceived;
            webGameManager.OnGoalReceived += (msg) => { AddPoint(Vector2.zero); }; 
            webGameManager.OnGameRestart += HandleRestart;
            webGameManager.OnOpponentLost += HandleLostOpponent;
            if(webGameManager.gameStarted)
            {
                HandleGameStart(webGameManager.role,webGameManager.playerID);
            }
        }

        ball = FindObjectOfType<BallController>();
        ball.OnStop += () => { webGameManager.SendMessage(new RestartMessage()); };
        //ball.OnStop += GameOver;
        GoalGateTrigger[] g = FindObjectsOfType<GoalGateTrigger>();
        foreach (GoalGateTrigger trigger in g)
        {
            trigger.OnGateEnter += Gate_OnEnterSwipeArea;
            trigger.OnGateExit += Gate_OnExitSwipeArea;
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && gameState == GameStates.game && OnEscapePressed != null)
        {
            OnEscapePressed.Invoke();
        }
    }

    private void GameOver()
    {
        attemptsPlayerMade++;
        if (attemptsPlayerMade >= 5)
            WinGame();
        else
            Restart();
    }

    private void Gate_OnEnterSwipeArea()
    {
        FindObjectOfType<BallController>().gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        OnEnterSwipeArea?.Invoke();
    }

    private void Gate_OnExitSwipeArea()
    {
        if(playerRole == PlayerTypes.goalkeeper)
        {
            StartCoroutine(LoseOnEndDelay());
        }
        OnSwipeExit?.Invoke();
    }

    private void AddPoint(Vector2 winEntryPosition)
    {
        if (gameState == GameStates.game)
        {
            moneyCount++;
            OnCollect?.Invoke(1);
        }
    }

    private void WinGame()
    {
        if (gameState == GameStates.game)
        {
            gameState = GameStates.gameOver;
            OnWin?.Invoke(moneyCount);
        }
    }

    private void Restart()
    {
        ball.BackToStartPos();
        OnRestart?.Invoke();
    }

    private IEnumerator LoseOnEndDelay()
    {
        yield return new WaitForSeconds(4f);
        webGameManager.SendMessage(new RestartMessage());
        //GameOver();
    }

    private void HandleGameStart(string role, string playerID)
    {
        this.playerID = playerID;

        if (role == "kicker")
        {
            this.playerRole = PlayerTypes.kicker;
            Debug.Log("Роль игрока: нападающий");
        }
        else if (role == "goalkeeper")
        {
            this.playerRole = PlayerTypes.goalkeeper;
            Debug.Log("Роль игрока: вратарь");

            WinTrigger[] w = FindObjectsOfType<WinTrigger>();
            foreach (WinTrigger trigger in w)
                trigger.OnWinEnter += AddPoint;
        }
    }

    private void HandleRestart()
    {
        if (playerRole == PlayerTypes.kicker)
        {
            playerRole = PlayerTypes.goalkeeper;
        }
        else if (playerRole == PlayerTypes.goalkeeper)
        {
            playerRole = PlayerTypes.kicker;
        } 
        GameOver();    
    }

    private void HandleLostOpponent()
    {
        SceneManager.LoadScene(0);
    }

    private void HandleBallStatsReceived(BallStatsMessage ballStatsMessage)
    {

    }

    private void OnDestroy()
    {
        if (webGameManager != null)
        {
            webGameManager.OnGameStart -= HandleGameStart;
            webGameManager.OnBallStatsReceived -= HandleBallStatsReceived;
        }
    }
}

public enum StrikeType{
    force,
    spin,
    kick
}

public enum GameStates{
    game,
    gameOver
}

public enum PlayerTypes{
    kicker,
    goalkeeper,
    playerDefault
}
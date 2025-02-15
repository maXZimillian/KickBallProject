using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameController : MonoBehaviour
{   
    [SerializeField] private float swipeAreaTimeScale = 1f;
    public int moneyCount { get; private set; } = 0;
    public StrikeType selectedStrikeType = StrikeType.force;
    private GameStates gameState = GameStates.game;
    private BallController ball;
    private int attemptsPlayerMade = 0;
    private bool isGoal = false;
    public event Action OnEnterSwipeArea;
    public event Action OnSwipeExit;
    public event Action<int> OnWin;
    public event Action OnGameOver;
    public event Action OnHintEnter;
    public event Action OnEscapePressed;
    public event Action OnObstacleHit;
    public event Action OnBallCenterMove;
    public event Action OnRestart;
    public event Action<int> OnCollect;

    private void Start() 
    {
        ball = GameObject.FindObjectOfType<BallController>();
        ball.OnStop += GameOver;

        GateMove d = GameObject.FindObjectOfType<GateMove>();
        d.OnSwiped += OnFinalSwipe;

        GoalGateTrigger[] g = GameObject.FindObjectsOfType<GoalGateTrigger>();
        foreach(GoalGateTrigger trigger in g)
        {
            trigger.OnGateEnter += Gate_OnEnterSwipeArea;
            trigger.OnGateExit += Gate_OnExitSwipeArea;
        }

        WinTrigger[] w = GameObject.FindObjectsOfType<WinTrigger>();
        foreach(WinTrigger trigger in w)
            trigger.OnWinEnter += AddPoint;

        GameOverTrigger[] t = GameObject.FindObjectsOfType<GameOverTrigger>();
        foreach(GameOverTrigger trigger in t)
            trigger.OnEnter += GameOver;
        
        HintController[] h = GameObject.FindObjectsOfType<HintController>();
        foreach(HintController item in h)
            item.OnHintEnter += OnHint;

        Obstacle[] o = GameObject.FindObjectsOfType<Obstacle>();
        foreach(Obstacle item in o)
            item.OnObstacleHit += OnObstacleCollide;

        CenterMoveTrigger[] e = GameObject.FindObjectsOfType<CenterMoveTrigger>();
        foreach(CenterMoveTrigger item in e)
            item.OnEnter += Ball_OnMoveCenter;

        //b.StartMove();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)&&gameState==GameStates.game&&OnEscapePressed!=null)
        {
            OnEscapePressed.Invoke();
        }
    }

    private void OnHint()
    {
        if(OnHintEnter!=null)OnHintEnter.Invoke();
    }

    private void OnObstacleCollide()
    {
        OnObstacleHit?.Invoke();
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
        GameObject.FindObjectOfType<BallController>().gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero; 
        Time.timeScale = swipeAreaTimeScale;
        if(OnEnterSwipeArea!=null)OnEnterSwipeArea.Invoke();
    }
    private void Gate_OnExitSwipeArea()
    {
        StartCoroutine(LoseOnEndDelay());
        if(OnSwipeExit!=null)OnSwipeExit.Invoke();
    }

    private void Ball_OnMoveCenter()
    {
        OnBallCenterMove?.Invoke();
    }

    private void OnFinalSwipe(){
        Time.timeScale = 1.0f;
    }

    private void AddPoint(Vector2 winEntryPosition)
    {
        if (gameState == GameStates.game)
        {
            moneyCount++;
            OnCollect?.Invoke(1);
            isGoal = true;   
        }
    }

    private void WinGame()
    {
        if (gameState==GameStates.game){
            gameState = GameStates.gameOver;
            if(OnWin!=null)OnWin.Invoke(moneyCount);
        }
    }

    private void Restart ()
    {
        ball.BackToStartPos();
        OnRestart.Invoke();
    }

    private IEnumerator LoseOnEndDelay(){
        yield return new WaitForSeconds(4f);
        GameOver();
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
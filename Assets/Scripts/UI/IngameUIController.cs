using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IngameUIController : MonoBehaviour
{
    [SerializeField] TouchHandler touchHandler;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject[] ingameElements;
    [SerializeField] GameObject swipeHintPopup;
    [SerializeField] GameObject damagePanel;
    [SerializeField] GameObject popupText;
    [SerializeField] int winPanelDelay = 3;
    [SerializeField] float popupShowSpeed = 0.1f;
    [SerializeField] GameObject[] pointsCollectListeners;
    [SerializeField] GameObject kickerButtonsContainer;
    [SerializeField] GameObject keeperButtonsContainer;
    private Coroutine swipePopupCoroutine = null;
    private Coroutine damageCoroutine = null;
    
    private GameController gameController;
    private GoalkeeperPlayerControl gkController;

    void Start()
    {
       gameController = FindObjectOfType<GameController>();
       gameController.OnWin += ShowPopupOnWin;
       gameController.OnGameOver += ShowLosePopup; 
       gameController.OnEnterSwipeArea += ShowSwipePopup;
       gameController.OnSwipeExit += CloseSwipePopup;
       gameController.OnEscapePressed += ShowPausePopup;
       gameController.OnRestart += ChangeButtons;
       ChangeButtons();
       FindObjectOfType<GoalGateTrigger>().SetTrigger();
       gkController = FindObjectOfType<GoalkeeperPlayerControl>();
    }

    void Update()
    {
        if(gameController.playerRole == PlayerTypes.kicker)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ForceButtonPressed();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                SpinButtonPressed();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                KickButtonPressed();
            }
        }
        else if (gameController.playerRole == PlayerTypes.goalkeeper)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                JumpPressed();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                SideJumpPressed();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                SideBlockPressed();
            }
        }
    }

    private void ChangeButtons()
    {
        if(gameController.playerRole == PlayerTypes.kicker)
        {
            keeperButtonsContainer.SetActive(false);
            kickerButtonsContainer.SetActive(true);
        }
        else if (gameController.playerRole == PlayerTypes.goalkeeper)
        {
            kickerButtonsContainer.SetActive(false);
            keeperButtonsContainer.SetActive(true);
        }
    }

    private void ShowPopupOnWin(int points){
        StartCoroutine(ShowWinPopup());
        popupText.GetComponent<Text>().text = points.ToString();
    }

    private void ShowLosePopup(){
        losePanel.SetActive(true);
        foreach(GameObject ingameElement in ingameElements)
            ingameElement.SetActive(false);
    }

    public void ShowPausePopup(){
        pausePanel.SetActive(true);
        foreach(GameObject ingameElement in ingameElements)
            ingameElement.SetActive(false);
    }

#region Kicker Buttons
    public void ForceButtonPressed(){
        gameController.selectedStrikeType = StrikeType.force;
        touchHandler.GenerateSwipelineFromJoystick();
    }

    public void SpinButtonPressed(){
        gameController.selectedStrikeType = StrikeType.spin;
        touchHandler.GenerateSwipelineFromJoystick();
    }

    public void KickButtonPressed(){
        gameController.selectedStrikeType = StrikeType.kick;
        touchHandler.GenerateSwipelineFromJoystick();
    }
#endregion

#region Keeper Buttons
    public void JumpPressed(){
        gkController.JumpHigh();
    }

    public void SideJumpPressed(){
        gkController.JumpMedium();
    }

    public void SideBlockPressed(){
        gkController.BlockLow();
    }
#endregion

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    private void ShowDamageEffect(){
        if(damageCoroutine!=null)
            StopCoroutine(damageCoroutine);
        damageCoroutine = StartCoroutine(DamageEffect());
    }

    private void ShowSwipePopup(){
        if(swipePopupCoroutine!=null)
            StopCoroutine(swipePopupCoroutine);
        swipePopupCoroutine = StartCoroutine(SwipePopupView());
    }
    private void CloseSwipePopup(){
        if(swipePopupCoroutine!=null)
            StopCoroutine(swipePopupCoroutine);
        swipePopupCoroutine = StartCoroutine(SwipePopupUnview());
    }

    IEnumerator ShowWinPopup(){
        yield return new WaitForSeconds(winPanelDelay);
        winPanel.SetActive(true);
        foreach(GameObject ingameElement in ingameElements)
            ingameElement.SetActive(false);
    }
    IEnumerator SwipePopupView(){
        swipeHintPopup.SetActive(true);
        CanvasGroup popupGroup = swipeHintPopup.GetComponent<CanvasGroup>();
        popupGroup.alpha = 0f;
        while(popupGroup.alpha<1f){
            popupGroup.alpha+=popupShowSpeed*Time.unscaledDeltaTime*10f;
            yield return new WaitForEndOfFrame();
        }
        swipePopupCoroutine = null;
    }
    IEnumerator SwipePopupUnview(){
        CanvasGroup popupGroup = swipeHintPopup.GetComponent<CanvasGroup>();
        while(popupGroup.alpha>0f){
            popupGroup.alpha-=popupShowSpeed*Time.unscaledDeltaTime*10f;
            yield return new WaitForEndOfFrame();
        }
        swipeHintPopup.SetActive(false);
        swipePopupCoroutine = null;
    }
    IEnumerator DamageEffect(){
        damagePanel.SetActive(true);
        CanvasGroup popupGroup = damagePanel.GetComponent<CanvasGroup>();
        popupGroup.alpha = 0f;
        while(popupGroup.alpha<1f){
            popupGroup.alpha+=popupShowSpeed;
            yield return new WaitForFixedUpdate();
        }
        while(popupGroup.alpha>0f){
            popupGroup.alpha-=popupShowSpeed;
            yield return new WaitForFixedUpdate();
        }
        damageCoroutine = null;
    }
}

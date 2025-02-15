using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameUIController : MonoBehaviour
{
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
    [SerializeField] Image[] buttonsArray;
    private Coroutine swipePopupCoroutine = null;
    private Coroutine damageCoroutine = null;
    
    private GameController gameController;

    void Start()
    {
       gameController = GameObject.FindObjectOfType<GameController>();
       gameController.OnWin += ShowPopupOnWin;
       gameController.OnGameOver += ShowLosePopup; 
       gameController.OnEnterSwipeArea += ShowSwipePopup;
       gameController.OnSwipeExit += CloseSwipePopup;
       gameController.OnEscapePressed += ShowPausePopup;
       gameController.OnObstacleHit += ShowDamageEffect;
       GameObject.FindObjectOfType<GoalGateTrigger>().SetTrigger();
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

    public void ForceButtonPressed(){
        gameController.selectedStrikeType = StrikeType.force;
        foreach (Image button in buttonsArray)
        {
            button.color = Color.white;
        }
        buttonsArray[0].color = Color.green;
    }

    public void SpinButtonPressed(){
        gameController.selectedStrikeType = StrikeType.spin;
        foreach (Image button in buttonsArray)
        {
            button.color = Color.white;
        }
        buttonsArray[1].color = Color.green;
    }

    public void KickButtonPressed(){
        gameController.selectedStrikeType = StrikeType.kick;
        foreach (Image button in buttonsArray)
        {
            button.color = Color.white;
        }
        buttonsArray[2].color = Color.green;
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

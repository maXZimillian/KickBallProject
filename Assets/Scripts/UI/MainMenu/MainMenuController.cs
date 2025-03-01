using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private WebGameManager webGameManager;
    private void Start()
    {
        //new SaveGame().Reset();
        webGameManager.OnGameStart += (string a, string b) => {OpenLevel(1);};
    }

    public void PlayButtonClick(){
        OpenLevel(1);//(levelToOpen);
    }

    public void ExitGame(){
        Application.Quit();
    }

    public void OpenLevel(int number){
        SceneManager.LoadScene(number+1);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractButton : MonoBehaviour
{
    [SerializeField] private GameObject interactButton;
    public delegate void Click();
    public event Click OnButtonPressed;

    private void Start() {
        GameController gameController = FindObjectOfType<GameController>();
    }

    private void ShowButton(){
        interactButton.SetActive(true);
    }

    public void PressButton(){
        if(OnButtonPressed!=null)OnButtonPressed.Invoke();
        interactButton.SetActive(false);
    }

}

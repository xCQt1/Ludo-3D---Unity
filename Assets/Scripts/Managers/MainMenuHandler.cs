using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject SettingsMenu;
    [SerializeField] private GameObject PlayerMenu;

    public void Play() {
        GameHandler.Instance.StartGame();
    }

    public void OpenSettingsMenu() {
        
    }
    
    public void QuitButton() {
        Application.Quit(0);
    }
}

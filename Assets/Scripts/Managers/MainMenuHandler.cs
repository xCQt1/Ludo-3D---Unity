using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private readonly GameObject _mainMenu;
    [SerializeField] private readonly GameObject _settingsMenu;
    [SerializeField] private readonly GameObject _playerMenu;

    public void Play() {
        GameHandler.Instance.StartGame();
    }

    public void OpenSettingsMenu() {
        
    }
    
    public void QuitButton() {
        Application.Quit(0);
    }
}

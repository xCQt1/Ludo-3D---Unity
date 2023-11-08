using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.Threading;

public class GameHandler : MonoBehaviour
{
    [SerializeField] public List<Player> players;
    public Player currentPlayer;
    public bool gameOver = false;

    public static GameHandler Instance {get; private set;}

    private void Awake() {
        Instance = this;
    }
    
    
    void Start()
    {
        StartGame();
    }

    public void StartGame() {
        Setup();
        SwitchToNextPlayer();
    }

    public void ResetGame() {
        foreach(Player player in players) {
            player.Reset();
        }
    }

    private void Setup() {
        currentPlayer = players[^1];
    }

    private bool HasPlayerWon(Player player) {
        return false;
    }

    public void SwitchToNextPlayer() {
        gameOver = HasPlayerWon(currentPlayer);
        if (gameOver) HandleGameOver(currentPlayer);
        NumberGenerator.Instance.Reset();

        // next player
        currentPlayer = players[(players.IndexOf(currentPlayer) + 1) % 4];
        StartCoroutine(currentPlayer.Turn());

    }

    private void HandleGameOver(Player player) {
        Debug.Log($"Game Over! {player.name} hat gewonnen!");
        Application.Quit();
    }
}

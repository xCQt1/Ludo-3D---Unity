using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.Threading;
using System.Linq;

public class GameHandler : MonoBehaviour
{
    [SerializeField] public List<Player> players;
    [HideInInspector] public Player currentPlayer;
    [HideInInspector] public bool gameOver = false;

    public static GameHandler Instance {get; private set;}
    private bool HasPlayerWon(Player player) => player.pieces.All(piece => piece.IsInBox());

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

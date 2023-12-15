using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float SpotlightMoveDuration = 0.75f;

    [Header("References")]
    [SerializeField] private GameObject spotlight;
    [SerializeField] public List<Player> players;

    
    [HideInInspector] public Player currentPlayer;
    [HideInInspector] public bool gameOver = false;
    [HideInInspector] public bool inAnimation = false;
    [HideInInspector] public GameState gameState = GameState.MAINMENU;
    [HideInInspector] public Difficulty difficulty = Difficulty.NORMAL;

    public static GameHandler Instance {get; private set;}
    private bool HasPlayerWon(Player player) => player.pieces.All(piece => piece.Checks.IsInEndFields());

    private void Awake() {
        Instance = this;
    }

    public void StartGame(Difficulty difficulty = Difficulty.NORMAL) {
        SceneManager.UnloadSceneAsync("MenuScene");
        this.difficulty = difficulty;
        ResetGame();
        Setup();
        SwitchToNextPlayer();
    }

    public void ResetGame() {   // resets all pieces to their start position
        foreach(Player player in players) {
            player.Reset();
        }
    }

    public void PlacePiecesRandomly() {
        foreach(Player player in players) {
            player.Reset();
            foreach(Piece piece in player.pieces) {
                int chance = new System.Random().Next(1,11);
                if (chance < 4) break;  // piece stays on box
                if (chance > 9) piece.MoveToField(player.endFields[new System.Random().Next(0,4)]);     // piece gets moved to a random end field
                else {      // piece gets moved to a random field
                    piece.MoveToField(player.spawnField);
                    int random;
                    do {
                        random = new System.Random().Next(1,40);
                    } while(!piece.MoveFields(random));
                }
            }
        }
    }

    private void Setup() {
        gameState = GameState.GAME;
        currentPlayer = players[^1];
    }

    public void SwitchToNextPlayer() {  // ends the current players turn and transitions to the next ones
        gameOver = HasPlayerWon(currentPlayer);
        if (gameOver) HandleGameOver(currentPlayer);
        NumberGenerator.Instance.Reset();

        // next player
        currentPlayer = players[(players.IndexOf(currentPlayer) + 1) % 4];
        MoveSpotlightTo(currentPlayer.transform.position + Vector3.up * 4);
        currentPlayer.StartTurn();

    }

    private void HandleGameOver(Player player) {
        Debug.Log($"Game Over! {player.name} hat gewonnen!");
        Application.Quit();
    }

    private void MoveSpotlightTo(Vector3 position) {    // moves the turn-indicating spotlight to a certain player
        StartCoroutine(moveSpotlight(position));
    }

    private IEnumerator moveSpotlight(Vector3 targetPosition) {  // coroutine for MoveSpotlightTo()
        inAnimation = true;

        float timeElapsed = 0f;
        Vector3 startPosition = spotlight.transform.position;

        while(timeElapsed < SpotlightMoveDuration) {
            spotlight.transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed/SpotlightMoveDuration);
            Physics.SyncTransforms();
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        inAnimation = false;
    }
}

public enum GameState {
    MAINMENU,
    GAME,
    PAUSEMENU
}

public enum Difficulty {
    EASY,
    NORMAL
}

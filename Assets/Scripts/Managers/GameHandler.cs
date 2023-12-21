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
    [SerializeField] private GameObject _spotlight;
    [SerializeField] public List<Player> Players;

    
    [HideInInspector] public Player CurrentPlayer;
    [HideInInspector] public bool GameOver = false;
    [HideInInspector] public bool InAnimation = false;
    [HideInInspector] public GameState GameState = GameState.MAINMENU;
    [HideInInspector] public Difficulty Difficulty = Difficulty.NORMAL;

    public static GameHandler Instance {get; private set;}
    private bool HasPlayerWon(Player player) => player.Pieces.All(piece => piece.Checks.IsInEndFields());

    private void Awake() {
        Instance = this;
    }

    public void StartGame(Difficulty difficulty = Difficulty.NORMAL) {  // Starts the game
        SceneManager.UnloadSceneAsync("MenuScene");
        this.Difficulty = difficulty;
        ResetGame();
        Setup();
        SwitchToNextPlayer();
    }

    public void ResetGame() {   // resets all pieces to their start position
        foreach(Player player in Players) {
            player.Reset();
        }
    }

    public void PlacePiecesRandomly() {     // randomizes pieces by moving each a random amount of fields
        foreach(Player player in Players) {
            player.Reset();
            foreach(Piece piece in player.Pieces) {
                int chance = new System.Random().Next(1,11);
                if (chance < 4) break;  // piece stays on box
                if (chance > 9) piece.MoveToField(player.EndFields[new System.Random().Next(0,4)]);     // piece gets moved to a random end field
                else {      // piece gets moved to a random field
                    piece.MoveToField(player.SpawnField);
                    int random;
                    do {
                        random = new System.Random().Next(1,40);
                    } while(!piece.MoveFields(random));
                }
            }
        }
    }

    private void Setup() {
        GameState = GameState.GAME;
        CurrentPlayer = Players[^1];
    }

    public void SwitchToNextPlayer() {  // ends the current players turn and transitions to the next ones
        GameOver = HasPlayerWon(CurrentPlayer);
        if (GameOver) HandleGameOver(CurrentPlayer);
        NumberGenerator.Instance.Reset();

        // next player
        CurrentPlayer = Players[(Players.IndexOf(CurrentPlayer) + 1) % Players.Count];
        MoveSpotlightTo(CurrentPlayer.transform.position + Vector3.up * 4);
        CurrentPlayer.StartTurn();

    }

    private void HandleGameOver(Player player) {    // is called when game is over
        Debug.Log($"Game Over! {player.name} hat gewonnen!");
        Application.Quit();
    }

    private void MoveSpotlightTo(Vector3 position) {    // moves the turn-indicating spotlight to a certain player
        StartCoroutine(MoveSpotlight(position));
    }

    private IEnumerator MoveSpotlight(Vector3 targetPosition) {  // coroutine for MoveSpotlightTo(), moves spotlight to specific position
        InAnimation = true;

        float timeElapsed = 0f;
        Vector3 startPosition = _spotlight.transform.position;

        while(timeElapsed < SpotlightMoveDuration) {
            _spotlight.transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed/SpotlightMoveDuration);
            Physics.SyncTransforms();
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        InAnimation = false;
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

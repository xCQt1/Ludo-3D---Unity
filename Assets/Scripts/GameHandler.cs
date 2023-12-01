using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.Threading;
using System.Linq;

public class GameHandler : MonoBehaviour
{
    [SerializeField] private float SpotlightMoveDuration = 0.75f;
    [SerializeField] private GameObject spotlight;
    [SerializeField] public List<Player> players;
    [HideInInspector] public Player currentPlayer;
    [HideInInspector] public bool gameOver = false;
    [HideInInspector] public bool inAnimation = false;

    public static GameHandler Instance {get; private set;}
    private bool HasPlayerWon(Player player) => player.pieces.All(piece => piece.Checks.IsInEndFields());

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

    public void ResetGame() {   // resets all pieces to their start position
        foreach(Player player in players) {
            player.Reset();
        }
    }

    private void Setup() {
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

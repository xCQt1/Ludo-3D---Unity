using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.Threading;
using System.Linq;

public class GameHandler : MonoBehaviour
{
    [SerializeField] public bool VerboseLogging = true;
    [SerializeField] private float SpotlightMoveDuration = 0.75f;
    [SerializeField] private GameObject spotlight;
    [SerializeField] public List<Player> players;
    [HideInInspector] public Player currentPlayer;
    [HideInInspector] public bool gameOver = false;
    [HideInInspector] public bool inAnimation = false;

    public static GameHandler Instance {get; private set;}
    private bool HasPlayerWon(Player player) => player.pieces.All(piece => piece.IsInEndFields());

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
        MoveSpotlightTo(currentPlayer.transform.position + Vector3.up * 4);
        currentPlayer.StartTurn();

    }

    private void HandleGameOver(Player player) {
        Debug.Log($"Game Over! {player.name} hat gewonnen!");
        Application.Quit();
    }

    private void MoveSpotlightTo(Vector3 position) {
        StartCoroutine(moveSpotlight(position));
    }

    private IEnumerator moveSpotlight(Vector3 targetPosition) {
        inAnimation = true;

        float timeElapsed = 0f;
        Vector3 startPosition = spotlight.transform.position;

        while(timeElapsed < SpotlightMoveDuration) {
            spotlight.transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed/SpotlightMoveDuration);
            Physics.SyncTransforms();
            yield return null;
        }

        inAnimation = false;
    }
}

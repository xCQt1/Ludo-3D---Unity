using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class NumberGenerator : MouseClickable
{
    [Header("Parameters")]
    [SerializeField] private float AnimationDuration;
    
    [Header("References")]
    [SerializeField] private List<GameObject> numbers;
    private GameObject currentNumber;
    [HideInInspector] public Player lastPlayer;
    [HideInInspector] public int lastNumber = 0;
    [HideInInspector] public bool inAnimation {get; private set;}

    public static NumberGenerator Instance {get; private set;}  // static instance of this class
    
    
    private new void Awake() {
        base.Awake();
        Instance = this;
    }

    public void Reset() {
        DestroyNumber();
        lastNumber = 0;
    }

    public void GetNewNumber(Player player) {   // spawns a new number, aka rolling the dice
        if (!player.CanThrowDice()) return;

        int result = new System.Random().Next(1,7);
        
        lastNumber = result;
        lastPlayer = player;

        player.UpdateMoveablePieces();
        player.IncreaseDiceThrows();
        player.HasThrownDice = true;

        SpawnNumber(result);
        Debug.Log($"{lastPlayer.name} has thrown a {result}");
    }

    private void SpawnNumber(int number) {  // spawns the spinning number
        if (currentNumber is not null) {
            DestroyNumber();
        }
        currentNumber = Instantiate(numbers[number-1], transform, true);
        currentNumber.transform.position = transform.position + Vector3.up * 1;
        currentNumber.GetComponentInChildren<Renderer>().material = lastPlayer.PlayerMaterial;
    }

    private void DestroyNumber() {
        Destroy(currentNumber);
        currentNumber = null;
    }

    public void StartAnimation(Piece piece) {
        //StartCoroutine(AnimateNumberFly(piece));
    }

    public override void OnClick() {    // this class' implementation of OnClick(), inherited from base
        if (!GameHandler.Instance.currentPlayer.isBot) GetNewNumber(GameHandler.Instance.currentPlayer);
    }

    protected override void OnHoverBegin() {}

    protected override void OnHoverStop() {}

    protected override Color DetermineColor() {
        return GameHandler.Instance.currentPlayer.isBot || !GameHandler.Instance.currentPlayer.CanThrowDice()? Color.grey : Color.green;
    }
}

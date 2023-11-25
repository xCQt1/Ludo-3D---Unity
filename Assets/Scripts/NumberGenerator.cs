using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class NumberGenerator : MouseClickable
{
    [SerializeField] private List<GameObject> numbers;
    [SerializeField] private float AnimationDuration;
    private GameObject currentNumber;
    [HideInInspector] public Player lastPlayer;
    [HideInInspector] public int lastNumber = 0;
    [HideInInspector] public bool inAnimation {get; private set;}

    public static NumberGenerator Instance {get; private set;}
    
    
    private new void Awake() {
        base.Awake();
        Instance = this;
    }

    public void Reset() {
        DestroyNumber();
        lastNumber = 0;
    }

    public void GetNewNumber(Player player) {
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

    public void RedoThrow() {
        if (lastNumber == 0) return;
        lastNumber = new System.Random().Next(1,7);
    }

    private void SpawnNumber(int number) {
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

    private IEnumerator AnimateNumberFly(Piece piece) {
        inAnimation = true;

        float timeElapsed = 0f;
        Vector3 startPos = currentNumber.transform.position;
        Vector3 targetPos = piece.transform.position;

        Vector3 startScale = currentNumber.transform.localScale;
        Vector3 targetScale = Vector3.one * .7f;

        while (timeElapsed < AnimationDuration) {
            currentNumber.transform.position = Vector3.Lerp(startPos, targetPos, timeElapsed/AnimationDuration);
            currentNumber.transform.localScale = Vector3.Lerp(startScale, targetScale, timeElapsed/AnimationDuration);
            
            Physics.SyncTransforms();
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Reset();
        inAnimation = false;
    }

    public override void OnClick() {
        if (!GameHandler.Instance.currentPlayer.isBot) GetNewNumber(GameHandler.Instance.currentPlayer);
    }

    protected override void OnHoverBegin() {}

    protected override void OnHoverStop() {}

    protected override Color DetermineColor() {
        return GameHandler.Instance.currentPlayer.isBot || !GameHandler.Instance.currentPlayer.CanThrowDice()? Color.grey : Color.green;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        player.IncreaseDiceThrows();
        SpawnNumber(result);
        Debug.Log(result);

        lastNumber = result;
        lastPlayer = player;
        player.HasThrownDice = true;
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
    }

    private void DestroyNumber() {
        Destroy(currentNumber);
    }

    public void StartAnimation(Piece piece) {
        StartCoroutine(AnimateNumberFly(piece));
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
        GetNewNumber(GameHandler.Instance.currentPlayer);
    }

    protected override Color DetermineColor() {
        return GameHandler.Instance.currentPlayer is Bot || !GameHandler.Instance.currentPlayer.CanThrowDice()? Color.grey : Color.green;
    }
}

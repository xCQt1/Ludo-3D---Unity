using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Piece : MouseClickable
{
    [SerializeField] public float MoveAnimationDuration = .5f; // in millisecs
    [SerializeField] public Player player;
    private NumberGenerator gen;
    private Field currentField;
    [HideInInspector] public int FieldsMoved {get; private set; } = 0;
    [HideInInspector] public bool inAnimation {get; private set;}
    
    public bool CanMove(int fields) => GameHandler.Instance.currentPlayer == player &&      // der Spieler der Figur am Zug ist
                                       NumberGenerator.Instance.lastNumber != 0 &&          // die letzte gewürfelte Nummer keine 0 ist
                                       ((currentField is not BoxField && 
                                            GetField(fields) is not null &&
                                            (GetField(fields).IsFree || GetField(fields).GetCurrentPiece().player != player)) || 
                                        (currentField is BoxField && 
                                            fields == 6 && 
                                            (player.spawnField.IsFree || player.spawnField.GetCurrentPiece().player != player)));
    
    public bool AllowedToMove() => GameHandler.Instance.currentPlayer == player &&
                                   NumberGenerator.Instance.lastNumber != 0 && 
                                   player.moveablePieces.Contains(this);
    public bool CanCapture(int fields) => CanMove(fields) && !GetField(fields).IsFree && !GetField(fields).GetCurrentPiece().player == player;
    public bool CanClearStartField(int fields) => currentField is SpawnField && CanMove(fields);
    public bool CanLeaveBox(int fields) => currentField is BoxField && CanMove(fields);
    public bool CanEnterEndFields(int fields) => GetField(fields) is EndField;
    public bool IsInBox() => currentField is BoxField;
    public bool IsInEndFields() => currentField is EndField;
    
    // Start is called before the first frame update
    void Start()
    {
        SetColor();
        MovePieceToCurrentField();
        gen = NumberGenerator.Instance;
    }

    private void SetColor() {
        GetComponentInChildren<Renderer>().material.color = player.color;
    }

    public void SetStartField(BoxField field) {
        currentField = field;
        currentField.PlacePiece(this);
        MovePieceToCurrentField();
    }

    protected void MovePieceToCurrentField() {
        transform.position = currentField.transform.position;
    }

    public void Capture() {
        foreach (BoxField boxField in player.boxFields) {
            if (boxField.IsFree) {
                MoveToField(boxField);
                FieldsMoved = 0;
                return;
            }
        }
        Debug.LogError("Critical Error: No boxfield empty!");
    }

    public void Move() {
        if (inAnimation || !AllowedToMove()) return;
        if (currentField is BoxField) {
            MoveToField(player.spawnField);
            Debug.Log($"{player.name} has moved {this.name} to his spawn field");
        } else {
            MoveFields(gen.lastNumber);
            Debug.Log($"{player.name} has moved {this.name} for {gen.lastNumber} fields");
        }
        gen.Reset();
    }

    public bool MoveToField(Field field) {
        if (field is null) return false;
        if (field.PlacePiece(this)) {
            currentField.RemoveCurrentPiece();
            currentField = field;
            currentField.PlacePiece(this);
            
            player.HasMoved = true;

            NumberGenerator.Instance.StartAnimation(this);
            StartCoroutine(AnimatePieceMove(field));
            return true;
        } else {
            Debug.Log("Piece move declined: Target field isnt empty");
            return false;
        }
        
    }

    public Field GetField(int numberOfFields) {
        Field targetField = currentField.nextField;
        for (int i=0; i<numberOfFields-1; i++) {
            if (targetField is null || targetField is EndField && !targetField.IsFree) return null;
            targetField = targetField is EndField ? null : targetField.endField is null || targetField.endField.player != this.player ? targetField.nextField : targetField.endField;
        }
        return targetField;
    }

    public void MoveFields(int numberOfFields) {
        if (MoveToField(GetField(numberOfFields))) FieldsMoved += numberOfFields;
    }

    private IEnumerator AnimatePieceMove(Field newField) {
        inAnimation = true;

        while(NumberGenerator.Instance.inAnimation) {
            yield return new WaitForSeconds(.5f);
        }

        float timeElapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = newField.transform.position;

        while (timeElapsed < MoveAnimationDuration) {
            transform.position = Vector3.Lerp(startPos, targetPos, timeElapsed/MoveAnimationDuration);
            Physics.SyncTransforms();
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        inAnimation = false;
        MovePieceToCurrentField();
    }

    public override void OnClick() {
        if (!GameHandler.Instance.currentPlayer.isBot) Move();
    }

    protected override Color DetermineColor() {
        return player != GameHandler.Instance.currentPlayer || GameHandler.Instance.currentPlayer.isBot || gen.lastNumber == 0 || player.HasMoved ? Color.grey : AllowedToMove() ? Color.green : Color.red;
    }
}

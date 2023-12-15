using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class Piece : MouseClickable
{
    [Header("Parameters")]
    [SerializeField] public static float MoveAnimationDuration = .5f;

    [Header("References")]
    [SerializeField] public Player player;
    private NumberGenerator gen;
    private Field currentField;
    [HideInInspector] public int FieldsMoved {get; private set; } = 0;
    [HideInInspector] public bool inAnimation {get; private set;}
    public PieceChecks Checks {get; private set;}   // instance of PieceChecks
    
    new void Awake() {
        base.Awake();   // calls the base class' awake method to prevent certain errors
        Checks = new PieceChecks(this);
    }

    void Start()
    {
        SetMaterial();
        MovePieceToCurrentField();
        gen = NumberGenerator.Instance;
    }

    private void SetMaterial() {    // sets the pieces material to the respective players material
        GetComponentInChildren<Renderer>().material = player.PlayerMaterial;
    }

    public void SetStartField(BoxField field) {     // sets the startfield(the field the piece is to start on)
        currentField = field;
        currentField.PlacePiece(this);
        MovePieceToCurrentField();
    }

    private void MovePieceToCurrentField() {
        transform.position = currentField.transform.position;
    }

    public void Capture() {     // if piece is being captured
        if (currentField is BoxField) return;
        foreach (BoxField boxField in player.BoxFields) {
            if (boxField.IsFree) {
                MoveToField(boxField);
                FieldsMoved = 0;
                return;
            }
        }
        Debug.LogError("Critical Error: No boxfield empty!");
    }

    public void Move() {    // method to handle moving the piece (so that you only have to call this one method)
        if (inAnimation || !Checks.AllowedToMove()) return;
        if (currentField is BoxField) {
            MoveToField(player.SpawnField);     // moves piece to spawn field
            Debug.Log($"{player.name} has moved {this.name} to his spawn field");
        } else {
            MoveFields(gen.lastNumber);
            Debug.Log($"{player.name} has moved {this.name} for {gen.lastNumber} fields");
        }
        gen.Reset();
    }

    public bool MoveToField(Field field) {  // general class to move to another field if outside boxfields (since theyre not connected, it wouldnt technically work)
        if (field is null) return false;
        if (!field.PlacePiece(this)) {
            Debug.Log("Piece move declined: Target field isnt empty");
            return false;
        }
        currentField.RemoveCurrentPiece(); // removes piece from current field
        currentField = field;
        currentField.PlacePiece(this);  // places piece on new field
            
        player.HasMoved = true;

        NumberGenerator.Instance.StartAnimation(this);
        StartCoroutine(AnimatePieceMove(field));    // start move animation
        return true;
        
    }

    public Field GetField(int numberOfFields) {   // returns the target field for a move
        Field targetField = currentField;
        for (int i=0; i<numberOfFields; i++) {
            targetField = targetField.EndField is null || targetField?.EndField.Player != player ? targetField.NextField : targetField.EndField;
            if (targetField is null || (targetField is EndField && !targetField.IsFree)) return null;
        }
        return targetField;
    }

    public bool MoveFields(int numberOfFields) {
        if (MoveToField(GetField(numberOfFields))) {
            FieldsMoved += numberOfFields;
            return true;
        }
        return false;
    }

    private IEnumerator AnimatePieceMove(Field newField) {
        if (GameHandler.Instance.GameState == GameState.MAINMENU) goto end;
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
        end:
        MovePieceToCurrentField();
    }

    public override void OnClick() {
        if (!GameHandler.Instance.CurrentPlayer.IsBot) Move();
    }

    protected override void OnHoverBegin() {}

    protected override void OnHoverStop() {}

    protected override Color DetermineColor() {
        return player != GameHandler.Instance.CurrentPlayer || GameHandler.Instance.CurrentPlayer.IsBot || gen.lastNumber == 0 || player.HasMoved ? Color.grey : Checks.AllowedToMove() ? Color.green : Color.red;
    }

    public struct PieceChecks {  // used to check the status of the piece, mostly for turns and to determine whether this piece is moveable
        Piece piece;
        public PieceChecks(Piece piece) {
            this.piece = piece;
        }
        
    
    public bool CanMove(int fields) => GameHandler.Instance.CurrentPlayer == piece.player &&      // der Spieler der Figur am Zug ist
                                       NumberGenerator.Instance.lastNumber != 0 &&          // die letzte gewürfelte Nummer keine 0 ist
                                       ((piece.currentField is not BoxField && 
                                            piece.GetField(fields) is not null &&
                                            (piece.GetField(fields).IsFree || piece.GetField(fields).GetCurrentPiece().player != piece.player)) || 
                                        (piece.currentField is BoxField && 
                                            fields == 6 && 
                                            (piece.player.SpawnField.IsFree || piece.player.SpawnField.GetCurrentPiece().player != piece.player)));
    
    public bool AllowedToMove() => GameHandler.Instance.CurrentPlayer == piece.player &&
                                   NumberGenerator.Instance.lastNumber != 0 && 
                                   piece.player._MoveablePieces.Contains(piece);
    public bool CanCapture(int fields) => CanMove(fields) && !piece.GetField(fields).IsFree && !piece.GetField(fields).GetCurrentPiece().player == piece.player;
    public bool CanClearStartField(int fields) => piece.currentField == piece.player.SpawnField && CanMove(fields);
    public bool CanLeaveBox(int fields) => piece.currentField is BoxField && CanMove(fields);
    public bool CanEnterEndFields(int fields) => piece.GetField(fields) is EndField && piece.currentField is not EndField && CanMove(fields);
    public bool CanAdvanceInEndFields(int fields) => piece.gen.lastNumber < 4 && piece.currentField is EndField && piece.GetField(fields) == piece.player.GetHighestFreeEndField() && CanMove(fields);
    public bool IsInBox() => piece.currentField is BoxField;
    public bool IsInEndFields() => piece.currentField is EndField;
    }
}

using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.ComponentModel;

public class Player : MonoBehaviour
{
    [SerializeField] public List<BoxField> boxFields;
    [SerializeField] public List<EndField> endFields;
    [SerializeField] public SpawnField spawnField;
    [SerializeField] public GameObject piecePrefab;
    [SerializeField] public Material PlayerMaterial;
    [SerializeField] public Transform CamTransform;
    [SerializeField] public bool isBot;

    protected NumberGenerator gen;
    protected int numberOfThrows;
    [HideInInspector] public List<Piece> pieces {get; private set;} = new();
    [HideInInspector] public bool HasMoved = false;
    [HideInInspector] public bool HasThrownDice = false;
    [HideInInspector] public List<Piece> moveablePieces = new();
    
    public bool NoPiecesMovable() => moveablePieces.Count == 0;
    public bool CanThrowDice() => (numberOfThrows < 1 || (NoPiecesMovable() && gen.lastNumber != 6 && numberOfThrows < 3)) && !HasMoved;
    public EndField GetHighestFreeEndField() => endFields.LastOrDefault(field => field.IsFree);
    
    // Start is called before the first frame update
    protected void Start()
    {
        InstantiatePieces();
        gen = NumberGenerator.Instance;
    }

    protected void InstantiatePieces() {    // instantiates 4 pieces
        foreach (BoxField field in boxFields) {
            GameObject go = Instantiate(piecePrefab, position: transform.position, rotation: Quaternion.identity, parent: transform);
            Piece piece = go.GetComponent<Piece>();
            pieces.Add(go.GetComponent<Piece>());
            piece.SetStartField(field);
            piece.player = this;
        }
    }

    public void UpdateMoveablePieces() {    // redetermines the moveable pieces in the current turn for this player
        if (gen.lastNumber == 0) return;

        if (HasMoved) {
            moveablePieces = new();
            return;
        }
        // Raussetzen
        moveablePieces = pieces.FindAll(piece => piece.Checks.CanLeaveBox(gen.lastNumber));
        // Freimachen
        if (moveablePieces.Count == 0) moveablePieces = pieces.FindAll(piece => piece.Checks.CanClearStartField(gen.lastNumber));
        // Schlagen
        if (moveablePieces.Count == 0) moveablePieces = pieces.FindAll(piece => piece.Checks.CanCapture(gen.lastNumber));
        // andere Züge
        if (moveablePieces.Count == 0) moveablePieces = pieces.FindAll(piece => piece.Checks.CanMove(gen.lastNumber));
    }

    public void StartTurn() { 
        if (isBot) StartCoroutine(BotTurn());
        else StartCoroutine(Turn());
    }

    private IEnumerator Turn() {    // player-controlled turn
        CameraController.Instance.TransitionToPlayerPerspective(this);
        ResetTurnVariables();
        Debug.Log($"{this.name}'s turn has begun");

        // rolling the dice
        while(!HasThrownDice) {
            yield return null;
        }
        
        // moving a piece
        while(!HasMoved && !(numberOfThrows > 2 && NoPiecesMovable())) {
            UpdateMoveablePieces();
            yield return null;
        }

        yield return new WaitForSeconds(1);
        Debug.Log($"{this.name}'s turn has ended");
        ResetTurnVariables();
        GameHandler.Instance.SwitchToNextPlayer();
    }

    private IEnumerator BotTurn() {     // bot-controlled turn
        ResetTurnVariables();
        yield return new WaitForSeconds(2.0f);

        // rolling the dice
        while(CanThrowDice()) {
            gen.GetNewNumber(this);

            UpdateMoveablePieces();
            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitForSeconds(1);

        // move a piece
        if (!NoPiecesMovable()) {
            DetermineBestPieceToMove().Move();
            yield return new WaitForSeconds(2.0f);
        }
        
        ResetTurnVariables();
        GameHandler.Instance.SwitchToNextPlayer();
    }

    private Piece DetermineBestPieceToMove() {  // determines the best piece to move
        UpdateMoveablePieces();
        if (moveablePieces.Count == 0) return null;
        Piece bestPiece = BestPieceToMove(moveablePieces);
        return bestPiece;
    }

    private Piece BestPieceToMove(List<Piece> pieces) { // actually determines the best piece to move
        List<Piece> temp = pieces;
        if (pieces.Count != 1) {
            temp = pieces.FindAll(piece => piece.Checks.CanEnterEndFields(gen.lastNumber));
            if (temp.Count != 1) temp = pieces; else goto x;

            temp = pieces.FindAll(piece => piece.Checks.CanAdvanceInEndFields(gen.lastNumber));
            if (temp.Count != 1) temp = pieces; else goto x;
        }

        x:
        return temp[0];
    }

    public void Reset() {   // returns all pieces to box by capturing them
        foreach (Piece piece in pieces) {
            piece.Capture();
        }
    }

    private void ResetTurnVariables() {     // resets all variabled connected to the current turn
        HasMoved = false;
        HasThrownDice = false;
        numberOfThrows = 0;
        moveablePieces = new();
    }

    public void IncreaseDiceThrows() => numberOfThrows++;
    public void ResetDiceThrows() => numberOfThrows = 0;
}

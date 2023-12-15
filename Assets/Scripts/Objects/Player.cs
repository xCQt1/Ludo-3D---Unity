using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.ComponentModel;

public class Player : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] public bool IsBot;

    [Header("References")]
    [SerializeField] public List<BoxField> BoxFields;
    [SerializeField] public List<EndField> EndFields;
    [SerializeField] public SpawnField SpawnField {get; private set;}
    [SerializeField] private GameObject _piecePrefab;
    [SerializeField] public Material PlayerMaterial;
    [SerializeField] public Transform CamTransform;

    private NumberGenerator _gen;
    private int _numberOfThrows;
    [HideInInspector] public List<Piece> Pieces {get; private set;} = new();
    [HideInInspector] public bool HasMoved = false;
    [HideInInspector] public bool HasThrownDice = false;
    [HideInInspector] public List<Piece> _MoveablePieces {get; private set;} = new();
    
    public bool NoPiecesMovable() => _MoveablePieces.Count == 0;
    public bool CanThrowDice() => (_numberOfThrows < 1 || (NoPiecesMovable() && _gen.lastNumber != 6 && _numberOfThrows < 3)) && !HasMoved;
    public EndField GetHighestFreeEndField() => EndFields.LastOrDefault(field => field.IsFree);
    
    // Start is called before the first frame update
    protected void Start()
    {
        InstantiatePieces();
        _gen = NumberGenerator.Instance;
    }

    protected void InstantiatePieces() {    // instantiates 4 pieces
        foreach (BoxField field in BoxFields) {
            GameObject go = Instantiate(_piecePrefab, position: transform.position, rotation: Quaternion.identity, parent: transform);
            Piece piece = go.GetComponent<Piece>();
            Pieces.Add(go.GetComponent<Piece>());
            piece.SetStartField(field);
            piece.player = this;
        }
    }

    public void UpdateMoveablePieces() {    // redetermines the moveable pieces in the current turn for this player
        if (_gen.lastNumber == 0) return;

        if (HasMoved) {
            _MoveablePieces = new();
            return;
        }
        // Raussetzen
        _MoveablePieces = Pieces.FindAll(piece => piece.Checks.CanLeaveBox(_gen.lastNumber));
        // Freimachen
        if (_MoveablePieces.Count == 0) _MoveablePieces = Pieces.FindAll(piece => piece.Checks.CanClearStartField(_gen.lastNumber));
        // Schlagen
        if (_MoveablePieces.Count == 0) _MoveablePieces = Pieces.FindAll(piece => piece.Checks.CanCapture(_gen.lastNumber));
        // andere Züge
        if (_MoveablePieces.Count == 0) _MoveablePieces = Pieces.FindAll(piece => piece.Checks.CanMove(_gen.lastNumber));
    }

    public void StartTurn() { 
        if (IsBot) StartCoroutine(BotTurn());
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
        while(!HasMoved && !(_numberOfThrows > 2 && NoPiecesMovable())) {
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
            _gen.GetNewNumber(this);

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
        if (_MoveablePieces.Count == 0) return null;
        Piece bestPiece = BestPieceToMove(_MoveablePieces);
        return bestPiece;
    }

    private Piece BestPieceToMove(List<Piece> pieces) { // actually determines the best piece to move
        List<Piece> temp = pieces;
        if (pieces.Count != 1) {
            temp = pieces.FindAll(piece => piece.Checks.CanEnterEndFields(_gen.lastNumber));
            if (temp.Count != 1) temp = pieces; else goto x;

            temp = pieces.FindAll(piece => piece.Checks.CanAdvanceInEndFields(_gen.lastNumber));
            if (temp.Count != 1) temp = pieces; else goto x;
        }

        x:
        return temp[0];
    }

    public void Reset() {   // returns all pieces to box by capturing them
        foreach (Piece piece in Pieces) {
            piece.Capture();
        }
    }

    private void ResetTurnVariables() {     // resets all variabled connected to the current turn
        HasMoved = false;
        HasThrownDice = false;
        _numberOfThrows = 0;
        _MoveablePieces = new();
    }

    public void IncreaseDiceThrows() => _numberOfThrows++;
    public void ResetDiceThrows() => _numberOfThrows = 0;
}

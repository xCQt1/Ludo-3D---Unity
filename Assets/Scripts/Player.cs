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
    public EndField GetHighestFreeEndField() => endFields.FirstOrDefault(field => field.IsFree);
    
    // Start is called before the first frame update
    protected void Start()
    {
        InstantiatePieces();
        gen = NumberGenerator.Instance;
    }

    protected void InstantiatePieces() {
        foreach (BoxField field in boxFields) {
            GameObject go = Instantiate(piecePrefab, position: transform.position, rotation: Quaternion.identity, parent: transform);
            Piece piece = go.GetComponent<Piece>();
            pieces.Add(go.GetComponent<Piece>());
            piece.SetStartField(field);
            piece.player = this;
        }
    }

    public void UpdateMoveablePieces() {
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

    private IEnumerator Turn() {
        CameraController.Instance.TransitionToPlayerPerspective(this);
        ResetTurnVariables();
        Debug.Log($"{this.name}'s turn has begun");

        while(!HasThrownDice) {
            yield return null;
        }
        
        while(!HasMoved && !(numberOfThrows > 2 && NoPiecesMovable())) {
            UpdateMoveablePieces();
            yield return null;
        }

        yield return new WaitForSeconds(1);
        Debug.Log($"{this.name}'s turn has ended");
        ResetTurnVariables();
        GameHandler.Instance.SwitchToNextPlayer();
    }

    private IEnumerator BotTurn() {
        ResetTurnVariables();
        yield return new WaitForSeconds(2.0f);

        while(CanThrowDice()) {
            gen.GetNewNumber(this);

            UpdateMoveablePieces();
            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitForSeconds(1);

        if (!NoPiecesMovable()) {
            DetermineBestPieceToMove().Move();
            yield return new WaitForSeconds(2.0f);
        }
        
        ResetTurnVariables();
        GameHandler.Instance.SwitchToNextPlayer();
    }

    private Piece DetermineBestPieceToMove() {  // Big-Brain des Bots
        UpdateMoveablePieces();
        if (moveablePieces.Count == 0) return null;
        Piece bestPiece = BestPieceToMove(moveablePieces);
        return bestPiece;
    }

    private Piece BestPieceToMove(List<Piece> pieces) {
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

    public void Reset() {
        foreach (Piece piece in pieces) {
            piece.Capture();
        }
    }

    private void ResetTurnVariables() {
        HasMoved = false;
        HasThrownDice = false;
        numberOfThrows = 0;
        moveablePieces = new();
    }

    public void IncreaseDiceThrows() => numberOfThrows++;
    public void ResetDiceThrows() => numberOfThrows = 0;
}

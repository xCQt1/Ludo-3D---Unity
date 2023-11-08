using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using System.Threading;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] public List<BoxField> boxFields;
    [SerializeField] public List<EndField> endFields;
    [SerializeField] public SpawnField spawnField;
    [SerializeField] public GameObject piecePrefab;
    [SerializeField] public Color color;
    [SerializeField] public Transform CamTransform;

    protected NumberGenerator gen;
    protected List<Piece> pieces = new();
    protected int numberOfThrows;
    public bool HasMoved = false;
    public bool HasThrownDice = false;
    public List<Piece> moveablePieces = new();
    
    public bool NoPiecesMovable() => pieces.All(piece => !piece.CanMove(gen.lastNumber));
    public bool CanThrowDice() => (numberOfThrows < 1 || (NoPiecesMovable() && gen.lastNumber != 6 && numberOfThrows < 3)) && !HasMoved;
    
    // Start is called before the first frame update
    void Start()
    {
        InstantiatePieces();
        gen = NumberGenerator.Instance;
    }

    private void InstantiatePieces() {
        foreach (BoxField field in boxFields) {
            GameObject go = Instantiate(piecePrefab, position: transform.position, rotation: Quaternion.identity, parent: transform);
            Piece piece = go.GetComponent<Piece>();
            pieces.Add(go.GetComponent<Piece>());
            piece.SetStartField(field);
            piece.player = this;
        }
    }

    protected void UpdateMoveablePieces() {
        if (gen.lastNumber == 0) return;

        if (HasMoved) {
            moveablePieces = null;
            return;
        }
        // Raussetzen
        moveablePieces = pieces.FindAll(piece => piece.CanLeaveBox(gen.lastNumber));
        // Freimachen
        if (moveablePieces.Count == 0) moveablePieces = pieces.FindAll(piece => piece.CanClearStartField(gen.lastNumber));
        // Schlagen
        if (moveablePieces.Count == 0) moveablePieces = pieces.FindAll(piece => piece.CanCapture(gen.lastNumber));
        // andere Züge
        if (moveablePieces.Count == 0) moveablePieces = pieces.FindAll(piece => piece.CanMove(gen.lastNumber));
        
        Debug.Log(moveablePieces.Count);
    }

    virtual public IEnumerator Turn() {
        // Variablen
        HasMoved = false;
        HasThrownDice = false;
        numberOfThrows = 0;
        moveablePieces = new();

        CameraController.Instance.TransitionToPlayerPerspective(this);
        Debug.Log($"{this.name}'s turn");

        while(!HasThrownDice) {
            yield return new WaitForSeconds(0.5f);
        }
        
        while(!HasMoved && !(numberOfThrows > 2 && NoPiecesMovable())) {
            UpdateMoveablePieces();
            yield return new WaitForSeconds(0.5f);
        }
        
        moveablePieces = new();
        
        yield return new WaitForSeconds(1);
        Debug.Log($"{this.name}'s turn has ended");
        GameHandler.Instance.SwitchToNextPlayer();
    }

    public void Reset() {
        foreach (Piece piece in pieces) {
            piece.Capture();
        }
    }

    public void IncreaseDiceThrows() => numberOfThrows++;
    public void ResetDiceThrows() => numberOfThrows = 0;
}

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
    private int numberOfThrows;
    public bool hasMoved = false;
    public List<Piece> movablePieces = new();
    
    public bool NoPiecesMovable() => pieces.All(piece => !piece.CanMove(gen.lastNumber));
    public bool CanThrowDice() => (numberOfThrows < 1 || (NoPiecesMovable() && gen.lastNumber != 6 && numberOfThrows < 3)) && !hasMoved;
    
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

    private void DetermineMoveablePieces() {
        if (gen.lastNumber == 0) return;
        // Raussetzen
        movablePieces = pieces.FindAll(piece => piece.CanLeaveBox(gen.lastNumber));
        // Freimachen
        movablePieces ??= pieces.FindAll(piece => piece.CanClearStartField(gen.lastNumber));
        // Schlagen
        movablePieces ??= pieces.FindAll(piece => piece.CanCapture(gen.lastNumber));
        // andere Züge
        movablePieces ??= pieces.FindAll(piece => piece.CanMove(gen.lastNumber));
        
        // if (movablePieces.Count == 0) Debug.LogError("No Pieces Moveable");
        Debug.Log(movablePieces.Count);
    }

    virtual public IEnumerator Turn() {
        // Variablen
        hasMoved = false;
        numberOfThrows = 0;
        CameraController.Instance.TransitionToPlayerPerspective(this);
        Debug.Log($"{this.name}'s turn");
        
        while(!hasMoved && !(numberOfThrows > 2 && NoPiecesMovable())) {
            //DetermineMoveablePieces();
            yield return null;
        }

        yield return new WaitForSeconds(1);
        movablePieces = null;
        Debug.Log($"{this.name}'s turn has ended");
        GameHandler.Instance.SwitchToNextPlayer();
    }

    public void IncreaseDiceThrows() => numberOfThrows++;
    public void ResetDiceThrows() => numberOfThrows = 0;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    [SerializeField] public Field nextField;
    [SerializeField] public EndField endField;
    private Piece CurrentPiece;
    public bool IsFree => CurrentPiece == null;
    public Field GetNextField() => nextField;
    public Piece GetCurrentPiece() => CurrentPiece;

    // Start is called before the first frame update
    void Start()
    {
        if (!nextField) Debug.LogError("Field not connected to next field (nextField is null)");
    }

    public bool PlacePiece(Piece piece) {
        if (IsFree){
             CurrentPiece = piece;
        } else if (CurrentPiece.player != piece.player){
            Debug.Log($"{piece.player} has captured a piece from {CurrentPiece.player}");
            CurrentPiece.Capture();
            CurrentPiece = piece;
        } else {
            return false;
        }
        return true;
    }

    public void RemoveCurrentPiece() {
        CurrentPiece = null;
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    private Piece CurrentPiece;
    public bool IsFree => CurrentPiece == null;
    public Field nextField;
    public EndField endField;

    // Start is called before the first frame update
    void Start()
    {
        if (!nextField) Debug.LogError("Field not connected to next field (nextField is null)");
    }

    public Field GetNextField() {
        return nextField;
    }

    public Piece GetCurrentPiece() {
        return CurrentPiece;
    }

    public bool PlacePiece(Piece piece) {
        if (IsFree){
             CurrentPiece = piece;
        } else if (CurrentPiece.player != piece.player){
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

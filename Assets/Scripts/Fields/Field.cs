using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Field NextField;
    [SerializeField] public EndField EndField;

    [HideInInspector] private Piece _currentPiece;

    public bool IsFree => _currentPiece == null;
    public Field GetNextField() => NextField;
    public Piece GetCurrentPiece() => _currentPiece;

    // Start is called before the first frame update
    void Start()
    {
        if (!NextField) Debug.LogError("Field not connected to next field (nextField is null)");
        if (NextField == this) Debug.LogError("NextField is identical to this field");
    }

    public bool PlacePiece(Piece piece) {   // places a given piece on this field
        if (IsFree){
             _currentPiece = piece;
        } else if (_currentPiece.player != piece.player){
            Debug.Log($"{piece.player} has captured a piece from {_currentPiece.player}");
            _currentPiece.Capture();
            _currentPiece = piece;
        } else {
            return false;
        }
        return true;
    }

    public void RemoveCurrentPiece() {
        _currentPiece = null;        
    }
}

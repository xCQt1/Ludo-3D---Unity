using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class Bot : Player
{
    // Start is called before the first frame update
    new void Start() {
        base.Start();  
    }

    override public IEnumerator Turn() {
        while(CanThrowDice()) {
            gen.GetNewNumber(this);
            yield return null;
        }

        if (!NoPiecesMovable()) {
            DetermineBestPieceToMove().Move();
        }
        
        GameHandler.Instance.SwitchToNextPlayer();
    }

    private Piece DetermineBestPieceToMove() {  // Big-Brain des Bots
        UpdateMoveablePieces();
        Piece bestPiece = BestPieceToMove(moveablePieces);
        return bestPiece;
    }

    private Piece BestPieceToMove(List<Piece> pieces) {
        if (!pieces.All(piece => piece.IsInBox()) || pieces.Count > 1) {
            pieces = pieces.FindAll(piece => piece.CanEnterEndFields(gen.lastNumber));  // Figuren die in EndFelder ziehen können
            if (pieces.Count != 1) pieces.OrderBy(piece => piece.FieldsMoved).LastOrDefault();  // Figuren nach gezogenen Feldern sortieren
        }
        return pieces[0];
    }
}

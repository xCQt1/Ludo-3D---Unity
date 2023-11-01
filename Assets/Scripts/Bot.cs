using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : Player
{
    // Start is called before the first frame update
    void Start()
    {
        gen = NumberGenerator.Instance;   
    }

    override public IEnumerator Turn() {
        if (gen.lastNumber == 6) {
            //BoxFields auf Figuren überprüfen, falls vorhanden die rausschicken und returnen
        }
        if (!NoPiecesMovable()) {
            DetermineBestPieceToMove().MoveFields(gen.lastNumber);
        }
        return null;
    }

    private Piece DetermineBestPieceToMove() {
        Piece bestPiece = null;
        return bestPiece;
    }
}

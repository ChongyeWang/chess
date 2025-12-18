using System.Runtime.CompilerServices;
using System.Linq;


public class Board
{
    public List<ChessPiece> Pieces { get; set; } = new List<ChessPiece>();

    public ChessPiece GetPieceAtPosition(int x, int y)
    {
        return Pieces.FirstOrDefault(p => p.XPosition == x && p.YPosition == y);

    }

    public Board Clone()
    {
        var newBoard = new Board();
        foreach (var piece in Pieces)
        {
            newBoard.Pieces.Add(piece.Clone());
        }
        return newBoard;
    }

    
    public ChessPiece GetKing(string color)
    {
        char kingSymbol = color == "white" ? 'K' : 'k';
        return Pieces.FirstOrDefault(p => p.Symbol == kingSymbol);
    }


    public bool IsSquareAttacked(int x, int y, string opponentColor)
    {

        var snapshotPieces = Pieces.ToList();
        foreach (var piece in snapshotPieces)
        {
            if (piece.Color != opponentColor)
            {
                continue;
            }

            if (piece is Pawn)
            {
                int direction = opponentColor == "white" ? -1 : 1;
                if (Math.Abs(piece.XPosition - x) == 1 && y - piece.YPosition == direction)
                {
                    return true;
                }
                continue;
            }
            else{

                if (piece.IsValidMove(x, y, this))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsCheck(string color)
    {
        var king = GetKing(color);
        if (king == null)
        {
            return false;
        }

        string opponentColor = color == "white" ? "black" : "white";
        return IsSquareAttacked(king.XPosition, king.YPosition, opponentColor);
    }

    public bool HasLegalMoves(string color)
    {
        var ownPieces = Pieces.Where(p => p.Color == color).ToList();
        foreach (var piece in ownPieces)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if(piece.IsValidMove(x,y,this))
                    {
                        var clonedBoard = this.Clone();
                        var pieceClone = clonedBoard.GetPieceAtPosition(piece.XPosition, piece.YPosition);
                        if(pieceClone == null)
                        {
                           continue;
                        }

                        var targetPiece = clonedBoard.GetPieceAtPosition(x, y);
                        if (targetPiece != null && targetPiece.Color == color)
                        {
                            continue;
                        }

                        if (targetPiece != null)
                        {
                            clonedBoard.Pieces.Remove(targetPiece);
                        }
                        pieceClone.Move(x, y);

                        if (!clonedBoard.IsCheck(color))
                        {
                            return true;
                        }
                    }
                }
            }
        
        }
        return false;
    }


    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    public bool IsOwn(string color, int x, int y)
    {
        var piece = GetPieceAtPosition(x, y);
        return piece != null && piece.Color == color;
    }

    public bool ISsOpponent(string color, int x, int y)
    {
        var piece = GetPieceAtPosition(x, y);
        return piece != null && piece.Color != color;
    }


    public bool IsOccupied(int fromX, int fromY, int toX, int toY)
    {
        int dx = toX - fromX;
        int dy = toY - fromY;

        bool straight = dx == 0 || dy == 0;
        bool diagonal = Math.Abs(dx) == Math.Abs(dy);

        if (!straight && !diagonal) return true; 
        if (dx == 0 && dy == 0) return false;

        int stepX = Math.Sign(dx);
        int stepY = Math.Sign(dy);

        int currentX = fromX + stepX;
        int currentY = fromY + stepY;

        while (currentX != toX || currentY != toY)
        {
            if (GetPieceAtPosition(currentX, currentY) != null)
            {
                return true;
            }
            currentX += stepX;
            currentY += stepY;
        }
        return false;
    }


    public void Initialize()
    {
        Pieces.Clear();

        SetupRows("white", 6, 7);
        SetupRows("black", 1, 0);

    }

    private void SetupRows(string color, int pawnRow, int backRow)
    {
        AddPawns(color, pawnRow);
        AddBackRow(color, backRow);
    }

    private void AddPawns(string color, int row)
    {
        for (int i = 0; i < 8; i++)
        {
            Pieces.Add(PieceFactory.CreatePiece("Pawn", color, i, row));
        }
    }


    private void AddBackRow(string color, int row)
    {
        Pieces.Add(PieceFactory.CreatePiece("Rook", color, 0, row));
        Pieces.Add(PieceFactory.CreatePiece("Knight", color, 1, row));
        Pieces.Add(PieceFactory.CreatePiece("Bishop", color, 2, row));
        Pieces.Add(PieceFactory.CreatePiece("Queen", color, 3, row));
        Pieces.Add(PieceFactory.CreatePiece("King", color, 4, row));
        Pieces.Add(PieceFactory.CreatePiece("Bishop", color, 5, row));
        Pieces.Add(PieceFactory.CreatePiece("Knight", color, 6, row));
        Pieces.Add(PieceFactory.CreatePiece("Rook", color, 7, row));
    }


    public bool MovePiece(int fromX, int fromY, int toX, int toY, string currentPlayerColor, bool validateSelfCheck = true)
    {
        var piece = GetPieceAtPosition(fromX, fromY);


        if (piece == null || piece.Color != currentPlayerColor)
        {
            return false;
        }

        if (!piece.IsValidMove(toX, toY, this))
        {
            return false;
        }

        var targetPiece = GetPieceAtPosition(toX, toY);
      
        if (targetPiece != null && targetPiece.Color == piece.Color)
        {
            return false;
        }

        if (validateSelfCheck)
        {
            var clonedBoard = this.Clone();
            var clonePiece = clonedBoard.GetPieceAtPosition(fromX, fromY);
            var cloneTargetPiece = clonedBoard.GetPieceAtPosition(toX, toY);

            if (clonePiece == null)
            {
                return false;
            }
            if(cloneTargetPiece != null)
            {
                clonedBoard.Pieces.Remove(cloneTargetPiece);
            }

            clonePiece.Move(toX, toY);
            if(clonedBoard.IsCheck(currentPlayerColor))
            {
                return false;
            }
        }
            
    

        if(targetPiece != null)
        {
            Pieces.Remove(targetPiece);
        }

        piece.Move(toX, toY);


        return true;
    }
}

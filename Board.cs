using System.Runtime.CompilerServices;

public class Board
{
    public List<ChessPiece> Pieces { get; set; } = new List<ChessPiece>();

    public ChessPiece GetPieceAtPosition(int x, int y)
    {
        return Pieces.FirstOrDefault(p => p.XPosition == x && p.YPosition == y);
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
        int stepX = Math.Sign(toX - fromX);
        int stepY = Math.Sign(toY - fromY);

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


    public bool MovePiece(int fromX, int fromY, int toX, int toY, string currentPlayerColor)
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
        if (targetPiece != null && targetPiece.Color != piece.Color)
        {
            Pieces.Remove(targetPiece);
        }
        else if (targetPiece != null && targetPiece.Color == piece.Color)
        {
            return false;
        }

        piece.Move(toX, toY);
        return true;
    }


}
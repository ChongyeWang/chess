public static class PieceFactory
{
    public static ChessPiece CreatePiece(string pieceType, string color, int xPosition, int yPosition)
    {
        switch (pieceType.ToLower())
        {
            case "rook":
                return new Rook(color, xPosition, yPosition);
            case "king":
                return new King(color, xPosition, yPosition);
            case "bishop":
                return new Bishop(color, xPosition, yPosition);
            case "queen":
                return new Queen(color, xPosition, yPosition);
            case "knight":
                return new Knight(color, xPosition, yPosition);
            case "pawn":
                return new Pawn(color, xPosition, yPosition);
            default:
                throw new ArgumentException("Invalid piece type");
        }
    }
}
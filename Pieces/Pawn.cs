public class Pawn : ChessPiece
{
    public Pawn(string color, int xPosition, int yPosition) : base(color, xPosition, yPosition, color == "white" ? 'P' : 'p') { }

   

    public override bool IsValidMove(int newX, int newY, Board board)
    {
        if (IsSameSquare(newX, newY))
        {
            return false;
        }

        if (!board.IsInBounds(newX, newY))
        {
            return false;
        }

        int direction = Color == "white" ? -1 : 1;
        int startRow = Color == "white" ? 6 : 1;

        int dx = newX - XPosition;
        int dy = newY - YPosition;

        var targetPiece = board.GetPieceAtPosition(newX, newY);


        if (dx == 0)
        {
            if (dy == direction && targetPiece == null)
            {
                return true;
            }

            if (dy == 2 * direction && YPosition == startRow && targetPiece == null && board.GetPieceAtPosition(newX, YPosition + direction) == null)
            {
                return true;
            }
        }
        else if (Math.Abs(dx) == 1 && dy == direction)
        {
            if (targetPiece != null && targetPiece.Color != this.Color)
            {
                return true;
            }

        }

        return false;
    }

    

}
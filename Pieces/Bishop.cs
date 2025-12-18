public class Bishop : ChessPiece
{
    public Bishop(string color, int xPosition, int yPosition) : base(color, xPosition, yPosition, color == "white" ? 'B' : 'b') { }

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

        if (board.IsOwn(Color, newX, newY))
        {
            return false;
        }

        

        int dx = newX - XPosition;
        int dy = newY - YPosition;

        if (Math.Abs(dx) != Math.Abs(dy))
        {
            return false;
        }

        if (board.IsOccupied(XPosition, YPosition, newX, newY))
        {
            return false;
        }


        return true;
    }
}
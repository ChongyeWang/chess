public class Rook : ChessPiece
{
    public Rook(string color, int xPosition, int yPosition) : base(color, xPosition, yPosition, color == "white" ? 'R' : 'r') { }

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

        bool sameRow = newX == XPosition;
        bool sameColumn = newY == YPosition;
        if (!sameRow && !sameColumn)
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
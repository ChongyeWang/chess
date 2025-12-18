public class Queen : ChessPiece
{
    public Queen(string color, int xPosition, int yPosition) : base(color, xPosition, yPosition, color == "white" ? 'Q' : 'q') { }

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

        bool isDiagonalMove = Math.Abs(dx) == Math.Abs(dy);
        bool isStraightMove = newX == XPosition || newY == YPosition;

        if (!isDiagonalMove && !isStraightMove)
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
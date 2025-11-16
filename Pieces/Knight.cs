public class Knight : ChessPiece
{
    public Knight(string color, int xPosition, int yPosition) : base(color, xPosition, yPosition, color == "white" ? 'N' : 'n') { }

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

        int dx = Math.Abs(newX - XPosition);
        int dy = Math.Abs(newY - YPosition);

        bool isLShapedMove = (dx == 2 && dy == 1) || (dx == 1 && dy == 2);

        if (!isLShapedMove)
        {
            return false;
        }

        return true;
    }
}
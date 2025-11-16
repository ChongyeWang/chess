public class King : ChessPiece
{
    public King(string color, int xPosition, int yPosition) : base(color, xPosition, yPosition, color == "white" ? 'K' : 'k') { }

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

        if (dx > 1 || dy > 1)
        {
            return false;
        }


        return true;

    }
}
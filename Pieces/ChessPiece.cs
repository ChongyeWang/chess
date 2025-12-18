public abstract class ChessPiece
{
    public string Color { get; set; }
    public int XPosition { get; set; }
    public int YPosition { get; set; }
    public char Symbol { get; set; }

    public ChessPiece(string color, int xPosition, int yPosition, char symbol)
    {
        Color = color;
        XPosition = xPosition;
        YPosition = yPosition;
        Symbol = symbol;
    }

    public virtual ChessPiece Clone()
    {
        return (ChessPiece)this.MemberwiseClone();
    }

    public void Move(int newX, int newY)
    {
        XPosition = newX;
        YPosition = newY;
    }

    

    protected bool IsSameSquare(int newX, int newY)
    {
        return XPosition == newX && YPosition == newY;
    }
    public abstract bool IsValidMove(int newX, int newY, Board board);
    
}
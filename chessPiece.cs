public class chessPiece
{
    public string type { get; set; }
    public string color { get; set; }
    public int xPosition { get; set; }
    public int yPosition { get; set; }


    public chessPiece(string Type, string Color, int XPosition, int YPosition)
    {
        type = Type;
        color = Color;
        xPosition = XPosition;
        yPosition = YPosition;
    }

    public void Move(int newX, int newY)
    {
        xPosition = newX;
        yPosition = newY;
    }
}
public class ChessPieceFactory
{
    private static ChessPieceFactory? _instance;
    private static readonly List<chessPiece> _standardBoardTemplate = new List<chessPiece>();

    private ChessPieceFactory()
    {
    }

    public static ChessPieceFactory Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ChessPieceFactory();
            }
            return _instance;
        }
    }

    private static void InitializeStandardBoardTemplate()
    {
        if (_standardBoardTemplate.Count > 0)
        {
            return;
        }

        string[] pieceTypes = { "r", "n", "b", "q", "k", "b", "n", "r" };

        for (int i = 0; i < 8; i++)
        {
            _standardBoardTemplate.Add(new chessPiece(pieceTypes[i], "black", i, 0));
            _standardBoardTemplate.Add(new chessPiece("p", "black", i, 1));
        }

        for (int i = 0; i < 8; i++)
        {
            _standardBoardTemplate.Add(new chessPiece("p", "white", i, 6));
            _standardBoardTemplate.Add(new chessPiece(pieceTypes[i], "white", i, 7));
        }
    }

    private static bool IsValidPieceType(string pieceType)
    {
        string[] validTypes = { "p", "r", "n", "b", "q", "k" };
        return validTypes.Contains(pieceType.ToLower());
    }

    public static chessPiece CreatePiece(string pieceType, string color, int xPosition, int yPosition)
    {
        string normalizedType = pieceType.ToLower();
        
        if (!IsValidPieceType(normalizedType))
        {
            throw new ArgumentException($"Invalid piece type: {pieceType}", nameof(pieceType));
        }

        if (color.ToLower() != "white" && color.ToLower() != "black")
        {
            throw new ArgumentException($"Invalid color: {color}. Must be 'white' or 'black'", nameof(color));
        }

        if (xPosition < 0 || xPosition > 7 || yPosition < 0 || yPosition > 7)
        {
            throw new ArgumentException($"Invalid position: ({xPosition}, {yPosition}). Must be within board bounds (0-7)", nameof(xPosition));
        }

        return new chessPiece(normalizedType, color.ToLower(), xPosition, yPosition);
    }

    public static chessPiece CreatePawn(string color, int xPosition, int yPosition)
    {
        return CreatePiece("p", color, xPosition, yPosition);
    }

    public static chessPiece CreateRook(string color, int xPosition, int yPosition)
    {
        return CreatePiece("r", color, xPosition, yPosition);
    }

    public static chessPiece CreateKnight(string color, int xPosition, int yPosition)
    {
        return CreatePiece("n", color, xPosition, yPosition);
    }

    public static chessPiece CreateBishop(string color, int xPosition, int yPosition)
    {
        return CreatePiece("b", color, xPosition, yPosition);
    }

    public static chessPiece CreateQueen(string color, int xPosition, int yPosition)
    {
        return CreatePiece("q", color, xPosition, yPosition);
    }

    public static chessPiece CreateKing(string color, int xPosition, int yPosition)
    {
        return CreatePiece("k", color, xPosition, yPosition);
    }

    public static List<chessPiece> CreateStandardBoard()
    {
        InitializeStandardBoardTemplate();
        
        var board = new List<chessPiece>();
        foreach (var piece in _standardBoardTemplate)
        {
            board.Add(new chessPiece(piece.type, piece.color, piece.xPosition, piece.yPosition));
        }
        return board;
    }
}


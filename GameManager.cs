using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

public class GameManager
{
    private ConcurrentDictionary<string, GameRoom> rooms = new ConcurrentDictionary<string, GameRoom>();
    private ConcurrentDictionary<string, string> playerRooms = new ConcurrentDictionary<string, string>();

    public GameRoom CreateRoom(string playerId)
    {
        var roomId = Guid.NewGuid().ToString();
        var room = new GameRoom();
        room.RoomId = roomId;
        room.WhitePlayer = playerId;
        room.Board = InitializeBoard();
        room.CurrentTurn = "white";

        rooms[roomId] = room;
        playerRooms[playerId] = roomId;
        return room;
    }

    public GameRoom JoinRoom(string roomId, string playerId)
    {
        if (rooms.ContainsKey(roomId))
        {
            var room = rooms[roomId];
            if (room.BlackPlayer == null)
            {
                room.BlackPlayer = playerId;
                playerRooms[playerId] = roomId;
                return room;
            }
        }
        return null;
    }

    public GameRoom FindAvailableRoom(string playerId)
    {
        foreach (var room in rooms.Values)
        {
            if (room.BlackPlayer == null)
            {
                return JoinRoom(room.RoomId, playerId);
            }
        }
        return null;
    }

    public GameRoom GetRoomByPlayerId(string playerId)
    {
        if (playerRooms.ContainsKey(playerId))
        {
            string roomId = playerRooms[playerId];
            if (rooms.ContainsKey(roomId))
            {
                return rooms[roomId];
            }
        }
        return null;
    }

    public void RemovePlayer(string playerId)
    {
        if (playerRooms.ContainsKey(playerId))
        {
            string roomId = playerRooms[playerId];
            playerRooms.TryRemove(playerId, out _);
            rooms.TryRemove(roomId, out _);
        }
    }

    private List<chessPiece> InitializeBoard()
    {
        var pieces = new List<chessPiece>();
        string[] blackRanks = { "r", "n", "b", "q", "k", "b", "n", "r" };
        string[] whiteRanks = { "R", "N", "B", "Q", "K", "B", "N", "R" };

        for (int i = 0; i < 8; i++)
        {
            pieces.Add(new chessPiece(blackRanks[i], "black", i, 0));
            pieces.Add(new chessPiece("p", "black", i, 1));
            pieces.Add(new chessPiece("P", "white", i, 6));
            pieces.Add(new chessPiece(whiteRanks[i], "white", i, 7));
        }

        return pieces;
    }

    private bool IsOccupied(int x, int y, List<chessPiece> pieces)
    {
        return pieces.Any(p => p.xPosition == x && p.yPosition == y);
    }

    private bool IsOccupiedByOpponent(int x, int y, string color, List<chessPiece> pieces)
    {
        return pieces.Any(p => p.xPosition == x && p.yPosition == y && p.color != color);
    }

    private bool IsBlocked(int fromX, int fromY, int toX, int toY, List<chessPiece> pieces)
    {
        int stepX = Math.Sign(toX - fromX);
        int stepY = Math.Sign(toY - fromY);

        int currentX = fromX + stepX;
        int currentY = fromY + stepY;

        while (currentX != toX || currentY != toY)
        {
            if (IsOccupied(currentX, currentY, pieces))
            {
                return true;
            }
            currentX += stepX;
            currentY += stepY;
        }
        return false;
    }


    public bool isMoveValid(chessPiece piece, int toX, int toY, List<chessPiece> board)
    {
        int dx = toX - piece.xPosition;
        int dy = toY - piece.yPosition;
        int absDx = Math.Abs(dx);
        int absDy = Math.Abs(dy);

        if (toX < 0 || toX > 7 || toY < 0 || toY > 7)
        {
            return false;
        }

        if (board.Any(p => p.xPosition == toX && p.yPosition == toY && p.color == piece.color))
        {
            return false;
        }

        switch (piece.type.ToLower())
        {
            case "p":
                int direction = (piece.color == "white") ? -1 : 1;
                int startRow = (piece.color == "white") ? 6 : 1;

                if (dx == 0 && dy == direction && !IsOccupied(toX, toY, board))
                    return true;

                if (dx == 0 && dy == 2 * direction && piece.yPosition == startRow &&
                    !IsOccupied(toX, toY, board) && !IsOccupied(toX, toY - direction, board))
                    return true;

                if (absDx == 1 && dy == direction && IsOccupiedByOpponent(toX, toY, piece.color, board))
                    return true;

                return false;

            case "r":
                if (dx != 0 && dy != 0) return false;
                return !IsBlocked(piece.xPosition, piece.yPosition, toX, toY, board);

            case "b":
                if (absDx != absDy) return false;
                return !IsBlocked(piece.xPosition, piece.yPosition, toX, toY, board);

            case "q":
                if (dx == 0 || dy == 0 || absDx == absDy)
                    return !IsBlocked(piece.xPosition, piece.yPosition, toX, toY, board);
                return false;

            case "n":
                return (absDx == 1 && absDy == 2) || (absDx == 2 && absDy == 1);

            case "k":
                return absDx <= 1 && absDy <= 1;

            default:
                return false;

        }
    }
}



public class GameRoom
{
    public string RoomId { get; set; }
    public string WhitePlayer { get; set; }
    public string BlackPlayer { get; set; }
    public List<chessPiece> Board { get; set; } = new List<chessPiece>();
    public string CurrentTurn { get; set; }
}


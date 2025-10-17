using System.Collections.Concurrent;

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

    private string[][] InitializeBoard()
    {
        var board = new string[8][];
        board[0] = new string[] { "r", "n", "b", "q", "k", "b", "n", "r" };
        board[1] = new string[] { "p", "p", "p", "p", "p", "p", "p", "p" };
        board[2] = new string[] { "", "", "", "", "", "", "", "" };
        board[3] = new string[] { "", "", "", "", "", "", "", "" };
        board[4] = new string[] { "", "", "", "", "", "", "", "" };
        board[5] = new string[] { "", "", "", "", "", "", "", "" };
        board[6] = new string[] { "P", "P", "P", "P", "P", "P", "P", "P" };
        board[7] = new string[] { "R", "N", "B", "Q", "K", "B", "N", "R" };
        return board;
    }
}

public class GameRoom
{
    public string RoomId { get; set; }
    public string WhitePlayer { get; set; }
    public string BlackPlayer { get; set; }
    public string[][] Board { get; set; }
    public string CurrentTurn { get; set; }
}


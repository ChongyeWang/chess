using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

public class GameManager
{
    private ConcurrentDictionary<string, GameRoom> rooms = new ConcurrentDictionary<string, GameRoom>();
    private ConcurrentDictionary<string, string> playerRooms = new ConcurrentDictionary<string, string>();

    public GameRoom CreateRoom(string playerId, string playerName, string userDbId)
    {
        var roomId = Guid.NewGuid().ToString();
        var room = new GameRoom();
        room.RoomId = roomId;
        room.WhitePlayerId = playerId;
        room.WhitePlayerName = playerName;
        room.WhiteUserDbId = userDbId;
        room.CurrentTurn = "white";
        rooms[roomId] = room;
        playerRooms[playerId] = roomId;
        return room;
    }

    public GameRoom JoinRoom(string roomId, string playerId, string playerName, string userDbId)
    {
        if (rooms.ContainsKey(roomId))
        {
            var room = rooms[roomId];
            if (room.BlackPlayerId == null)
            {
                room.BlackPlayerId = playerId;
                room.BlackPlayerName = playerName;
                room.BlackUserDbId = userDbId;
                playerRooms[playerId] = roomId;
                return room;
            }
        }
        return null;
    }

    public GameRoom FindAvailableRoom(string playerId, String playerName, string userDbId)
    {
        foreach (var room in rooms.Values)
        {
            if (room.BlackPlayerId == null)
            {
                return JoinRoom(room.RoomId, playerId, playerName, userDbId);
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
}


public class GameRoom
{
    public string RoomId { get; set; }
    public string WhitePlayer { get; set; }
    public string BlackPlayer { get; set; }
    public string WhitePlayerId { get; set; }
    public string BlackPlayerId { get; set; }
    public string WhitePlayerName { get; set; }
    public string BlackPlayerName { get; set; }
    public string WhiteUserDbId { get; set; }
    public string BlackUserDbId { get; set; }
    public string CurrentTurn { get; set; }
    public List<MoveRecord> Moves { get; set; }
    public DateTime StartTime { get; set; }
    public MoveNotifier MoveNotifier { get; set; }

    public Board Board { get; set; } = new Board();
    public GameRoom()
    {
        Moves = new List<MoveRecord>();
        Board.Initialize();
        StartTime = DateTime.UtcNow;
        MoveNotifier = new MoveNotifier();
    }
}
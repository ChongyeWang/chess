using Microsoft.AspNetCore.SignalR;

public class ChessHub : Hub
{
    private GameManager gameManager;
    private MongoDbService mongoService;

    public ChessHub(GameManager gm, MongoDbService mongo)
    {
        gameManager = gm;
        mongoService = mongo;
    }

    public async Task FindGame(string username, string userId)
    {
        string playerId = Context.ConnectionId;
        
        var room = gameManager.FindAvailableRoom(playerId, username, userId);
        
        if (room == null)
        {
            room = gameManager.CreateRoom(playerId, username, userId);
            await Clients.Caller.SendAsync("WaitingForOpponent", new { roomId = room.RoomId });
        }
        else
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomId);
            await Groups.AddToGroupAsync(room.WhitePlayerId, room.RoomId);
            
            await Clients.Group(room.RoomId).SendAsync("GameStart", new
            {
                roomId = room.RoomId,
                whitePlayer = room.WhitePlayerName,
                blackPlayer = room.BlackPlayerName,
                board = room.Board,
                currentTurn = room.CurrentTurn
            });

            await Clients.Client(room.WhitePlayerId).SendAsync("AssignColor", "white");
            await Clients.Client(room.BlackPlayerId).SendAsync("AssignColor", "black");
        }
    }

    public async Task MovePiece(string fromRow, string fromCol, string toRow, string toCol)
    {
        string playerId = Context.ConnectionId;
        var room = gameManager.GetRoomByPlayerId(playerId);

        if (room == null)
        {
            await Clients.Caller.SendAsync("Error", "Not in a game");
            return;
        }

        string playerColor = playerId == room.WhitePlayerId ? "white" : "black";
        if (playerColor != room.CurrentTurn)
        {
            await Clients.Caller.SendAsync("Error", "Not your turn");
            return;
        }

        int fromR = int.Parse(fromRow);
        int fromC = int.Parse(fromCol);
        int toR = int.Parse(toRow);
        int toC = int.Parse(toCol);

        string piece = room.Board[fromR][fromC];
        room.Board[toR][toC] = piece;
        room.Board[fromR][fromC] = "";

        var moveRecord = new MoveRecord
        {
            MoveNumber = room.Moves.Count + 1,
            Player = playerColor,
            FromRow = fromR,
            FromCol = fromC,
            ToRow = toR,
            ToCol = toC,
            Piece = piece,
            Timestamp = DateTime.UtcNow
        };
        room.Moves.Add(moveRecord);

        room.CurrentTurn = room.CurrentTurn == "white" ? "black" : "white";

        await Clients.Group(room.RoomId).SendAsync("PieceMoved", new
        {
            fromRow = fromR,
            fromCol = fromC,
            toRow = toR,
            toCol = toC,
            piece = piece,
            currentTurn = room.CurrentTurn,
            moveNumber = moveRecord.MoveNumber
        });
    }
    
    public async Task EndGame(string reason)
    {
        string playerId = Context.ConnectionId;
        var room = gameManager.GetRoomByPlayerId(playerId);

        if (room == null)
        {
            await Clients.Caller.SendAsync("Error", "Not in a game");
            return;
        }

        var gameHistory = new GameHistory
        {
            WhitePlayerId = room.WhiteUserDbId,
            BlackPlayerId = room.BlackUserDbId,
            WhitePlayerName = room.WhitePlayerName,
            BlackPlayerName = room.BlackPlayerName,
            Moves = room.Moves,
            Result = reason,
            StartTime = room.StartTime,
            EndTime = DateTime.UtcNow,
            EndReason = reason
        };

        await mongoService.GameHistories.InsertOneAsync(gameHistory);

        await Clients.Group(room.RoomId).SendAsync("GameEnded", new
        {
            reason = reason,
            totalMoves = room.Moves.Count,
            gameId = gameHistory.Id
        });

        gameManager.RemovePlayer(room.WhitePlayerId);
        if (room.BlackPlayerId != null)
        {
            gameManager.RemovePlayer(room.BlackPlayerId);
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        string playerId = Context.ConnectionId;
        var room = gameManager.GetRoomByPlayerId(playerId);

        if (room != null)
        {
            await Clients.Group(room.RoomId).SendAsync("OpponentDisconnected");
            gameManager.RemovePlayer(playerId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}


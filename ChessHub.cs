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

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public async Task FindGame()
    {
        string playerId = Context.ConnectionId;
        var room = gameManager.FindAvailableRoom(playerId, username, userId);

        if (room == null)
        {
            room = gameManager.CreateRoom(playerId, username, userId);
            room.WhitePlayer = playerId;

            await Clients.Caller.SendAsync("WaitingForOpponent", new { roomId = room.RoomId });
            return;
        }


        room.BlackPlayer = playerId;
        room.CurrentTurn = "white";

        await Groups.AddToGroupAsync(room.WhitePlayer, room.RoomId);
        await Groups.AddToGroupAsync(room.BlackPlayer, room.RoomId);


        await Clients.Client(room.WhitePlayer).SendAsync("AssignColor", "white");
        await Clients.Client(room.BlackPlayer).SendAsync("AssignColor", "black");

        await Clients.Group(room.RoomId).SendAsync("GameStart", new
        {
            roomId = room.RoomId,
            whitePlayer = room.WhitePlayer,
            blackPlayer = room.BlackPlayer,
            board = room.Board,
            currentTurn = room.CurrentTurn
        });
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

        string playerColor = (playerId == room.WhitePlayer) ? "white" : "black";

        if (playerColor != room.CurrentTurn)
        {
            await Clients.Caller.SendAsync("Error", "Not your turn!");
            return;
        }

        int fromR = int.Parse(fromRow);
        int fromC = int.Parse(fromCol);
        int toR = int.Parse(toRow);
        int toC = int.Parse(toCol);

        var piece = room.Board.FirstOrDefault(p => p.xPosition == fromC && p.yPosition == fromR);
        if (piece == null)
        {
            await Clients.Caller.SendAsync("Error", "No piece at that position");
            return;
        }

        if (piece.color != playerColor)
        {
            await Clients.Caller.SendAsync("Error", "You can't move your opponent's piece!");
            return;
        }

        bool isValidMove = gameManager.isMoveValid(piece, toC, toR, room.Board);
        if (!isValidMove)
        {
            await Clients.Caller.SendAsync("Error", "Invalid move");
            return;
        }

        var captured = room.Board.FirstOrDefault(p => p.xPosition == toC && p.yPosition == toR);
        if (captured != null)
        {
            room.Board.Remove(captured);
        }

        piece.xPosition = toC;
        piece.yPosition = toR;

        room.CurrentTurn = (room.CurrentTurn == "white") ? "black" : "white";

        await Clients.Group(room.RoomId).SendAsync("UpdateBoard", room.Board, room.CurrentTurn);



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


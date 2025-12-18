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

    public async Task FindGame(string username, string userId)
    {
        string playerId = Context.ConnectionId;
        var room = gameManager.FindAvailableRoom(playerId, username, userId);

        if (room == null)
        {
            room = gameManager.CreateRoom(playerId, username, userId);
            room.WhitePlayer = playerId;
            room.WhitePlayerId = playerId;
            room.WhitePlayerName = username;
            room.WhiteUserDbId = userId;

            await Clients.Caller.SendAsync("WaitingForOpponent", new { roomId = room.RoomId });
            return;
        }

        room.BlackPlayer = playerId;
        room.BlackPlayerId = playerId;
        room.BlackPlayerName = username;
        room.BlackUserDbId = userId;
        room.CurrentTurn = "white";

        var moveDisplayObserver = new MoveDisplayObserver();
        room.MoveNotifier.Attach(moveDisplayObserver);

        await Groups.AddToGroupAsync(room.WhitePlayer, room.RoomId);
        await Groups.AddToGroupAsync(room.BlackPlayer, room.RoomId);

        await Clients.Client(room.WhitePlayer).SendAsync("AssignColor", "white");
        await Clients.Client(room.BlackPlayer).SendAsync("AssignColor", "black");

        await Clients.Group(room.RoomId).SendAsync("GameStart", new
        {
            roomId = room.RoomId,
            whitePlayer = room.WhitePlayerName,
            blackPlayer = room.BlackPlayerName,
            board = room.Board,
            currentTurn = room.CurrentTurn
        });
    }

    public async Task MovePiece(string fromRow, string fromCol, string toRow, string toCol)
    {
        Console.WriteLine($"[MovePiece] START {fromRow},{fromCol} -> {toRow},{toCol} conn={Context.ConnectionId}");

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

        Console.WriteLine("A parsed/turn ok");


        var piece = room.Board.GetPieceAtPosition(fromC, fromR);

        Console.WriteLine("B got piece");

        if (piece == null)
        {
            await Clients.Caller.SendAsync("Error", "No piece at that position");
            return;
        }

        if (piece.Color != playerColor)
        {
            await Clients.Caller.SendAsync("Error", "You can't move your opponent's piece!");
            return;
        }

        bool moved = room.Board.MovePiece(fromC, fromR, toC, toR, playerColor);
        if (!moved)
        {
            await Clients.Caller.SendAsync("Error", "Invalid move for that piece");
            return;
        }


        var moveRecord = new MoveRecord
        {
            MoveNumber = room.Moves.Count + 1,
            Player = playerColor,
            FromRow = fromR,
            FromCol = fromC,
            ToRow = toR,
            ToCol = toC,
            Piece = piece.Symbol.ToString(),
            Timestamp = DateTime.UtcNow
        };
        room.Moves.Add(moveRecord);
        
        Console.WriteLine("C move recorded");

        string opponentColor = (playerColor == "white") ? "black" : "white";
        bool isCheck = room.Board.IsCheck(opponentColor);
        Console.WriteLine($"D isCheck={isCheck}");
        bool HasLegalMoves = room.Board.HasLegalMoves(opponentColor);
        Console.WriteLine($"E hasLegalMoves={HasLegalMoves}");


        if(isCheck && !HasLegalMoves)
        {
            await Clients.Group(room.RoomId).SendAsync("UpdateBoard", room.Board, "Game Over");
            await Clients.Group(room.RoomId).SendAsync("Game Over", new {
                winner = playerColor,
                reason = "checkmate"
            });
            await EndGame($"Checkmate. {playerColor} wins.");
            return;
        }

        if(!isCheck && !HasLegalMoves)
        {
            await Clients.Group(room.RoomId).SendAsync("UpdateBoard", room.Board, "Game Over");
            await Clients.Group(room.RoomId).SendAsync("Game Over", new {
                winner = "Draw",
                reason = "stalemate"
            });
            await EndGame("Stalemate. It's a draw.");
            return;
        }
        room.CurrentTurn = opponentColor;

        room.MoveNotifier.Notify(moveRecord);
        Console.WriteLine("F move notified");

        await Clients.Group(room.RoomId).SendAsync("UpdateBoard", room.Board, room.CurrentTurn);
        Console.WriteLine("G board updated");
        await Clients.Group(room.RoomId).SendAsync("MoveMade", new
        {
            moveNumber = moveRecord.MoveNumber,
            player = moveRecord.Player,
            fromRow = moveRecord.FromRow,
            fromCol = moveRecord.FromCol,
            toRow = moveRecord.ToRow,
            toCol = moveRecord.ToCol,
            piece = moveRecord.Piece,
            timestamp = moveRecord.Timestamp
        });

        if(isCheck)
        {
            await Clients.Group(room.RoomId).SendAsync("Check", opponentColor);
        }

        Console.WriteLine("[MovePiece] END (about to return)");


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

        var gameHistory = new GameHistoryBuilder()
            .FromGameRoom(room)
            .WithResult(reason)
            .WithEndReason(reason)
            .Build();

        await mongoService.GameHistories.InsertOneAsync(gameHistory);

        await Clients.Group(room.RoomId).SendAsync("GameEnded", new
        {
            reason = reason,
            totalMoves = room.Moves.Count,
            gameId = gameHistory.Id
        });

        gameManager.RemovePlayer(room.WhitePlayer);
        if (room.BlackPlayer != null)
        {
            gameManager.RemovePlayer(room.BlackPlayer);
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


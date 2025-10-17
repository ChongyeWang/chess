using Microsoft.AspNetCore.SignalR;

public class ChessHub : Hub
{
    private GameManager gameManager;

    public ChessHub(GameManager gm)
    {
        gameManager = gm;
    }

    public async Task FindGame()
    {
        string playerId = Context.ConnectionId;
        
        var room = gameManager.FindAvailableRoom(playerId);
        
        if (room == null)
        {
            room = gameManager.CreateRoom(playerId);
            await Clients.Caller.SendAsync("WaitingForOpponent", new { roomId = room.RoomId });
        }
        else
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomId);
            await Groups.AddToGroupAsync(room.WhitePlayer, room.RoomId);
            
            await Clients.Group(room.RoomId).SendAsync("GameStart", new
            {
                roomId = room.RoomId,
                whitePlayer = room.WhitePlayer,
                blackPlayer = room.BlackPlayer,
                board = room.Board,
                currentTurn = room.CurrentTurn
            });

            await Clients.Client(room.WhitePlayer).SendAsync("AssignColor", "white");
            await Clients.Client(room.BlackPlayer).SendAsync("AssignColor", "black");
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

        string playerColor = playerId == room.WhitePlayer ? "white" : "black";
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

        room.CurrentTurn = room.CurrentTurn == "white" ? "black" : "white";

        await Clients.Group(room.RoomId).SendAsync("PieceMoved", new
        {
            fromRow = fromR,
            fromCol = fromC,
            toRow = toR,
            toCol = toC,
            piece = piece,
            currentTurn = room.CurrentTurn
        });
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


public class GameHistoryBuilder
{
    private string _whitePlayerId;
    private string _blackPlayerId;
    private string _whitePlayerName;
    private string _blackPlayerName;
    private List<MoveRecord> _moves;
    private string _result;
    private DateTime _startTime;
    private DateTime _endTime;
    private string _endReason;

    public GameHistoryBuilder()
    {
        _moves = new List<MoveRecord>();
        _endTime = DateTime.UtcNow;
    }

    public GameHistoryBuilder WithWhitePlayer(string playerId, string playerName)
    {
        _whitePlayerId = playerId;
        _whitePlayerName = playerName;
        return this;
    }

    public GameHistoryBuilder WithBlackPlayer(string playerId, string playerName)
    {
        _blackPlayerId = playerId;
        _blackPlayerName = playerName;
        return this;
    }

    public GameHistoryBuilder WithMoves(List<MoveRecord> moves)
    {
        _moves = moves ?? new List<MoveRecord>();
        return this;
    }

    public GameHistoryBuilder WithResult(string result)
    {
        _result = result;
        return this;
    }

    public GameHistoryBuilder WithStartTime(DateTime startTime)
    {
        _startTime = startTime;
        return this;
    }

    public GameHistoryBuilder WithEndTime(DateTime endTime)
    {
        _endTime = endTime;
        return this;
    }

    public GameHistoryBuilder WithEndReason(string endReason)
    {
        _endReason = endReason;
        return this;
    }

    public GameHistoryBuilder FromGameRoom(GameRoom room)
    {
        _whitePlayerId = room.WhiteUserDbId;
        _blackPlayerId = room.BlackUserDbId;
        _whitePlayerName = room.WhitePlayerName;
        _blackPlayerName = room.BlackPlayerName;
        _moves = room.Moves ?? new List<MoveRecord>();
        _startTime = room.StartTime;
        return this;
    }

    public GameHistory Build()
    {
        return new GameHistory
        {
            WhitePlayerId = _whitePlayerId,
            BlackPlayerId = _blackPlayerId,
            WhitePlayerName = _whitePlayerName,
            BlackPlayerName = _blackPlayerName,
            Moves = _moves,
            Result = _result,
            StartTime = _startTime,
            EndTime = _endTime,
            EndReason = _endReason
        };
    }
}


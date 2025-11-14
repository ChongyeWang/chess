using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class GameHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string WhitePlayerId { get; set; }
    public string BlackPlayerId { get; set; }
    public string WhitePlayerName { get; set; }
    public string BlackPlayerName { get; set; }
    public List<MoveRecord> Moves { get; set; }
    public string Result { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string EndReason { get; set; }
}

public class MoveRecord
{
    public int MoveNumber { get; set; }
    public string Player { get; set; }
    public int FromRow { get; set; }
    public int FromCol { get; set; }
    public int ToRow { get; set; }
    public int ToCol { get; set; }
    public string Piece { get; set; }
    public DateTime Timestamp { get; set; }
}

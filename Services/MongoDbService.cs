using MongoDB.Driver;

public class MongoDbService
{
    private IMongoDatabase database;
    
    public MongoDbService(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        database = client.GetDatabase(databaseName);
    }
    
    public IMongoCollection<User> Users => database.GetCollection<User>("chess_users");
    public IMongoCollection<GameHistory> GameHistories => database.GetCollection<GameHistory>("chess_game_history");
}


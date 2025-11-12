using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

public class AuthService
{
    private MongoDbService mongoService;
    
    public AuthService(MongoDbService mongo)
    {
        mongoService = mongo;
    }
    
    public async Task<User> Register(string username, string password, string email)
    {
        var existingUser = await mongoService.Users
            .Find(u => u.Username == username)
            .FirstOrDefaultAsync();
            
        if (existingUser != null)
        {
            return null;
        }
        
        var user = new User
        {
            Username = username,
            Password = HashPassword(password),
            Email = email,
            CreatedAt = DateTime.UtcNow,
            Wins = 0,
            Losses = 0,
            Rating = 1200
        };
        
        await mongoService.Users.InsertOneAsync(user);
        return user;
    }
    
    public async Task<User> Login(string username, string password)
    {
        var hashedPassword = HashPassword(password);
        var user = await mongoService.Users
            .Find(u => u.Username == username && u.Password == hashedPassword)
            .FirstOrDefaultAsync();
            
        return user;
    }
    
    public async Task<User> GetUserById(string userId)
    {
        return await mongoService.Users
            .Find(u => u.Id == userId)
            .FirstOrDefaultAsync();
    }
    
    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}


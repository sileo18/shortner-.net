public class User
{
    public User(string username)
    {
        Id = Guid.NewGuid();
        Username = username;
    }

    public Guid Id { get; set; }
    public string Username { get; set; }
}
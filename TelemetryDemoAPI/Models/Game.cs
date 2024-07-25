namespace GameLibraryAPI.Models;

public class Game
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Genre { get; set; }
    public bool IsCompleted { get; set; }
    public string? Secret { get; set; }
}
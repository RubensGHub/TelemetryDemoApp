namespace GameLibraryAPI.Models;

public class GameDTO
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Genre { get; set; }
    public bool IsOnSteam { get; set; }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameLibraryAPI.Models;
using System.Diagnostics.Metrics;

namespace GameLibraryAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GamesController(GameContext context, ILogger<GamesController> logger, Meter meter) : ControllerBase
{
    private readonly GameContext _context = context;
    private readonly ILogger<GamesController> _logger = logger;
    private readonly UpDownCounter<int> _gameUpDownCounter = meter.CreateUpDownCounter<int>("game_count", unit: "game", description: "Total number of games in the library.");

    // GET: api/Games
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameDTO>>> GetGames()
    {   
        _logger.LogInformation("Fonction GetGames appelée.");

        var games = await _context.Games
            .Select(x => ItemToDTO(x))
            .ToListAsync();

        _logger.LogInformation("GetGames a retourné {Count} jeux", games.Count);

        return games;
    }

    // GET: api/games/5
    // <snippet_GetByID>
    [HttpGet("{id}")]
    public async Task<ActionResult<GameDTO>> GetGameByID(long id)
    {
        _logger.LogInformation("Fonction GetGameByID appelée.");

        var Game = await _context.Games.FindAsync(id);

        if (Game == null)
        {
            _logger.LogWarning("l'ID '{id}' ne correspond à aucun jeu dans la bibliothèque.", id);
            return NotFound();
        }

        _logger.LogInformation("jeu trouvé : {name}", Game.Name);

        return ItemToDTO(Game);
    }
    // </snippet_GetByID>

    // PUT: api/Games/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // <snippet_Update>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGame(long id, GameDTO GameDTO)
    {
        _logger.LogInformation("Fonction UpdateGame appelée");

        if (id != GameDTO.Id)
        {
            _logger.LogWarning("les ID ne correspondent pas ({id1} et {id2})", id, GameDTO.Id);
            return BadRequest();
        }

        var Game = await _context.Games.FindAsync(id);

        if (Game == null)
        {   
            _logger.LogWarning("l'ID '{id}' ne correspond à aucun jeu dans la bibliothèque.", id);
            return NotFound();
        }

        Game.Name = GameDTO.Name;
        Game.Genre = GameDTO.Genre;
        Game.IsOnSteam = GameDTO.IsOnSteam;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!GameExists(id))
        {
            _logger.LogWarning("l'ID '{id}' ne correspond à aucun jeu dans la bibliothèque.", id);
            return NotFound();
        }

        _logger.LogInformation("Jeu mis à jour.");

        return NoContent();
    }
    // </snippet_Update>

    // POST: api/Games
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // <snippet_Create>
    [HttpPost]
    public async Task<ActionResult<GameDTO>> PostGame(GameDTO GameDTO)
    {
        _logger.LogInformation("Fonction PostGame appelée.");

        var Game = new Game
        {   
            Name = GameDTO.Name,
            Genre = GameDTO.Genre,
            IsOnSteam = GameDTO.IsOnSteam            
        };

        _context.Games.Add(Game);
        await _context.SaveChangesAsync();

        _gameUpDownCounter.Add(1);

        _logger.LogInformation("'{name}' ajouté à la bibliothèque.", Game.Name);

        return CreatedAtAction(
            nameof(GetGameByID),
            new { id = Game.Id },
            ItemToDTO(Game));
    }
    // </snippet_Create>

    // DELETE: api/Games/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGame(long id)
    {
        _logger.LogInformation("Fonction DeleteGame appelée.");

        var Game = await _context.Games.FindAsync(id);

        if (Game == null)
        {   
            _logger.LogWarning("l'ID '{id}' ne correspond à aucun jeu dans la bibliothèque.", id);
            return NotFound();
        }

        _context.Games.Remove(Game);
        await _context.SaveChangesAsync();

        _gameUpDownCounter.Add(-1);

        _logger.LogInformation("le jeu '{name}' a été supprimé avec succès.", Game.Name);

        return NoContent();
    }

    private bool GameExists(long id)
    {
        return _context.Games.Any(e => e.Id == id);
    }

    private static GameDTO ItemToDTO(Game Game) =>
       new()
       {
           Id = Game.Id,
           Name = Game.Name,
           Genre = Game.Genre,
           IsOnSteam = Game.IsOnSteam
       };
}
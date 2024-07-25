using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameLibraryAPI.Models;

namespace GameLibraryAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GamesController : ControllerBase
{
    private readonly GameContext _context;

    public GamesController(GameContext context)
    {
        _context = context;
    }

    // GET: api/Games
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameDTO>>> GetGames()
    {
        return await _context.Games
            .Select(x => ItemToDTO(x))
            .ToListAsync();
    }

    // GET: api/games/5
    // <snippet_GetByID>
    [HttpGet("{id}")]
    public async Task<ActionResult<GameDTO>> GetGame(long id)
    {
        var Game = await _context.Games.FindAsync(id);

        if (Game == null)
        {
            return NotFound();
        }

        return ItemToDTO(Game);
    }
    // </snippet_GetByID>

    // PUT: api/Games/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // <snippet_Update>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutGame(long id, GameDTO GameDTO)
    {
        if (id != GameDTO.Id)
        {
            return BadRequest();
        }

        var Game = await _context.Games.FindAsync(id);
        if (Game == null)
        {
            return NotFound();
        }

        Game.Name = GameDTO.Name;
        Game.Genre = GameDTO.Genre;
        Game.IsCompleted = GameDTO.IsCompleted;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!GameExists(id))
        {
            return NotFound();
        }

        return NoContent();
    }
    // </snippet_Update>

    // POST: api/Games
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // <snippet_Create>
    [HttpPost]
    public async Task<ActionResult<GameDTO>> PostGame(GameDTO GameDTO)
    {
        var Game = new Game
        {   
            Name = GameDTO.Name,
            Genre = GameDTO.Genre,
            IsCompleted = GameDTO.IsCompleted            
        };

        _context.Games.Add(Game);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetGame),
            new { id = Game.Id },
            ItemToDTO(Game));
    }
    // </snippet_Create>

    // DELETE: api/Games/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGame(long id)
    {
        var Game = await _context.Games.FindAsync(id);
        if (Game == null)
        {
            return NotFound();
        }

        _context.Games.Remove(Game);
        await _context.SaveChangesAsync();

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
           IsCompleted = Game.IsCompleted
       };
}
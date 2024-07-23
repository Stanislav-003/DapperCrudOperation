using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace DapperCrud.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SuperHeroController(IConfiguration configuration) : ControllerBase
{
    //private readonly string _connectionString;

    //public SuperHeroController(IConfiguration configuration)
    //{
    //    _connectionString = configuration.GetConnectionString("DefaultConnection");
    //}

    [HttpGet]
    public async Task<ActionResult<List<SuperHero>>> GetAllSuperHeroes()
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var heroes = await SelectAllHeroes(connection);
            return Ok(heroes);
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{heroId}")]
    public async Task<ActionResult<SuperHero>> GetHero(int heroId)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var hero = await connection.QueryFirstOrDefaultAsync<SuperHero>(
                "SELECT * FROM superheroes WHERE id = @Id",
                new { Id = heroId });

            if (hero == null)
            {
                return NotFound();
            }

            return Ok(hero);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<List<SuperHero>>> CreateHero(SuperHero superHero)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "INSERT INTO superheroes (name, firstname, lastname, place) VALUES (@Name, @FirstName, @LastName, @Place)",
                superHero);

            return await GetAllSuperHeroes(); 
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut]
    public async Task<ActionResult<List<SuperHero>>> UpdateHero(SuperHero superHero)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE superheroes SET name = @Name, firstname = @FirstName, lastname = @LastName, place = @Place WHERE id = @Id",
                superHero);

            if (rowsAffected == 0)
            {
                return NotFound();
            }

            return await GetAllSuperHeroes();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    private static async Task<IEnumerable<SuperHero>> SelectAllHeroes(SqlConnection connection)
    {
        return await connection.QueryAsync<SuperHero>("SELECT * FROM superheroes");
    }
}
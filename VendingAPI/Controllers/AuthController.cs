using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace VendingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string ConnectionString = "Data Source=vending.db";

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, FullName, Email, Role FROM Users WHERE Email = $email AND PasswordHash = $password";
        cmd.Parameters.AddWithValue("$email", request.Email);
        cmd.Parameters.AddWithValue("$password", request.Password); // Без хэширования!

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return Ok(new UserDto
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Email = reader.GetString(2),
                Role = reader.GetString(3)
            });
        }
        return Unauthorized(new { message = "Неверный email или пароль" });
    }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}
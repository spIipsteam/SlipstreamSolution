using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace VendingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private const string ConnectionString = "Data Source=vending.db";

    [HttpGet]
    public IActionResult Get()
    {
        var news = new List<object>();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Title, Content, date(PublishDate) as PublishDate FROM News ORDER BY PublishDate DESC LIMIT 5";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            news.Add(new
            {
                Title = reader.GetString(0),
                Content = reader.GetString(1),
                Date = reader.GetString(2)
            });
        }
        return Ok(news);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace VendingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private const string ConnectionString = "Data Source=vending.db";

    [HttpGet("last10days")]
    public IActionResult GetLast10Days()
    {
        var result = new List<object>();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT date(SaleDate) as Date, SUM(Amount) as TotalAmount, COUNT(*) as TotalQuantity
            FROM Sales 
            WHERE SaleDate >= date('now', '-10 days')
            GROUP BY date(SaleDate)
            ORDER BY Date DESC";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new
            {
                Date = reader.GetString(0),
                Amount = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                Quantity = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
            });
        }
        return Ok(result);
    }
}
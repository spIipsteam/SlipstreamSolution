using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace VendingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendingController : ControllerBase
{
    private const string ConnectionString = "Data Source=vending.db";

    [HttpGet]
    public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = null)
    {
        var machines = new List<VendingMachine>();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        int offset = (page - 1) * pageSize;
        var cmd = connection.CreateCommand();

        string sql = @"SELECT Id, SerialNumber, InventoryNumber, Name, Model, 
                       IFNULL(Company, 'Не указан') as Company, 
                       IFNULL(ModemId, '-1') as ModemId, 
                       Location, CommissioningDate, Status 
                       FROM VendingMachines WHERE 1=1";

        if (!string.IsNullOrEmpty(search))
        {
            sql += " AND Name LIKE $search";
            cmd.Parameters.AddWithValue("$search", $"%{search}%");
        }

        sql += " LIMIT $limit OFFSET $offset";
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("$limit", pageSize);
        cmd.Parameters.AddWithValue("$offset", offset);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            machines.Add(new VendingMachine
            {
                Id = reader.GetInt32(0),
                SerialNumber = reader.GetString(1),
                InventoryNumber = reader.GetString(2),
                Name = reader.GetString(3),
                Model = reader.GetString(4),
                Company = reader.GetString(5),
                ModemId = reader.GetString(6),
                Location = reader.GetString(7),
                CommissioningDate = reader.IsDBNull(8) ? "" : reader.GetString(8),
                Status = reader.GetString(9)
            });
        }
        return Ok(new { data = machines, total = GetTotalCount(connection, search) });
    }

    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM VendingMachines";
        int total = Convert.ToInt32(cmd.ExecuteScalar());

        cmd.CommandText = "SELECT COUNT(*) FROM VendingMachines WHERE Status = 'Работает'";
        int working = Convert.ToInt32(cmd.ExecuteScalar());

        cmd.CommandText = "SELECT SUM(TotalRevenue) FROM VendingMachines";
        object result = cmd.ExecuteScalar();
        decimal revenue = result == DBNull.Value ? 0 : Convert.ToDecimal(result);

        return Ok(new { total, working, revenue });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM VendingMachines WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
        return Ok(new { message = "Удалено" });
    }

    [HttpPut("{id}/unbind-modem")]
    public IActionResult UnbindModem(int id)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE VendingMachines SET ModemId = NULL WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
        return Ok(new { message = "Модем отвязан" });
    }

    private int GetTotalCount(SqliteConnection connection, string search)
    {
        var cmd = connection.CreateCommand();
        string sql = "SELECT COUNT(*) FROM VendingMachines WHERE 1=1";
        if (!string.IsNullOrEmpty(search))
        {
            sql += " AND Name LIKE $search";
            cmd.Parameters.AddWithValue("$search", $"%{search}%");
        }
        cmd.CommandText = sql;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}

public class VendingMachine
{
    public int Id { get; set; }
    public string SerialNumber { get; set; }
    public string InventoryNumber { get; set; }
    public string Name { get; set; }
    public string Model { get; set; }
    public string Company { get; set; }
    public string ModemId { get; set; }
    public string Location { get; set; }
    public string CommissioningDate { get; set; }
    public string Status { get; set; }
}
namespace VendingAPI.Models;

public class VendingMachine
{
    public int Id { get; set; }
    public string SerialNumber { get; set; }
    public string InventoryNumber { get; set; }
    public string Name { get; set; }
    public string Model { get; set; }
    public string Location { get; set; }
    public string Status { get; set; }
    public string ModemId { get; set; }
    public decimal CashAmount { get; set; } // для монитора
}
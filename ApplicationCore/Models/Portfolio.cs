namespace ApplicationCore.Models;

public class Portfolio
{
    public Dictionary<string, decimal> Balances { get; } = new Dictionary<string, decimal>();
}
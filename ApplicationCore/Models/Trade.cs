namespace ApplicationCore.Models;

public class Trade
{
    /// <summary>
    /// Валютная пара
    /// </summary>
    public string Pair { get; set; }

    /// <summary>
    /// Цена трейда
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Объем трейда
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Направление (buy/sell)
    /// </summary>
    public string Side { get; set; }

    /// <summary>
    /// Время трейда
    /// </summary>
    public DateTimeOffset Time { get; set; }


    /// <summary>
    /// Id трейда
    /// </summary>
    public string Id { get; set; }

    public Trade()
    {
        
    }

    public Trade(string pair, decimal price, decimal amount, string side, DateTimeOffset time, string id)
    {
        Pair = pair;
        Price = price;
        Amount = amount;
        Side = side;
        Time = time;
        Id = id;
    }
}
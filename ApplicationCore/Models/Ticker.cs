namespace ApplicationCore.Models;

public class Ticker
{
    /// <summary>
    /// Валютная пара
    /// </summary>
    public string Pair { get; set; }

    /// <summary>
    /// Лучшая цена покупки
    /// </summary>
    public decimal BestBidPrice { get; set; }

    /// <summary>
    /// Объем по лучшей цене покупки
    /// </summary>
    public decimal BestBidQuantity { get; set; }

    /// <summary>
    /// Лучшая цена продажи
    /// </summary>
    public decimal BestAskPrice { get; set; }

    /// <summary>
    /// Объем по лучшей цене продажи
    /// </summary>
    public decimal BestAskQuantity { get; set; }

    /// <summary>
    /// Изменение цены за последние 24 часа
    /// </summary>
    public decimal DailyChange { get; set; }

    /// <summary>
    /// Изменение цены за последние 24 часа (в процентах)
    /// </summary>
    public decimal DailyChangePercentage { get; set; }

    /// <summary>
    /// Последняя цена, по которой была совершена сделка
    /// </summary>
    public decimal LastPrice { get; set; }

    /// <summary>
    /// Объем торгов за последние 24 часа
    /// </summary>
    public decimal Volume { get; set; }
    
    /// <summary>
    /// Максимальная цена
    /// </summary>
    public decimal HighPrice { get; set; }
    
    /// <summary>
    /// Минимальная цена
    /// </summary>
    public decimal LowPrice { get; set; }

    public Ticker()
    {
        
    }

    public Ticker(string pair, decimal bestBidPrice, decimal bestBidQuantity, decimal bestAskPrice, decimal bestAskQuantity, decimal dailyChange, decimal dailyChangePercentage, decimal lastPrice, decimal volume, decimal highPrice, decimal lowPrice)
    {
        Pair = pair;
        BestBidPrice = bestBidPrice;
        BestBidQuantity = bestBidQuantity;
        BestAskPrice = bestAskPrice;
        BestAskQuantity = bestAskQuantity;
        DailyChange = dailyChange;
        DailyChangePercentage = dailyChangePercentage;
        LastPrice = lastPrice;
        Volume = volume;
        HighPrice = highPrice;
        LowPrice = lowPrice;
    }
}
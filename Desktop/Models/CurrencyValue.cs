namespace Desktop.Models;

public class CurrencyValue
{
    public string CurrencyPair { get; set; }
    
    public decimal Value { get; set; }

    public CurrencyValue(string currencyPair, decimal value)
    {
        CurrencyPair = currencyPair;
        Value = value;
    }
}
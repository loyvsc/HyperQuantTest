using System.Collections.ObjectModel;
using System.Windows.Input;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Desktop.Commands;
using Desktop.Models;
using Desktop.Utilities;
using Microsoft.Extensions.Logging;

namespace Desktop.ViewModels;

public class MainWindowViewModel : NotifyPropertyChangedBase
{
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly ITestConnector _testConnector;
    private Portfolio _portfolio;
    private bool _isBusy = true;

    public ICommand CalculateCommand { get; }

    public Portfolio Portfolio
    {
        get => _portfolio;
        set => SetValue(ref _portfolio, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetValue(ref _isBusy, value);
    }

    public ObservableCollection<CurrencyValue> PortfolioValues { get; } = new ObservableCollection<CurrencyValue>();

    public MainWindowViewModel(ILogger<MainWindowViewModel> logger, ITestConnector testConnector)
    {
        _logger = logger;
        _testConnector = testConnector;

        Portfolio = new Portfolio();
        Portfolio.Balances.Add("BTC", 1);
        Portfolio.Balances.Add("XRP", 15000);
        Portfolio.Balances.Add("XMR", 50);
        Portfolio.Balances.Add("DASH", 30);

        CalculateCommand = new AsyncCommand(CalculateAsync);

        _logger.LogInformation("MainWindowViewModel Initialized");
    }

    private async Task CalculateAsync()
    {
        IsBusy = false;
        var targetCurrencies = new List<string>(["USDT", "BTC", "XRP", "XMR", "DASH"]);
        
        PortfolioValues.Clear();
        foreach (var targetCurrency in targetCurrencies)
        {
            var value = await CalculatePortfolioValue(_portfolio, targetCurrency);
            PortfolioValues.Add(value);
        }
        
        IsBusy = true;
    }
    
    public async Task<CurrencyValue> CalculatePortfolioValue(Portfolio portfolio, string targetCurrency)
    {
        decimal totalValue = 0;
        var conversionStrategies = new Func<string, decimal, Task<decimal?>>[]
        {
            (currency, amount) => TryDirectConversion(currency, targetCurrency, amount),
            (currency, amount) => TryReverseConversion(currency, targetCurrency, amount),
            (currency, amount) => TryCrossConversionViaUsdt(currency, targetCurrency, amount)
        };

        foreach (var holding in portfolio.Balances)
        {
            if (holding.Key == targetCurrency)
            {
                totalValue += holding.Value;
                continue;
            }
            
            foreach (var strategy in conversionStrategies)
            {
                var convertedAmount = await strategy(holding.Key, holding.Value);
                if (convertedAmount.HasValue)
                {
                    totalValue += convertedAmount.Value;
                    break;
                }
            }
        }

        return new CurrencyValue(targetCurrency, totalValue);
    }

    private async Task<decimal?> TryDirectConversion(string fromCurrency, string toCurrency, decimal amount)
    {
        var ticker = await _testConnector.GetTickerAsync($"{fromCurrency}{toCurrency}");
        return ticker?.LastPrice * amount;
    }

    private async Task<decimal?> TryReverseConversion(string fromCurrency, string toCurrency, decimal amount)
    {
        var ticker = await _testConnector.GetTickerAsync($"{toCurrency}{fromCurrency}");
        return ticker != null ? amount / ticker.LastPrice : null;
    }

    private async Task<decimal?> TryCrossConversionViaUsdt(string fromCurrency, string toCurrency, decimal amount)
    {
        if (fromCurrency == "USDT" || toCurrency == "USDT") 
            return null;
        
        try
        {
            var usdtValue = await ConvertToUsdt(fromCurrency, amount);
            return await ConvertFromUsdt(toCurrency, usdtValue);
        }
        catch
        {
            return null;
        }
    }

    private async Task<decimal> ConvertToUsdt(string currency, decimal amount)
    {
        if (currency == "USDT") return amount;

        var ticker = await _testConnector.GetTickerAsync($"{currency}USDT");
        return amount * ticker.LastPrice;
    }

    private async Task<decimal> ConvertFromUsdt(string currency, decimal amount)
    {
        if (currency == "USDT") return amount;

        var ticker = await _testConnector.GetTickerAsync($"USDT{currency}");
        return amount / ticker.LastPrice;
    }
}
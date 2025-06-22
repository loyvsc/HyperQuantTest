using ApplicationCore.Interfaces;
using DAL.Connectors;

namespace Tests.Connectors;

public class BitfinexConnectorTests
{
    private readonly ITestConnector _connector = new BitfinexConnector();

    [Fact]
    public async Task GetTickerAsync_ValidPair_ReturnsData()
    {
        const string symbol = "BTCUSD";
        
        var result = await _connector.GetTickerAsync(symbol);
        
        Assert.NotNull(result);
        Assert.Equal("tBTCUSD", result.Pair);
        Assert.True(result.LastPrice > 0);
    }

    [Fact]
    public async Task GetTickerAsync_InvalidPair_ThrowsException()
    {
        const string symbol = "";
        
        await Assert.ThrowsAsync<ArgumentNullException>(() => _connector.GetTickerAsync(symbol));
    }

    [Fact]
    public async Task GetNewTradesAsync_ValidPair_ReturnsTrades()
    {
        const string symbol = "BTCUSD";
        const int maxCount = 10;

        var result = await _connector.GetNewTradesAsync(symbol, maxCount);
        
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.All(t => t.Price > 0));
    }
    
    [Fact]
    public async Task GetNewTradesAsync_InvalidPair_ThrowsException()
    {
        const string symbol = "";
        const int maxCount = 10;
        
        await Assert.ThrowsAsync<ArgumentNullException>(() => _connector.GetNewTradesAsync(symbol, maxCount));
    }

    [Fact]
    public async Task GetNewTradesAsync_ZeroCount_ThrowsException()
    {
        const string symbol = "BTCUSD";
        const int maxCount = 0;

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _connector.GetNewTradesAsync(symbol, maxCount));
    }
    
    [Fact]
    public async Task GetNewTradesAsync_MaxCount_ReturnsData()
    {
        const string symbol = "BTCUSD";
        const int periodInSec = 10000;

        var result = await _connector.GetNewTradesAsync(symbol, periodInSec);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetCandleSeriesAsync_ValidParameters_ReturnsCandles()
    {
        const string symbol = "BTCUSD";
        const int periodInSec = 60;
        const int count = 10;
        
        var result = await _connector.GetCandleSeriesAsync(symbol, periodInSec, null, count: count);
        
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.All(c => c.OpenPrice > 0));
    }
    
    [Fact]
    public async Task GetCandleSeriesAsync_InvalidPeriod_ThrowsException()
    {
        const string symbol = "BTCUSD";

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _connector.GetCandleSeriesAsync(symbol, -1, null));
    }

    [Fact]
    public async Task GetCandleSeriesAsync_ZeroCount_ReturnsEmpty()
    {
        const string symbol = "BTCUSD";
        const int periodInSec = 60;
        const int count = 0;
        
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _connector.GetCandleSeriesAsync(symbol, periodInSec, null, count: count));
    }

    [Fact]
    public async Task GetCandleSeriesAsync_MaxCount_ReturnsData()
    {
        const string symbol = "BTCUSD";
        const int periodInSec = 60;
        
        var result = await _connector.GetCandleSeriesAsync(symbol, periodInSec, null, count: 10000);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task SubscribeTrades_ValidPair_SubscribesSuccessfully()
    {
        const string symbol = "BTCUSD";
        bool eventFired = false;
        _connector.NewBuyTrade += trade => eventFired = true;
        
        _connector.SubscribeTrades(symbol);
        
        await Task.Delay(2000);
        Assert.True(eventFired);
    }
    
    [Fact]
    public async Task SubscribeTrades_SellTrade_TriggersEvent()
    {
        const string symbol = "BTCUSD";
        bool eventFired = false;
        _connector.NewSellTrade += _ => eventFired = true;
        
        _connector.SubscribeTrades(symbol);
        
        await Task.Delay(2000);
        Assert.True(eventFired);
    }
    
    [Fact]
    public void SubscribeCandles_InvalidPeriod_ThrowsException()
    {
        const string symbol = "BTCUSD";
        
        Assert.Throws<ArgumentOutOfRangeException>(() => _connector.SubscribeCandles(symbol, -1));
    }
    
    [Fact]
    public void SubscribeCandles_ValidPair_SubscribesSuccessfully()
    {
        const string symbol = "BTCUSD";
        
        _connector.SubscribeCandles(symbol, 60);
        Assert.True(IsSubscribed(symbol));
    }

    [Fact]
    public void SubscribeCandles_AlreadySubscribed_DoesNothing()
    {
        const string symbol = "BTCUSD";
        
        _connector.SubscribeCandles(symbol, 60);
        _connector.SubscribeCandles(symbol, 60);
        Assert.True(IsSubscribed(symbol));
    }

    [Fact]
    public void UnsubscribeCandles_ExistingSubscription_RemovesSubscription()
    {
        const string symbol = "BTCUSD";
        
        _connector.SubscribeCandles(symbol, 60);
        _connector.UnsubscribeCandles(symbol);
        Assert.False(IsSubscribed(symbol));
    }

    [Fact]
    public async Task SubscribeCandles_TriggersCandleEvent()
    {
        bool eventFired = false;
        _connector.CandleSeriesProcessing += _ => eventFired = true;
        
        _connector.SubscribeCandles("BTCUSD", 60);
        
        await Task.Delay(2000);
        Assert.True(eventFired);
    }
    
    [Fact]
    public void SubscribeTrades_FirstTime_AddsSubscription()
    {
        const string symbol = "BTCUSD";
        
        _connector.SubscribeTrades(symbol);
        
        Assert.True(IsSubscribed(symbol));
    }

    [Fact]
    public void SubscribeTrades_AlreadySubscribed_DoesNothing()
    {
        const string symbol = "BTCUSD";

        _connector.SubscribeTrades(symbol);
        _connector.SubscribeTrades(symbol);
        
        Assert.True(IsSubscribed(symbol));
    }

    [Fact]
    public void UnsubscribeTrades_ExistingSubscription_RemovesSubscription()
    {
        const string symbol = "BTCUSD";

        _connector.SubscribeTrades(symbol);
        _connector.UnsubscribeTrades(symbol);
        
        Assert.False(IsSubscribed(symbol));
    }

    [Fact]
    public void UnsubscribeTrades_NotSubscribed_DoesNothing()
    {
        const string symbol = "BTCUSD";

        _connector.UnsubscribeTrades(symbol);
    }

    private bool IsSubscribed(string symbol)
    {
        return ((BitfinexConnector) _connector).Subscriptions.ContainsKey(symbol);
    }
}
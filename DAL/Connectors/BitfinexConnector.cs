using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Utilities;
using Bitfinex.Net.Clients;
using Bitfinex.Net.Enums;
using Bitfinex.Net.Objects.Models;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;

namespace DAL.Connectors;

public class BitfinexConnector : ITestConnector
{
    private readonly BitfinexRestClient _restClient;
    private readonly BitfinexSocketClient _socketClient;
    
    public BitfinexConnector()
    {
        _restClient = new BitfinexRestClient();
        _socketClient = new BitfinexSocketClient();
    }
    
    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
    {
        if (string.IsNullOrWhiteSpace(pair))
            throw new ArgumentNullException(nameof(pair), "pair cannot be null or empty");
        
        if (maxCount is > 10000 or <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxCount), "count must be between 0 and 10000");
        
        var sb = StringBuilderPool.Rent();
        if (pair[0] != 't')
        {
            sb.Append('t');
        }
        sb.Append(pair);
        
        var response = await _restClient.SpotApi.ExchangeData.GetTradeHistoryAsync(sb.ToString(), maxCount);
        StringBuilderPool.Return(sb);

        if (!response.Success)
            ThrowException(response.Error);
        
        return response.Data.Select(x => new Trade(pair, x.Price, x.Quantity, x.Quantity > 0 ? "buy" : "sell", x.Timestamp, x.Id.ToString()));
    }

    public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
    {
        if (string.IsNullOrWhiteSpace(pair))
            throw new ArgumentNullException(nameof(pair), "pair cannot be null or empty");
        
        if (periodInSec <= 0)
            throw new ArgumentOutOfRangeException(nameof(periodInSec), "periodInSec must be greater than zero");
        
        if (count is > 10000 or <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "count must be between 0 and 10000");
        
        if (Enum.TryParse<KlineInterval>(periodInSec.ToString(), out var interval))
        {
            var sb = StringBuilderPool.Rent();
            if (pair[0] != 't')
            {
                sb.Append('t');
            }
            sb.Append(pair);
            
            var response = await _restClient.SpotApi.ExchangeData.GetKlinesAsync(sb.ToString(), interval, limit: (int?)count, startTime: from?.DateTime, endTime: to?.DateTime);
            StringBuilderPool.Return(sb);

            if (!response.Success)
                ThrowException(response.Error);

            return response.Data.Select(x =>
                new Candle(pair, x.OpenPrice, x.HighPrice, x.LowPrice, x.ClosePrice, 0, x.Volume, x.OpenTime));
        }
        
        throw new Exception($"Invalid period {periodInSec}");
    }

    public async Task<Ticker?> GetTickerAsync(string pair)
    {
        if (string.IsNullOrWhiteSpace(pair))
            throw new ArgumentNullException(nameof(pair), "pair cannot be null or empty");
        
        var sb = StringBuilderPool.Rent();
        if (pair[0] != 't')
        {
            sb.Append('t');
        }
        sb.Append(pair);
        
        var response = await _restClient.SpotApi.ExchangeData.GetTickerAsync(sb.ToString());
        StringBuilderPool.Return(sb);

        if (!response.Success)
            return null;

        return new Ticker(pair: response.Data.Symbol, 
            bestBidPrice: response.Data.BestBidPrice, bestBidQuantity: response.Data.BestBidQuantity,
            bestAskPrice: response.Data.BestAskPrice, bestAskQuantity: response.Data.BestAskQuantity, 
            dailyChange: response.Data.DailyChange, dailyChangePercentage: response.Data.DailyChangePercentage, 
            lastPrice: response.Data.LastPrice, 
            volume: response.Data.Volume, 
            highPrice: response.Data.HighPrice, lowPrice: response.Data.LowPrice);
    }

    private void ThrowException(Error error)
    {
        switch (error.Code)
        {
            case 10020:
            {
                throw new Exception("Invalid period");
            }
            default:
            {
                throw new Exception($"Code: {error.Code}; Message: {error.Message}");
            }
        }
    }
    
    public Dictionary<string, int> Subscriptions { get; } = new Dictionary<string, int>();

    public event Action<Trade>? NewBuyTrade;
    public event Action<Trade>? NewSellTrade;
    
    public void SubscribeTrades(string pair, int maxCount = 100)
    {
        if (string.IsNullOrWhiteSpace(pair))
            throw new ArgumentNullException(nameof(pair), "pair cannot be null or empty");
        
        if (Subscriptions.ContainsKey(pair))
            return;
        
        var sb = StringBuilderPool.Rent();
        if (pair[0] != 't')
        {
            sb.Append('t');
        }
        sb.Append(pair);

        var response = _socketClient.SpotApi.SubscribeToTradeUpdatesAsync(sb.ToString(), TradeUpdatesHandler).GetAwaiter().GetResult();
        StringBuilderPool.Return(sb);

        if (!response.Success)
            ThrowException(response.Error);

        Subscriptions.Add(pair, response.Data.Id);
    }

    private void TradeUpdatesHandler(DataEvent<BitfinexTradeSimple[]> obj)
    {
        foreach (var trade in obj.Data)
        {
            if (trade.Quantity > 0)
            {
                NewBuyTrade?.Invoke(new Trade(obj.Symbol, trade.Price, trade.Quantity, "buy", new DateTimeOffset(trade.Timestamp), trade.Id.ToString()));
            }
            else
            {
                NewSellTrade?.Invoke(new Trade(obj.Symbol, trade.Price, trade.Quantity, "sell", new DateTimeOffset(trade.Timestamp), trade.Id.ToString()));
            }
        }
    }

    public void UnsubscribeTrades(string pair)
    {
        if (Subscriptions.Remove(pair, out var subscriptionId))
        {
            _socketClient.SpotApi.UnsubscribeAsync(subscriptionId).Wait();
        }
    }

    public event Action<Candle>? CandleSeriesProcessing;

    public void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null,
        long? count = 0)
    {
        if (string.IsNullOrWhiteSpace(pair))
            throw new ArgumentNullException(nameof(pair), "pair cannot be null or empty");
        
        if (periodInSec <= 0)
            throw new ArgumentOutOfRangeException(nameof(periodInSec), "periodInSec must be greater than zero");
        
        if (Subscriptions.ContainsKey(pair))
            return;
        
        if (Enum.TryParse<KlineInterval>(periodInSec.ToString(), out var interval))
        {
            var sb = StringBuilderPool.Rent();
            if (pair[0] != 't')
            {
                sb.Append('t');
            }
            sb.Append(pair);
            
            var response = _socketClient.SpotApi.SubscribeToKlineUpdatesAsync(sb.ToString(), interval, CandlesUpdatesHandler).GetAwaiter().GetResult();
            StringBuilderPool.Return(sb);

            if (!response.Success)
                ThrowException(response.Error);

            Subscriptions.Add(pair, response.Data.Id);
            return;
        }
        
        throw new Exception($"Invalid period {periodInSec}");
    }
    
    private void CandlesUpdatesHandler(DataEvent<BitfinexKline[]> obj)
    {
        foreach (var kline in obj.Data)
        {
            CandleSeriesProcessing?.Invoke(new Candle(obj.Symbol, kline.OpenPrice, kline.HighPrice, kline.LowPrice, kline.ClosePrice, 0, kline.Volume, new DateTimeOffset(kline.OpenTime)));
        }
    }

    public void UnsubscribeCandles(string pair)
    {
        if (Subscriptions.Remove(pair, out var subscriptionId))
        {
            _socketClient.SpotApi.UnsubscribeAsync(subscriptionId).Wait();
        }
    }

    public void Dispose()
    {
        _socketClient.SpotApi.UnsubscribeAllAsync().Wait();
        
        _restClient.Dispose();
        _socketClient.Dispose();
    }
}
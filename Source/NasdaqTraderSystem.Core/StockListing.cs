using System.Globalization;
using NasdaqTrader.Bot.Core;

namespace NasdaqTraderSystem.Core;

public class StockListing : IStockListing
{
    public string Name { get; set; } = "";
    public string Ticker { get; set; } = "";

    public IPricePoint[] PricePoints { get; set; } = Array.Empty<IPricePoint>();
}

public class PricePoint : IPricePoint
{
    public DateOnly Date { get; set; }
    public decimal Price { get; set; }
    
    public string DateAsString => Date.ToDateTime(new TimeOnly(12,0)).ToString("o", CultureInfo.InvariantCulture);
    public string PriceAsString => Price.ToString("0");
    
}
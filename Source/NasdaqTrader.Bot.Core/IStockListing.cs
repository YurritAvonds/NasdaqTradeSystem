namespace NasdaqTrader.Bot.Core;

public interface IStockListing
{
    public string Name { get; }

    public IPricePoint[] PricePoints { get; }
}
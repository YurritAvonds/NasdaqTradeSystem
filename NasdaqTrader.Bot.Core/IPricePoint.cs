namespace NasdaqTrader.Bot.Core;

public interface IPricePoint
{
    public DateOnly Date { get; }
    public decimal Price { get; }
}
namespace NasdaqTrader.Bot.Core;

public static class Extensions
{
    public static decimal GetCurrentValue(this List<IHolding> holdings, ITraderSystemContext context)
    {
        return holdings.Sum(h => context.GetPriceOnDay(h.Listing) * h.Amount);
    }
}
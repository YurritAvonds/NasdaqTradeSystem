using NasdaqTrader.Bot.Core;

namespace NasdaqTraderSystem.Core;

public class Trade : ITrade
{
    public DateOnly ExecutedOn { get; set; }
    public IStockListing Listing { get; set; }
    public int Amount { get; set; }
    public decimal AtPrice { get; set; }
}
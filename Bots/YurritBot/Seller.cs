using NasdaqTrader.Bot.Core;
using YurritBot.Logging;

namespace YurritBot;

class Seller(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, int indexReferenceDay, ILogger logger) : ITrader
{
    public ITraderBot TraderBot { get; private set; } = traderBot;
    public ITraderSystemContext SystemContext { get; private set; } = systemContext;
    public int IndexToday { get; private set; } = indexToday;
    public int IndexReferenceDay { get; private set; } = indexReferenceDay;
    public ILogger Logger { get; private set; } = logger;

    private const int cashLowerLimit = 10;

    public void ExecuteStrategy()
    {
        // TODO some are sold with losses because original price is not taken into account
        var sellableHoldings = SystemContext.GetHoldings(TraderBot).Where(holding
            => holding.Amount > 0
                && ((holding.Listing.PricePoints[IndexReferenceDay].Price - holding.Listing.PricePoints[IndexToday].Price) < 0));

        foreach (var holding in sellableHoldings)
        {
            var success = SystemContext.SellStock(TraderBot, holding.Listing, holding.Amount);

            if (!success)
            {
                Logger.Log($"Failed SELL - Current {holding.Listing.Name} | Price {holding.Listing.PricePoints[IndexToday].Price} | Amount {holding.Amount}");
            }
        }
    }
}

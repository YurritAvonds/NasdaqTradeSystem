using NasdaqTrader.Bot.Core;

namespace YurritBot;

class Seller(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, int indexTomorrow, Logger logger) : ITrader
{
    public ITraderBot TraderBot { get; private set; } = traderBot;
    public ITraderSystemContext TraderSystemContext { get; private set; } = systemContext;
    public int IndexToday { get; private set; } = indexToday;
    public int IndexReferenceDay { get; private set; } = indexTomorrow;
    public Logger Logger { get; private set; } = logger;

    private const int cashLowerLimit = 10;

    public void ExecuteStrategy()
    {
        // TODO some are sold with losses because original price is not taken into account
        var sellableHoldings = systemContext.GetHoldings(TraderBot).Where(holding
            => holding.Amount > 0
                && ((holding.Listing.PricePoints[indexTomorrow].Price - holding.Listing.PricePoints[indexToday].Price) < 0));

        foreach (var holding in sellableHoldings)
        {
            var success = systemContext.SellStock(TraderBot, holding.Listing, holding.Amount);

            if (!success)
            {
                logger.LogToFile($"Failed SELL - Current {holding.Listing.Name} | Price {holding.Listing.PricePoints[indexToday].Price} | Amount {holding.Amount}");
            }
        }
    }
}

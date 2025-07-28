using NasdaqTrader.Bot.Core;
using YurritBot.Logging;

namespace YurritBot;

class Seller()
{
    private const int cashLowerLimit = 10;

    public void ExecuteStrategy(ITraderBot traderBot, ITraderSystemContext systemContext, ILogger logger)
    {
        var holdings = systemContext.GetHoldings(traderBot);
        if (holdings.Length == 0)
        {
            return;
        }

        foreach (var holding in holdings)
        {
            if (holding.Amount == 0)
            {
                continue;
            }

            var success = systemContext.SellStock(traderBot, holding.Listing, holding.Amount);

            //Logger.LogTransaction(
            //    category: success ? "" : "ERR",
            //    ticker: holding.Listing.Ticker,
            //    currentCash: SystemContext.GetCurrentCash(TraderBot),
            //    pricePoint: holding.Listing.PricePoints[IndexToday].Price,
            //    amount: holding.Amount);
        }
    }
}

using NasdaqTrader.Bot.Core;
using YurritBot.Logging;

namespace YurritBot;

class Seller()
{
    public void ExecuteStrategy(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, ILogger logger)
    {
        logger.LogHeader2($"SELL");

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

            logger.LogTransaction(
                category: success ? "" : "ERR",
                ticker: holding.Listing.Ticker,
                currentCash: systemContext.GetCurrentCash(traderBot),
                pricePoint: holding.Listing.PricePoints[indexToday].Price,
                amount: holding.Amount);
        }

        logger.Log($"- €{systemContext.GetCurrentCash(traderBot):F2}");
    }
}

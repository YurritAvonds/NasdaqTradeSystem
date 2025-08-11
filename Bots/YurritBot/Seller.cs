﻿using NasdaqTrader.Bot.Core;
using YurritBot.Logging;

namespace YurritBot;

class Seller()
{
    public void ExecuteStrategy(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, ILogger logger)
    {
        logger.LogHeader2($"Sell");

        logger.Log($"€{systemContext.GetCurrentCash(traderBot):F2}");

        var holdings = systemContext.GetHoldings(traderBot)
            .Where(holding => holding.Amount > 0);
        if (!holdings.Any())
        {
            logger.Log($"no holdings");
            return;
        }

        foreach (var holding in holdings)
        {
            TrySellHolding(traderBot, systemContext, indexToday, logger, holding);
        }
    }

    private static void TrySellHolding(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, ILogger logger, IHolding holding)
    {
        if (holding.Amount == 0)
        {
            return;
        }

        var success = systemContext.SellStock(traderBot, holding.Listing, holding.Amount);

        logger.LogTransaction(
            category: success ? "" : "ERR",
            ticker: holding.Listing.Ticker,
            currentCash: systemContext.GetCurrentCash(traderBot),
            pricePoint: holding.Listing.PricePoints[indexToday].Price,
            amount: holding.Amount);
    }
}

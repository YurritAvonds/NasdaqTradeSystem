using NasdaqTrader.Bot.Core;
using System.Diagnostics;
using YurritBot.Logging;

namespace YurritBot;

public class Buyer()
{
    private const decimal cashlowerLimit = 100;
    private const int maximumBuyAmountPerStock = 1000;

    public void ExecuteStrategy(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, int indexReferenceDay, ILogger logger)
    {
        logger.LogHeader2($"BUY");
        logger.Log($"- €{systemContext.GetCurrentCash(traderBot):F2}");

        logger.Log($"listings: {systemContext.GetListings().Count()}");

        var listingsByExpectedIncrease = RankListings(traderBot, systemContext, indexToday, indexReferenceDay);
        logger.Log($"ranked listings: {listingsByExpectedIncrease.Count()}");

        var buyCalculator = new BuyCalculator(maximumBuyAmountPerStock);

        if (!listingsByExpectedIncrease.Any())
        {
            logger.Log($"no listings");
            return;
        }

        foreach (var listing in listingsByExpectedIncrease)
        {
            var currentCash = systemContext.GetCurrentCash(traderBot);
            if (systemContext.GetTradesLeftForToday(traderBot) <= 0)
            {
                logger.Log($"no trades left");
                return;
            }

            if (currentCash < cashlowerLimit)
            {
                logger.Log($"cash below limit");
                return;
            }

            TryBuyListing(traderBot, systemContext, indexToday, logger, currentCash, buyCalculator, listing);
        }

        logger.Log($"- €{systemContext.GetCurrentCash(traderBot):F2}");
    }

    private static List<IStockListing> RankListings(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, int indexReferenceDay)
    {
        var currentCash = systemContext.GetCurrentCash(traderBot);
        //return systemContext.GetListings()
        //    .Where(listing => listing.PricePoints[indexToday].Price <= currentCash)
        //    .Where(listing => listing.PricePoints[indexReferenceDay].Price / listing.PricePoints[indexToday].Price > 1)
        //    .OrderByDescending(listing => (listing.PricePoints[indexReferenceDay].Price / listing.PricePoints[indexToday].Price));

        return systemContext.GetListings()
            .Select(l => new
            {
                Listing = l,
                PriceToday = l.PricePoints[indexToday].Price,
                PriceRatio = l.PricePoints[indexReferenceDay].Price / l.PricePoints[indexToday].Price
            })
            .Where(x => x.PriceToday <= currentCash 
                     && x.PriceRatio > 1)
            .OrderByDescending(x => x.PriceRatio)
            .Select(x => x.Listing)
            .ToList();
    }

    private static void TryBuyListing(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, ILogger logger, decimal currentCash, BuyCalculator buyCalculator, IStockListing listing)
    {
        logger.Log($"try buy {listing.Ticker}");

        if (currentCash < listing.PricePoints[indexToday].Price)
        {
            logger.Log($"too expensive");
            return;
        }

        var maxBuyAmount = buyCalculator.CalculateMaximuumBuyAmount(currentCash, listing.PricePoints[indexToday].Price);
        var success = systemContext.BuyStock(traderBot, listing, maxBuyAmount);

        logger.LogTransaction(
            category: success ? "" : "ERR",
            ticker: listing.Ticker,
            currentCash: systemContext.GetCurrentCash(traderBot),
            pricePoint: listing.PricePoints[indexToday].Price,
            amount: maxBuyAmount);
    }
}

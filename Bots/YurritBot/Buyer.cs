using NasdaqTrader.Bot.Core;
using YurritBot.Logging;

namespace YurritBot;

public class Buyer()
{
    private const decimal cashlowerLimit = 100;
    private const int maximumBuyAmountPerStock = 1000;

    public void ExecuteStrategy(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, int indexReferenceDay, ILogger logger)
    {
        logger.LogHeader2($"BUY");

        var currentCash = systemContext.GetCurrentCash(traderBot);
        var listingsByExpectedIncrease = systemContext.GetListings()
            .Where(listing => listing.PricePoints[indexToday].Price <= currentCash)
            .Where(listing => listing.PricePoints[indexReferenceDay].Price / listing.PricePoints[indexToday].Price > 1)
            .OrderByDescending(listing => (listing.PricePoints[indexReferenceDay].Price / listing.PricePoints[indexToday].Price));

        var buyCalculator = new BuyCalculator(maximumBuyAmountPerStock);

        foreach (var listing in listingsByExpectedIncrease)
        {
            currentCash = systemContext.GetCurrentCash(traderBot);
            if (systemContext.GetTradesLeftForToday(traderBot) <= 0
                || currentCash < cashlowerLimit)
            {
                return;
            }

            var maxBuyAmount = buyCalculator.CalculateMaximuumBuyAmount(currentCash, listing.PricePoints[indexToday].Price);

            if (maxBuyAmount < 1)
            {
                continue;
            }

            var success = systemContext.BuyStock(traderBot, listing, maxBuyAmount);

            logger.LogTransaction(
                category: success ? "" : "ERR",
                ticker: listing.Ticker,
                currentCash: systemContext.GetCurrentCash(traderBot),
                pricePoint: listing.PricePoints[indexToday].Price,
                amount: maxBuyAmount);
        }

        logger.Log($"- €{systemContext.GetCurrentCash(traderBot):F2}");
    }
}

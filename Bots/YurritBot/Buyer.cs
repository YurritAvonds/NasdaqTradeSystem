using NasdaqTrader.Bot.Core;
using YurritBot.Logging;

namespace YurritBot;

public class Buyer()
{
    private const decimal cashlowerLimit = 100;
    private const int maximumBuyAmountPerStock = 1000;

    public void ExecuteStrategy(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, int indexReferenceDay, ILogger logger)
    {
        var currentCash = systemContext.GetCurrentCash(traderBot);
        var listingsByExpectedIncrease = systemContext.GetListings()
            .Where(listing => listing.PricePoints[indexToday].Price <= currentCash)
            .Where(listing => listing.PricePoints[indexReferenceDay].Price / listing.PricePoints[indexToday].Price > 1)
            .OrderByDescending(listing => (listing.PricePoints[indexReferenceDay].Price / listing.PricePoints[indexToday].Price));

        //var buyCalculator = new BuyCalculator(maximumBuyAmountPerStock);

        foreach (var listing in listingsByExpectedIncrease)
        {
            currentCash = systemContext.GetCurrentCash(traderBot);
            if (systemContext.GetTradesLeftForToday(traderBot) <= 0
                || currentCash < cashlowerLimit)
            {
                return;
            }

            var maxBuyAmount = CalculateMaximuumBuyAmount(currentCash, listing.PricePoints[indexToday].Price);

            if (maxBuyAmount < 1)
            {
                continue;
            }

            var success = systemContext.BuyStock(traderBot, listing, maxBuyAmount);

            //Logger.LogTransaction(
            //    category: success ? "" : "ERR",
            //    ticker: listing.Ticker,
            //    currentCash: SystemContext.GetCurrentCash(TraderBot),
            //    pricePoint: listing.PricePoints[IndexToday].Price,
            //    amount: maxBuyAmount);
        }
    }

    public int CalculateMaximuumBuyAmount(decimal currentCash, decimal listingPrice)
    {
        decimal maximumBuyAmountWithCurrentCash = currentCash / listingPrice;
        return (int)Math.Floor(Math.Min(maximumBuyAmountPerStock, maximumBuyAmountWithCurrentCash));
    }
}

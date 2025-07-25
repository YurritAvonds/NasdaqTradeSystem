using NasdaqTrader.Bot.Core;

namespace YurritBot;

class Buyer(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, int indexReferenceDay, Logger logger) : ITrader
{
    public ITraderBot TraderBot { get; private set; } = traderBot;
    public ITraderSystemContext TraderSystemContext { get; private set; } = systemContext;
    public int IndexToday { get; private set; } = indexToday;
    public int IndexReferenceDay { get; private set; } = indexReferenceDay;
    public Logger Logger { get; private set; } = logger;

    private const decimal cashlowerLimit = 50;

    public void ExecuteStrategy()
    {
        var listingsByExpectedIncrease = systemContext.GetListings().OrderBy(listing
            => (listing.PricePoints[IndexReferenceDay].Price - listing.PricePoints[IndexToday].Price) / listing.PricePoints[IndexToday].Price);

        var buyCalculator = new BuyCalculator();

        foreach (var listing in listingsByExpectedIncrease)
        {
            var currentCash = systemContext.GetCurrentCash(TraderBot);
            if (currentCash < cashlowerLimit
                || systemContext.GetTradesLeftForToday(TraderBot) <= 0)
            {
                return;
            }

            // TODO find more efficient way to spot this situation upfront
            var pricePointToday = listing.PricePoints[indexToday].Price;
            if (currentCash < pricePointToday)
            {
                continue;
            }

            var maxBuyAmount = buyCalculator.CalculateMaximuumBuyAmount(currentCash, pricePointToday);
            var success = systemContext.BuyStock(TraderBot, listing, maxBuyAmount);
            if (!success)
            {
                logger.LogToFile($"Failed BUY - Current {currentCash} | Price {pricePointToday} | Amount {maxBuyAmount}");
            }
        }
    }
}

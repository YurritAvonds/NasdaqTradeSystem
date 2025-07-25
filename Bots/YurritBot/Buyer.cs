using NasdaqTrader.Bot.Core;
using YurritBot.Logging;

namespace YurritBot;

class Buyer(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, int indexReferenceDay, ILogger logger) : ITrader
{
    public ITraderBot TraderBot { get; private set; } = traderBot;
    public ITraderSystemContext SystemContext { get; private set; } = systemContext;
    public int IndexToday { get; private set; } = indexToday;
    public int IndexReferenceDay { get; private set; } = indexReferenceDay;
    public ILogger Logger { get; private set; } = logger;

    private const decimal cashlowerLimit = 50;
    private const int maxBuyAmountPerStock = 25;

    public void ExecuteStrategy()
    {
        var listingsByExpectedIncrease = SystemContext.GetListings().OrderBy(listing
            => (listing.PricePoints[IndexReferenceDay].Price - listing.PricePoints[IndexToday].Price) / listing.PricePoints[IndexToday].Price);

        var buyCalculator = new BuyCalculator(maxBuyAmountPerStock);

        foreach (var listing in listingsByExpectedIncrease)
        {
            var currentCash = SystemContext.GetCurrentCash(TraderBot);
            if (currentCash < cashlowerLimit
                || SystemContext.GetTradesLeftForToday(TraderBot) <= 0)
            {
                return;
            }

            // TODO find more efficient way to spot this situation upfront
            var pricePointToday = listing.PricePoints[IndexToday].Price;
            if (currentCash < pricePointToday)
            {
                continue;
            }

            var maxBuyAmount = buyCalculator.CalculateMaximuumBuyAmount(currentCash, pricePointToday);
            var success = SystemContext.BuyStock(TraderBot, listing, maxBuyAmount);
            if (!success)
            {
                Logger.Log($"Failed BUY - Current {currentCash} | Price {pricePointToday} | Amount {maxBuyAmount}");
            }
        }
    }
}

using NasdaqTrader.Bot.Core;

namespace YurritBot;

public class YurritBot : ITraderBot
{
    
    private const int cashLowerLimit = 10;

    public string CompanyName => "Stock Out Like a Sore Thumb";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        LogToFile($"{systemContext.CurrentDate}");

        int indexToday = DetermineTodayIndex(systemContext);
        int indexYesterday = indexToday - 1;
        int indexTomorrow = indexToday + 1;

        ExecuteSellStrategy(systemContext, indexToday, indexTomorrow);
        ExecuteBuyStrategy(systemContext, indexToday, indexTomorrow);
    }

    private void ExecuteSellStrategy(ITraderSystemContext systemContext, int indexToday, int indexTomorrow)
    {
        var sellableHoldings = systemContext.GetHoldings(this).Where(holding
            => (holding.Listing.PricePoints[indexTomorrow].Price - holding.Listing.PricePoints[indexToday].Price) < 0);

        foreach (var holding in sellableHoldings)
        {
            var success = systemContext.SellStock(this, holding.Listing, holding.Amount);

            if (!success)
            {
                LogToFile($"Failed SELL - Current {holding.Listing.Name} | Price {holding.Listing.PricePoints[indexToday].Price} | Amount {holding.Amount}");
            }
        }
    }

    private void ExecuteBuyStrategy(ITraderSystemContext systemContext, int indexToday, int indexTomorrow)
    {
        var listingsByExpectedIncrease = systemContext.GetListings().OrderBy(listing
            => (listing.PricePoints[indexTomorrow].Price - listing.PricePoints[indexToday].Price) / listing.PricePoints[indexToday].Price);

        var buyCalculator = new BuyCalculator();

        foreach (var listing in listingsByExpectedIncrease)
        {
            var currentCash = systemContext.GetCurrentCash(this);
            if (currentCash < cashLowerLimit
                || systemContext.GetTradesLeftForToday(this) <= 0)
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
            var success = systemContext.BuyStock(this, listing, maxBuyAmount);
            if (!success)
            {
                LogToFile($"Failed BUY - Current {currentCash} | Price {pricePointToday} | Amount {maxBuyAmount}");
            }
        }
    }

    private static void LogToFile(string text)
    {
        var logFilePath = Path.Combine(AppContext.BaseDirectory, "yurritbot_errors.log");
        File.AppendAllText(logFilePath, $"{text}{Environment.NewLine}");
    }

    private static int DetermineTodayIndex(ITraderSystemContext systemContext)
    {
        return (int)(systemContext.CurrentDate.ToDateTime(TimeOnly.MinValue)
            - systemContext.StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
    }

    
}
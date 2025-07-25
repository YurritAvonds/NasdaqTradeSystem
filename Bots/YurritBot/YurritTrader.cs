using NasdaqTrader.Bot.Core;
using YurritBot.Logging;

namespace YurritBot;

public class YurritBot : ITraderBot
{
    public string CompanyName => "Stock Out Like a Sore Thumb";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        var logger = new NullLogger();
        logger.Log($"{systemContext.CurrentDate}");

        int indexToday = DetermineTodayIndex(systemContext);
        int indexReferenceDay = indexToday + 5;

        new Seller(this, systemContext, indexToday, indexReferenceDay, logger)
            .ExecuteStrategy();
        new Buyer(this, systemContext, indexToday, indexReferenceDay, logger)
            .ExecuteStrategy();
    }

    private static int DetermineTodayIndex(ITraderSystemContext systemContext)
    {
        return (int)(systemContext.CurrentDate.ToDateTime(TimeOnly.MinValue)
            - systemContext.StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
    }
}
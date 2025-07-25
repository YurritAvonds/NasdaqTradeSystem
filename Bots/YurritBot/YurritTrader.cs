using NasdaqTrader.Bot.Core;

namespace YurritBot;

public class YurritBot : ITraderBot
{
    public string CompanyName => "Stock Out Like a Sore Thumb";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        var logger = new Logger();
        logger.LogToFile($"{systemContext.CurrentDate}");

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
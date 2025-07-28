using NasdaqTrader.Bot.Core;
using System.Diagnostics;
using YurritBot.Logging;

namespace YurritBot;

public class YurritBot : ITraderBot
{
    public string CompanyName => "Stock Out Like a Sore Thumb";
    
    private const int timeScale = 1;
    private const string logFileName = "yurritbot_errors.log";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        var logger = new FileLogger(Path.Combine(AppContext.BaseDirectory, logFileName));

        logger.LogHeader1($"{systemContext.CurrentDate}");

        // SLOW
        int indexToday = new DateCalculator()
            .DetermineDateIndex(systemContext.CurrentDate, systemContext.GetListings().First().PricePoints);

        //logger.Log($"- €{systemContext.GetCurrentCash(this):F2}");

        //logger.LogHeader2($"SELL");

        new Seller().ExecuteStrategy(this, systemContext, logger);

        //logger.Log($"- €{systemContext.GetCurrentCash(this):F2}");

        //logger.LogHeader2($"BUY");

        new Buyer().ExecuteStrategy(this, systemContext, indexToday, indexToday + timeScale, logger);

        //logger.Log($"- €{systemContext.GetCurrentCash(this):F2}");
    }

    
}
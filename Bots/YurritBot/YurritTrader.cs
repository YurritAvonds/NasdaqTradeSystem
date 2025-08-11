using NasdaqTrader.Bot.Core;
using System.Diagnostics;
using YurritBot.Logging;

namespace YurritBot;

public class YurritBot : ITraderBot
{
    public string CompanyName => "Stock Out Like a Sore Thumb";
    
    private const int timeScale = 1;
    private const string logFileName = "yurritbot_log.html";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        var logger = new HtmlLogger(Path.Combine(AppContext.BaseDirectory, logFileName));
        logger.LogHeader1($"{systemContext.CurrentDate}");

        // SLOW
        int indexToday = new DateCalculator()
            .DetermineDateIndex(systemContext.CurrentDate, systemContext.GetListings().First().PricePoints);

        if (indexToday == 0)
        {
            logger.Erase();
            logger.Log($"{DateTime.Now}");
        }

        new Seller().ExecuteStrategy(this, systemContext, indexToday, logger);
        new Buyer().ExecuteStrategy(this, systemContext, indexToday, indexToday + timeScale, logger);
        
    }


}
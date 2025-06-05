namespace NasdaqTrader.Bot.Core;

public interface ITraderBot
{
    void DoTurn(ITraderSystemContext systemContext);
    string CompanyName { get; }
}
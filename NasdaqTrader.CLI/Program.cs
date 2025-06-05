// See https://aka.ms/new-console-template for more information

using NasdaqTrader.Bot.Core;
using NasdaqTraderSystem.Core;

BotLoader botLoader = new BotLoader();

var botTypes = new Dictionary<string, Type>();
botLoader.DetermineBots(AppContext.BaseDirectory + "\\Bots", botTypes);

var stocksLoader = new StockLoader(args[0], 100);

TraderSystemSimulation traderSystemSimulation = new TraderSystemSimulation(
    botTypes.Values.Select(b=>(ITraderBot)Activator.CreateInstance(b)).ToList(), 
    100000, new DateOnly(2021,01,01), 
    new DateOnly(2025,01,01), 5,
    stocksLoader);

while (traderSystemSimulation.DoSimulationStep())
{
    
}

Console.WriteLine("Results:");
foreach (var player in traderSystemSimulation.Players)
{
    Console.WriteLine($"{player.CompanyName}    -   ${traderSystemSimulation.BankAccounts[player] + traderSystemSimulation.Holdings[player].GetCurrentValue(traderSystemSimulation.GetContext())}");
}


    
using NasdaqTrader.Bot.Core;

namespace NasdaqTraderSystem.Core;

public class TraderSystemSimulation
{
    public List<ITraderBot> Players { get; set; } = new();

    public List<IStockListing> StockListings { get; set; } = new();
    public Dictionary<ITraderBot, decimal> BankAccounts { get; set; } = new();
    public Dictionary<ITraderBot, List<IHolding>> Holdings { get; set; } = new();
    public Dictionary<ITraderBot, List<ITrade>> Trades { get; set; } = new();


    private TraderSystemContext _systemContext;

    public TraderSystemSimulation(List<ITraderBot> players, decimal startingCash,
        DateOnly from, DateOnly to,
        int amountOfTradesPerDay, StockLoader stocksLoader)
    {
        Players = players;
        foreach (var player in Players)
        {
            BankAccounts.Add(player, startingCash);
            Holdings.Add(player, new());
            Trades.Add(player, new());
        }

        _systemContext = new TraderSystemContext(this);
        _systemContext.CurrentDate = from;
        _systemContext.StartDate = from;
        _systemContext.EndDate = to;
        _systemContext.AmountOfTradesPerDay = amountOfTradesPerDay;
        StockListings = stocksLoader.GetListings(from, to);
        stocksLoader.Dispose();
    }

    public bool DoSimulationStep()
    {
        if (_systemContext.CurrentDate == _systemContext.EndDate)
        {
            return false;
        }

        _systemContext.CurrentDate = _systemContext.CurrentDate.AddDays(1);
        foreach (var player in Players)
        {
            try
            {
                player.DoTurn(_systemContext);
            }
            catch (Exception e)
            {
            }
        }

        return true;
    }

    public DateOnly GetCurrentDate()
    {
        return _systemContext.CurrentDate;
    }

    public ITraderSystemContext GetContext()
    {
        return _systemContext;
    }

    public decimal GetPriceOnDay(IStockListing listing, DateOnly currentDate)
    {
        return StockListings.First(b => b == listing).PricePoints.First(p => p.Date == currentDate)
            .Price;
    }

    public bool ProcessTrade(ITraderBot traderBot, Trade trade)
    {

        return false;
    }
}
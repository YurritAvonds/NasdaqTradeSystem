namespace NasdaqTrader.Bot.Core;

public interface IHolding
{
    public IStockListing Listing { get;  }
    public int Amount { get;  }
}
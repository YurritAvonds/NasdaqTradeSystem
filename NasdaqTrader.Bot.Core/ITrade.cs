namespace NasdaqTrader.Bot.Core;

public interface ITrade
{   
    public DateOnly ExecutedOn { get;  }
    public IStockListing Listing { get;  }
    public int Amount { get;  }
}
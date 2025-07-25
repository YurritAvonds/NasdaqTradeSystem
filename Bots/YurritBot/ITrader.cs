using NasdaqTrader.Bot.Core;

namespace YurritBot
{
    internal interface ITrader
    {
        int IndexToday { get; }
        int IndexReferenceDay { get; }
        Logger Logger { get; }
        ITraderBot TraderBot { get; }
        ITraderSystemContext TraderSystemContext { get; }

        void ExecuteStrategy();
    }
}
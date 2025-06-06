using System.Reflection;
using NasdaqTrader.Bot.Core;

namespace NasdaqTraderSystem.Core;

public class BotLoader
{
    public void DetermineBots(string botFolder, Dictionary<string, Type> bots)
    {
        var type = typeof(ITraderBot);
        foreach (string file in Directory.GetFiles(botFolder, "*.dll"))
        {
            try
            {
                var assm = Assembly.LoadFrom(file);

                foreach (var botType in assm.ExportedTypes.Where(b =>
                             b.IsAssignableTo(typeof(ITraderBot)) && !b.IsAbstract))
                {
                    bots.Add(((ITraderBot)Activator.CreateInstance(botType)).CompanyName, botType);
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}
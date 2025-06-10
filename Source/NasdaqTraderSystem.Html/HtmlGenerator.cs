using System.Reflection;
using HandlebarsDotNet;
using NasdaqTrader.Bot.Core;
using NasdaqTraderSystem.Core;
using ScottPlot;
using ScottPlot.Plottables;

namespace NasdaqTraderSystem.Html;

public class HtmlGenerator
{
    public string GetGameHtml(SimulationResults results)
    {
        var indexTemplate = GetTemplate("GameResult.html");

        var template = Handlebars.Compile(indexTemplate);

        var result = template(results);
        return result;
    }

    private string GetTemplate(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"NasdaqTraderSystem.Html.Templates.{name}";

        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (StreamReader reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }

    public void GenerateFiles(string baseDirectory, TraderSystemSimulation traderSystemSimulation)
    {
        var gameDate = DateTime.Now;

        SimulationResults results = new SimulationResults();
        results.StartDate = traderSystemSimulation.GetContext().StartDate;
        results.EndDate = traderSystemSimulation.GetContext().EndDate;
        results.RunAt = gameDate.ToString("dd-MM-yyyy HH:mm");
        results.Listings = traderSystemSimulation.StockListings.ToArray();
        results.Trades = traderSystemSimulation.Trades;

        GenerateStockPages(baseDirectory, traderSystemSimulation, gameDate, results);
        results.Companies = traderSystemSimulation.Players.Select(c =>
            new CompanyResult()
            {
                Name = c.CompanyName,
                Cash = traderSystemSimulation.BankAccounts[c].ToString("0.00"),
            }).ToArray();

        GenerateCompaniesPlot(baseDirectory, traderSystemSimulation, gameDate);
        Directory.CreateDirectory($"{baseDirectory}{gameDate:dd-MM-yyyy-HH-mm}\\");
        File.WriteAllText($"{baseDirectory}{gameDate:dd-MM-yyyy-HH-mm}\\GameResult.html", GetGameHtml(results));

        var files = Directory.GetDirectories(baseDirectory).Reverse().ToArray();
        GenerateIndex(baseDirectory, files);
    }

    private void GenerateIndex(string baseDirectory, string[] files)
    {
        var context = new IndexContext();
        context.Games = files.Select(b => new IndexGame()
        {
            GameHTML = Path.GetFileNameWithoutExtension(b) + "\\GameResult.html",
            Name = Path.GetFileNameWithoutExtension(b)
        }).ToArray();
        var indexTemplate = GetTemplate("Index.html");

        var template = Handlebars.Compile(indexTemplate);

        var result = template(context);
        File.WriteAllText($"{baseDirectory}index.html", result);
    }

    private void GenerateStockPages(string baseDirectory, TraderSystemSimulation traderSystemSimulation,
        DateTime gameDate, SimulationResults results)
    {
        Directory.CreateDirectory($"{baseDirectory}{gameDate:dd-MM-yyyy-HH-mm}\\stocks\\");
        foreach (var listing in results.Listings)
        {
            Plot listingsPlot = new();
            var scatter = listingsPlot.Add.Scatter(
                listing.PricePoints.Select(c => c.Date.ToDateTime(new TimeOnly(12, 0))
                ).ToArray(),
                listing.PricePoints.Select(c => c.Price).ToArray());
            scatter.LegendText = listing.Name;
            listingsPlot.Axes.DateTimeTicksBottom();
            listingsPlot.SavePng($"{baseDirectory}{gameDate:dd-MM-yyyy-HH-mm}\\stocks\\{listing.Ticker}.png", 1920,
                1080);

            Directory.CreateDirectory($"{baseDirectory}{gameDate:dd-MM-yyyy-HH-mm}\\");
            File.WriteAllText($"{baseDirectory}{gameDate:dd-MM-yyyy-HH-mm}\\stocks\\{listing.Ticker}.html",
                GetStockHtml(listing, results));
        }
    }

    private string? GetStockHtml(IStockListing listing, SimulationResults results)
    {
        TickerTemplateContext context = new();
        context.Listing = listing;
        context.TradesForPlayers = results.Trades.Where(b => b.Value
            .Any(c => c.Listing.Ticker == listing.Ticker)).Select(b =>
            new TradesForPlayer
            {
                CompanyName = b.Key.CompanyName,
                Trades = b.Value.Where(c => c.Listing.Ticker == listing.Ticker).ToArray()
            }
        ).ToArray();
        var indexTemplate = GetTemplate("StockTemplate.html");

        var template = Handlebars.Compile(indexTemplate);

        var result = template(context);
        return result;
    }

    private void GenerateCompaniesPlot(string baseDirectory, TraderSystemSimulation traderSystemSimulation,
        DateTime gameDate)
    {
        Plot companiesPlot = new();
        companiesPlot.Title("Companies");
        foreach (var player in traderSystemSimulation.Players)
        {
            Plot playerPlot = new();

            var recordsOfPlayer = traderSystemSimulation.Records.Where(b => b.Name == player.CompanyName).ToList();
            var playerScatter = GetTotalWorthScatter(companiesPlot, recordsOfPlayer);
            var playerPlotScatter = GetTotalWorthScatter(playerPlot, recordsOfPlayer);

            playerScatter.LegendText = player.CompanyName + " - Total worth";
            playerPlotScatter.LegendText = "Total worth";


            var playerCashScatter = CashScatter(companiesPlot, recordsOfPlayer);
            var playerPlotCashScatter = CashScatter(playerPlot, recordsOfPlayer);

            playerCashScatter.LegendText = player.CompanyName + " - Cash";
            playerPlotCashScatter.LegendText = "Cash";

            var playerHoldingScatter = HoldingScatter(companiesPlot, recordsOfPlayer);
            var playerPlotHoldingScatter = HoldingScatter(playerPlot, recordsOfPlayer);

            playerHoldingScatter.LegendText = player.CompanyName + " - Holdings";
            playerPlotHoldingScatter.LegendText = "Holdings";

            playerPlot.Axes.DateTimeTicksBottom();
            playerPlot.SavePng($"{baseDirectory}{gameDate:dd-MM-yyyy-HH-mm}\\{player.CompanyName}.png", 1920,
                1080);

            string html = GenerateHtmlForPlayer(player, baseDirectory, traderSystemSimulation,
                gameDate);
            File.WriteAllText($"{baseDirectory}{gameDate:dd-MM-yyyy-HH-mm}\\{player.CompanyName}.html", html);
        }

        companiesPlot.Axes.DateTimeTicksBottom();
        companiesPlot.SavePng($"{baseDirectory}{gameDate:dd-MM-yyyy-HH-mm}\\results.png", 1920,
            1080);
    }

    private static Scatter HoldingScatter(Plot plot, List<HistoricCompanyRecord> recordsOfPlayer)
    {
        var playerHoldingScatter = plot.Add.Scatter(
            recordsOfPlayer.Select(b => b.OnDate.ToDateTime(new TimeOnly(12, 0))).ToArray(),
            recordsOfPlayer.Select(b => b.TotalHolding).ToArray()
        );
        return playerHoldingScatter;
    }

    private static Scatter CashScatter(Plot plot, List<HistoricCompanyRecord> recordsOfPlayer)
    {
        var playerCashScatter = plot.Add.Scatter(
            recordsOfPlayer.Select(b => b.OnDate.ToDateTime(new TimeOnly(12, 0))).ToArray(),
            recordsOfPlayer.Select(b => b.Cash).ToArray()
        );
        return playerCashScatter;
    }

    private static Scatter GetTotalWorthScatter(Plot plot, List<HistoricCompanyRecord> recordsOfPlayer)
    {
        var playerScatter = plot.Add.Scatter(
            recordsOfPlayer.Select(b => b.OnDate.ToDateTime(new TimeOnly(12, 0))).ToArray(),
            recordsOfPlayer.Select(b => b.TotalWorth).ToArray()
        );
        return playerScatter;
    }

    private string GenerateHtmlForPlayer(ITraderBot player, string baseDirectory,
        TraderSystemSimulation traderSystemSimulation, DateTime gameDate)
    {
        PlayerTemplateContext context = new();
        context.CompanyName = player.CompanyName;
        context.TradesForPlayer = traderSystemSimulation.Trades[player].ToArray();
        var indexTemplate = GetTemplate("PlayerResult.html");

        var template = Handlebars.Compile(indexTemplate);

        var result = template(context);
        return result;
    }
}

internal class IndexGame
{
    public string GameHTML { get; set; }
    public string Name { get; set; }
}

internal class IndexContext
{
    public IndexGame[] Games { get; set; }
}

internal class PlayerTemplateContext
{
    public string CompanyName { get; set; }
    public ITrade[] TradesForPlayer { get; set; }
}

internal class TradesForPlayer
{
    public string CompanyName { get; set; }
    public ITrade[] Trades { get; set; }
}

internal class TickerTemplateContext
{
    public IStockListing Listing { get; set; }
    public TradesForPlayer[] TradesForPlayers { get; set; }
}
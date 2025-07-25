namespace YurritBot.Logging;

public class FileLogger : ILogger
{
    public void Log(string text)
    {
        var logFilePath = Path.Combine(AppContext.BaseDirectory, "yurritbot_errors.log");
        File.AppendAllText(logFilePath, $"{text}{Environment.NewLine}");
    }
}

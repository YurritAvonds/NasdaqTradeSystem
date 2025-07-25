namespace YurritBot;

public class Logger
{
    public void LogToFile(string text)
    {
        var logFilePath = Path.Combine(AppContext.BaseDirectory, "yurritbot_errors.log");
        File.AppendAllText(logFilePath, $"{text}{Environment.NewLine}");
    }
}

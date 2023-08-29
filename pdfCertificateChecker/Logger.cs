

public class Logger
{
    private readonly string logFilePath;

    public Logger(string logFilePath)
    {
        this.logFilePath = logFilePath;
    }

    public void Log(string message)
    {
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }
    }
}

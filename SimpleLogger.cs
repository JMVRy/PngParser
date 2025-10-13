namespace Logging;

class SimpleLogger
{
    public bool IsDebugEnabled { get; set; } = false;

    public void Log( LogLevel level, params object[] messages )
    {
        string prefix = level switch
        {
            LogLevel.Debug => "[DEBUG]",
            LogLevel.Info => "[INFO]",
            LogLevel.Warning => "[WARNING]",
            LogLevel.Error => "[ERROR]",
            _ => "[LOG]"
        };

        if ( level == LogLevel.Error || level == LogLevel.Warning )
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine( $"{prefix} {string.Join( " ", messages )}" );
            Console.ResetColor();
            return;
        }

        if ( level == LogLevel.Debug && !IsDebugEnabled )
            return;

        Console.WriteLine( $"{prefix} {string.Join( " ", messages )}" );
    }

    public void Info( string message ) => Log( LogLevel.Info, message );
    public void Warning( string message ) => Log( LogLevel.Warning, message );
    public void Error( string message ) => Log( LogLevel.Error, message );
    public void Debug( string message ) => Log( LogLevel.Debug, message );
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

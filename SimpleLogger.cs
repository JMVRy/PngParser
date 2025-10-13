namespace Logging;

class SimpleLogger
{
    public bool IsDebugEnabled { get; set; } = false;

    public void Log( LogLevel level, params string[] messages )
    {
        string prefix = level switch
        {
            LogLevel.Debug => "[DEBUG]",
            LogLevel.Info => "[INFO]",
            LogLevel.Warning => "[WARNING]",
            LogLevel.Error => "[ERROR]",
            _ => "[LOG]"
        };

        if ( level == LogLevel.Debug && !IsDebugEnabled )
            return;

        if ( level == LogLevel.Error || level == LogLevel.Warning )
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine( $"{prefix} {string.Join( " ", messages )}" );
            Console.ResetColor();
            return;
        }

        Console.WriteLine( $"{prefix} {string.Join( " ", messages )}" );
    }

    public void Info( params string[] messages ) => Log( LogLevel.Info, messages );
    public void Warning( params string[] messages ) => Log( LogLevel.Warning, messages );
    public void Error( params string[] messages ) => Log( LogLevel.Error, messages );
    public void Debug( params string[] messages ) => Log( LogLevel.Debug, messages );
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

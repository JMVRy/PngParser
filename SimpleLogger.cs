#pragma warning disable IDE0130 // Disable namespace matching folder structure warning for simplicity
namespace Logging;
#pragma warning restore IDE0130

static class SimpleLogger
{
    public static bool IsDebugEnabled { get; set; } = false;

    public static void Log( LogLevel level, params object[] messages )
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
            Console.Error.WriteLine( $"{prefix} {string.Join( " ", messages.Select( m => m.ToString() ) )}" );
            Console.ResetColor();
            return;
        }

        Console.WriteLine( $"{prefix} {string.Join( " ", messages.Select( m => m.ToString() ) )}" );
    }

    public static void Info( params object[] messages ) => Log( LogLevel.Info, messages );
    public static void Warning( params object[] messages ) => Log( LogLevel.Warning, messages );
    public static void Error( params object[] messages ) => Log( LogLevel.Error, messages );
    public static void Debug( params object[] messages ) => Log( LogLevel.Debug, messages );
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

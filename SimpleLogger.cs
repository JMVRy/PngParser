using System;


namespace Logging;

static class SimpleLogger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public static void Log( LogLevel level, string message )
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
            Console.Error.WriteLine( $"{prefix} {message}" );
            Console.ResetColor();
            return;
        }

        Console.WriteLine( $"{prefix} {message}" );
    }

    public static void Info( string message ) => Log( LogLevel.Info, message );
    public static void Warning( string message ) => Log( LogLevel.Warning, message );
    public static void Error( string message ) => Log( LogLevel.Error, message );
    public static void Debug( string message ) => Log( LogLevel.Debug, message );
}

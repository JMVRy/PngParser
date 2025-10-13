using Logging;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
class Program
{
    static readonly SimpleLogger Logger = new();

    static void Main()
    {
        Logger.Debug( "PNG Parser started, entering try-catch block" );

        try
        {
            Logger.Debug( "Before parsing PNG data" );
            var pngData = PngParser.PngParser.Parse( [] );
            Logger.Debug( "PNG data successfully parsed" );
        }
        catch ( Exception ex )
        {
            Logger.Error( ex.Message );
        }
    }
}

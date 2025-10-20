using Logging;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
class Program
{
    static void Main()
    {
        SimpleLogger.Debug( "PNG Parser started, entering try-catch block" );

        try
        {
            SimpleLogger.Debug( "Before parsing PNG data" );
            try
            {
                var pngData = PngParser.PngParser.Parse( File.OpenRead( "test.png" ) );
                SimpleLogger.Debug( "PNG data successfully parsed" );
            }
            catch ( Exception ex )
            {
                SimpleLogger.Error( "Failed to parse PNG data: ", ex.Message );
            }
        }
        catch ( Exception ex )
        {
            SimpleLogger.Error( ex.Message );
        }
    }
}

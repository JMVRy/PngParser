using Logging;

class Program
{
    static readonly SimpleLogger Logger = new SimpleLogger();

    static void Main( string[] args )
    {
        Logger.Info( "Hello, World!" );

        try
        {
            PngParser.PngParser.Parse( [] );
        }
        catch ( Exception ex )
        {
            Logger.Error( ex.Message );
        }
    }
}

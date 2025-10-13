using System;
using Logging;

class Program
{
    static void Main( string[] args )
    {
        Console.WriteLine( "Hello, World!" );

        try
        {
            PngParser.Parse( [] );
        }
        catch ( Exception ex )
        {
            SimpleLogger.Error( ex.Message );
        }
    }
}

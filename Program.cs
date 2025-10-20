using Logging;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
class Program
{
    static void Main()
    {
        SimpleLogger.Debug( "PNG Parser started, entering try-catch block" );

        byte[] exampleData = [ 0x00, 0x11, 0x22, 0x33 ]; // Invalid PNG data for testing
        PngParser.PngParser.PngParserOptions options = new PngParser.PngParser.PngParserOptions
        {
            StopAtFirstError = true
        };

        try
        {
            SimpleLogger.Debug( "Before parsing PNG data" );
            try
            {
                using MemoryStream memoryStream = new( exampleData );
                var pngData = PngParser.PngParser.Parse( memoryStream, options );
                SimpleLogger.Debug( "PNG data successfully parsed" );

                foreach ( var textChunk in pngData.TextChunks )
                {
                    SimpleLogger.Info( $"Text Chunk - Keyword: {textChunk.Keyword}, Text: {textChunk.Text}, Language Tag: {textChunk.LanguageTag}, Translated Keyword: {textChunk.TranslatedKeyword}" );
                }
            }
            catch ( Exception ex )
            {
                SimpleLogger.Error( "Failed to parse PNG data:", ex.Message );
            }
        }
        catch ( Exception ex )
        {
            SimpleLogger.Error( ex.Message );
        }
    }
}

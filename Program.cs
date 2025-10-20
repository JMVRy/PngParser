using Logging;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
class Program
{
    static void Main( string[] args )
    {
        SimpleLogger.Debug( "PNG Parser started, entering try-catch block" );

        string pngPath = args.Length > 0 ? args[ 0 ] : "test.png";

        try
        {
            SimpleLogger.Debug( "Before parsing PNG data" );
            try
            {
                var pngData = PngParser.PngParser.Parse( File.OpenRead( "test.png" ) );
                SimpleLogger.Debug( "PNG data successfully parsed" );

                SimpleLogger.Info( $"Parsed PNG Data: Width={pngData.ImageData.GetLength( 0 )}, Height={pngData.ImageData.GetLength( 1 )}, BitDepth={pngData.BitDepth}, ColorType={pngData.ColorType}" );
                SimpleLogger.Info( $"Palette Entries: {( pngData.Palette != null ? pngData.Palette.Length.ToString() : "No Palette" )}" );
                SimpleLogger.Info( $"Number of Text Chunks: {pngData.TextChunks.Length}" );
                SimpleLogger.Info( $"Last modified: {pngData.LastModified.ToString() ?? "N/A"}" );

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

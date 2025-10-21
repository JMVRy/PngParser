using Logging;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
class Program
{
    static void Main( string[] args )
    {
        SimpleLogger.Debug( "PNG Parser started, entering try-catch block" );

        string pngPath = "test.png";
        PngParser.PngParser.PngParserOptions options = new PngParser.PngParser.PngParserOptions
        {
            StopAtFirstError = false
        };

        foreach ( var arg in args )
        {
            switch ( arg )
            {
                case "--debug":
                    SimpleLogger.IsDebugEnabled = true;
                    SimpleLogger.Debug( "Debug logging enabled" );
                    break;
                case "--error":
                    options.StopAtFirstError = true;
                    SimpleLogger.Debug( "StopAtFirstError option enabled" );
                    break;
                default:
                    pngPath = arg;
                    SimpleLogger.Debug( $"PNG path set to: {pngPath}" );
                    break;
            }
        }

        if ( !File.Exists( pngPath ) )
        {
            SimpleLogger.Error( $"File not found: {pngPath}" );
            return;
        }

        try
        {
            SimpleLogger.Debug( "Before parsing PNG data" );
            try
            {
                var pngData = PngParser.PngParser.Parse( File.OpenRead( pngPath ), options );
                SimpleLogger.Debug( "PNG data successfully parsed" );

                SimpleLogger.Info( $"Parsed PNG Data: Width={pngData.ImageData.GetLength( 0 )}, Height={pngData.ImageData.GetLength( 1 )}, BitDepth={pngData.BitDepth}, ColorType={pngData.ColorType}" );
                SimpleLogger.Info( $"Palette Entries: {( pngData.Palette != null ? pngData.Palette.Length.ToString() : "No Palette" )}" );
                SimpleLogger.Info( $"Number of Text Chunks: {pngData.TextChunks.Length}" );
                SimpleLogger.Info( $"Last modified: {pngData.LastModified.ToString() ?? "N/A"}" );

                foreach ( var textChunk in pngData.TextChunks )
                {
                    if ( textChunk.IsBinaryData )
                    {
                        SimpleLogger.Info( $"Text Chunk - Keyword: \"{textChunk.Keyword}\", Text (binary data): {BitConverter.ToString( System.Text.Encoding.Latin1.GetBytes( textChunk.Text ) )}, Language Tag: \"{textChunk.LanguageTag}\", Translated Keyword: \"{textChunk.TranslatedKeyword}\"" );
                    }
                    else
                    {
                        SimpleLogger.Info( $"Text Chunk - Keyword: \"{textChunk.Keyword}\", Text: \"{textChunk.Text}\", Language Tag: \"{textChunk.LanguageTag}\", Translated Keyword: \"{textChunk.TranslatedKeyword}\"" );
                    }
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

namespace PngParser;

static class PngParser
{
    static readonly byte[] PngSignature = [ 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a ];

    public static void Parse( byte[] pngData )
    {
        if ( pngData.Length < 8 )
        {
            throw new Exception( "PNG file too short" );
        }

        if ( !PngSignature.SequenceEqual( pngData.Take( 8 ) ) )
        {
            throw new Exception( "Not a valid PNG file" );
        }
    }
}

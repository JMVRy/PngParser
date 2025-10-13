
static class PngParser
{
    static readonly byte[] PngSignature = [ 137, 80, 78, 71, 13, 10, 26, 10 ];

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

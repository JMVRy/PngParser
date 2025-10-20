
public static class Utils
{
    public static uint ToUint32( this string str )
    {
        if ( str.Length != 4 )
            throw new ArgumentException( "String must be exactly 4 characters long to convert to uint32." );

        byte[] bytes = System.Text.Encoding.ASCII.GetBytes( str );
        return ( uint ) ( bytes[ 0 ] << 24 | bytes[ 1 ] << 16 | bytes[ 2 ] << 8 | bytes[ 3 ] );
    }
}

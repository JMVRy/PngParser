
public static class Utils
{
    public static uint ToUint32( this string str ) => BitConverter.ToUInt32( System.Text.Encoding.ASCII.GetBytes( str ), 0 );
}

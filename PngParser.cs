using Logging;

namespace PngParser;

class PngParser
{
    static readonly byte[] PngSignature = [ 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a ];

    static readonly PngParserOptions DefaultOptions = new();

    public static PngData Parse( Stream pngData )
    {
        PngParser parser = new();
        return parser.Parse( pngData, DefaultOptions );
    }

    public PngData Parse( Stream pngStream, PngParserOptions options )
    {
        try
        {
            this.VerifyHeader( pngStream );
        }
        catch ( InvalidDataException ex )
        {

        }
        catch ( Exception ex )
        {
            if ( options.StopAtFirstError )
                throw;

            SimpleLogger.Error( "Error verifying PNG header: ", ex.Message );
            // If we are not stopping at the first error, we can try to continue parsing
        }

        return new();
    }

    public void VerifyHeader( Stream pngStream /*, PngParserOptions options */ )
    {
        // A PNG file must start with an 8-byte signature

        byte[] signature = new byte[ 8 ];
        pngStream.ReadExactly( signature, 0, 8 );

        // If ReadExactly throws, the stream ended before we could read 8 bytes
        // This means either the file is too short to be a PNG file, or the stream was closed prematurely
        // Either way, we should allow the exception to propogate back

        if ( !PngSignature.SequenceEqual( signature ) )
        {
            throw new InvalidPngHeaderException( "PNG file signature does not match expected signature" );

            // Normally, I wanted to use options to determine whether to throw or just log a warning
            // But then I realized, I don't need to pass options here, since I can just catch the exception in Parse and handle it there
            // This method only verifies the header, so it makes sense to just throw if it's invalid
            // Later methods, however, might want to use options to determine behavior
        }

        // The signature is valid, this file is probably a PNG file
        // Its validity, however, is yet to be determined
    }

    public PngChunk[] ParseChunks( Stream pngStream, PngParserOptions options )
    {
        List<PngChunk> chunks = [];

        // TODO: Implement chunk parsing logic here

        return [ .. chunks ];
    }

    /// <summary>
    /// Options for parsing PNG files.
    /// </summary>
    public class PngParserOptions
    {
        // Here is where different parsing options would go
        // Do we want to validate CRCs?
        // Do we want to ignore certain chunk types, or throw errors?

        /// <summary>
        /// Whether to validate the CRC of each chunk.
        /// This will not error if <see cref="ErrorIfInvalidCrc"/> is false. 
        /// </summary>
        /// <value>Validate the CRC of the chunk</value>
        public bool ValidateCrc { get; set; } = true; // Validate CRC, then use ErrorIfInvalidCrc to either throw an error, or output a warning
        /// <summary>
        /// Whether to throw an error if the CRC is invalid.
        /// This only takes effect if <see cref="ValidateCrc"/> is true.
        /// </summary>
        /// <value>Throw an error when the CRC is invalid</value>
        public bool ErrorIfInvalidCrc { get; set; } = false; // If CRC is invalid, throw an error
        /// <summary>
        /// Whether to ignore unknown chunk types.
        /// The PNG specs say to ignore unknown chunk types, but eh, more configuration options are nice.
        /// </summary>
        /// <value>Ignore unknown chunk types</value>
        public bool IgnoreUnknownChunks { get; set; } = true; // If an unknown chunk type is found, ignore it (default behavior according to PNG specs)
        /// <summary>
        /// Whether to stop parsing at the first error.
        /// If an error is found, we might want to stop all parsing. <br/>
        /// If you only want valid data, this should be true.
        /// If you want to get all possible data, this should be false.
        /// </summary>
        /// <value>Stop parsing immediately if data is invalid</value>
        public bool StopAtFirstError { get; set; } = false; // If an error is found, stop parsing immediately
    }

    public class InvalidPngException( string message ) : Exception( message )
    { }

    public class CrcMismatchException( string message ) : InvalidPngException( message )
    { }

    public class InvalidPngHeaderException( string message ) : InvalidPngException( message )
    { }
}

/// <summary>
/// Represents parsed PNG data.
/// </summary>
public class PngData
{
    /// <summary>
    /// The chunks contained in the PNG file.
    /// </summary>
    /// <value>A list of PNG chunks</value>
    public List<PngChunk> Chunks { get; set; } = [];
}

/// <summary>
/// A single PNG chunk.
/// Contains the length of the chunk, the type, the internal data, and the CRC to prevent data corruption.
/// </summary>
public struct PngChunk
{
    /// <summary>
    /// The length of the chunk data, as listed in the PNG file.
    /// A 4-byte unsigned integer.
    /// </summary>
    public uint Length;
    /// <summary>
    /// The type of the chunk.
    /// 4-bytes, normally represented as ASCII characters. <br/>
    /// However, the PNG specs say that encoders and decoders should treat codes as fixed binary values, not as text (due to various character encodings).
    /// I could represent this as a string, but in case I want to add support for re-encoding PNG files later, I don't want to allow users to make a string that isn't 4 bytes long.
    /// </summary>
    public uint Type;
    /// <summary>
    /// The data in the chunk
    /// The length of this array should be equal to <see cref="Length"/>
    /// If it isn't, that's an implementation error.
    /// </summary>
    public byte[] Data;
    /// <summary>
    /// The CRC of the chunk data.
    /// 4-bytes, unsigned, used to verify data integrity.
    /// </summary>
    public uint Crc;
}

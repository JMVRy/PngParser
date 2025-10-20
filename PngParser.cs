using Logging;
using System.IO.Compression;

namespace PngParser;

static class PngParser
{
    /// <summary>
    /// The signature bytes that identify a file as a PNG file.
    /// This is the same for all PNG files. A magic number, of sorts.
    /// </summary>
    static readonly byte[] PngSignature = [ 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a ];

    /// <summary>
    /// The default options to use when parsing a PNG file.
    /// </summary>
    static readonly PngParserOptions DefaultOptions = new();

    static readonly uint[] CrcTable = [ 0x0u, 0x77073096u, 0xee0e612cu, 0x990951bau, 0x76dc419u, 0x706af48fu, 0xe963a535u, 0x9e6495a3u, 0xedb8832u, 0x79dcb8a4u, 0xe0d5e91eu, 0x97d2d988u, 0x9b64c2bu, 0x7eb17cbdu, 0xe7b82d07u, 0x90bf1d91u, 0x1db71064u, 0x6ab020f2u, 0xf3b97148u, 0x84be41deu, 0x1adad47du, 0x6ddde4ebu, 0xf4d4b551u, 0x83d385c7u, 0x136c9856u, 0x646ba8c0u, 0xfd62f97au, 0x8a65c9ecu, 0x14015c4fu, 0x63066cd9u, 0xfa0f3d63u, 0x8d080df5u, 0x3b6e20c8u, 0x4c69105eu, 0xd56041e4u, 0xa2677172u, 0x3c03e4d1u, 0x4b04d447u, 0xd20d85fdu, 0xa50ab56bu, 0x35b5a8fau, 0x42b2986cu, 0xdbbbc9d6u, 0xacbcf940u, 0x32d86ce3u, 0x45df5c75u, 0xdcd60dcfu, 0xabd13d59u, 0x26d930acu, 0x51de003au, 0xc8d75180u, 0xbfd06116u, 0x21b4f4b5u, 0x56b3c423u, 0xcfba9599u, 0xb8bda50fu, 0x2802b89eu, 0x5f058808u, 0xc60cd9b2u, 0xb10be924u, 0x2f6f7c87u, 0x58684c11u, 0xc1611dabu, 0xb6662d3du, 0x76dc4190u, 0x1db7106u, 0x98d220bcu, 0xefd5102au, 0x71b18589u, 0x6b6b51fu, 0x9fbfe4a5u, 0xe8b8d433u, 0x7807c9a2u, 0xf00f934u, 0x9609a88eu, 0xe10e9818u, 0x7f6a0dbbu, 0x86d3d2du, 0x91646c97u, 0xe6635c01u, 0x6b6b51f4u, 0x1c6c6162u, 0x856530d8u, 0xf262004eu, 0x6c0695edu, 0x1b01a57bu, 0x8208f4c1u, 0xf50fc457u, 0x65b0d9c6u, 0x12b7e950u, 0x8bbeb8eau, 0xfcb9887cu, 0x62dd1ddfu, 0x15da2d49u, 0x8cd37cf3u, 0xfbd44c65u, 0x4db26158u, 0x3ab551ceu, 0xa3bc0074u, 0xd4bb30e2u, 0x4adfa541u, 0x3dd895d7u, 0xa4d1c46du, 0xd3d6f4fbu, 0x4369e96au, 0x346ed9fcu, 0xad678846u, 0xda60b8d0u, 0x44042d73u, 0x33031de5u, 0xaa0a4c5fu, 0xdd0d7cc9u, 0x5005713cu, 0x270241aau, 0xbe0b1010u, 0xc90c2086u, 0x5768b525u, 0x206f85b3u, 0xb966d409u, 0xce61e49fu, 0x5edef90eu, 0x29d9c998u, 0xb0d09822u, 0xc7d7a8b4u, 0x59b33d17u, 0x2eb40d81u, 0xb7bd5c3bu, 0xc0ba6cadu, 0xedb88320u, 0x9abfb3b6u, 0x3b6e20cu, 0x74b1d29au, 0xead54739u, 0x9dd277afu, 0x4db2615u, 0x73dc1683u, 0xe3630b12u, 0x94643b84u, 0xd6d6a3eu, 0x7a6a5aa8u, 0xe40ecf0bu, 0x9309ff9du, 0xa00ae27u, 0x7d079eb1u, 0xf00f9344u, 0x8708a3d2u, 0x1e01f268u, 0x6906c2feu, 0xf762575du, 0x806567cbu, 0x196c3671u, 0x6e6b06e7u, 0xfed41b76u, 0x89d32be0u, 0x10da7a5au, 0x67dd4accu, 0xf9b9df6fu, 0x8ebeeff9u, 0x17b7be43u, 0x60b08ed5u, 0xd6d6a3e8u, 0xa1d1937eu, 0x38d8c2c4u, 0x4fdff252u, 0xd1bb67f1u, 0xa6bc5767u, 0x3fb506ddu, 0x48b2364bu, 0xd80d2bdau, 0xaf0a1b4cu, 0x36034af6u, 0x41047a60u, 0xdf60efc3u, 0xa867df55u, 0x316e8eefu, 0x4669be79u, 0xcb61b38cu, 0xbc66831au, 0x256fd2a0u, 0x5268e236u, 0xcc0c7795u, 0xbb0b4703u, 0x220216b9u, 0x5505262fu, 0xc5ba3bbeu, 0xb2bd0b28u, 0x2bb45a92u, 0x5cb36a04u, 0xc2d7ffa7u, 0xb5d0cf31u, 0x2cd99e8bu, 0x5bdeae1du, 0x9b64c2b0u, 0xec63f226u, 0x756aa39cu, 0x26d930au, 0x9c0906a9u, 0xeb0e363fu, 0x72076785u, 0x5005713u, 0x95bf4a82u, 0xe2b87a14u, 0x7bb12baeu, 0xcb61b38u, 0x92d28e9bu, 0xe5d5be0du, 0x7cdcefb7u, 0xbdbdf21u, 0x86d3d2d4u, 0xf1d4e242u, 0x68ddb3f8u, 0x1fda836eu, 0x81be16cdu, 0xf6b9265bu, 0x6fb077e1u, 0x18b74777u, 0x88085ae6u, 0xff0f6a70u, 0x66063bcau, 0x11010b5cu, 0x8f659effu, 0xf862ae69u, 0x616bffd3u, 0x166ccf45u, 0xa00ae278u, 0xd70dd2eeu, 0x4e048354u, 0x3903b3c2u, 0xa7672661u, 0xd06016f7u, 0x4969474du, 0x3e6e77dbu, 0xaed16a4au, 0xd9d65adcu, 0x40df0b66u, 0x37d83bf0u, 0xa9bcae53u, 0xdebb9ec5u, 0x47b2cf7fu, 0x30b5ffe9u, 0xbdbdf21cu, 0xcabac28au, 0x53b39330u, 0x24b4a3a6u, 0xbad03605u, 0xcdd70693u, 0x54de5729u, 0x23d967bfu, 0xb3667a2eu, 0xc4614ab8u, 0x5d681b02u, 0x2a6f2b94u, 0xb40bbe37u, 0xc30c8ea1u, 0x5a05df1bu, 0x2d02ef8du ];

    /// <inheritdoc cref="Parse(Stream, PngParserOptions)" path="/*[not(self::summary)]" />
    /// <summary>
    /// Parses a PNG file from a stream.
    /// Uses the default parsing options.
    /// </summary>
    /// <param name="pngData">The stream containing the PNG data.</param>
    public static PngData Parse( Stream pngData )
    {
        return PngParser.Parse( pngData, DefaultOptions );
    }

    /// <summary>
    /// Parses a PNG file from a stream.
    /// Uses the provided parsing options.
    /// </summary>
    /// <param name="pngStream">The stream containing the PNG data.</param>
    /// <param name="options">The options to use when parsing the PNG data.</param>
    /// <returns>The parsed PNG data.</returns>
    /// <exception cref="ArgumentOutOfRangeException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidPngHeaderException" />
    /// <exception cref="Exception" />
    public static PngData Parse( Stream pngStream, PngParserOptions options )
    {
        try
        {
            // Make sure the PNG file starts with the correct header
            SimpleLogger.Debug( "Verifying PNG header." );
            PngParser.VerifyHeader( pngStream );
        }
        // Exceptions that indicate we cannot continue parsing
        catch ( ArgumentOutOfRangeException outOfRange )
        {
            SimpleLogger.Error( "PNG stream ended prematurely while verifying header:", outOfRange.Message );
            throw;
        }
        catch ( ArgumentNullException nullEx )
        {
            SimpleLogger.Error( "PNG stream was null while verifying header:", nullEx.Message );
            throw;
        }
        catch ( EndOfStreamException endOfStream )
        {
            SimpleLogger.Error( "PNG stream ended before header could be fully read:", endOfStream.Message );
            throw;
        }
        // Exceptions that indicate possible invalid PNG data
        catch ( InvalidPngHeaderException invalidHeader )
        {
            if ( options.StopAtFirstError )
            {
                SimpleLogger.Debug( "Entered with InvalidPngHeaderException, rethrowing due to StopAtFirstError." );
                throw;
            }

            SimpleLogger.Error( "Invalid PNG header:", invalidHeader.Message );
        }
        // Catch-all for any other exceptions
        catch ( Exception ex )
        {
            SimpleLogger.Error( "Unexpected error while verifying PNG header:", ex.Message );
            SimpleLogger.Error( "Error type:", ex.GetType().ToString() );

            if ( options.StopAtFirstError )
            {
                SimpleLogger.Debug( "Entered with Exception, rethrowing due to StopAtFirstError." );
                throw;
            }

            SimpleLogger.Error( "Error verifying PNG header:", ex.Message );
        }

        SimpleLogger.Debug( "Reading PNG chunks." );
        var chunks = PngParser.ReadChunks( pngStream );
        SimpleLogger.Debug( $"Found {chunks.Length} PNG chunks." );

        if ( chunks.Length < 1 )
        {
            SimpleLogger.Warning( "No PNG chunks found in the provided stream." );
            return new PngData();
        }

        SimpleLogger.Debug( "Verifying first PNG chunk." );
        if ( chunks[ 0 ].Type != PngChunkType.IHDR )
        {
            SimpleLogger.Error( "First PNG chunk is not IHDR." );

            if ( options.StopAtFirstError )
                throw new InvalidPngException( "First PNG chunk is not IHDR." );

            SimpleLogger.Debug( "Continuing parsing despite invalid first chunk." );
        }

        // Now we can actually do things with the PNG data

        SimpleLogger.Debug( "Parsing PNG chunks." );
        var pngData = PngParser.ParseChunks( chunks, options );
        SimpleLogger.Debug( "Finished parsing PNG data." );

        if ( chunks.Last().Type != PngChunkType.IEND )
        {
            SimpleLogger.Warning( "Last PNG chunk is not IEND." );
        }

        PngParser.VerifyPngData( pngData, options );

        return pngData;
    }

    /// <summary>
    /// Verifies that the given stream starts with a valid PNG header.
    /// </summary>
    /// <param name="pngStream">The stream containing the PNG data.</param>
    private static void VerifyHeader( Stream pngStream /*, PngParserOptions options */ )
    {
        // A PNG file must start with an 8-byte signature

        byte[] signature = new byte[ 8 ];
        pngStream.ReadExactly( signature, 0, 8 );

        // If ReadExactly throws, the stream ended before we could read 8 bytes
        // This means either the file is too short to be a PNG file, or the stream was closed prematurely
        // Either way, we should allow the exception to propogate back

        if ( !PngSignature.SequenceEqual( signature ) )
        {
            throw new InvalidPngSignatureException( "PNG file signature does not match expected signature" );

            // Normally, I wanted to use options to determine whether to throw or just log a warning
            // But then I realized, I don't need to pass options here, since I can just catch the exception in Parse and handle it there
            // This method only verifies the header, so it makes sense to just throw if it's invalid
            // Later methods, however, might want to use options to determine behavior
        }

        // The signature is valid, this file is probably a PNG file
        // Its validity, however, is yet to be determined
    }

    /// <summary>
    /// Reads the chunks from the given PNG stream.
    /// </summary>
    /// <param name="pngStream">The stream containing the PNG data.</param>
    /// <returns>The parsed PNG chunks.</returns>
    private static PngChunk[] ReadChunks( Stream pngStream /*, PngParserOptions options */ ) // we're only reading chunks, so we don't need any options yet
    {
        List<PngChunk> chunks = [];

        // TODO: Implement chunk parsing logic here

        pngStream.Seek( 8, SeekOrigin.Begin ); // Skip the PNG signature, in case VerifyHeader didn't already move the stream position

        while ( pngStream.Position < pngStream.Length )
        {
            // Read chunk length (4 bytes)
            byte[] lengthBytes = new byte[ 4 ];
            pngStream.ReadExactly( lengthBytes, 0, 4 );
            uint length;
            if ( BitConverter.IsLittleEndian )
                length = BitConverter.ToUInt32( [ .. lengthBytes.Reverse() ], 0 ); // PNG uses big-endian
            else
                length = BitConverter.ToUInt32( lengthBytes, 0 );

            // Read chunk type (4 bytes)
            byte[] typeBytes = new byte[ 4 ];
            pngStream.ReadExactly( typeBytes, 0, 4 );
            uint type;
            if ( BitConverter.IsLittleEndian )
                type = BitConverter.ToUInt32( [ .. typeBytes.Reverse() ], 0 );
            else
                type = BitConverter.ToUInt32( typeBytes, 0 );

            // Read chunk data (length bytes)
            if ( length > int.MaxValue )
                throw new InvalidPngException( "Chunk length exceeds maximum supported size" );

            byte[] data = new byte[ length ];
            if ( length > 0 )
                pngStream.ReadExactly( data, 0, ( int ) length );

            // Read chunk CRC (4 bytes)
            byte[] crcBytes = new byte[ 4 ];
            pngStream.ReadExactly( crcBytes, 0, 4 );
            uint crc;
            if ( BitConverter.IsLittleEndian )
                crc = BitConverter.ToUInt32( [ .. crcBytes.Reverse() ], 0 );
            else
                crc = BitConverter.ToUInt32( crcBytes, 0 );

            SimpleLogger.Debug( $"Read chunk: Type={type} ({( ( PngChunkType ) type ).AsciiName}), Length={length}, CRC={crc:X8}" );

            PngChunk chunk = new()
            {
                Length = length,
                Type = type,
                Data = data,
                Crc = crc
            };

            chunks.Add( chunk );
        }

        return [ .. chunks ];
    }

    /// <summary>
    /// Parses the chunks of a PNG file.
    /// </summary>
    /// <param name="chunks">The PNG chunks to parse.</param>
    /// <param name="options">The parser options.</param>
    /// <returns>The parsed PNG data.</returns>
    private static PngData ParseChunks( PngChunk[] chunks, PngParserOptions options )
    {
        PngData pngData = new()
        {
            Chunks = [ .. chunks ]
        };

        // Time to process the chunks based on their types

        foreach ( PngChunk chunk in chunks )
        {
            SimpleLogger.Debug( $"Processing chunk: {chunk.Type}" );
            SimpleLogger.Debug( "Checking chunk CRC" );

            uint calculatedCrc;
            byte[] typeBytes = BitConverter.GetBytes( ( uint ) chunk.Type );

            if ( BitConverter.IsLittleEndian )
                calculatedCrc = PngParser.CalculateCrc( [ .. typeBytes.Reverse(), .. chunk.Data ] );
            else
                calculatedCrc = PngParser.CalculateCrc( [ .. typeBytes, .. chunk.Data ] );

            SimpleLogger.Debug( $"Calculated CRC: {calculatedCrc:X8}" );

            if ( calculatedCrc != chunk.Crc )
            {
                string crcMessage = $"CRC mismatch for chunk {chunk.Type}: found {chunk.Crc:X8}, calculated {calculatedCrc:X8}";

                if ( options.ValidateCrc )
                {
                    if ( options.ErrorIfInvalidCrc )
                    {
                        SimpleLogger.Error( crcMessage );
                        if ( options.StopAtFirstError )
                            throw new CrcMismatchException( crcMessage );
                    }
                    else
                    {
                        SimpleLogger.Warning( crcMessage );
                    }
                }
                else
                {
                    SimpleLogger.Debug( "CRC validation is disabled, continuing despite mismatch." );
                }
            }
            else
                SimpleLogger.Debug( "Chunk CRC is valid." );

            switch ( chunk.Type.NameAsUint )
            {
                case PngChunkType.IHDR:
                    // Process IHDR chunk
                    SimpleLogger.Debug( "Processing IHDR chunk." );
                    PngParser.ParseIHDRChunk( chunk, ref pngData, options );
                    break;

                case PngChunkType.PLTE:
                    // Process PLTE chunk
                    SimpleLogger.Debug( "Processing PLTE chunk." );

                    if ( pngData.ParsedPalette )
                    {
                        string message = "Multiple PLTE chunks found in PNG file.";
                        SimpleLogger.Error( message );
                        if ( options.StopAtFirstError )
                            throw new InvalidPngException( message );
                        SimpleLogger.Warning( "Ignoring subsequent PLTE chunk, overwriting existing data." );
                    }

                    PngParser.ParsePLTEChunk( chunk, ref pngData, options );
                    break;

                case PngChunkType.IDAT:
                    // Process IDAT chunk
                    SimpleLogger.Debug( "Processing IDAT chunk." );
                    PngParser.ParseIDATChunk( chunk, ref pngData, options );
                    break;

                case PngChunkType.IEND:
                    // Process IEND chunk
                    SimpleLogger.Debug( "Processing IEND chunk." );
                    //PngParser.ParseIENDChunk( chunk, ref pngData, options ); // IEND chunk has no data to process, so all we need to do is make sure the length is 0

                    if ( chunk.Length != 0 )
                    {
                        SimpleLogger.Error( "IEND chunk has non-zero length." );

                        if ( options.StopAtFirstError )
                            throw new InvalidPngException( "IEND chunk has non-zero length." );
                    }

                    if ( options.StopAtIEND )
                    {
                        SimpleLogger.Debug( "Stopping parsing at IEND chunk as per options." );
                        return pngData;
                    }

                    break;

                case PngChunkType.tEXt:
                case PngChunkType.zTXt:
                case PngChunkType.iTXt:
                    // Process tEXt chunk
                    SimpleLogger.Debug( "Processing tEXt chunk." );
                    PngParser.ParseTextChunk( chunk, ref pngData, options );
                    break;

                case PngChunkType.tIME:
                    // Process tIME chunk
                    SimpleLogger.Debug( "Processing tIME chunk." );
                    PngParser.ParseTimeChunk( chunk, ref pngData, options );
                    break;

                default:
                    SimpleLogger.Info( "Unknown chunk type encountered:", chunk.Type );
                    if ( ( chunk.Type & 0x20000000 ) == 0 ) // Critical chunk
                    {
                        if ( options.IgnoreUnknownCriticalChunks )
                        {
                            SimpleLogger.Warning( "Ignoring unknown critical chunk as per options." );
                        }
                        else
                        {
                            SimpleLogger.Error( "Unknown critical chunk encountered." );
                            if ( options.StopAtFirstError )
                                throw new InvalidPngException( "Unknown critical chunk encountered." );
                        }
                    }
                    else // Ancillary chunk
                    {
                        if ( options.IgnoreUnknownAncillaryChunks )
                        {
                            SimpleLogger.Debug( "Ignoring unknown ancillary chunk as per options." );
                        }
                        else
                        {
                            SimpleLogger.Warning( "Processing unknown ancillary chunk." );
                            // Here we could add logic to store ancillary chunks if desired
                        }
                    }
                    break;
            }
        }

        return pngData;
    }

    /// <summary>
    /// Parses the IHDR chunk of a PNG file.
    /// </summary>
    /// <param name="chunk">The IHDR chunk.</param>
    /// <param name="pngData">The PNG data structure to populate.</param>
    /// <param name="options">The parser options.</param>
    /// <exception cref="InvalidPngHeaderException" />
    private static void ParseIHDRChunk( PngChunk chunk, ref PngData pngData, PngParserOptions options )
    {
        // IHDR chunk must be 13 bytes long
        if ( chunk.Length != 13 )
        {
            SimpleLogger.Error( "IHDR chunk has invalid length:", chunk.Length.ToString() );

            if ( options.StopAtFirstError )
                throw new InvalidPngHeaderException( "IHDR chunk has invalid length." );

            return;
        }

        // IHDR data: Width (4), Height (4), Bit depth (1), Color type (1), Compression method (1), Filter method (1), Interlace method (1)
        byte[] data = chunk.Data;
        uint width;
        if ( BitConverter.IsLittleEndian )
            width = BitConverter.ToUInt32( [ .. data.Take( 4 ).Reverse() ], 0 );
        else
            width = BitConverter.ToUInt32( data, 0 );

        uint height;
        if ( BitConverter.IsLittleEndian )
            height = BitConverter.ToUInt32( [ .. data.Skip( 4 ).Take( 4 ).Reverse() ], 0 );
        else
            height = BitConverter.ToUInt32( data, 4 );

        byte bitDepth = data[ 8 ];
        byte colorType = data[ 9 ];
        byte compressionMethod = data[ 10 ];
        byte filterMethod = data[ 11 ];
        byte interlaceMethod = data[ 12 ];

        SimpleLogger.Debug( $"IHDR Chunk - Width: {width}, Height: {height}, Bit Depth: {bitDepth}, Color Type: {colorType}, Compression Method: {compressionMethod}, Filter Method: {filterMethod}, Interlace Method: {interlaceMethod}" );

        // Now we pass that data into the PngData structure

        pngData.ImageData = new Color[ width, height ];
        pngData.BitDepth = bitDepth;
        pngData.ColorType = colorType;
        pngData.CompressionMethod = compressionMethod;
        pngData.FilterMethod = filterMethod;
        pngData.InterlaceMethod = interlaceMethod;

        PngParser.EnsureIHDRValid( width, height, bitDepth, colorType, compressionMethod, filterMethod, interlaceMethod, options );
    }

    /// <summary>
    /// Ensures that the IHDR chunk data is valid according to the PNG specification.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="bitDepth">The bit depth of the image.</param>
    /// <param name="colorType">The color type of the image.</param>
    /// <param name="compressionMethod">The compression method used.</param>
    /// <param name="filterMethod">The filter method used.</param>
    /// <param name="interlaceMethod">The interlace method used.</param>
    /// <param name="options">The parser options.</param>
    /// <exception cref="InvalidPngHeaderException" />
    private static void EnsureIHDRValid( uint width, uint height, byte bitDepth, byte colorType, byte compressionMethod, byte filterMethod, byte interlaceMethod, PngParserOptions options )
    {
        const uint MaxPngDimension = 1u << 31; // 2^31
        if ( height == 0 || width == 0 )
        {
            string message = "Image width and height must be greater than 0.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngHeaderException( message );
        }
        else if ( height > MaxPngDimension || width > MaxPngDimension ) // PNG spec limits dimensions to 2^31 "to accommodate languages that have difficulty with unsigned 4-byte values."
        {
            string message = "Image width and height exceed maximum allowed dimensions.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngHeaderException( message );
        }
        /* else if ( height * width > int.MaxValue )
        {
            string message = "Image dimensions are too large to be processed.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngException( message );
        } */ // NOTE: This check is to prevent memory issues (4 exabytes assuming 1 byte per pixel and max int size), but I might be able to handle it later down the line. I'd rather get a memory exception right now anyways.

        PngParser.EnsureColorTypeAndBitDepthValid( colorType, bitDepth, options );

        if ( compressionMethod != 0 )
        {
            string message = $"Invalid compression method {compressionMethod}. Only method 0 (deflate/inflate) is supported.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngHeaderException( message );
        }

        if ( filterMethod != 0 )
        {
            string message = $"Invalid filter method {filterMethod}. Only method 0 (adaptive filtering with five basic filter types) is supported.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngHeaderException( message );
        }

        if ( interlaceMethod != 0 && interlaceMethod != 1 )
        {
            string message = $"Invalid interlace method {interlaceMethod}. Only methods 0 (no interlace) and 1 (Adam7 interlace) are supported.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngHeaderException( message );
        }
    }

    /// <summary>
    /// Ensures that the given color type and bit depth are valid according to the PNG specification.
    /// </summary>
    /// <param name="colorType">The color type provided</param>
    /// <param name="bitDepth">The bit depth provided</param>
    /// <param name="options">The parser options</param>
    /// <exception cref="InvalidPngHeaderException" />
    private static void EnsureColorTypeAndBitDepthValid( byte colorType, byte bitDepth, PngParserOptions options )
    {
        if ( bitDepth != 1 && bitDepth != 2 && bitDepth != 4 && bitDepth != 8 && bitDepth != 16 )
        {
            string message = $"Invalid bit depth {bitDepth}.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngHeaderException( message );
        }

        switch ( colorType )
        {
            case 0: // Grayscale
                if ( bitDepth != 1 && bitDepth != 2 && bitDepth != 4 && bitDepth != 8 && bitDepth != 16 )
                {
                    string message = $"Invalid bit depth {bitDepth} for color type {colorType} (Grayscale).";
                    SimpleLogger.Error( message );
                    if ( options.StopAtFirstError )
                        throw new InvalidPngHeaderException( message );
                }
                break;

            case 2: // Truecolor
                if ( bitDepth != 8 && bitDepth != 16 )
                {
                    string message = $"Invalid bit depth {bitDepth} for color type {colorType} (Grayscale).";
                    SimpleLogger.Error( message );
                    if ( options.StopAtFirstError )
                        throw new InvalidPngHeaderException( message );
                }
                break;

            case 3: // Indexed-color
                if ( bitDepth != 1 && bitDepth != 2 && bitDepth != 4 && bitDepth != 8 )
                {
                    string message = $"Invalid bit depth {bitDepth} for color type {colorType} (Indexed-color).";
                    SimpleLogger.Error( message );
                    if ( options.StopAtFirstError )
                        throw new InvalidPngHeaderException( message );
                }
                break;

            case 4: // Grayscale with alpha
                if ( bitDepth != 8 && bitDepth != 16 )
                {
                    string message = $"Invalid bit depth {bitDepth} for color type {colorType} (Grayscale with alpha).";
                    SimpleLogger.Error( message );
                    if ( options.StopAtFirstError )
                        throw new InvalidPngHeaderException( message );
                }
                break;

            case 6: // Truecolor with alpha
                if ( bitDepth != 8 && bitDepth != 16 )
                {
                    string message = $"Invalid bit depth {bitDepth} for color type {colorType} (Truecolor with alpha).";
                    SimpleLogger.Error( message );
                    if ( options.StopAtFirstError )
                        throw new InvalidPngHeaderException( message );
                }
                break;

            default:
                string defaultMessage = $"Unknown color type {colorType}.";
                SimpleLogger.Error( defaultMessage );
                if ( options.StopAtFirstError )
                    throw new InvalidPngHeaderException( defaultMessage );
                break;
        }
    }

    private static void ParseIDATChunk( PngChunk chunk, ref PngData pngData, PngParserOptions options )
    {
        // TODO: Parse IDAT chunk data, which contains the actual image data
        // This requires decompressing the data and then applying PNG filters
        // That's too much work for right now, so I'll leave it for later

        if ( pngData.ImageData == null || pngData.ImageData.Length < 1 )
        {
            string message = "Cannot parse IDAT chunk before IHDR chunk.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngHeaderException( message );

            pngData.ImageData = new Color[ GetLargestFactorsOfInteger( chunk.Length ).Item2, GetLargestFactorsOfInteger( chunk.Length ).Item1 ]; // Prefer wider image

            static Tuple<uint, uint> GetLargestFactorsOfInteger( uint value )
            {
                uint factor1 = 1;
                uint factor2 = value;

                for ( uint i = 1; i <= Math.Sqrt( value ); i++ )
                {
                    if ( value % i == 0 )
                    {
                        factor1 = i;
                        factor2 = value / i;
                    }
                }

                return Tuple.Create( factor1, factor2 );
            }
        }

        uint imageWidth = ( uint ) pngData.ImageData.GetLength( 0 );
        uint imageHeight = ( uint ) pngData.ImageData.GetLength( 1 );

        if ( chunk.Length > imageWidth * imageHeight )
        {
            SimpleLogger.Warning( "IDAT chunk length exceeds image data size. Truncating data for placeholder parsing." );
        }

        for ( uint i = 0; i < Math.Min( chunk.Length, imageWidth * imageHeight ); i++ )
        {
            // Simple grayscale mapping as placeholder
            pngData.ImageData[ i % imageWidth, i / imageWidth ] = new Color( chunk.Data[ i ], chunk.Data[ i ], chunk.Data[ i ] );
        }
    }

    private static void ParsePLTEChunk( PngChunk chunk, ref PngData pngData, PngParserOptions options )
    {
        // A palette chunk contains a list of colors used in indexed-color images
        // It is a multiple of 3 bytes long, with each color represented by 3 bytes (RGB)
        // The maximum number of colors is 256, so the maximum length is 768 bytes

        if ( chunk.Length % 3 != 0 )
        {
            string message = $"Invalid PLTE chunk length {chunk.Length}. Must be a multiple of 3.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngHeaderException( message );
        }
        else if ( chunk.Length > 768 )
        {
            string message = $"Invalid PLTE chunk length {chunk.Length}. Exceeds maximum of 768 bytes.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngHeaderException( message );
        }

        byte numOfColors = ( byte ) ( chunk.Length / 3 );
        SimpleLogger.Debug( $"PLTE Chunk - Number of colors: {numOfColors}" );

        Color[] palette = new Color[ numOfColors ];
        for ( int i = 0; i < numOfColors; i++ )
        {
            byte r = chunk.Data[ i * 3 ];
            byte g = chunk.Data[ i * 3 + 1 ];
            byte b = chunk.Data[ i * 3 + 2 ];
            palette[ i ] = new Color( r, g, b );
        }

        pngData.Palette = palette;
    }

    private static void ParseTextChunk( PngChunk chunk, ref PngData pngData, PngParserOptions options )
    {
        SimpleLogger.Debug( $"Parsing text chunk of type {chunk.Type}." );

        if ( chunk.Type == PngChunkType.iTXt )
        {
            // Handle iTXt chunk (international text)
            // iTXt chunk format:
            // - Keyword (null-terminated)
            // - Compression flag (1 byte)
            // - Compression method (1 byte)
            // - Language tag (null-terminated)
            // - Translated keyword (null-terminated)
            // - Text (remaining bytes, possibly compressed)

            int index = 0;
            // Read keyword
            int firstNull = Array.IndexOf( chunk.Data, ( byte ) 0, index );
            if ( firstNull == -1 )
            {
                string message = "Invalid iTXt chunk: missing null terminator for keyword.";
                SimpleLogger.Error( message );
                if ( options.StopAtFirstError )
                    throw new InvalidPngException( message );
                return;
            }

            string keyword = System.Text.Encoding.UTF8.GetString( chunk.Data, index, firstNull - index );
            index = firstNull + 1;
            SimpleLogger.Debug( $"iTXt Chunk - Keyword: {keyword}" );

            // Read compression flag
            byte compressionFlag = chunk.Data[ index ];
            index++;
            SimpleLogger.Debug( $"iTXt Chunk - Compression Flag: {compressionFlag}" );

            // Read compression method
            byte compressionMethod = chunk.Data[ index ];
            index++;
            SimpleLogger.Debug( $"iTXt Chunk - Compression Method: {compressionMethod} (ignored)" );

            // Read language tag
            firstNull = Array.IndexOf( chunk.Data, ( byte ) 0, index );
            if ( firstNull == -1 )
            {
                string message = "Invalid iTXt chunk: missing null terminator for language tag.";
                SimpleLogger.Error( message );
                if ( options.StopAtFirstError )
                    throw new InvalidPngException( message );
                return;
            }

            string languageTag = System.Text.Encoding.UTF8.GetString( chunk.Data, index, firstNull - index );
            index = firstNull + 1;
            SimpleLogger.Debug( $"iTXt Chunk - Language Tag: {languageTag}" );

            // Read translated keyword
            firstNull = Array.IndexOf( chunk.Data, ( byte ) 0, index );
            if ( firstNull == -1 )
            {
                string message = "Invalid iTXt chunk: missing null terminator for translated keyword.";
                SimpleLogger.Error( message );
                if ( options.StopAtFirstError )
                    throw new InvalidPngException( message );
                return;
            }

            string translatedKeyword = System.Text.Encoding.UTF8.GetString( chunk.Data, index, firstNull - index );
            index = firstNull + 1;
            SimpleLogger.Debug( $"iTXt Chunk - Translated Keyword: {translatedKeyword}" );

            // Read text
            byte[] textData = new byte[ chunk.Length - index ];
            Array.Copy( chunk.Data, index, textData, 0, textData.Length );
            string text;
            if ( compressionFlag == 1 )
            {
                // Decompress text data
                using MemoryStream compressedStream = new( textData );
                using DeflateStream deflateStream = new( compressedStream, CompressionMode.Decompress );
                using StreamReader reader = new( deflateStream, System.Text.Encoding.UTF8 );
                text = reader.ReadToEnd();
            }
            else
            {
                text = System.Text.Encoding.UTF8.GetString( textData );
            }

            SimpleLogger.Debug( $"iTXt Chunk - Text: {text}" );

            pngData.TextChunks = [ .. pngData.TextChunks, ( new TextMetadata( keyword, text, languageTag, translatedKeyword ) ) ];
        }
        else if ( chunk.Type == PngChunkType.tEXt || chunk.Type == PngChunkType.zTXt )
        {
            // Handle tEXt and zTXt chunks
            // tEXt chunk format:
            // - Keyword (null-terminated)
            // - Text (remaining bytes)
            // zTXt chunk format:
            // - Keyword (null-terminated)
            // - Compression method (1 byte)
            // - Compressed text (remaining bytes)

            int index = 0;
            // Read keyword
            int firstNull = Array.IndexOf( chunk.Data, ( byte ) 0, index );
            if ( firstNull == -1 )
            {
                string message = "Invalid text chunk: missing null terminator for keyword.";
                SimpleLogger.Error( message );
                if ( options.StopAtFirstError )
                    throw new InvalidPngException( message );
                return;
            }

            string keyword = System.Text.Encoding.Latin1.GetString( chunk.Data, index, firstNull - index );
            index = firstNull + 1;
            SimpleLogger.Debug( $"Text Chunk - Keyword: {keyword}" );

            string text;
            if ( chunk.Type == PngChunkType.tEXt )
            {
                // Read text
                byte[] textData = new byte[ chunk.Length - index ];
                Array.Copy( chunk.Data, index, textData, 0, textData.Length );
                text = System.Text.Encoding.Latin1.GetString( textData );
            }
            else
            {
                // zTXt chunk
                // Read compression method
                byte compressionMethod = chunk.Data[ index ];
                index++;

                // Read compressed text
                byte[] compressedTextData = new byte[ chunk.Length - index ];
                Array.Copy( chunk.Data, index, compressedTextData, 0, compressedTextData.Length );

                // Decompress text data
                using MemoryStream compressedStream = new( compressedTextData );
                using DeflateStream deflateStream = new( compressedStream, CompressionMode.Decompress );
                using StreamReader reader = new( deflateStream, System.Text.Encoding.Latin1 );
                text = reader.ReadToEnd();
            }

            SimpleLogger.Debug( $"Text Chunk - Text: {text}" );

            pngData.TextChunks = [ .. pngData.TextChunks, ( new TextMetadata( keyword, text ) ) ];
        }
    }

    private static void ParseTimeChunk( PngChunk chunk, ref PngData pngData, PngParserOptions options )
    {
        if ( chunk.Data.Length != 7 )
        {
            string message = "Invalid tIME chunk length. Must be 7 bytes.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngException( message );

            if ( chunk.Data.Length < 7 )
                return; // Can't parse incomplete time data
        }

        pngData.LastModified = new DateTime(
            year: ( ushort ) ( ( chunk.Data[ 0 ] << 8 ) | chunk.Data[ 1 ] ),
            month: chunk.Data[ 2 ],
            day: chunk.Data[ 3 ],
            hour: chunk.Data[ 4 ],
            minute: chunk.Data[ 5 ],
            second: chunk.Data[ 6 ],
            kind: DateTimeKind.Utc
        );

        SimpleLogger.Debug( $"tIME Chunk - Last Modified: {pngData.LastModified} (UTC)" );
    }

    private static void VerifyPngData( PngData? pngData, PngParserOptions options )
    {
        if ( pngData is null )
        {
            string message = "Parsed PNG data is null.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngException( message );

            return; // We can't do anything else if pngData is null
        }

        if ( pngData.ImageData == null || pngData.ImageData.Length < 1 )
        {
            string message = "Parsed PNG image data is empty.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngException( message );
        }

        if ( pngData.Chunks == null || pngData.Chunks.Count < 1 )
        {
            string message = "Parsed PNG chunks are empty.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngException( message );
        }

        if ( pngData.ColorType == 3 && ( pngData.Palette == null || pngData.Palette.Length < 1 ) ) // Indexed-color
        {
            string message = "PNG image uses indexed color but has no palette.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngException( message );
        }

        if ( pngData.Palette != null && ( pngData.ColorType == 0 || pngData.ColorType == 4 ) ) // Palette exists but color type does not allow it
        {
            string message = "PNG image has a palette but uses a color type that does not support palettes.";
            SimpleLogger.Error( message );
            if ( options.StopAtFirstError )
                throw new InvalidPngException( message );
        }
    }

    /// <summary>ArgumentOutOfRangeException
    /// Updates the CRC value for the given data.
    /// </summary>
    /// <param name="crc">The existing CRC value.</param>
    /// <param name="data">The data to update the CRC with.</param>
    /// <returns>The updated CRC value.</returns>
    public static uint UpdateCrc( uint crc, byte[] data )
    {
        uint c = crc;
        for ( int i = 0; i < data.Length; i++ )
        {
            c = CrcTable[ ( c ^ data[ i ] ) & 0xFF ] ^ ( c >> 8 );
        }
        return c;
    }

    /// <summary>
    /// Calculates the CRC for some given data.
    /// </summary>
    /// <param name="data">The data to calculate the CRC for.</param>
    /// <returns>The calculated CRC value.</returns>
    public static uint CalculateCrc( byte[] data )
    {
        return UpdateCrc( 0xFFFFFFFF, data ) ^ 0xFFFFFFFF;
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
        /// Whether to ignore unknown ancillary chunks.
        /// It should be safe to ignore unknown ancillary chunks since they're not necessary to decode the image, but eh, more configuration options are nice.
        /// </summary>
        /// <value>Ignore chunks that do not matter</value>
        public bool IgnoreUnknownAncillaryChunks { get; set; } = true; // If an unknown ancillary chunk is found, ignore it
        /// <summary>
        /// Whether to ignore unknown critical chunks.
        /// Critical chunks are necessary to decode the image, so ignoring them might lead to incomplete or invalid data.
        /// However, I like to get all information from the file, so this option is provided.
        /// </summary>
        /// <value>Throw an error on unknown critical chunks</value>
        public bool IgnoreUnknownCriticalChunks { get; set; } = false; // If an unknown critical chunk type is found, throw an error
        /// <summary>
        /// Whether to stop parsing at the first error.
        /// If an error is found, we might want to stop all parsing. <br/>
        /// If you only want valid data, this should be true.
        /// If you want to get all possible data, this should be false.
        /// </summary>
        /// <value>Stop parsing immediately if data is invalid</value>
        public bool StopAtFirstError { get; set; } = false; // If an error is found, stop parsing immediately

        public bool StopAtIEND { get; set; } = true; // Stop parsing when IEND chunk is found
    }
}

public class InvalidPngException( string message ) : Exception( message )
{ }

public class CrcMismatchException( string message ) : InvalidPngException( message )
{ }

public class InvalidPngSignatureException( string message ) : InvalidPngException( message )
{ }

public class InvalidPngHeaderException( string message ) : InvalidPngException( message )
{ }

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

    public Color[,] ImageData { get; set; } = new Color[ 0, 0 ]; // [ width, height ] (see: (x, y) coordinate system)
    public byte BitDepth { get; set; }
    public byte ColorType { get; set; }
    public byte CompressionMethod { get; set; }
    public byte FilterMethod { get; set; }
    public byte InterlaceMethod { get; set; }
    public Color[]? Palette { get; set; }
    public TextMetadata[] TextChunks { get; set; } = [];
    public DateTime LastModified { get; set; } = DateTime.MinValue;

    public bool ParsedPalette => Palette != null && Palette.Length > 0;
    public bool ParsedImageData => ImageData != null && ImageData.Length > 0;
}

public struct Color( ushort r, ushort g, ushort b, ushort a = 255 )
{
    public ushort R = r;
    public ushort G = g;
    public ushort B = b;
    public ushort A = a;
}

public struct TextMetadata( string keyword, string text, string languageTag = "", string translatedKeyword = "" )
{
    public string Keyword = keyword;
    public string Text = text;
    public string? LanguageTag = languageTag;
    public string? TranslatedKeyword = translatedKeyword;
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
    public PngChunkType Type;
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

    public static readonly PngChunk Empty = new()
    {
        Length = 0,
        Type = 0,
        Data = [],
        Crc = 0
    };

    public static readonly PngChunk IEND = new()
    {
        Length = 0,
        Type = "IEND".ToUint32(),
        Data = [],
        Crc = PngParser.CalculateCrc( BitConverter.GetBytes( "IEND".ToUint32() ) )
    };
}

/// <summary>
/// The type of a PNG chunk.
/// </summary>
public readonly struct PngChunkType
{
    /// <summary>
    /// The ASCII representation of the chunk type.
    /// </summary>
    /// <value>The ASCII name of the chunk type.</value>
    /// <exception cref="ArgumentException" />
    public readonly string AsciiName
    {
        get
        {
            var bytes = BitConverter.GetBytes( NameAsUint );
            if ( BitConverter.IsLittleEndian )
            {
                Array.Reverse( bytes );
            }
            return System.Text.Encoding.ASCII.GetString( bytes );
        }
    }
    public uint NameAsUint { get; }

    public PngChunkType( uint nameAsUint )
    {
        NameAsUint = nameAsUint;
    }

    public static implicit operator uint( PngChunkType chunkType ) => chunkType.NameAsUint;
    public static implicit operator PngChunkType( uint nameAsUint ) => new( nameAsUint );
    public static implicit operator string( PngChunkType chunkType ) => chunkType.AsciiName;
    public static implicit operator PngChunkType( string asciiName ) => new( asciiName.ToUint32() );
    public override string ToString() => AsciiName;

    public const uint IHDR = 0x49484452; // "IHDR"
    public const uint PLTE = 0x504C5445; // "PLTE"
    public const uint IDAT = 0x49444154; // "IDAT"
    public const uint IEND = 0x49454E44; // "IEND"
    public const uint tRNS = 0x74524E53; // "tRNS"
    public const uint gAMA = 0x67414D41; // "gAMA"
    public const uint cHRM = 0x6348524D; // "cHRM"
    public const uint sRGB = 0x73524742; // "sRGB"
    public const uint iCCP = 0x69434350; // "iCCP"
    public const uint iTXt = 0x69545874; // "iTXt"
    public const uint tEXt = 0x74455874; // "tEXt"
    public const uint zTXt = 0x7A545874; // "zTXt"
    public const uint bKGD = 0x624B4744; // "bKGD"
    public const uint pHYs = 0x70485973; // "pHYs"
    public const uint sBIT = 0x73424954; // "sBIT"
    public const uint sPLT = 0x73504C54; // "sPLT"
    public const uint hIST = 0x68495354; // "hIST"
    public const uint tIME = 0x74494D45; // "tIME"
}

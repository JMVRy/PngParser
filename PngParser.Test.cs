using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace Testing;

[TestFixture]
[ExcludeFromCodeCoverage]
public class PngParserTest
{
    [Test]
    public void ValidPng()
    {
        byte[] validPng = [ 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a ];
        using MemoryStream memoryStream = new( validPng );
        Assert.Multiple( () =>
        {
            Assert.DoesNotThrow( () => PngParser.PngParser.Parse( memoryStream, new PngParser.PngParser.PngParserOptions() { StopAtFirstError = true } ) );

            memoryStream.Seek( 0, SeekOrigin.Begin ); // We need to reset the stream position before parsing again

            Assert.That( () => PngParser.PngParser.Parse( memoryStream, new PngParser.PngParser.PngParserOptions() { StopAtFirstError = true } ), Is.TypeOf<PngParser.PngData>() );
        } );
    }

    [Test]
    public void ShortPng()
    {
        Assert.Multiple( () =>
        {
            Assert.Throws<EndOfStreamException>( BadCode );

            Assert.That( BadCode, Throws.Exception.With.Message.EqualTo( "Unable to read beyond the end of the stream." ) );
        } );

        static void BadCode()
        {
            byte[] invalidPng = [ 0x00, 0x11, 0x22, 0x33 ];
            using MemoryStream memoryStream = new( invalidPng );
            PngParser.PngParser.Parse( memoryStream );
        }
    }

    [Test]
    public void InvalidPngSignature()
    {
        Assert.Multiple( () =>
        {
            Assert.Throws<PngParser.InvalidPngSignatureException>( BadCode );

            Assert.That( BadCode, Throws.Exception.With.Message.EqualTo( "PNG file signature does not match expected signature" ) );
        } );

        static void BadCode()
        {
            byte[] invalidPng = [ 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 ];
            using MemoryStream pngStream = new( invalidPng );
            PngParser.PngParser.Parse( pngStream, new PngParser.PngParser.PngParserOptions() { StopAtFirstError = true } );
        }
    }

    [Test]
    public void EmptyPng()
    {
        Assert.Multiple( () =>
        {
            Assert.Throws<EndOfStreamException>( BadCode );

            Assert.That( BadCode, Throws.Exception.With.Message.EqualTo( "Unable to read beyond the end of the stream." ) );
        } );

        static void BadCode()
        {
            byte[] emptyPng = [];
            using MemoryStream pngStream = new( emptyPng );
            PngParser.PngParser.Parse( pngStream, new PngParser.PngParser.PngParserOptions() { StopAtFirstError = true } );
        }
    }
}

[TestFixture]
[ExcludeFromCodeCoverage]
public class SimpleLoggerTest
{
    private StringWriter _consoleStdout;
    private StringWriter _consoleStderr;

    [SetUp]
    public void Setup()
    {
        _consoleStdout = new StringWriter();
        _consoleStderr = new StringWriter();
        Console.SetOut( _consoleStdout );
        Console.SetError( _consoleStderr );
    }

    [TearDown]
    public void Teardown()
    {
        _consoleStdout.Dispose();
        _consoleStderr.Dispose();
    }

    [Test]
    public void LogInfoMessage()
    {
        Assert.Multiple( () =>
        {
            Assert.DoesNotThrow( () => Logging.SimpleLogger.Info( "This is an info message." ) );
            Assert.That( _consoleStdout.ToString(), Does.Contain( "[INFO] This is an info message." ) );
            Assert.That( _consoleStderr.ToString(), Is.Empty );
        } );

    }

    [Test]
    public void LogWarningMessage()
    {
        Assert.Multiple( () =>
        {
            Assert.DoesNotThrow( () => Logging.SimpleLogger.Warning( "This is a warning message." ) );
            Assert.That( _consoleStderr.ToString(), Does.Contain( "[WARNING] This is a warning message." ) );
            Assert.That( _consoleStdout.ToString(), Is.Empty );
        } );

    }

    [Test]
    public void LogErrorMessage()
    {
        Assert.Multiple( () =>
        {
            Assert.DoesNotThrow( () => Logging.SimpleLogger.Error( "This is an error message." ) );
            Assert.That( _consoleStderr.ToString(), Does.Contain( "[ERROR] This is an error message." ) );
            Assert.That( _consoleStdout.ToString(), Is.Empty );
        } );

    }

    [Test]
    public void LogDebugMessageWhenEnabled()
    {
        Logging.SimpleLogger.IsDebugEnabled = true;

        Assert.Multiple( () =>
        {
            Assert.DoesNotThrow( () => Logging.SimpleLogger.Debug( "This is a debug message." ) );
            Assert.That( _consoleStdout.ToString(), Does.Contain( "[DEBUG] This is a debug message." ) );
            Assert.That( _consoleStderr.ToString(), Is.Empty );
        } );
    }
}

[TestFixture]
[ExcludeFromCodeCoverage]
public class UtilsTest
{
    [Test]
    public void ToUint32_ValidString_ReturnsCorrectValue()
    {
        string input = "ABCD";
        uint expected = 0x41424344; // ASCII values: A=65, B=66, C=67, D=68

        uint result = Utils.ToUint32( input );

        Assert.That( result, Is.EqualTo( expected ) );
    }

    [Test]
    public void ToUint32_InvalidString_ThrowsArgumentException()
    {
        string input = "ABC"; // Invalid length

        Assert.Throws<ArgumentException>( () => Utils.ToUint32( input ) );
    }
}

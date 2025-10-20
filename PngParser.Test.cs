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
            Assert.DoesNotThrow( () => PngParser.PngParser.Parse( memoryStream ) );

            Assert.That( () => PngParser.PngParser.Parse( memoryStream ), Is.TypeOf<PngParser.PngData>() );
        } );
    }

    [Test]
    public void ShortPng()
    {
        Assert.Multiple( () =>
        {
            Assert.Throws<Exception>( BadCode );

            Assert.That( BadCode, Throws.Exception.With.Message.EqualTo( "PNG file too short" ) );
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
            Assert.Throws<Exception>( BadCode );

            Assert.That( BadCode, Throws.Exception.With.Message.EqualTo( "Invalid PNG signature" ) );
        } );

        static void BadCode()
        {
            byte[] invalidPng = [ 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 ];
            using MemoryStream pngStream = new( invalidPng );
            PngParser.PngParser.Parse( pngStream );
        }
    }

    [Test]
    public void EmptyPng()
    {
        Assert.Multiple( () =>
        {
            Assert.Throws<Exception>( BadCode );

            Assert.That( BadCode, Throws.Exception.With.Message.EqualTo( "PNG file too short" ) );
        } );

        static void BadCode()
        {
            byte[] emptyPng = [];
            using MemoryStream pngStream = new( emptyPng );
            PngParser.PngParser.Parse( pngStream );
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

    [Test]
    public void LoggerDoesNotLogDebugByDefault()
    {
        Assert.Multiple( () =>
        {
            Assert.That( Logging.SimpleLogger.IsDebugEnabled, Is.False );
            Assert.DoesNotThrow( () => Logging.SimpleLogger.Debug( "This debug message should not appear." ) );
            Assert.That( _consoleStdout.ToString(), Is.Empty );
            Assert.That( _consoleStderr.ToString(), Is.Empty );
        } );

    }
}

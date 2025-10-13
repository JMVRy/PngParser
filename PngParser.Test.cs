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
        Assert.Multiple( () =>
        {
            Assert.DoesNotThrow( () => PngParser.PngParser.Parse( validPng ) );

            Assert.That( () => PngParser.PngParser.Parse( validPng ), Is.TypeOf<PngParser.PngData>() );
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
            PngParser.PngParser.Parse( invalidPng );
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
            PngParser.PngParser.Parse( invalidPng );
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
            PngParser.PngParser.Parse( emptyPng );
        }
    }
}

[TestFixture]
[ExcludeFromCodeCoverage]
public class SimpleLoggerTest
{
    private Logging.SimpleLogger _logger;
    private StringWriter _consoleStdout;
    private StringWriter _consoleStderr;

    [SetUp]
    public void Setup()
    {
        _logger = new Logging.SimpleLogger();
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
            Assert.DoesNotThrow( () => _logger.Info( "This is an info message." ) );
            Assert.That( _consoleStdout.ToString(), Does.Contain( "[INFO] This is an info message." ) );
            Assert.That( _consoleStderr.ToString(), Is.Empty );
        } );

    }

    [Test]
    public void LogWarningMessage()
    {
        Assert.Multiple( () =>
        {
            Assert.DoesNotThrow( () => _logger.Warning( "This is a warning message." ) );
            Assert.That( _consoleStderr.ToString(), Does.Contain( "[WARNING] This is a warning message." ) );
            Assert.That( _consoleStdout.ToString(), Is.Empty );
        } );

    }

    [Test]
    public void LogErrorMessage()
    {
        Assert.Multiple( () =>
        {
            Assert.DoesNotThrow( () => _logger.Error( "This is an error message." ) );
            Assert.That( _consoleStderr.ToString(), Does.Contain( "[ERROR] This is an error message." ) );
            Assert.That( _consoleStdout.ToString(), Is.Empty );
        } );

    }

    [Test]
    public void LogDebugMessageWhenEnabled()
    {
        _logger.IsDebugEnabled = true;

        Assert.Multiple( () =>
        {
            Assert.DoesNotThrow( () => _logger.Debug( "This is a debug message." ) );
            Assert.That( _consoleStdout.ToString(), Does.Contain( "[DEBUG] This is a debug message." ) );
            Assert.That( _consoleStderr.ToString(), Is.Empty );
        } );

    }

    [Test]
    public void LoggerDoesNotLogDebugByDefault()
    {
        Assert.Multiple( () =>
        {
            Assert.That( _logger.IsDebugEnabled, Is.False );
            Assert.DoesNotThrow( () => _logger.Debug( "This debug message should not appear." ) );
            Assert.That( _consoleStdout.ToString(), Is.Empty );
            Assert.That( _consoleStderr.ToString(), Is.Empty );
        } );

    }
}

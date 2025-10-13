using NUnit.Framework;

namespace Testing;

[TestFixture]
public class PngParserTest
{
    [Test]
    public void TestValidPng()
    {
        Assert.DoesNotThrow( () =>
        {
            byte[] validPng = [ 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a ];
            PngParser.PngParser.Parse( validPng );
        } );
    }

    [Test]
    public void TestInvalidPngHeader()
    {
        Assert.Throws<Exception>( () =>
        {
            byte[] invalidPng = [ 0x00, 0x11, 0x22, 0x33 ];
            PngParser.PngParser.Parse( invalidPng );
        } );
    }

    [Test]
    public void TestEmptyPng()
    {
        Assert.Throws<Exception>( () =>
        {
            byte[] emptyPng = [];
            PngParser.PngParser.Parse( emptyPng );
        } );
    }
}

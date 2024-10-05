using System.Text;
using System.Text.Json;

namespace PlexApi.UnitTests.Converters;

public class StringToBool : BaseUnitTest
{
    public StringToBool(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData("{\"x\": \"1\"}", true)]
    [InlineData("{\"x\": \"0\"}", false)]
    [InlineData("{\"x\": \"rubbish\"}", false)]
    public void ShouldConvertToTrue_WhenStringOfOne(string json, bool expected)
    {
        // Arrange
        var sut = new PlexRipper.PlexApi.Converters.StringToBool();
        var utf8JsonReader = new Utf8JsonReader(
            Encoding.UTF8.GetBytes(json),
            false,
            new JsonReaderState(new JsonReaderOptions())
        );
        while (utf8JsonReader.Read())
        {
            if (utf8JsonReader.TokenType == JsonTokenType.String)
                break;
        }

        // Act
        var deserializedBoolean = sut.Read(ref utf8JsonReader, typeof(string), new JsonSerializerOptions());

        // Assert
        deserializedBoolean.ShouldBe(expected);
    }
}

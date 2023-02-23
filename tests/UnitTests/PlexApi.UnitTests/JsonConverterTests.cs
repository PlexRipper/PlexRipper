using System.Text.Json;

namespace PlexApi.UnitTests;

public class JsonConverterTests : BaseUnitTest
{
    public JsonConverterTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ShouldReturnValidLongFromJsonString_WhenJsonStringIsValidLong()
    {
        // Arrange
        var jsonString = "{\"contentChangedAt\" : \"2247395385730901353\"}";

        // Act
        var longValue = JsonSerializer.Deserialize<TestDTO>(jsonString);

        // Assert
        longValue.ShouldNotBeNull();
        longValue.ContentChangedAt.ShouldBeGreaterThanOrEqualTo(long.MinValue);
        longValue.ContentChangedAt.ShouldBeLessThanOrEqualTo(long.MaxValue);
    }
}
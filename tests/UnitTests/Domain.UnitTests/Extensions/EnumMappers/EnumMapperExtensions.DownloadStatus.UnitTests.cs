namespace Reaparr.Domain.UnitTests;

public class EnumMapperExtensionsDownloadStatusUnitTests : BaseUnitTest
{
    [Test]
    public void ShouldMapAllDownloadStatusEnumValues_ToStringAndBack()
    {
        foreach (var status in Enum.GetValues<DownloadStatus>())
        {
            var statusString = status.ToDownloadStatusString();

            statusString.ShouldBe(
                status.ToString(),
                $"Missing {status.ToString()} from {nameof(EnumMapperExtensions)}"
            );

            var parsedStatus = statusString.ToDownloadStatus();
            parsedStatus.ShouldBe(status);
        }
    }

    [Test]
    public void ShouldMapAllDownloadStatusNames_ToEnumAndBack()
    {
        foreach (var name in Enum.GetNames<DownloadStatus>())
        {
            var parsedStatus = name.ToDownloadStatus();
            parsedStatus.ToString().ShouldBe(name, $"Missing {name} from {nameof(EnumMapperExtensions)}");

            var mappedBackToString = parsedStatus.ToDownloadStatusString();
            mappedBackToString.ShouldBe(name);
        }
    }
}

namespace Domain.UnitTests;

public class StringExtensions_GetFileName : BaseUnitTest
{
    public StringExtensions_GetFileName(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldRetrieveTheFileName_WhenThePathIsValid()
    {
        // Arrange
        var testPathList = new List<string>
        {
            @"C:\Users\User\Downloads\test.mp4",
            @"M:\movies\Bad Boys Ride or Die (2024)\Bad.Boys.Ride.Or.Die.2024.HKG.1080p.BluRay.REMUX.AVC.DTS-HD.MA.5.1-IONICBOY.mkv",
            "/media/Multimedia/Peliculas/Bad Boys. Ride or Die (2024) [WEB-DL AMZN 1080p AVC ES DD+ 5.1 Subs].mkv",
            "some/path/to/file.mp4",
            "movie.mp4",
        };
        var resultList = new List<string>
        {
            "test.mp4",
            "Bad.Boys.Ride.Or.Die.2024.HKG.1080p.BluRay.REMUX.AVC.DTS-HD.MA.5.1-IONICBOY.mkv",
            "Bad Boys. Ride or Die (2024) [WEB-DL AMZN 1080p AVC ES DD+ 5.1 Subs].mkv",
            "file.mp4",
            "movie.mp4",
        };

        // Act / Assert
        for (var i = 0; i < testPathList.Count; i++)
        {
            var testPath = testPathList[i];
            var result = testPath.GetFileName();
            result.ShouldBe(resultList[i]);
        }
    }
}

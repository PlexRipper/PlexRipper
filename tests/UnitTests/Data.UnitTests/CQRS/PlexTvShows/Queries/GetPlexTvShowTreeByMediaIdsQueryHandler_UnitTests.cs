using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Logging;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.BaseTests;
using PlexRipper.Data.CQRS.PlexTvShows;
using PlexRipper.Data.PlexTvShows;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Data.UnitTests
{
    public class GetPlexTvShowTreeByMediaIdsQueryHandler_UnitTests
    {
        public GetPlexTvShowTreeByMediaIdsQueryHandler_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldReturnAllTvShowSeasonAndEpisodeIds_WhenAllAreGiven()
        {
            // Arrange
            await using var context = MockDatabase.GetMemoryDbContext().AddPlexServers().AddMedia();
            var handle = new GetPlexTvShowTreeByMediaIdsQueryHandler(context);

            List<int> tvShowIds = new List<int> { 1, 2, 4, 5 };
            List<int> tvShowSeasonIds = new List<int> { 4, 5, 9, 4 };
            List<int> tvShowEpisodeIds = new List<int> { 67, 87, 95, 125 };

            var request = new GetPlexTvShowTreeByMediaIdsQuery(tvShowIds, tvShowSeasonIds, tvShowEpisodeIds);

            // Act
            var result = await handle.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var value = result.Value;
            value.ShouldNotBeEmpty();
            value.ShouldContain(x => tvShowIds.Contains(x.Id));
        }

        [Fact]
        public async Task ShouldOnlyReturnThoseTvShows_WhichHaveBeenGiven()
        {
            // Arrange
            await using var context = MockDatabase.GetMemoryDbContext().AddPlexServers().AddMedia();
            var handle = new GetPlexTvShowTreeByMediaIdsQueryHandler(context);

            List<int> tvShowIds = new List<int> { 1, 2, 4, 5 };
            List<int> tvShowSeasonIds = new List<int>();
            List<int> tvShowEpisodeIds = new List<int>();
            var request = new GetPlexTvShowTreeByMediaIdsQuery(tvShowIds, tvShowSeasonIds, tvShowEpisodeIds);

            // Act
            var result = await handle.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var value = result.Value;
            value.ShouldNotBeEmpty();
            value.Count.ShouldBe(tvShowIds.Count);
            value.ShouldContain(x => tvShowIds.Contains(x.Id));
            foreach (var season in value.SelectMany(x => x.Seasons))
            {
                season.Episodes.ShouldNotBeEmpty();
                foreach (var episode in season.Episodes)
                {
                    episode.TvShow.ShouldNotBeNull();
                    episode.TvShowSeason.ShouldNotBeNull();
                }
            }
        }
    }
}
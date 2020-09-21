using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Moq;
using PlexRipper.Application.PlexMovies;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Fixtures;
using PlexRipper.Data;
using PlexRipper.Data.Commands.PlexMovies;
using PlexRipper.Domain;
using Xunit;
using Xunit.Abstractions;

namespace Data.UnitTests
{
    public class PlexMoviesCommandTests
    {
        private BaseContainer Container { get; }

        public PlexMoviesCommandTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = fixture.Container;
        }

        private List<PlexMovie> GetFakePlexMovies()
        {

        }

        [Fact]
        public async Task CreateOrUpdatePlexMoviesCommand_CreateMovies()
        {
            // Arrange
            var mediator = new Mock<IMediator>();

            var command = new CreateOrUpdatePlexMoviesCommand();
            var handler = new CreateOrUpdatePlexMoviesHandler(Container.PlexRipperDbContext);

            // Act
            var x = await handler.Handle(command, System.Threading.CancellationToken.None);

            //Asert
            //Do the assertion

            //something like:
            mediator.Verify(x => x.Publish(It.IsAny<CustomersChanged>()));
        }
    }
}
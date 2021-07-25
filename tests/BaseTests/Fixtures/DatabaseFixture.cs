namespace PlexRipper.BaseTests
{
    public class DatabaseFixture
    {
        public BaseContainer Container { get; }

        public DatabaseFixture()
        {
            Container = new BaseContainer();
        }
    }
}
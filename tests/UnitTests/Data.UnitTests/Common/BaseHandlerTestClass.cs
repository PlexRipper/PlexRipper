using PlexRipper.Data;
using PlexRipper.Data.Common;

namespace Data.UnitTests.Common
{
    /// <summary>
    /// Created to test the <see cref="BaseHandler"/> class without having to change the access of the protected constructor.
    /// </summary>
    public class BaseHandlerTestClass : BaseHandler
    {
        public BaseHandlerTestClass(PlexRipperDbContext dbContext) : base(dbContext) { }
    }
}
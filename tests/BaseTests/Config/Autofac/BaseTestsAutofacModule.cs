using Autofac;

namespace PlexRipper.BaseTests.Config
{
    public class BaseTestsAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MockServer>().As<IMockServer>().SingleInstance();
            builder.RegisterType<FakeData>().As<IFakeData>().SingleInstance();
        }
    }
}
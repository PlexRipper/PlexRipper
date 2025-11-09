using System.Reflection;
using Autofac;
using Autofac.Core;

namespace Reaparr.BaseTests;

public static class AutoMockOverrides
{
    /// <summary>
    /// Registers all mocks that are currently present in the provided AutoMock container
    /// into the target ContainerBuilder as their underlying service types.
    /// Only explicitly created mocks (Mock&lt;T&gt;) are considered.
    /// </summary>
    public static void RegisterAllAutoMocks(this ContainerBuilder builder, AutoMock autoMock)
    {
        var registrations = autoMock.Container.ComponentRegistry.Registrations.ToList();

        foreach (var registration in registrations)
        {
            foreach (var service in registration.Services.OfType<TypedService>())
            {
                var serviceType = service.ServiceType;
                if (!serviceType.IsGenericType)
                    continue;

                var genericDef = serviceType.GetGenericTypeDefinition();
                if (genericDef != typeof(Mock<>))
                    continue;

                var targetType = serviceType.GenericTypeArguments[0];

                // Resolve Mock<T> instance from the AutoMock container
                var mockObj = autoMock.Container.Resolve(serviceType);

                // Read Mock<T>.Object using reflection
                var objectProp = mockObj.GetType().GetProperty("Object", BindingFlags.Instance | BindingFlags.Public);
                if (objectProp is null)
                    continue;

                var impl = objectProp.GetValue(mockObj);
                if (impl is null)
                    continue;

                // Register the mock implementation as the target service type
                builder.RegisterInstance(impl).As(targetType).SingleInstance();
            }
        }
    }
}

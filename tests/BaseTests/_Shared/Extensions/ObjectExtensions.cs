using System.Reflection;

namespace Reaparr.BaseTests;

/// <summary>
/// Extension methods for working with objects, particularly for updating init-only properties.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Useful for updating private, protected or init properties on an object.
    /// </summary>
    /// <param name="obj">The object to update.</param>
    /// <param name="propertyName">The name of the property to update.</param>
    /// <param name="newValue">The new value to set.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown when the property is not found or cannot be written to.</exception>
    public static void UpdateInitProperty<T>(this T obj, string propertyName, object newValue)
    {
        var property = obj
            ?.GetType()
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property == null || !property.CanWrite)
        {
            throw new InvalidOperationException($"Property '{propertyName}' not found or cannot be written to.");
        }

        property.SetValue(obj, newValue);
    }
}

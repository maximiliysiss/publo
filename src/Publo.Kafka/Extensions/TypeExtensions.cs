using System;

namespace Publo.Kafka.Extensions;

internal static class TypeExtensions
{
    private static string GetVersionFreeFullName(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentException.ThrowIfNullOrEmpty(type.FullName);

        var assemblyName = type.Assembly.GetName();

        ArgumentException.ThrowIfNullOrEmpty(assemblyName.Name);

        return $"{type.FullName}, {assemblyName.Name}";
    }

    public static string GetMessageKey(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentException.ThrowIfNullOrEmpty(type.FullName);

        return $"{Guid.NewGuid():N}:{type.GetVersionFreeFullName()}";
    }

    public static Type? GetMessageType(this string key)
    {
        var typeName = key.Split(":")[1];
        return Type.GetType(typeName);
    }
}

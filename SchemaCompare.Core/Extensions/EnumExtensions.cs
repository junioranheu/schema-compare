using System.ComponentModel;
using System.Reflection;

namespace SchemaCompare.Core.Extensions;

/// <summary>
/// Provides extensions for enums.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the description of an enum value using the Description attribute.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <returns>The description, or the enum value name if no description is found.</returns>
    public static string GetDescription(this Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());
        DescriptionAttribute? attribute = field?.GetCustomAttribute<DescriptionAttribute>();

        return attribute?.Description ?? value.ToString();
    }
}
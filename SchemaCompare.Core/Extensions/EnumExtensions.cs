using System.ComponentModel;
using System.Reflection;

namespace SchemaCompare.Core.Extensions;

/// <summary>
/// Extensões para enums
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Obtém a descrição de um valor de enum usando o atributo Description
    /// </summary>
    /// <param name="value">Valor do enum</param>
    /// <returns>Descrição ou string vazia se não encontrada</returns>
    public static string GetDescription(this Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());
        DescriptionAttribute? attribute = field?.GetCustomAttribute<DescriptionAttribute>();

        return attribute?.Description ?? value.ToString();
    }
}
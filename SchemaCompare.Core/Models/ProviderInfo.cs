using SchemaCompare.Core.Enums;

namespace SchemaCompare.Core.Models;

/// <summary>
/// Modelo que contém informações sobre um provider de banco de dados
/// </summary>
public class ProviderInfo
{
    /// <summary>Tipo do provider</summary>
    public required ProviderTypeEnum ProviderType { get; init; }

    /// <summary>Nome amigável do provider</summary>
    public required string DisplayName { get; init; }

    /// <summary>Status de testes do provider</summary>
    public required TestingStatusEnum TestingStatus { get; init; }
}
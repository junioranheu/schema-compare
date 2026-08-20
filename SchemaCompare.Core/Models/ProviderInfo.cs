using SchemaCompare.Core.Enums;

namespace SchemaCompare.Core.Models;

/// <summary>
/// Model containing information about a database provider.
/// </summary>
public class ProviderInfo
{
    public required ProviderTypeEnum ProviderType { get; init; }

    public required string DisplayName { get; init; }
    public required TestingStatusEnum TestingStatus { get; init; }
    public required DateOnly? RealTestingDate { get; init; } // Date when the provider was last tested against a real-world schema.
    public required bool HasUnitTests { get; init; }
}
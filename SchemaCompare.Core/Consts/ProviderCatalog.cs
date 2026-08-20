using SchemaCompare.Core.Enums;
using SchemaCompare.Core.Extensions;
using SchemaCompare.Core.Models;

namespace SchemaCompare.Core.Consts;

/// <summary>
/// Catalog containing information about all available database providers.
/// Includes testing status and the date of the latest test for each provider.
/// </summary>
public sealed class ProviderCatalog
{
    /// <summary>
    /// Immutable list of all available database providers.
    /// </summary>
    public static readonly IReadOnlyList<ProviderInfo> AllProviders = new List<ProviderInfo>
    {
        new()
        {
            ProviderType = ProviderTypeEnum.PostgreSql,
            DisplayName = ProviderTypeEnum.PostgreSql.GetDescription(),
            TestingStatus = TestingStatusEnum.TestedWithSimpleTable,
            TestingDate = new DateOnly(2026, 8, 20)
        },
        new()
        {
            ProviderType = ProviderTypeEnum.SqlServer,
            DisplayName = ProviderTypeEnum.SqlServer.GetDescription(),
            TestingStatus = TestingStatusEnum.NotTested,
            TestingDate = null
        },
        new()
        {
            ProviderType = ProviderTypeEnum.MySql,
            DisplayName = ProviderTypeEnum.MySql.GetDescription(),
            TestingStatus = TestingStatusEnum.NotTested,
            TestingDate = null
        },
        new()
        {
            ProviderType = ProviderTypeEnum.Oracle,
            DisplayName = ProviderTypeEnum.Oracle.GetDescription(),
            TestingStatus = TestingStatusEnum.NotTested,
            TestingDate = null
        },
        new()
        {
            ProviderType = ProviderTypeEnum.Firebird,
            DisplayName = ProviderTypeEnum.Firebird.GetDescription(),
            TestingStatus = TestingStatusEnum.NotTested,
            TestingDate = null
        },
        new()
        {
            ProviderType = ProviderTypeEnum.MariaDb,
            DisplayName = ProviderTypeEnum.MariaDb.GetDescription(),
            TestingStatus = TestingStatusEnum.NotTested,
            TestingDate = null
        }
    }.AsReadOnly();

    /// <summary>
    /// Gets information about a specific provider.
    /// </summary>
    /// <param name="providerType">The provider type.</param>
    /// <returns>The provider information, or null if not found.</returns>
    public static ProviderInfo? GetProviderInfo(ProviderTypeEnum providerType)
    {
        return AllProviders.FirstOrDefault(x => x.ProviderType == providerType);
    }

    /// <summary>
    /// Gets all providers with a specific testing status.
    /// </summary>
    /// <param name="status">The desired testing status.</param>
    /// <returns>A list of providers with the specified status.</returns>
    public static IEnumerable<ProviderInfo> GetProvidersByTestingStatus(TestingStatusEnum status)
    {
        return AllProviders.Where(x => x.TestingStatus == status);
    }
}
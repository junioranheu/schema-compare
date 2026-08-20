using SchemaCompare.Core.Enums;
using SchemaCompare.Core.Extensions;
using SchemaCompare.Core.Models;

namespace SchemaCompare.Core.Consts;

/// <summary>
/// Catálogo com a lista de todos os provedores de banco de dados disponíveis
/// Contém informações sobre cada provider incluindo status de testes e data do último teste
/// </summary>
public sealed class ProviderCatalog
{
    /// <summary>
    /// Lista imutável com todos os provedores disponíveis
    /// </summary>
    public static readonly IReadOnlyList<ProviderInfo> AllProviders = new List<ProviderInfo>
    {
        new()
        {
            ProviderType = ProviderTypeEnum.PostgreSql,
            DisplayName = ProviderTypeEnum.PostgreSql.GetDescription(),
            TestingStatus = TestingStatusEnum.TestedWithSimpleTable
        },
        new()
        {
            ProviderType = ProviderTypeEnum.SqlServer,
            DisplayName = ProviderTypeEnum.SqlServer.GetDescription(),
            TestingStatus = TestingStatusEnum.NotTested
        },
        new()
        {
            ProviderType = ProviderTypeEnum.MySql,
            DisplayName = ProviderTypeEnum.MySql.GetDescription(),
            TestingStatus = TestingStatusEnum.NotTested
        },
        new()
        {
            ProviderType = ProviderTypeEnum.Oracle,
            DisplayName = ProviderTypeEnum.Oracle.GetDescription(),
            TestingStatus = TestingStatusEnum.NotTested
        },
        new()
        {
            ProviderType = ProviderTypeEnum.Firebird,
            DisplayName = ProviderTypeEnum.Firebird.GetDescription(),
            TestingStatus = TestingStatusEnum.NotTested
        },
        new()
        {
            ProviderType = ProviderTypeEnum.MariaDb,
            DisplayName = ProviderTypeEnum.MariaDb.GetDescription(),
            TestingStatus = TestingStatusEnum.NotTested
        }
    }.AsReadOnly();

    /// <summary>
    /// Obtém informações de um provider específico
    /// </summary>
    /// <param name="providerType">Tipo do provider</param>
    /// <returns>Informações do provider ou null se não encontrado</returns>
    public static ProviderInfo? GetProviderInfo(ProviderTypeEnum providerType)
    {
        return AllProviders.FirstOrDefault(x => x.ProviderType == providerType);
    }

    /// <summary>
    /// Obtém todos os provedores com um status específico de testes
    /// </summary>
    /// <param name="status">Status de teste desejado</param>
    /// <returns>Lista de provedores com o status especificado</returns>
    public static IEnumerable<ProviderInfo> GetProvidersByTestingStatus(TestingStatusEnum status)
    {
        return AllProviders.Where(x => x.TestingStatus == status);
    }
}
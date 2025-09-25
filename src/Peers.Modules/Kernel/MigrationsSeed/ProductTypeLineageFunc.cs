using Humanizer;
using Microsoft.EntityFrameworkCore.Migrations;
using Peers.Modules;
using Peers.Modules.Catalog.Domain;

namespace Peers.Modules.Migrations;

public partial class ProductTypeLineageFunc
{
    public const string Schema = "catalog";
    public const string Name = "ufn_ProductTypeLineage";
    public const string FullName = $"[{Schema}].[{Name}]";

    /// <inheritdoc />
    protected override void Up([NotNull] MigrationBuilder migrationBuilder) => migrationBuilder.Sql(UpSql());
    /// <inheritdoc />
    protected override void Down([NotNull] MigrationBuilder migrationBuilder) => migrationBuilder.Sql(DownSql());

    internal static string UpSql(bool checkExist = false)
    {
        var tbl = nameof(ProductType).Underscore();
        var id = nameof(Entity.Id).Underscore();
        var parentId = nameof(ProductType.ParentId).Underscore();
        var existsSql = checkExist ? "OR ALTER " : string.Empty;

        return
            $"""
            CREATE {existsSql}FUNCTION [{Schema}].[{Name}] (@startNodeId int)
            RETURNS TABLE
            AS
            RETURN
            WITH anc AS (
                -- Anchor: start node (Lvl 0)
                SELECT [{id}], [{parentId}], 0 AS [lvl]
                FROM [{Schema}].[{tbl}]
                WHERE [{id}] = @startNodeId
                UNION ALL
                -- Recurse upward (Lvl decreases)
                SELECT [t].[{id}], [t].[{parentId}], [a].[lvl] - 1
                FROM [{Schema}].[{tbl}] AS [t]
                JOIN anc AS [a] ON [t].[{id}] = [a].[{parentId}]
            ),
            des AS (
                -- Anchor: start node (Lvl 0)
                SELECT [{id}], [{parentId}], 0 AS [lvl]
                FROM [{Schema}].[{tbl}]
                WHERE [{id}] = @startNodeId
                UNION ALL
                -- Recurse downward (Lvl increases)
                SELECT [t].[{id}], [t].[{parentId}], [d].[lvl] + 1
                FROM [{Schema}].[{tbl}] AS [t]
                JOIN des AS [d] ON [t].[{parentId}] = [d].[{id}]
            )
            SELECT [{id}], [{parentId}], [lvl] FROM anc WHERE [lvl] < 0
            UNION ALL
            SELECT [{id}], [{parentId}], [lvl] FROM des WHERE [lvl] > 0
            UNION ALL
            SELECT [{id}], [{parentId}], 0 AS [lvl] FROM [{Schema}].[{tbl}] WHERE [{id}] = @startNodeId;
            """;
    }

    internal static string DownSql() => $"DROP FUNCTION [{Schema}].[{Name}];";
}

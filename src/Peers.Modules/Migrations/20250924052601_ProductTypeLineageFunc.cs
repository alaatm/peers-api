using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peers.Modules.Migrations
{
    /// <inheritdoc />
    public partial class ProductTypeLineageFunc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(UpSql());

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(DownSql());
    }
}

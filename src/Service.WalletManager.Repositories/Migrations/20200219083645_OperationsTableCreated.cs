using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Service.WalletManager.Repositories.Migrations
{
    public partial class OperationsTableCreated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "operations",
                schema: "wallet_manager",
                columns: table => new
                {
                    BlockchianId = table.Column<string>(nullable: false),
                    BlockchainAssetId = table.Column<string>(nullable: false),
                    WalletAddress = table.Column<string>(nullable: false),
                    OperationId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BlockNumber = table.Column<long>(nullable: false),
                    BalanceChange = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operations", x => new { x.BlockchianId, x.BlockchainAssetId, x.WalletAddress, x.OperationId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "operations",
                schema: "wallet_manager");
        }
    }
}

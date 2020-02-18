using Microsoft.EntityFrameworkCore.Migrations;

namespace Service.WalletManager.Repositories.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wallet_manager");

            migrationBuilder.CreateTable(
                name: "enrolled_balance",
                schema: "wallet_manager",
                columns: table => new
                {
                    BlockchianId = table.Column<string>(nullable: false),
                    BlockchainAssetId = table.Column<string>(nullable: false),
                    WalletAddress = table.Column<string>(nullable: false),
                    BlockNumber = table.Column<long>(nullable: false),
                    Balance = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrolled_balance", x => new { x.BlockchianId, x.BlockchainAssetId, x.WalletAddress });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enrolled_balance",
                schema: "wallet_manager");
        }
    }
}

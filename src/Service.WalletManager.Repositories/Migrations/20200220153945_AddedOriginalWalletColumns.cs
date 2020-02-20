using Microsoft.EntityFrameworkCore.Migrations;

namespace Service.WalletManager.Repositories.Migrations
{
    public partial class AddedOriginalWalletColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginalWalletAddress",
                schema: "wallet_manager",
                table: "operations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalWalletAddress",
                schema: "wallet_manager",
                table: "enrolled_balance",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalWalletAddress",
                schema: "wallet_manager",
                table: "operations");

            migrationBuilder.DropColumn(
                name: "OriginalWalletAddress",
                schema: "wallet_manager",
                table: "enrolled_balance");
        }
    }
}

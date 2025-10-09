using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Litee.Api.Migrations
{
    /// <inheritdoc />
    public partial class create_receipt_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceiptId",
                table: "Transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Base64Image = table.Column<string>(type: "text", nullable: false),
                    TransactionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReceiptId",
                table: "Transactions",
                column: "ReceiptId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Receipts_ReceiptId",
                table: "Transactions",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Receipts_ReceiptId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ReceiptId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ReceiptId",
                table: "Transactions");
        }
    }
}

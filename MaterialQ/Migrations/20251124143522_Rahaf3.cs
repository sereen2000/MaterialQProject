using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaterialQ.Migrations
{
    /// <inheritdoc />
    public partial class Rahaf3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Colors_ColorId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_ColorId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "Items");

            migrationBuilder.CreateTable(
                name: "ColorItem",
                columns: table => new
                {
                    ColorId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<float>(type: "real", nullable: false),
                    ItemsModelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorItem", x => new { x.ItemId, x.ColorId });
                    table.ForeignKey(
                        name: "FK_ColorItem_Colors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "Colors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ColorItem_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ColorItem_Items_ItemsModelId",
                        column: x => x.ItemsModelId,
                        principalTable: "Items",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ColorItem_ColorId",
                table: "ColorItem",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ColorItem_ItemsModelId",
                table: "ColorItem",
                column: "ItemsModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColorItem");

            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ColorId",
                table: "Items",
                column: "ColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Colors_ColorId",
                table: "Items",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

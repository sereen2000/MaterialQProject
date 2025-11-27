using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaterialQ.Migrations
{
    /// <inheritdoc />
    public partial class ResetSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
       name: "Vat",
       table: "QuotationItems",
       type: "decimal(18,2)",
       nullable: false,
       defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

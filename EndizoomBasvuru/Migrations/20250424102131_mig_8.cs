using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EndizoomBasvuru.Migrations
{
    /// <inheritdoc />
    public partial class mig_8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$1P2VXYzqZEZmGIgUWRieCerQ4ej.QLq.78Lx3msOcCNe64WS2DlQ.");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$kXypbWj/O1SvbKhgGFBx2uk.YFMF4FvoLYMWJTHcAp2mIylLDP/cq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$lRfU4OwfuKWNpEY0BGOGwOdhqI6wQIpZlJXz.2criJauV1MJ9gEJ6");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$OaxRTrgpEF5IevYIxoA3O.k7iNqJ8TKeiMprBH8Od01NNAWOV4W3y");
        }
    }
}

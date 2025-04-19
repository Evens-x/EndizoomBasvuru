using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EndizoomBasvuru.Migrations
{
    /// <inheritdoc />
    public partial class mig_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Commission",
                table: "Companies",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRate",
                table: "Companies",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Revenue",
                table: "Companies",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$.buZG8kt2cSCe.ctM3sd3emUTExBlYF0ZeeEUuNIid.zYiAeolLgS");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$R2aBNHC9D7WFO/m.WiukFeuec7ac3VmG7i/Z0f03G.ejp3AFqOu6i");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Commission",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CommissionRate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Revenue",
                table: "Companies");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$Fs93JEB5aM2xirDmpCV5/OmbEycQhjX9T56nkMyuTfr/OcN91sHry");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$9YhO3e45yC9UqkaOSQixbeQtY8RTtXC1XIgQ3CyUv4V0VV1rqfw/2");
        }
    }
}

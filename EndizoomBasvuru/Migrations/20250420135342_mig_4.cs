using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EndizoomBasvuru.Migrations
{
    /// <inheritdoc />
    public partial class mig_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Admins",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IsActive", "Password" },
                values: new object[] { true, "$2a$11$5Op/syT/R.ovphHoytbS3eLr3DGzp1CtyxI95tp1llJuHRtbByI1W" });

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IsActive", "Password" },
                values: new object[] { true, "$2a$11$bkPVEMy148dLaunknGO7fegLkgX4clgecDUenayf0JZL48KnBzZIC" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Admins");

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
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yaml.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "K8s_Config",
                table: "TBL_YAML_APP_INFO",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "K8s_Config",
                table: "TBL_YAML_APP_INFO");
        }
    }
}

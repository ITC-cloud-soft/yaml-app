using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yaml.Migrations
{
    /// <inheritdoc />
    public partial class port : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "port",
                table: "TBL_YAML_CLUSTER_INFO",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "target_port",
                table: "TBL_YAML_CLUSTER_INFO",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "port",
                table: "TBL_YAML_CLUSTER_INFO");

            migrationBuilder.DropColumn(
                name: "target_port",
                table: "TBL_YAML_CLUSTER_INFO");
        }
    }
}

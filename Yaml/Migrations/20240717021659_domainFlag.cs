using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yaml.Migrations
{
    /// <inheritdoc />
    public partial class domainFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "domain_flag",
                table: "TBL_YAML_CLUSTER_INFO",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "domain_flag",
                table: "TBL_YAML_CLUSTER_INFO");
        }
    }
}

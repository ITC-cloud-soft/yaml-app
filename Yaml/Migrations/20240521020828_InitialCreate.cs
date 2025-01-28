using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yaml.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_YAML_APP_INFO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    app_name = table.Column<string>(type: "TEXT", nullable: true),
                    cr = table.Column<string>(type: "TEXT", nullable: true),
                    token = table.Column<string>(type: "TEXT", nullable: true),
                    mail_address = table.Column<string>(type: "TEXT", nullable: true),
                    cloud_type = table.Column<int>(type: "INTEGER", nullable: true),
                    keyvault_flag = table.Column<bool>(type: "INTEGER", nullable: false),
                    tenantid = table.Column<string>(type: "TEXT", nullable: true),
                    key_vault_name = table.Column<string>(type: "TEXT", nullable: true),
                    managed_id = table.Column<string>(type: "TEXT", nullable: true),
                    netdata_flag = table.Column<bool>(type: "INTEGER", nullable: false),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    Kube_Config = table.Column<string>(type: "TEXT", nullable: true),
                    create_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    create_user = table.Column<string>(type: "TEXT", nullable: true),
                    update_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    update_user = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_YAML_APP_INFO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_YAML_CLUSTER_CONFIG_MAP_FILE_INFO",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    file_path = table.Column<string>(type: "TEXT", nullable: true),
                    file_link = table.Column<string>(type: "TEXT", nullable: true),
                    cluster_id = table.Column<int>(type: "INTEGER", nullable: true),
                    create_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    create_user = table.Column<string>(type: "TEXT", nullable: true),
                    update_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    update_user = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_YAML_CLUSTER_CONFIG_MAP_FILE_INFO", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_YAML_CLUSTER_CONFIG_MAP_INFO",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    config_key = table.Column<string>(type: "TEXT", nullable: true),
                    value = table.Column<string>(type: "TEXT", nullable: true),
                    cluster_id = table.Column<int>(type: "INTEGER", nullable: true),
                    create_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    create_user = table.Column<string>(type: "TEXT", nullable: true),
                    update_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    update_user = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_YAML_CLUSTER_CONFIG_MAP_INFO", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_YAML_CLUSTER_DISK_INFO",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    path = table.Column<string>(type: "TEXT", nullable: true),
                    cluster_id = table.Column<int>(type: "INTEGER", nullable: false),
                    disk_size = table.Column<string>(type: "TEXT", nullable: true),
                    pvc_name = table.Column<string>(type: "TEXT", nullable: true),
                    disk_type = table.Column<string>(type: "TEXT", nullable: true),
                    create_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    create_user = table.Column<string>(type: "TEXT", nullable: true),
                    update_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    update_user = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_YAML_CLUSTER_DISK_INFO", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_YAML_CLUSTER_DOMAIN_INFO",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    domain_name = table.Column<string>(type: "TEXT", nullable: true),
                    certification = table.Column<string>(type: "TEXT", nullable: true),
                    private_key = table.Column<string>(type: "TEXT", nullable: true),
                    cluster_id = table.Column<int>(type: "INTEGER", nullable: false),
                    create_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    create_user = table.Column<string>(type: "TEXT", nullable: true),
                    update_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    update_user = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_YAML_CLUSTER_DOMAIN_INFO", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_YAML_CLUSTER_INFO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cluster_name = table.Column<string>(type: "TEXT", nullable: true),
                    image = table.Column<string>(type: "TEXT", nullable: true),
                    pod_num = table.Column<int>(type: "INTEGER", nullable: true),
                    cpu = table.Column<string>(type: "TEXT", nullable: true),
                    memory = table.Column<string>(type: "TEXT", nullable: true),
                    managed_label = table.Column<string>(type: "TEXT", nullable: true),
                    prefix = table.Column<string>(type: "TEXT", nullable: true),
                    app_id = table.Column<int>(type: "INTEGER", nullable: true),
                    keyVault_flag = table.Column<bool>(type: "INTEGER", nullable: false),
                    configmap_flag = table.Column<bool>(type: "INTEGER", nullable: false),
                    configmap_file_flag = table.Column<bool>(type: "INTEGER", nullable: false),
                    diskInfo_flag = table.Column<bool>(type: "INTEGER", nullable: false),
                    create_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    create_user = table.Column<string>(type: "TEXT", nullable: true),
                    update_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    update_user = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_YAML_CLUSTER_INFO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_YAML_KEY_VAULT_INFO",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    config_key = table.Column<string>(type: "TEXT", nullable: true),
                    value = table.Column<string>(type: "TEXT", nullable: true),
                    cluster_id = table.Column<int>(type: "INTEGER", nullable: true),
                    app_id = table.Column<int>(type: "INTEGER", nullable: true),
                    create_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    create_user = table.Column<string>(type: "TEXT", nullable: true),
                    update_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    update_user = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_YAML_KEY_VAULT_INFO", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_YAML_User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NAME = table.Column<string>(type: "TEXT", nullable: true),
                    PASSWORD = table.Column<string>(type: "TEXT", nullable: true),
                    TOKEN = table.Column<string>(type: "TEXT", nullable: true),
                    MAIL_ADDRESS = table.Column<string>(type: "TEXT", nullable: true),
                    create_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    create_user = table.Column<string>(type: "TEXT", nullable: true),
                    update_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    update_user = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_YAML_User", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_YAML_APP_INFO");

            migrationBuilder.DropTable(
                name: "TBL_YAML_CLUSTER_CONFIG_MAP_FILE_INFO");

            migrationBuilder.DropTable(
                name: "TBL_YAML_CLUSTER_CONFIG_MAP_INFO");

            migrationBuilder.DropTable(
                name: "TBL_YAML_CLUSTER_DISK_INFO");

            migrationBuilder.DropTable(
                name: "TBL_YAML_CLUSTER_DOMAIN_INFO");

            migrationBuilder.DropTable(
                name: "TBL_YAML_CLUSTER_INFO");

            migrationBuilder.DropTable(
                name: "TBL_YAML_KEY_VAULT_INFO");

            migrationBuilder.DropTable(
                name: "TBL_YAML_User");
        }
    }
}

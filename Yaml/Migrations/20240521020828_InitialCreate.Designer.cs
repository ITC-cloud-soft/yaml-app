﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Yaml.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20240521020828_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.5");

            modelBuilder.Entity("Yaml.Domain.Entity.YamlAppInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AppName")
                        .HasColumnType("TEXT")
                        .HasColumnName("app_name");

                    b.Property<int?>("CloudType")
                        .HasColumnType("INTEGER")
                        .HasColumnName("cloud_type");

                    b.Property<string>("Cr")
                        .HasColumnType("TEXT")
                        .HasColumnName("cr");

                    b.Property<DateTime?>("CreateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_date");

                    b.Property<string>("CreateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_user");

                    b.Property<bool>("KeyVaultFlag")
                        .HasColumnType("INTEGER")
                        .HasColumnName("keyvault_flag");

                    b.Property<string>("KeyVaultName")
                        .HasColumnType("TEXT")
                        .HasColumnName("key_vault_name");

                    b.Property<string>("KubeConfig")
                        .HasColumnType("TEXT")
                        .HasColumnName("Kube_Config");

                    b.Property<string>("MailAddress")
                        .HasColumnType("TEXT")
                        .HasColumnName("mail_address");

                    b.Property<string>("ManagedId")
                        .HasColumnType("TEXT")
                        .HasColumnName("managed_id");

                    b.Property<bool>("NetdataFlag")
                        .HasColumnType("INTEGER")
                        .HasColumnName("netdata_flag");

                    b.Property<string>("Tenantid")
                        .HasColumnType("TEXT")
                        .HasColumnName("tenantid");

                    b.Property<string>("Token")
                        .HasColumnType("TEXT")
                        .HasColumnName("token");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_date");

                    b.Property<string>("UpdateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_user");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.ToTable("TBL_YAML_APP_INFO");
                });

            modelBuilder.Entity("Yaml.Domain.Entity.YamlClusterConfigFileInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<int?>("ClusterId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("cluster_id");

                    b.Property<DateTime?>("CreateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_date");

                    b.Property<string>("CreateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_user");

                    b.Property<string>("FileLink")
                        .HasColumnType("TEXT")
                        .HasColumnName("file_link");

                    b.Property<string>("FilePath")
                        .HasColumnType("TEXT")
                        .HasColumnName("file_path");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_date");

                    b.Property<string>("UpdateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_user");

                    b.HasKey("Id");

                    b.ToTable("TBL_YAML_CLUSTER_CONFIG_MAP_FILE_INFO");
                });

            modelBuilder.Entity("Yaml.Domain.Entity.YamlClusterConfigMapInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<int?>("ClusterId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("cluster_id");

                    b.Property<string>("ConfigKey")
                        .HasColumnType("TEXT")
                        .HasColumnName("config_key");

                    b.Property<DateTime?>("CreateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_date");

                    b.Property<string>("CreateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_user");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_date");

                    b.Property<string>("UpdateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_user");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT")
                        .HasColumnName("value");

                    b.HasKey("Id");

                    b.ToTable("TBL_YAML_CLUSTER_CONFIG_MAP_INFO");
                });

            modelBuilder.Entity("Yaml.Domain.Entity.YamlClusterDiskInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<int>("ClusterId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("cluster_id");

                    b.Property<DateTime?>("CreateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_date");

                    b.Property<string>("CreateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_user");

                    b.Property<string>("DiskSize")
                        .HasColumnType("TEXT")
                        .HasColumnName("disk_size");

                    b.Property<string>("DiskType")
                        .HasColumnType("TEXT")
                        .HasColumnName("disk_type");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.Property<string>("Path")
                        .HasColumnType("TEXT")
                        .HasColumnName("path");

                    b.Property<string>("PvcName")
                        .HasColumnType("TEXT")
                        .HasColumnName("pvc_name");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_date");

                    b.Property<string>("UpdateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_user");

                    b.HasKey("Id");

                    b.ToTable("TBL_YAML_CLUSTER_DISK_INFO");
                });

            modelBuilder.Entity("Yaml.Domain.Entity.YamlClusterDomainInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Certification")
                        .HasColumnType("TEXT")
                        .HasColumnName("certification");

                    b.Property<int>("ClusterId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("cluster_id");

                    b.Property<DateTime?>("CreateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_date");

                    b.Property<string>("CreateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_user");

                    b.Property<string>("DomainName")
                        .HasColumnType("TEXT")
                        .HasColumnName("domain_name");

                    b.Property<string>("PrivateKey")
                        .HasColumnType("TEXT")
                        .HasColumnName("private_key");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_date");

                    b.Property<string>("UpdateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_user");

                    b.HasKey("Id");

                    b.ToTable("TBL_YAML_CLUSTER_DOMAIN_INFO");
                });

            modelBuilder.Entity("Yaml.Domain.Entity.YamlClusterInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AppId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("app_id");

                    b.Property<string>("ClusterName")
                        .HasColumnType("TEXT")
                        .HasColumnName("cluster_name");

                    b.Property<bool>("ConfigMapFileFlag")
                        .HasColumnType("INTEGER")
                        .HasColumnName("configmap_file_flag");

                    b.Property<bool>("ConfigMapFlag")
                        .HasColumnType("INTEGER")
                        .HasColumnName("configmap_flag");

                    b.Property<string>("Cpu")
                        .HasColumnType("TEXT")
                        .HasColumnName("cpu");

                    b.Property<DateTime?>("CreateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_date");

                    b.Property<string>("CreateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_user");

                    b.Property<bool>("DiskInfoFlag")
                        .HasColumnType("INTEGER")
                        .HasColumnName("diskInfo_flag");

                    b.Property<string>("Image")
                        .HasColumnType("TEXT")
                        .HasColumnName("image");

                    b.Property<bool>("KeyVaultFlag")
                        .HasColumnType("INTEGER")
                        .HasColumnName("keyVault_flag");

                    b.Property<string>("ManageLabel")
                        .HasColumnType("TEXT")
                        .HasColumnName("managed_label");

                    b.Property<string>("Memory")
                        .HasColumnType("TEXT")
                        .HasColumnName("memory");

                    b.Property<int?>("PodNum")
                        .HasColumnType("INTEGER")
                        .HasColumnName("pod_num");

                    b.Property<string>("Prefix")
                        .HasColumnType("TEXT")
                        .HasColumnName("prefix");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_date");

                    b.Property<string>("UpdateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_user");

                    b.HasKey("Id");

                    b.ToTable("TBL_YAML_CLUSTER_INFO");
                });

            modelBuilder.Entity("Yaml.Domain.Entity.YamlKeyVaultInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<int?>("AppId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("app_id");

                    b.Property<int?>("ClusterId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("cluster_id");

                    b.Property<string>("ConfigKey")
                        .HasColumnType("TEXT")
                        .HasColumnName("config_key");

                    b.Property<DateTime?>("CreateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_date");

                    b.Property<string>("CreateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_user");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_date");

                    b.Property<string>("UpdateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_user");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT")
                        .HasColumnName("value");

                    b.HasKey("Id");

                    b.ToTable("TBL_YAML_KEY_VAULT_INFO");
                });

            modelBuilder.Entity("Yaml.Domain.Entity.YamlUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("CreateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_date");

                    b.Property<string>("CreateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("create_user");

                    b.Property<string>("MailAddress")
                        .HasColumnType("TEXT")
                        .HasColumnName("MAIL_ADDRESS");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasColumnName("NAME");

                    b.Property<string>("Password")
                        .HasColumnType("TEXT")
                        .HasColumnName("PASSWORD");

                    b.Property<string>("Token")
                        .HasColumnType("TEXT")
                        .HasColumnName("TOKEN");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_date");

                    b.Property<string>("UpdateUser")
                        .HasColumnType("TEXT")
                        .HasColumnName("update_user");

                    b.HasKey("Id");

                    b.ToTable("TBL_YAML_User");
                });
#pragma warning restore 612, 618
        }
    }
}
create table tbl_yaml_app_info
(
    ID             int auto_increment
        primary key,
    APP_NAME       varchar(100) not null,
    CR             varchar(100) not null,
    TOKEN          varchar(300) not null,
    MAIL_ADDRESS   varchar(50)  not null,
    KEYVAULT_FLAG  tinyint(1)   not null,
    TENANTID       varchar(50)  not null,
    KEY_VAULT_NAME varchar(50)  not null,
    MANAGED_ID     varchar(50)  not null,
    NETDATA_FLAG   tinyint(1)   not null,
    Kube_Config    varchar(100) null,
    User_Id        int          null,
    CREATE_DATE    datetime     null,
    CREATE_USER    varchar(30)  null,
    UPDATE_DATE    datetime     null,
    UPDATE_USER    varchar(30)  null
);

create table tbl_yaml_cluster_config_map_file_info
(
    ID          int auto_increment
        primary key,
    FILE_PATH   varchar(200) not null,
    FILE_LINK   varchar(200) not null,
    CLUSTER_ID  int          not null,
    CREATE_DATE datetime     null,
    CREATE_USER varchar(30)  null,
    UPDATE_DATE datetime     null,
    UPDATE_USER varchar(30)  null
);

create index CLUSTER_ID
    on tbl_yaml_cluster_config_map_file_info (CLUSTER_ID);

create table tbl_yaml_cluster_config_map_info
(
    ID          int auto_increment
        primary key,
    Config_KEY  varchar(50)  not null,
    VALUE       varchar(200) not null,
    CLUSTER_ID  int          not null,
    CREATE_DATE datetime     null,
    CREATE_USER varchar(30)  null,
    UPDATE_DATE datetime     null,
    UPDATE_USER varchar(30)  null
);

create index CLUSTER_ID
    on tbl_yaml_cluster_config_map_info (CLUSTER_ID);

create table tbl_yaml_cluster_disk_info
(
    ID          int auto_increment
        primary key,
    NAME        varchar(20)  null,
    PATH        varchar(100) not null,
    CLUSTER_ID  int          not null,
    DISK_TYPE   varchar(20)  null,
    PVC_NAME    varchar(20)  null,
    DISK_SIZE   varchar(10)  null,
    COMMENT     varchar(300) null,
    CREATE_DATE datetime     null,
    CREATE_USER varchar(30)  null,
    UPDATE_DATE datetime     null,
    UPDATE_USER varchar(30)  null
);

create table tbl_yaml_cluster_domain_info
(
    ID            int auto_increment
        primary key,
    Domain_Name   varchar(100) not null,
    Certification varchar(200) not null,
    Private_Key   varchar(200) not null,
    CLUSTER_ID    int          not null,
    CREATE_DATE   datetime     null,
    CREATE_USER   varchar(30)  null,
    UPDATE_DATE   datetime     null,
    UPDATE_USER   varchar(30)  null
);

create index CLUSTER_ID
    on tbl_yaml_cluster_domain_info (CLUSTER_ID);

create table tbl_yaml_cluster_info
(
    ID                  int auto_increment
        primary key,
    CLUSTER_NAME        varchar(50)  not null,
    IMAGE               varchar(100) not null,
    POD_NUM             int          not null,
    CPU                 varchar(20)  not null,
    MEMORY              varchar(20)  not null,
    MANAGED_LABEL       varchar(100) not null,
    Prefix              varchar(100) not null,
    APP_ID              int          not null,
    KeyVault_Flag       tinyint(1)   not null,
    ConfigMap_Flag      tinyint(1)   not null,
    ConfigMap_File_Flag tinyint(1)   not null,
    DiskInfo_Flag       tinyint      not null,
    CREATE_DATE         datetime     null,
    CREATE_USER         varchar(30)  null,
    UPDATE_DATE         datetime     null,
    UPDATE_USER         varchar(30)  null
);

create index APP_ID
    on tbl_yaml_cluster_info (APP_ID);

create table tbl_yaml_key_vault_info
(
    ID          int auto_increment
        primary key,
    CONFIG_KEY  varchar(50)  not null,
    VALUE       varchar(200) null,
    CLUSTER_ID  int          null,
    APP_ID      int          null,
    CREATE_DATE datetime     null,
    CREATE_USER varchar(30)  null,
    UPDATE_DATE datetime     null,
    UPDATE_USER varchar(30)  null
);

create table tbl_yaml_user
(
    ID           int auto_increment
        primary key,
    NAME         varchar(100) not null,
    PASSWORD     varchar(100) not null,
    TOKEN        varchar(300) null,
    MAIL_ADDRESS varchar(50)  null,
    TEL          varchar(50)  null,
    DEPT         varchar(50)  null,
    CREATE_DATE  datetime     null,
    CREATE_USER  varchar(30)  null,
    UPDATE_DATE  datetime     null,
    UPDATE_USER  varchar(30)  null
);

create table tbl_account_info
(
    ACCOUNT_ID   int auto_increment
        primary key,
    ACCOUNT_NAME varchar(50)  not null,
    ACCOUNT_FLG  int          not null,
    PASSWORD     varchar(200) not null,
    constraint tbl_account_info_pk
        unique (ACCOUNT_NAME)
);

create index tbl_account_info_ACCOUNT_NAME_index
    on tbl_account_info (ACCOUNT_NAME);



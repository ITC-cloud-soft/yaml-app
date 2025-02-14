<p align="center">

[//]: # (	<img alt="logo" src="https://oscimg.oschina.net/oscnet/up-b99b286755aef70355a7084753f89cdb7c9.png">)
</p>
<h1 align="center" style="margin: 30px 0 30px; font-weight: bold;">Yamler</h1>
<h4 align="center">基于 C#/Aspnet 的Kubernetes GUI CD 部署工具</h4>
<p align="center">
	<a href="https://gitee.com/y_project/RuoYi-Cloud/blob/master/LICENSE"><img src="https://img.shields.io/github/license/mashape/apistatus.svg"></a>
</p>
<p align="center">
  <a href="./README.md"><img alt="README in English" src="https://img.shields.io/badge/English-d9d9d9"></a>
  <a href="./README_CN.md"><img alt="简体中文版自述文件" src="https://img.shields.io/badge/简体中文-d9d9d9"></a>
  <a href="./README_JA.md"><img alt="日本語のREADME" src="https://img.shields.io/badge/日本語-d9d9d9"></a>
</p>

## 简介

Yamler是一款基于 C#/ASP.NET 和 Kubernetes 的可视化持续交付(CD)工具，旨在帮助开发者和运维人员通过直观的界面管理和部署 Kubernetes 集群和应用。
* 同时支持Azure 和 AWS的Kubernetes集群。
* 一次配置多处发布，通过导出配置文件，在其他安装Yamler的集群上也能发布应用。
* 支持Azure容器注册表连接与KeyVault集成。
* 提供丰富的配置选项，如容器镜像、Pod数量、CPU和内存限制、磁盘配置、ConfigMap与KeyVault设置等。
* 使用Helm和Yaml管理Kubernetes资源，简化了复杂的配置管理和应用部署流程。
* 前端基于React和Ant Design，后端采用C# ASP.NET Core和Quartz。
## Yamler 下载、导出、导入功能

Yamler 提供以下功能，支持开发环境中的高效管理和部署：

1. **下载 (Download)**:
   通过 Yamler 下载当前的配置，用户可以在本地查看和管理集群的配置。

2. **导出 (Export)**:
   用户可以导出配置并保存，稍后在相同或不同的环境中导入并应用相同的配置。

3. **导入 (Import)**:
   可以轻松导入其他集群导出的配置文件，快速部署相同的配置。这确保了在多个环境中应用一致的部署。

### 配置迁移功能（开发环境中的平行部署）

通过使用开发环境中的配置文件，Yamler 可以将已配置的设置应用于多个集群，实现跨环境的平行部署。配置文件一经设置，用户可以在其他安装了 Yamler 的集群上快速导入相同的应用并进行部署。
## 系统模块

~~~
Yamler-backend  
├── Application             // 应用程序模块
│       └── Command                                    // 命令模块 
│       └── Query                                      // 查询模块 
│       └── Response                                   // 响应模块 
│       └── Validator                                  // 验证模块 
├── Domain                  // 领域模块
│       └── AzureApi                                   // Azure API集成
│       └── K8s                                         // Kubernetes操作模块
│       └── Entity                                      // 实体模块
│       └── KubeConstants.cs                            // 常量配置
├── Infrastructure          // 基础设施模块
│       └── CustomService                               // 定制服务模块
│       └── Mappings                                   // 对象映射
│       └── Persistence                                // 数据持久化
└── Resource                // 静态资源模块
       └── YAML文件管理
       └── SQL脚本创建
~~~

## 功能模块

1. **集群管理**：支持通过界面管理 Kubernetes 集群，设置容器镜像、Pod数量、CPU和内存配置。
2. **KeyVault集成**：可以在部署中集成Azure KeyVault，方便安全存储和管理敏感信息，如密码和连接字符串。
3. **ConfigMap配置**：支持通过界面设置ConfigMap，管理应用的配置文件。
4. **磁盘挂载**：支持对磁盘进行挂载配置，包括Azure标准SSD磁盘。
5. **YAML文件管理**：支持导入、导出和在线编辑Kubernetes的YAML配置文件。

## 演示图
<table>
    <tr>
        <td><img  style="max-width: 60vh" src="https://itc-cloud-soft.github.io/doc-open/img/yamler/yamler_cn1.png"/></td>
        <td><img  style="max-width: 60vh" src="https://itc-cloud-soft.github.io/doc-open/img/yamler/yamler_cn2.png"/></td>
    </tr>
</table>

## 使用方法

1. 在Azure上创建容器注册表并配置KeyVault。
2. 使用Yamler工具，通过直观的界面配置Kubernetes集群的相关参数。
3. 管理ConfigMap和KeyVault中的敏感信息。
4. 使用Helm或YAML配置部署应用。
## 安装方式

您可以通过 Helm Charts 轻松安装 Yamler。请按照以下步骤操作：

1. 添加 Yamler Helm 仓库：
```bash
helm repo add yamler https://itc-cloud-soft.github.io/yaml-helm-open/
```
2. 更新 Helm 仓库：
```bash
helm repo update
```
3. 使用 Helm 安装 Yamler：
```bash
helm install yamler yamler/yamler-chart
```
4. 查看安装状态：
```bash
helm status yamler
```
## 访问 Yamler
当访问 Yamler 首页时，可以使用 Lens 或 Kubernetes 命令来代理 URL。

1. 使用 Lens 访问
按下转发按钮将自动转发到 URL
<table> <tr> <td><img  style="max-width: 60vh" src="https://itc-cloud-soft.github.io/doc-open/img/yamler/yamler_lens.png"/></td> </tr> </table>

2. 使用 Kubernetes 命令访问
```shell
kubectl port-forward service/my-helm-yarl 8081:8080 --namespace yamler
```
然后在浏览器中输入`https://localhost:8081`访问
## 许可证

Yamler 基于 [MIT License](LICENSE) 许可证。
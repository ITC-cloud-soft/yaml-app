<p align="center">

[//]: # (	<img alt="logo" src="https://oscimg.oschina.net/oscnet/up-b99b286755aef70355a7084753f89cdb7c9.png">)
</p>
<h1 align="center" style="margin: 30px 0 30px; font-weight: bold;">Yamler</h1>
<h4 align="center">Kubernetes GUI CD deployment tool based on C#/Aspnet</h4>
<p align="center">
	<a href="https://gitee.com/y_project/RuoYi-Cloud/blob/master/LICENSE"><img src="https://img.shields.io/github/license/mashape/apistatus.svg"></a>
</p>
<p align="center">
  <a href="./README.md"><img alt="README in English" src="https://img.shields.io/badge/English-d9d9d9"></a>
  <a href="./README_CN.md"><img alt="简体中文版自述文件" src="https://img.shields.io/badge/简体中文-d9d9d9"></a>
  <a href="./README_JA.md"><img alt="日本語のREADME" src="https://img.shields.io/badge/日本語-d9d9d9"></a>
</p>

## Introduction

Yamler is a visual continuous delivery (CD) tool based on C#/ASP.NET and Kubernetes, designed to help developers and operations staff manage and deploy Kubernetes clusters and applications through an intuitive interface.
* Kubernetes clusters on Azure and AWS
* Multi-location deployment with one configuration: by exporting configuration files, applications can be imported and deployed on other clusters where Yamler is installed.
* Supports integration with Azure Container Registry and KeyVault.
* Offers rich configuration options, such as container images, pod count, CPU and memory limits, disk settings, ConfigMap and KeyVault configurations, etc.
* Uses Helm and YAML to manage Kubernetes resources, simplifying complex configuration management and application deployment processes.
* Frontend based on React and Ant Design, backend using C# ASP.NET Core and Quartz.
## Yamler Download, Export, Import Features

Yamler offers the following features to support efficient management and deployment in development environments:

1. **Download**:
   Using Yamler, you can download the current configuration and manage the cluster settings locally.

2. **Export**:
   Users can export configuration files and save them, later importing them into the same or different environments to apply the same settings.

3. **Import**:
   You can easily import configuration files exported from other clusters to quickly deploy the same settings, ensuring consistent application deployment across multiple environments.

### Migration Functionality (Parallel Deployment in Development Environments)

Using configuration files in development environments, Yamler can apply the configured settings to multiple clusters, enabling parallel deployment across environments. Once the configuration is set, users can quickly import the same application and deploy it across other clusters where Yamler is installed.
## System Modules

~~~
Yamler-backend  
├── Application             // Application module
│       └── Command                                   // Command module 
│       └── Query                                     // Query module 
│       └── Response                                  // Response module 
│       └── Validator                                 // Validation module 
├── Domain                  // Domain module
│       └── AzureApi                                  // Azure API integration
│       └── K8s                                       // Kubernetes operation module
│       └── Entity                                    // Entity module
│       └── KubeConstants.cs                          // Constant configurations
├── Infrastructure          // Infrastructure module
│       └── CustomService                             // Custom service module
│       └── Mappings                                 // Object mappings
│       └── Persistence                              // Data persistence
└── Resource                // Static resource module
        └── YAML management
        └── SQL script creation
~~~

## Feature Modules

1. **Cluster Management**: Allows you to manage Kubernetes clusters through an interface, configure container images, pod count, CPU and memory settings.
2. **KeyVault Integration**: Integrates Azure KeyVault to securely store and manage sensitive information like passwords and connection strings during deployment.
3. **ConfigMap Configuration**: Provides a GUI for setting up ConfigMaps, managing application configuration files.
4. **Disk Mounting**: Supports disk mounting configurations, including Azure Standard SSD disks.
5. **YAML File Management**: Supports importing, exporting, and online editing of Kubernetes YAML configuration files.

## Demo Images
<table>
    <tr>
        <td><img style="max-width: 60vh" src="https://itc-cloud-soft.github.io/doc-open/img/yamler/yamler_en1.png"/></td>
        <td><img style="max-width: 60vh" src="https://itc-cloud-soft.github.io/doc-open/img/yamler/yamler_en2.png"/></td>
    </tr>
</table>

## Usage

1. Create an Azure Container Registry and configure KeyVault in Azure.
2. Use the Yamler tool to configure Kubernetes clusters through the GUI.
3. Manage sensitive information in ConfigMap and KeyVault.
4. Deploy applications using Helm or YAML.

## Installation

You can easily install Yamler via Helm Charts. Follow these steps:

1. Add the Yamler Helm repository:
```bash
helm repo add yamler https://itc-cloud-soft.github.io/yaml-helm-open/
```
2.	Update Helm repositories:
```bash[uploads](Yaml%2Fuploads)
helm repo update
```
3.	Install Yamler using Helm:
```
helm install yamler yamler/yamler-chart
```
4.	Check installation status:
```
helm status yamler
```
## Access Yamler Page
When access Yamler homepage could use Lens or Kubernetes command to proxy the url.
1. Use Lens to access
Press forward button will automatically forward to browser 
<table>
    <tr>
        <td><img style="max-width: 60vh" src="https://itc-cloud-soft.github.io/doc-open/img/yamler/yamler_lens.png"/></td>
    </tr>
</table>

2. Use kubernetes command to access
```shell
kubectl port-forward service/my-helm-yarl 8081:8080 --namespace yamler
```
then access `https://localhost:8081` in browser
### License

Yamler is licensed under the MIT License.
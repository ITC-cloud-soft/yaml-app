<p align="center">

[//]: # (	<img alt="logo" src="https://oscimg.oschina.net/oscnet/up-b99b286755aef70355a7084753f89cdb7c9.png">)
</p>
<h1 align="center" style="margin: 30px 0 30px; font-weight: bold;">Yamler</h1>
<h4 align="center">C#/Aspnet ベースのKubernetes GUI CDデプロイツール</h4>
<p align="center">
	<a href="https://gitee.com/y_project/RuoYi-Cloud/blob/master/LICENSE"><img src="https://img.shields.io/github/license/mashape/apistatus.svg"></a>
</p>
<p align="center">
  <a href="./README.md"><img alt="README in English" src="https://img.shields.io/badge/English-d9d9d9"></a>
  <a href="./README_CN.md"><img alt="简体中文版自述文件" src="https://img.shields.io/badge/简体中文-d9d9d9"></a>
  <a href="./README_JA.md"><img alt="日本語のREADME" src="https://img.shields.io/badge/日本語-d9d9d9"></a>
</p>

## 概要

Yamlerは、C#/ASP.NET および Kubernetes に基づく視覚的な継続的デリバリ（CD）ツールで、開発者や運用担当者が直感的なインターフェースを通じてKubernetesクラスターやアプリケーションを管理およびデプロイするためのツールです。
* Azure および AWS のKubernetesクラスター
* 一度の設定で複数箇所にデプロイ：設定ファイルをエクスポートすることで、Yamlerがイン
  ストールされている他のクラスターにもインポートしてアプリケーションをデプロイできます
* Azureコンテナレジストリ接続およびKeyVaultとの統合をサポートしています。
* コンテナイメージ、Pod数、CPUおよびメモリ制限、ディスク設定、ConfigMapおよびKeyVault設定など、豊富な構成オプションを提供します。
* KubernetesリソースをHelmおよびYAMLで管理し、複雑な構成管理およびアプリケーションデプロイプロセスを簡素化します。
* フロントエンドはReactおよびAnt Designをベースに、バックエンドはC# ASP.NET CoreおよびQuartzを使用しています。

## システムモジュール

~~~
Yamler-backend  
├── Application             // アプリケーションモジュール
│       └── Command                                   // コマンドモジュール 
│       └── Query                                     // クエリモジュール 
│       └── Response                                  // レスポンスモジュール 
│       └── Validator                                 // バリデータモジュール 
├── Domain                  // ドメインモジュール
│       └── AzureApi                                  // Azure API統合
│       └── K8s                                       // Kubernetes操作モジュール
│       └── Entity                                    // エンティティモジュール
│       └── KubeConstants.cs                          // 定数設定
├── Infrastructure          // インフラストラクチャモジュール
│       └── CustomService                             // カスタムサービスモジュール
│       └── Mappings                                 // オブジェクトマッピング
│       └── Persistence                              // データ永続化
├── Resource                // 静的リソースモジュール
│       └── YAMLファイル管理
│       └── SQLスクリプト作成
~~~

## 機能モジュール

1. **クラスター管理**：GUI を通じてKubernetesクラスターを管理し、コンテナイメージ、Pod数、CPUおよびメモリの設定を行います。
2. **KeyVault 統合**：Azure KeyVault をデプロイに統合し、パスワードや接続文字列などの機密情報を安全に保存および管理します。
3. **ConfigMap 設定**：GUI を使用して ConfigMap を設定し、アプリケーションの構成ファイルを管理します。
4. **ディスクマウント**：Azure標準SSDディスクなどのディスクマウント設定をサポートします。
5. **YAML ファイル管理**：KubernetesのYAML設定ファイルのインポート、エクスポート、およびオンライン編集をサポートします。

## デモ画像
<table>
    <tr>
        <td><img src="https://itc-cloud-soft.github.io/doc-open/img/yamler/yamler_1.png"/></td>
        <td><img src="https://itc-cloud-soft.github.io/doc-open/img/yamler/yamler_2.png"/></td>
    </tr>
</table>

## 使用方法

1. Azure でコンテナレジストリを作成し、KeyVault を設定します。
2. Yamlerツールを使用して、Kubernetesクラスターの設定を直感的なGUIを通じて行います。
3. ConfigMapやKeyVault内の機密情報を管理します。
4. Helm または YAML を使用してアプリケーションをデプロイします。

## インストール方法

Helm Charts を使用して Yamler を簡単にインストールできます。以下の手順に従ってください：

1. Yamler Helm レポジトリを追加します：
1. Add the Yamler Helm repository:
```bash
helm repo add yamler https://itc-cloud-soft.github.io/yaml-helm-open/
```
2.	Update Helm repositories:
```bash
helm repo update
```
3.	Helm を使用して Yamler をインストールします：
```bash
helm install yamler yamler/yamler-chart
```
4.	インストールステータスを確認します：
```bash
helm status yamler
```
### License
Yamler は MIT License に基づいてライセンスされています。
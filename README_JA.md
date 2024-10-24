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
## Yamler Download、Export、Import 機能

Yamler では、開発環境での効率的な管理とデプロイをサポートするために、以下の機能を提供しています：

1. **Download**: Yamler を利用して現在の設定をダウンロードし、クラスタの構成をローカルで確認および管理することができます。
2. **Export**: ユーザーは、設定をエクスポートして保存することができ、後で同じまたは異なる環境にインポートして、同じ構成を適用できます。
3. **Import**: 他のクラスタでエクスポートされた設定ファイルを簡単にインポートして、迅速に同じ設定を展開することができます。これにより、複数の環境でのアプリケーションの一貫したデプロイが可能になります。

### 平移機能（開発環境における平行展開）

開発環境での構成文書を利用することで、Yamler は一度設定した内容を複数のクラスタに適用し、各環境間でスムーズにアプリケーションを平行展開することが可能です。これにより、構成設定を一度行い、他の Yamler を導入したクラスターで同じアプリケーションを迅速にインポートし、デプロイすることができます。

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

## Yamler ホームページにアクセス
Yamler ホームページにアクセスする際に、Lens または Kubernetes コマンドを使用して URL をプロキシすることができます。

1. Lens を使用してアクセスする
転送ボタンを押すと、URL に自動的に転送されま
<table> <tr> <td><img src="https://itc-cloud-soft.github.io/doc-open/img/yamler/yamler_lens.png"/></td> </tr> </table>

2. Kubernetes コマンドを使用してアクセスする
```shell
kubectl port-forward service/my-helm-yarl 8081:8080 --namespace yamler
```
その後、ブラウザで https://localhost:8081 にアクセスします。
### License
Yamler は MIT License に基づいてライセンスされています。
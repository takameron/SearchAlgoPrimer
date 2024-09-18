囲みマス エージェント
====

[ゲームで学ぶ探索アルゴリズム実践入門～木探索とメタヒューリスティクス～](https://gihyo.jp/book/2023/978-4-297-13360-3)を参考に実装した、囲みマスで対戦するエージェントです。

## 開発環境

### Visual Studio

Visual Studio 2022で動作することを確認しています。

### Dev Container

Visual Studio Codeで動作することを確認しています。

ホスト側で下記のコマンド（秘密鍵は任意のもの）を実行することで、コンテナ内でSSH接続できるようになります。

```sh
ssh-add ~/.ssh/id_ed25519
```

### 実行

```
cd SearchAlgoPrimer #SearchAlgoPrimer.csprojが存在するディレクトリ
dotnet run
```

### 実行ファイルの生成

詳しくは[.NET CLI を使用した .NET アプリの発行](https://learn.microsoft.com/ja-jp/dotnet/core/deploying/deploy-with-cli)を参照ください。
RIDは[.NET RID カタログ](https://learn.microsoft.com/ja-jp/dotnet/core/rid-catalog)を参照ください。

```
dotnet publish -c Release -r <RID> --self-contained true
```

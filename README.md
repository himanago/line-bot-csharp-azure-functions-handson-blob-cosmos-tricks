# LINE Bot C# Azure Functions ハンズオン

[LINE Bot C# Azure Functions Starter](https://github.com/himanago/line-bot-csharp-azure-functions-starter) をベースにしたハンズオン環境です。

GitHub Codespaces で開くだけで、LINE Bot 開発に活かせる Azure の便利な機能を体験できます。

## 体験できる内容

### ① LINE Bot C# Azure Functions Starter による高速環境構築

GitHub Codespaces で開くだけで、最新の LINE OpenAPI に従った SDK をもとにした LINE Bot 開発が始められます。

参考: [【GitHub Codespaces ＋ LINE OpenAPI ＋ azd】最新の機能をサポートした C# SDK で LINE Bot 開発が始められるテンプレートを改良しました](https://himanago.hatenablog.com/entry/2024/03/27/190703)

### ② Azure Cosmos DB＋Azure Functionsで実現する安全なメッセージ配信機能

Azure Functions ＋ Cosmos DB トリガーでメッセージ送信を行うことで、Messaging API のエラー時にも効率よくリトライ処理が行える相性の良さを体験できます。

参考: [意外な相性！？Azure Cosmos DB × LINE で作る安心のメッセージ配信システム](https://www.slideshare.net/takumasahirabayashi/cosmosdb-messagingapireliability)


### ③ Durable Functions（Durable Entity）を使って"返信までに時間がかかる処理"を効果的にさばくアイディア

LINE Bot で時間のかかる処理を返信でさばくための拡張アイディアです。

返信の入れ違いを防ぎながら、リプライトークンの有効期限を有効に使うことができます。

参考: [【Azure Functions】Durable Entity を使った「リプライトークンリレー」で LINE Bot の返信処理を拡張する](https://himanago.hatenablog.com/entry/2024/03/22/174501)

### ④ Azure Blob Storage の SAS トークンでセキュアに画像を送る

LINE Bot で画像や動画などを送る際に必要な公開 URL を Blob Storage の SAS 機能を用いてセキュアに用意する方法です。

参考: [Azure Blob Storage の SAS トークンを使って LINE Bot でセキュアに画像を送る](https://himanago.hatenablog.com/entry/2020/11/09/103926)

## 使用技術

- C# (.NET 6 in-process)
- Azure Functions
- Azure Key Vault
- Azure Application Insights
- Azure Developer CLI (azd)
- LINE Messaging API
- OpenAPI Generator

## ハンズオンの進め方

### ① LINE Bot C# Azure Functions Starter による高速環境構築

#### 1. GitHub Codespaces で開く

本リポジトリの Use this template > Open in a codespace から開けます。

#### 2. 準備完了するまで待つ

Codespace の画面が表示されたあと、いくつかのコマンドが実行されます。

openapi-generator-cli により LINE OpenAPI の定義から SDK を `sdk` フォルダに生成するので、出来上がりを待ちます。

待っている間に、3 を進めます。

#### 3. LINE Messagng API チャネルの作成

LINE Developers のドキュメント、[Messaging APIを始めよう](https://developers.line.biz/ja/docs/messaging-api/getting-started/) を参考に、Messaging API チャネルを作成します。

#### 4. Azure Developer CLI のログイン

ターミナルで以下のコマンドを実行し、`azd` で Azure にログインします。

```bash
azd auth login --use-device-code
```

表示されるコードをコピーし、Enter 押下後に開くタブに貼り付け、使用する Azure アカウントでサインインしてください。

#### 5. Azure Developer CLI によるリソースの作成とデプロイ

`azure.yaml` ファイルを右クリックし、Azure Developer CLI (azd) > Package, Provision and Deploy (up) を選択します。

または、ターミナルで以下のコマンドを実行します。

```bash
azd up
```

ターミナルでサブスクリプションとリージョンを選んでください。

LINE Messaging API との接続のために、チャネルアクセストークンを入力するように言われるので、先ほど作成した LINE Messaging API チャネルのチャネルアクセストークンをコピーしてきて、貼り付けてください。

リソースの作成とコードのデプロイが開始するので、完了するまで待ちます。

#### 6. LINE Bot の Webhook URL の設定

デプロイが完了したら、Azure Functions の `WebhookEndpoint` 関数の URL をコピーして LINE Messaging API チャネルの Webhook URL に設定します。

このとき、あわせて LINE 公式アカウントの設定で応答メッセージをオフにしておいてください。

#### 7. オウム返しの動作確認と Bot の実装
​
Messaging API の管理画面の QR コードを読み取って Bot を LINE に友だち追加して、テキストメッセージを送ってください。

オウム返しで同じ内容を返信してくれたら成功です。

コードの中身も確認してみましょう。



### ② Azure Cosmos DB＋Azure Functionsで実現する安全なメッセージ配信機能

Functions のリソースを Azure ポータルで開き、`CosmosTrigger ` 関数の「モニター」を開き、「ログ」タブを選択しておきます。

次に別のブラウザタブで Cosmos DB のリソースを Azure ポータルから開き、メニューの「データエクスプローラー」を開きます。

`messagedb` にある `messages` コンテナーの Items を開き、「New Item」をクリックします。

登録するデータを編集するエディターが開くので、そこに

```json
{
    "text": "送信したいメッセージ"
}
```

と入力してください。`送信したいメッセージ` は好きなメッセージを入れてください。

LINE Bot の友だち全員にそのメッセージが送信されます。

内部的にわざと 50 % の確率でエラーが出るようにしているのですが、Azure Functions のリトライ機能によって自動で再実行されます。モニターに表示されるログを確認してみてください。

エラーの出方やリトライの様子を確認するために、Cosmos DB にデータをさらに追加することで何度か繰り返し試してみてください。

（コードも確認してみてください）

### ③ Durable Functions（Durable Entity）を使って"返信までに時間がかかる処理"を効果的にさばくアイディア ＋ ④ Azure Blob Storage の SAS トークンでセキュアに画像を送る

③ と ④ はあわせて確認していきます。

`WebhookEndpoint` のコードを確認すると、「○○の画像を送って」というメッセージにのみ、反応する分岐が用意されています。

Functions のリソースを Azure ポータルで開き、`WebhookEndpoint ` 関数の「モニター」を開き、「ログ」タブを選択しておきます。

たとえば `鳥の画像を送って` と LINE から送ると、

```
画像のリクエストがありました。from: (LINE ユーザー ID)
Message: 鳥の画像を送って
```

というログが出力されます。Bot は返信をしません。

その状態で LINE でさらに任意のメッセージを送ると、「準備中です。ちょっとまってね。」と返信が帰ってきます。

このとき、実は裏でリプライトークンがリレーされ、常に最新のトークンがエンティティ関数に保持されるようになっています。

次に、Blob Storage のリソースを Azure ポータルで開き、「コンテナー」で新しいコンテナーを `images` という名前で作成してください。

これを開き、アップロードから画像をアップロードしていきます。画像は拡張子が `jpg` または `png` のものを用意し、ファイル名を `LINEユーザーID_任意の名前.png` のようにしてください。

LINE ユーザー ID は、ログ出力されたものを使います。（例：`U7298s71491b0e24f4a106res215e19_tori.png`）

アップロードが完了すると、LINE にその画像が届きます。これはエンティティ関数に保持されていた最後のリプライトークンを使って送信されています。

また、`BlobTrigger` では SAS トークンつきの URL でこの送信処理を行っています。

その URL を含んだメッセージオブジェクトがログ出力するようにしているので、`BlobTrigger` のモニターで確認してみてください。

その後はまた任意のメッセージに対してオウム返しが動くようになります。

コードの流れも追ってみてください。
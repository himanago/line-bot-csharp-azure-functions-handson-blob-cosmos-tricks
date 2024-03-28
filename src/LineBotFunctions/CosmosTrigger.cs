using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using LineOpenApi.MessagingApi.Api;
using LineOpenApi.MessagingApi.Model;
using Newtonsoft.Json.Linq;

namespace LineBotFunctions;

public class CosmosTrigger
{
    private IMessagingApiApiAsync Api { get; }

    public CosmosTrigger(
        IMessagingApiApiAsync api)
    {
        Api = api;
    }

    //[FixedDelayRetry(10, "00:00:15")] // 固定の間隔で 10 回までリトライ（無制限の場合は -1 を指定）
    [ExponentialBackoffRetry(10, "00:00:04", "00:15:00")]   // 4秒から15分までだんだん間隔を大きくしながら 10 回までリトライ
    [FunctionName(nameof(CosmosTrigger))]
    public async Task Run([CosmosDBTrigger(
        databaseName: "messagedb",
        containerName: "messages",
        Connection = "cosmosDbConnectionString",
        CreateLeaseContainerIfNotExists = true,
        LeaseContainerName = "leases")]IReadOnlyList<JObject> input, ILogger log)
    {
        log.LogInformation("called");
        log.LogInformation($"input: {input}");
        try
        {
            var documents = input.Select(x => x.ToObject<MessageDocument>());

            // メッセージ（最大5件ずつ）
            foreach (var data in documents.Select((doc, idx) => (doc, idx))
                .GroupBy(x => x.idx / 5)
                .Select(x => x.Select(x => x.doc)))
            {
                // 50%の確率で失敗する！-> エラーで送信できないケースを想定
                if (new Random().Next(0, 2) == 0)
                {
                    throw new Exception("エラーが出て送信失敗しました。");
                }

                // メッセージ送信
                // 5件ずつに区切ったうちの1件目のIDをリトライキーとして使う
                // 5件ずつループしなければinput[0].IdでOK
                var response = await Api.BroadcastWithHttpInfoAsync(
                    broadcastRequest: new BroadcastRequest(data.Select(x => new TextMessage(x.Text)
                    {
                        Type = "text"
                    }).ToList<Message>()),
                    xLineRetryKey: new Guid(data.First().Id));

                log.LogInformation($"Response status: {response.StatusCode}");

                // 500番台だった場合は例外を投げる
                if ((int)response.StatusCode >= 500)
                {
                    throw new HttpRequestException("Messaging API で 500 エラーが発生！");
                }
                // 400番台だった場合は終了
                else if ((int)response.StatusCode >= 400)
                {
                    return;
                }

                // 50%の確率で失敗する！-> エラーが出たが送信できていたケースを想定
                if (new Random().Next(0, 2) == 0)
                {
                    throw new Exception("エラーが出たけど送信成功しました。");
                }

                // 「エラーが出て送信失敗」50%
                // 「エラーが出るけど送信成功」25%
                // 「エラーなしで送信成功」25%
            }
        }
        catch (Exception e)
        {
            log.LogError($"Error: {e.Message}");
            log.LogError($"Error: {e.StackTrace}");
            throw;
        }
    }

    public class MessageDocument
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }
}
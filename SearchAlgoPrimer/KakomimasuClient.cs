using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SearchAlgoPrimer
{

    public class KakomimasuClient
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };


        private static readonly string baseUrl = "https://api.kakomimasu.com";

        /**
         * ゲームに参加する
         */
        public async Task<ConnectionInfo> join()
        {
            string startUrl = $"{baseUrl}/v1/matches/ai/players";
            var startPayload = new JoinInfo
            {
                guestName = "C#くん",
                aiName = "a1",
                boardName = "A-1",
                nAgent = 1
            };
            StringContent? startContent = new StringContent(JsonSerializer.Serialize(startPayload, jsonOptions), new MediaTypeHeaderValue("application/json"));
            HttpResponseMessage? startResponse = await client.PostAsync(startUrl, startContent);
            string? startResponseBody = await startResponse.Content.ReadAsStringAsync();
            JsonElement game = JsonDocument.Parse(startResponseBody).RootElement;
            return JsonSerializer.Deserialize<ConnectionInfo>(game.GetRawText());
        }

        /**
         * ゲームに参加するための情報
         */
        public record JoinInfo()
        {
            // プレイヤーの紹介文
            public string? spec { get; set; }
            // アカウントを作成せずに参加する場合のプレイヤー名
            public string? guestName { get; set; }
            // aiの名前
            public string aiName { get; set; }
            // ボードの名前
            public string? boardName { get; set; }
            // エージェントの数
            public int? nAgent { get; set; }
            // ターン数
            public int? totalTurn { get; set; }
            // 行動ステップ時間(秒)
            public int? operationSec { get; set; }
            // 遷移ステップ時間(秒)
            public int? transitionSec { get; set; }
            // trueにするとAPIのテストができる
            public bool? dryRun { get; set; }
        }

        /**
         * 接続情報
         */
        public record ConnectionInfo()
        {
            // 参加プレイヤーのユーザID
            public string userId { get; set; }
            // 参加プレイヤーの紹介文
            public string spec { get; set; }
            // 参加したゲームID
            public string gameId { get; set; }
            // 参加したゲームのインデックス（ゲーム詳細を取得した際のplayers配列内の自分のインデックス）
            public int? index { get; set; }
            // 行動送信時に必要となるトークン（プレイヤー識別コード）
            public string pic { get; set; }
        }

        /**
         *  相手のターンを待つ
         */
        private async Task<(PlayVerbose, HttpStatusCode)> wait(ConnectionInfo connectionInfo)
        {
            string gameUrl = $"{baseUrl}/v1/matches/{connectionInfo.gameId}";

            // 赤忍者を待つ
            while (true)
            {
                HttpResponseMessage? waitStartResponse = await client.GetAsync(gameUrl);
                string waitStartResponseBody = await waitStartResponse.Content.ReadAsStringAsync();
                JsonElement game = JsonDocument.Parse(waitStartResponseBody).RootElement;
                PlayVerbose playVerbose = JsonSerializer.Deserialize<PlayVerbose>(game.GetRawText(), jsonOptions);

                if (game.GetProperty("startedAtUnixTime").GetInt64() != 0)
                {
                    return (playVerbose, waitStartResponse.StatusCode);
                }

                await Task.Delay(100);
            }
        }

        public async Task<PlayVerbose> waitStart(ConnectionInfo connectionInfo)
        {
            while (true)
            {
                var res = await wait(connectionInfo);
                var playVerbose = res.Item1;

                if (playVerbose.startedAtUnixTime != 0)
                {
                    return playVerbose;
                }

                await Task.Delay(100);
            }
        }

        public async Task<PlayVerbose> getPlayVerbose(ConnectionInfo connectionInfo)
        {
            while (true)
            {
                var res = await wait(connectionInfo);
                var playVerbose = res.Item1;
                var statusCode = res.Item2;

                if (statusCode == HttpStatusCode.OK)
                {
                    return playVerbose;
                }

                await Task.Delay(100);
            }
        }

        /**
        * プレイの詳細情報
        */
        public record PlayVerbose()
        {
            // ゲームの状態
            public GameStatus status { get; set; }
            // ゲームID
            public string id { get; set; }
            // ゲーム名
            public string name { get; set; }
            // ターンごとのログ
            public Log[] log { get; set; }
            // 行動ステップ時間（秒）
            public int operationSec { get; set; }
            // プレイヤー情報
            public Player[] players { get; set; }
            // ゲーム入出可能なユーザIDのリスト
            public string[] reservedUsers { get; set; }
            // ゲーム開始時刻(UNIX時間)
            public int startedAtUnixTime { get; set; }
            // フィールド情報
            public Field field { get; set; }
            // ゲームの総ターン数
            public int totalTurn { get; set; }
            // 参加プレイヤー数
            public int nPlayer { get; set; }
            // エージェント数
            public int nAgent { get; set; }
            // 遷移ステップ時間（秒）
            public int transitionSec { get; set; }
            // 現在のターン数
            public int turn { get; set; }
            // ゲームタイプ
            public GameType type { get; set; }
        }

        public record Log()
        {
            public PlayerInTern[] players { get; set; }
        }

        public record PlayerInTern()
        {
            public Point point { get; set; }
            public Action[] actions { get; set; }
        }

        /**
         * ゲームの状態
         */
        public enum GameStatus
        {
            FREE,
            READY,
            GAMING,
            ENDED
        }

        public record Point()
        {
            // 陣地ポイント
            public int areaPoint { get; set; }
            // 壁ポイント
            public int wallPoint { get; set; }
        }

        public record Action()
        {
            // 行動のタイプ
            public ActionType type { get; set; }
            // エージェントのID
            public int agentId { get; set; }
            // 行動先のx座標
            public int x { get; set; }
            // 行動先のy座標
            public int y { get; set; }
            // 行動の結果
            public ActionRes res { get; set; }
        }

        /**
         * 行動のタイプ
         */
        public enum ActionType : int
        {
            PUT = 1,
            NONE = 2,
            MOVE = 3,
            REMOVE = 4
        }

        /**
         * 行動の結果
         */
        public enum ActionRes : int
        {
            // 成功
            SUCCESS = 0,
            // 競合
            CONFLICT = 1,
            // 無効
            INVALID = 2,
            // 同じターンに複数の行動指示
            MULTIPLE_ORDER = 3,
            // 存在しないエージェントへの指示
            NOT_EXIST_AGENT = 4,
            // 存在しない行動の指示
            NOT_EXIST_ACTION = 5
        }

        public record Player()
        {
            // プレイヤーID
            public string id { get; set; }
            public Agent[] agents { get; set; }
            public Point point { get; set; }
            public PlayerType type { get; set; }
        }

        public record Agent()
        {
            // 現在のエージェントのx座標
            public int x { get; set; }
            // 現在のエージェントのy座標
            public int y { get; set; }
        }

        /**
         * プレイヤータイプ
         */
        public enum PlayerType
        {
            ACCOUNT,
            GUEST
        }

        public record Field()
        {
            // フィールドの幅
            public int width { get; set; }
            // フィールドの高さ
            public int height { get; set; }
            // 各マスのポイント
            public int[] points { get; set; }
            // マスの状態
            public Tile[] tiles { get; set; }
        }

        public record Tile()
        {
            // マスのタイプ
            // playerがnullの場合は値に関係なく空白マス
            public TileType type { get; set; }
            // マスを所持するプレイヤーID
            // nullの場合は空白マス
            public int? player { get; set; }
        }

        /**
         * マスのタイプ
         */
        public enum TileType
        {
            // 陣地
            POSITION = 0,
            // 壁
            WALL = 1
        }

        /**
         * ゲームタイプ
         */
        public enum GameType
        {
            NORMAL,
            SELF,
            PESONAL
        }

        public async Task<SendActionResponse> sendActions(ConnectionInfo connectionInfo, SendActionInfo sendActionInfo)
        {
            string url = $"{baseUrl}/v1/matches/{connectionInfo.gameId}/actions";

            // HttpRequestMessageを作成
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, url);
            // ヘッダーを設定
            request.Headers.Add("Authorization", connectionInfo.pic);
            // ボディを設定
            StringContent? content = new StringContent(JsonSerializer.Serialize(sendActionInfo, jsonOptions));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = content;
            // リクエストを送信
            HttpResponseMessage response = await client.SendAsync(request);
            string? startResponseBody = await response.Content.ReadAsStringAsync();
            JsonElement game = JsonDocument.Parse(startResponseBody).RootElement;
            return JsonSerializer.Deserialize<SendActionResponse>(game.GetRawText());
        }

        /**
         * プレイの詳細情報
         */
        public record SendActionInfo()
        {
            // 行動情報の配列
            public SendAction[] actions { get; set; }
            // trueにするとAPIのテストができます
            public bool dryRun { get; set; }
        }

        public record SendAction()
        {
            public int agentId { get; set; }
            public SendActionType type { get; set; }
            public int x { get; set; }
            public int y { get; set; }
        }

        /**
         * 行動のタイプ
         */
        public enum SendActionType
        {
            PUT,
            MOVE,
            REMOVE
        }

        public record SendActionResponse
        {
            // サーバにて受信した時刻(UNIX時間)
            public int receptionUnixTime { get; set; }
            // 行動が適用されるターン
            public int turn { get; set; }
        }
    }
}


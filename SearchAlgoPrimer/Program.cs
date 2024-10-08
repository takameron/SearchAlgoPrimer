﻿using SearchAlgoPrimer;
using State = SearchAlgoPrimer.MazeState;
using ScoreType = System.Int64;
using static SearchAlgoPrimer.MazeState;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        const ScoreType INF = 1000000000L;
 
        /// <summary>
        /// ランダムに行動を決定する
        /// </summary>
        /// <param name="state">盤面</param>
        /// <param name="index">キャラクタのインデックス</param>
        /// <returns></returns>
        static int randomAction(State state, int index)
        {
            var legal_actions = state.legalActions(index);
            Random mt_for_action = new Random(0);
            return legal_actions[mt_for_action.Next() % (legal_actions.Count())];
        }

        // 貪欲法で行動を決定する
        static int greedyAction(State state)
        {
            var legal_actions = state.legalActions();
            ScoreType best_score = -INF; // 絶対にありえない小さな値でベストスコアを初期化する
            int best_action = -1;        // ありえない行動で初期化する
            foreach (var action in legal_actions)
            {
                State now_state = state.copy();
                now_state.advance(action);
                now_state.evaluateScore();
                if (now_state.evaluated_score_ > best_score)
                {
                    best_score = now_state.evaluated_score_;
                    best_action = action;
                }
            }
            return best_action;
        }

        // ビームサーチで行動を決定する
        static int beamSearchAction(State state, int beam_width, int beam_depth, int index)
        {
            var now_beam = new PriorityQueue<State, State>(new MazeStateComparer());
            State best_state = state.copy();

            now_beam.Enqueue(state, state);
            for (int depth = 0; depth < beam_depth; depth++)
            {
                var next_beam = new PriorityQueue<State, State>(new MazeStateComparer());
                for (int width = 0; width < beam_width; width++)
                {
                    if (now_beam.Count == 0)
                    {
                        break;
                    }

                    var now_state = now_beam.Dequeue();
                    var legal_actions = now_state.legalActions(index);
                    foreach (var action in legal_actions)
                    {
                        State next_state = now_state.copy();
                        next_state.advance(action, index);
                        next_state.evaluateScore();
                        if (depth == 0)
                        {
                            next_state.first_action_ = action;
                        }
                        next_beam.Enqueue(next_state, next_state);
                    }
                }
                now_beam = next_beam;
                best_state = now_beam.Peek();
                if (best_state.isDone())
                {
                    break;
                }
            }
            return best_state.first_action_;
        }

        // シードを指定してゲーム状況を表示しながらAIにプレイさせる。
        static void playGame(int seed)
        {
            var state = new State(seed);
            Console.WriteLine(state.ToString());
            while (!state.isDone())
            {
                //state.advance(randomAction(state));
                //state.advance(greedyAction(state));
                state.advance(beamSearchAction(state, 2, 4, 0));
                Console.WriteLine(state.ToString());
            }
        }

        static async void playKakomimasu()
        {
            KakomimasuClient client = new KakomimasuClient();
            // 参加する
            var connectionInfo = client.join().Result;
            // 自身のプレイヤー番号を取得
            int OWN_PLAYER = connectionInfo.index;
            // 開始時刻を取得できるまで待つ
            var playVerbose = client.waitStart(connectionInfo).Result;
            var start = playVerbose.startedAtUnixTime;
            var opsec = playVerbose.operationSec;
            var trsec = playVerbose.transitionSec;
            // 1ターン目を待つ
            DateTimeOffset now = DateTimeOffset.UtcNow;
            int sleepTime = (int)Math.Max((long)start * 1000 - now.ToUnixTimeMilliseconds(), 0);
            Thread.Sleep(sleepTime);
            while (true)
            {
                // ターン情報を取得
                playVerbose = client.getPlayVerbose(connectionInfo).Result;
                // ゲームが終了していたら打ち切る
                if (playVerbose.status == KakomimasuClient.GameStatus.ENDED)
                {
                    return;
                }

                // 盤面の情報を取得する
                int height = playVerbose.field.height;
                int width = playVerbose.field.width;
                var points = new int[height, width];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var tile = playVerbose.field.tiles[y * width + x];
                        int point = playVerbose.field.points[y * width + x];
                        int convPoint = tile.player == OWN_PLAYER && tile.type == KakomimasuClient.TileType.WALL ? 0 : point;
                        points[y, x] = convPoint;
                    }
                }
                // 得点を取得する
                var pointInfo = playVerbose.players[OWN_PLAYER].point;
                int gameScore = pointInfo.areaPoint + pointInfo.wallPoint;
                // 盤面を変換する
                List<Coord> characters = new List<Coord>{};
                foreach (var a in playVerbose.players[OWN_PLAYER].agents) {
                    var character = new Coord {
                        x_ = a.x,
                        y_ = a.y
                    };
                    characters.Add(character);
                }
                var state = new State(width, height, playVerbose.totalTurn, playVerbose.turn, points, gameScore, characters);

                // 送信する行動情報のリスト
                var sendActions = new List<KakomimasuClient.SendAction> { };

                int index = 0;
                foreach (var agent in playVerbose.players[OWN_PLAYER].agents) {
                    // 強制的に1エージェントで動かす
                    // if (index != 0) continue;

                    // 初期座標であれば適当な場所に置く
                    if (agent.x == -1 || agent.y == -1)
                    {
                        Random rnd = new();
                        var firstAction = new KakomimasuClient.SendAction()
                        {
                            agentId = index,
                            type = KakomimasuClient.SendActionType.PUT,
                            x = rnd.Next(playVerbose.field.width),
                            y = rnd.Next(playVerbose.field.height)
                        };
                        sendActions.Add(firstAction);
                    }
                    // それ以外はビームサーチで行動を決定する
                    else
                    {
                        // 行動を決定する
                        var action = beamSearchAction(state, 2, 4, index);
                        // 行動を変換する
                        var nx = state.characters[index].x_ + State.dx[action];
                        var ny = state.characters[index].y_ + State.dy[action];
                        var type = playVerbose.field.tiles[ny * width + nx].type == KakomimasuClient.TileType.WALL && playVerbose.field.tiles[ny * width + nx].player != OWN_PLAYER ? KakomimasuClient.SendActionType.REMOVE : KakomimasuClient.SendActionType.MOVE;
                        // 前回と同じ座標だったら移動しない(sendActionsは前回の行動が入っている)
                        if (sendActions.Count >= index+1 && nx == sendActions[index].x && ny == sendActions[index].y && type == KakomimasuClient.SendActionType.MOVE) 
                        {
                            nx = state.characters[index].x_;
                            ny = state.characters[index].y_;
                        }
                        var kakomimasuAction = new KakomimasuClient.SendAction()
                        {
                            agentId = index,
                            type = type,
                            x = nx,
                            y = ny
                        };
                        sendActions.Add(kakomimasuAction);
                    }
                    index++;
                }
                // 盤面を表示
                Console.WriteLine(state.ToString());
                // 動きを送信する
                var sendActionInfo = new KakomimasuClient.SendActionInfo();
                sendActionInfo.dryRun = false;
                sendActionInfo.actions = sendActions.ToArray();
                var _ = client.sendActions(connectionInfo, sendActionInfo).Result;
                // 待つ
                now = DateTime.Now;
                sleepTime = (int)Math.Max(((long)start + (opsec + trsec) * playVerbose.turn) * 1000 - now.ToUnixTimeMilliseconds(), 0);
                Thread.Sleep(sleepTime);
            }
        }

        static void Main(string[] args)
        {
            // playGame(/*盤面初期化のシード*/ 121322);
            playKakomimasu();
        }
    }
}
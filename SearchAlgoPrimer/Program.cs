using System;
using SearchAlgoPrimer;
using State = SearchAlgoPrimer.MazeState;
using ScoreType = System.Int64;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        const ScoreType INF = 1000000000L;

        // ランダムに行動を決定する
        static int randomAction(State state)
        {
            var legal_actions = state.legalActions();
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
                State now_state = state;
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

        // シードを指定してゲーム状況を表示しながらAIにプレイさせる。
        static void playGame(int seed)
        {
            var state = new State(seed);
                Console.WriteLine(state.ToString());
            while (!state.isDone())
            {
                //state.advance(randomAction(state));
                state.advance(greedyAction(state));
                Console.WriteLine(state.ToString());
            }
        }

        static void Main(string[] args)
        {
            playGame(/*盤面初期化のシード*/ 121322);
        }
    }
}
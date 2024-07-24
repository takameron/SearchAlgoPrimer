using System;
using SearchAlgoPrimer;
using State = SearchAlgoPrimer.MazeState;
using ScoreType = System.Int64;
using static SearchAlgoPrimer.MazeState;

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
        static int beamSearchAction(State state, int beam_width, int beam_depth)
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
                    var legal_actions = now_state.legalActions();
                    foreach (var action in legal_actions)
                    {
                        State next_state = now_state.copy();
                        next_state.advance(action);
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
                state.advance(beamSearchAction(state, 2, END_TURN));
                Console.WriteLine(state.ToString());
            }
        }

        static void Main(string[] args)
        {
            playGame(/*盤面初期化のシード*/ 121322);
        }
    }
}
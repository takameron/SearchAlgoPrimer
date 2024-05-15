using System;
using SearchAlgoPrimer;
using State = SearchAlgoPrimer.MazeState;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        // ランダムに行動を決定する
        static int randomAction(State state)
        {
            var legal_actions = state.legalActions();
            Random mt_for_action = new Random(0);
            return legal_actions[mt_for_action.Next() % (legal_actions.Count())];
        }

        // シードを指定してゲーム状況を表示しながらAIにプレイさせる。
        static void playGame(int seed)
        {
            var state = new State(seed);
                Console.WriteLine(state.ToString());
            while (!state.isDone())
            {
                state.advance(randomAction(state));
                    Console.WriteLine(state.ToString());
                }
        }

        static void Main(string[] args)
        {
            playGame(/*盤面初期化のシード*/ 121322);
        }
    }
}
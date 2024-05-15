using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace SearchAlgoPrimer
{

    internal class MazeState
    {
        public const int H = 3;
        public const int W = 4;
        const int END_TURN = 4;

        private static int[] dx = new int[]{ 1, -1, 0, 0 }; // 右、左、下、上への移動方向のx成分
        private static int[] dy = new int[] { 0, 0, 1, -1 }; // 右、左、下、上への移動方向のy成分
        private int[,] points_ = new int[H,W];
        private int turn_ = 0;

        public Coord character_ = new Coord();
        public int game_score_ = 0;
        public MazeState() {}

        public MazeState(int seed)
        {
            Random rand = new Random(seed);
            this.character_.y_ = rand.Next(H);
            this.character_.x_ = rand.Next(W);

            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    if (y == character_.y_ && x == character_.x_)
                    {
                        continue;
                    }
                    this.points_[y,x] = rand.Next(10);
                }
        }

        // [どのゲームでも実装する] : ゲームの終了判定
        public bool isDone()
        {
            return this.turn_ == END_TURN;
        }

        // [どのゲームでも実装する] : 指定したactionでゲームを1ターン進める
        public void advance(int action)
        {
            this.character_.x_ += dx[action];
            this.character_.y_ += dy[action];
            var point = this.points_[this.character_.y_, this.character_.x_];
            if (point > 0)
            {
                this.game_score_ += point;
                this.points_[this.character_.y_, this.character_.x_] = 0;
            }
            this.turn_++;
        }

        // [どのゲームでも実装する] : 現在の状況でプレイヤーが可能な行動を全て取得する
        public List<int> legalActions()
        {
            List<int> actions = new List<int>();
            for (int action = 0; action < 4; action++)
            {
                int ty = this.character_.y_ + dy[action];
                int tx = this.character_.x_ + dx[action];
                if (ty >= 0 && ty < H && tx >= 0 && tx < W)
                {
                    actions.Add(action);
                }
            }
            return actions;
        }

        override
        public string ToString()
        {
            string ss = "";
            ss += $"turn:\t {this.turn_}\n";
            ss += $"score:\t {this.game_score_}\n";

            for (int h = 0; h < H; h++)
            {
                for (int w = 0; w < W; w++)
                {
                    if (this.character_.y_ == h && this.character_.x_ == w)
                    {
                        ss += '@';
                    }
                    else if (this.points_[h,w] > 0)
                    {
                        ss += points_[h,w];
                    }
                    else
                    {
                        ss += '.';
                    }
                }
                ss += '\n';
            }
            return ss;
        }

    public struct Coord
        {
            public Coord(int x, int y)
            {
                y_ = y;
                x_ = x;
            }

            public int y_ { get; set; }
            public int x_ { get; set; }

            public override string ToString() => $"({y_}, {x_})";
        }
    }
}

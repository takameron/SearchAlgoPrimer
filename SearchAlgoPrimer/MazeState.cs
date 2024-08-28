using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;
using ScoreType = System.Int64;

namespace SearchAlgoPrimer
{

    internal class MazeState
    {
        public int H = 3;
        public int W = 4;
        public int END_TURN = 4;

        public static int[] dx = new int[]{ 1, -1, 0, 0 }; // 右、左、下、上への移動方向のx成分
        public static int[] dy = new int[] { 0, 0, 1, -1 }; // 右、左、下、上への移動方向のy成分
        public int[,] points_;
        private int turn_ = 0;

        public Coord character_ = new Coord();
        public int game_score_ = 0;
        public ScoreType evaluated_score_ = 0; // 探索上で評価したスコア
        public int first_action_ = -1; // 最初に選択した行動

        public MazeState(int seed)
        {
            this.points_ = new int[H, W];
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

        public MazeState(int width, int height, int endTern, int turn, int character_x, int character_y, int[,] points, int game_score)
        {
            this.W = width;
            this.H = height;
            this.END_TURN = endTern;
            this.turn_ = turn;
            this.character_.x_ = character_x;
            this.character_.y_ = character_y;
            this.points_ = (int[,])points.Clone();
            this.game_score_ = game_score;
        }


        // [どのゲームでも実装する] : ゲームの終了判定
        public bool isDone()
        {
            return this.turn_ == END_TURN;
        }

        // [どのゲームでも実装する] : 探索用の盤面評価をする
        public void evaluateScore()
        {
            this.evaluated_score_ = this.game_score_; // 簡単のため、まずはゲームスコアをそのまま盤面の評価とする
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

        public class MazeStateComparer : IComparer<MazeState>
        {
            public int Compare(MazeState x, MazeState y)
            {
                // xとyを反転させることで、数字が大きいほうが優先度が高い
                return y.evaluated_score_.CompareTo(x.evaluated_score_);
            }
        }

        override
        public string ToString()
        {
            int padding = 3;
            string ss = "";
            ss += $"turn:\t {this.turn_}\n";
            ss += $"score:\t {this.game_score_}\n";

            for (int h = 0; h < H; h++)
            {
                for (int w = 0; w < W; w++)
                {
                    if (this.character_.y_ == h && this.character_.x_ == w)
                    {
                        ss += '@'.ToString().PadLeft(padding);
                    }
                    else if (this.points_[h,w] > 0)
                    {
                        ss += points_[h,w].ToString().PadLeft(padding);
                    }
                    else
                    {
                        ss += '.'.ToString().PadLeft(padding);
                    }
                }
                ss += '\n';
            }
            return ss;
        }

        /**
         * 複製する
         */
        public MazeState copy()
        {
            MazeState state = (MazeState)MemberwiseClone();
            state.points_ = (int[,])this.points_.Clone();
            return state;
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

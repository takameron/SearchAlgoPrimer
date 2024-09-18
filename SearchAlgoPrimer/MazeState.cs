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

        public List<Coord> characters;
        public int game_score_ = 0;
        public ScoreType evaluated_score_ = 0; // 探索上で評価したスコア
        public int first_action_ = -1; // 最初に選択した行動

        public MazeState(int seed)
        {
            this.points_ = new int[H, W];
            Random rand = new Random(seed);
            this.characters = new List<Coord>();
            Coord character = new Coord {
                y_ = rand.Next(H),
                x_ = rand.Next(W)
            };
            this.characters.Add(character);

            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    if (y == character.y_ && x == character.x_)
                    {
                        continue;
                    }
                    this.points_[y,x] = rand.Next(10);
                }
        }

        public MazeState(int width, int height, int endTern, int turn, int[,] points, int game_score, List<Coord> characters)
        {
            this.W = width;
            this.H = height;
            this.END_TURN = endTern;
            this.turn_ = turn;
            this.characters = characters;
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

        /// <summary>
        /// [どのゲームでも実装する] : 指定したactionでゲームを1ターン進める
        /// </summary>
        /// <param name="action">行動</param>
        /// <param name="index">キャラクタのインデックス</param>
        public void advance(int action, int index = 0)
        {
            Coord character = this.characters[index];
            character.x_ += dx[action];
            character.y_ += dy[action];
            this.characters[index] = character;
            var point = this.points_[character.y_, character.x_];
            if (point > 0)
            {
                this.game_score_ += point;
                this.points_[character.y_, character.x_] = 0;
            }
            this.turn_++;
        }

        /// <summary>
        /// [どのゲームでも実装する] : 現在の状況でプレイヤーが可能な行動を全て取得する
        /// </summary>
        /// <param name="index">キャラクタのインデックス</param>
        /// <returns></returns>
        public List<int> legalActions(int index = 0)
        {
            List<int> actions = new List<int>();
            for (int action = 0; action < 4; action++)
            {

                int ty = this.characters[index].y_ + dy[action];
                int tx = this.characters[index].x_ + dx[action];
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
                    if (this.characters.Where(character => character.y_ == h && character.x_ == w).Any())
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
            state.characters = new List<Coord> (this.characters);
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

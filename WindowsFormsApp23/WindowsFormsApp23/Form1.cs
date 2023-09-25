using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp23
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = GenerateMaze();
            lblResult.Text = result;
        }


        private string GenerateMaze()
        {
            var rowCnt = 21;
            var colCnt = 21;

            (var map, var mainAisle) = GenerateMainAisle(rowCnt, colCnt);

            if (map == null)
            {
                return "Failed to generate main aisle";
            }

            GenerateSubAisle(map, mainAisle);

            DrawMap(map, true);
            return "completed";
        }

        private List<char[]> GetInitializedMap(int rowCnt, int colCnt)
        {
            var map = new List<char[]>();

            for (int row = 0; row < rowCnt; row++)
            {
                var cols = new char[colCnt];
                map.Add(cols);
                for (int col = 0; col < colCnt; col++)
                {
                    cols[col] = (row % 2 == 0 || col % 2 == 0) ? '@' : '*';
                }
            }

            return map;
        }

        private (List<char[]> map, List<int[]> aisle) GenerateMainAisle(int rowCnt, int colCnt)
        {
            var lastRowIdx = rowCnt - 1;
            var lastColIdx = colCnt - 1;

            var aisle = new List<int[]>();
            List<char[]> map = null;
            int goalCol = lastColIdx - 1;
            int goalRow = lastRowIdx - 1;

            var triedCnt = 0;
            var msg = string.Empty;
            while (msg != "GOAL" && ++triedCnt < 200)
            {
                map = GetInitializedMap(rowCnt, colCnt);
                aisle.Clear();

                msg = GenerateAisle(map, aisle, 1, 1, goalRow, goalCol);
            }

            if (msg == "GOAL")
            {
                return (map, aisle);
            }
            else
            {
                return (null, null);
            }
        }

        private void GenerateSubAisle(List<char[]> map, List<int[]> aisle)
        {
            int aisleIdx = 0;

            var rand = new Random();
            var steps = rand.Next(2, 5);
            while (aisleIdx + steps < aisle.Count - 1)
            {
                aisleIdx += steps;
                int currRow = aisle[aisleIdx][0];
                int currCol = aisle[aisleIdx][1];

                var subAisle = new List<int[]>();
                GenerateAisle(map, subAisle, currRow, currCol, -1, -1);

                GenerateSubAisle(map, subAisle);

                steps = rand.Next(2, 5);
            }
        }

        private string GenerateAisle(List<char[]> map, List<int[]> aisle, int currRow, int currCol, int goalRow, int goalCol)
        {
            var rowCnt = map.Count;
            var colCnt = map[0].Length;

            var lastRowIdx = rowCnt - 1;
            var lastColIdx = colCnt - 1;

            const int up = 0;
            const int rg = 1;
            const int dn = 2;
            const int lf = 3;

            var rand = new Random();
            var usedDir = new bool[4];
            var step = 0;
            map[currRow][currCol] = ' ';
            DrawMap(map, true);
            aisle?.Add(new int[] { currRow, currCol });

            while (step++ < 10000)
            {
                if (usedDir.All(x => x == true))
                {
                    // どんづまり
                    Console.WriteLine("NG_STACK");
                    return "NG_STACK";
                }

                var dir = rand.Next(0, 4);

                while (usedDir[dir])
                {
                    dir = rand.Next(0, 4);
                }
                usedDir[dir] = true;

                int dirIncCol = 0;
                int dirIncRow = 0;
                switch (dir)
                {
                    case up:
                        dirIncRow = -1;
                        break;
                    case rg:
                        dirIncCol = 1;
                        break;
                    case dn:
                        dirIncRow = 1;
                        break;
                    case lf:
                        dirIncCol = -1;
                        break;
                }
                Console.WriteLine($"row:{currRow} col:{currCol} dir:{dir}");

                var nextCol = currCol + dirIncCol;
                var nextRow = currRow + dirIncRow;
                var nextCol_d = currCol + dirIncCol * 2;
                var nextRow_d = currRow + dirIncRow * 2;

                if (nextCol_d <= 0 || nextCol_d >= lastColIdx ||
                    nextRow_d <= 0 || nextRow_d >= lastRowIdx)
                {
                    // 先に進めない
                    Console.WriteLine("NG_1");
                    continue;
                }
                if (map[nextRow_d][nextCol_d] == ' ')
                {
                    // 通路が一本道でなくなってしまう
                    Console.WriteLine("NG_2");
                    continue;
                }

                var up_not_used = (map[nextRow - 1][nextCol] != ' ');
                var rg_not_used = (map[nextRow][nextCol + 1] != ' ');
                var dn_not_used = (map[nextRow + 1][nextCol] != ' ');
                var lf_not_used = (map[nextRow][nextCol - 1] != ' ');

                if ((dir != up && up_not_used && rg_not_used && lf_not_used) ||
                    (dir != rg && up_not_used && rg_not_used && dn_not_used) ||
                    (dir != dn && rg_not_used && dn_not_used && lf_not_used) ||
                    (dir != lf && up_not_used && dn_not_used && lf_not_used))
                {
                    // 通路が線で面になってしまう
                    Console.WriteLine("NG_3");
                    continue;
                }

                map[nextRow][nextCol] = ' '; // 壁に穴をあける
                map[nextRow_d][nextCol_d] = ' '; // 壁に穴をあける
                DrawMap(map, false);

                //通路の次の位置
                currCol = nextCol_d;
                currRow = nextRow_d;
                aisle?.Add(new int[] { currRow, currCol });
                foreach (var diridx in Enumerable.Range(0, usedDir.Length))
                {
                    usedDir[diridx] = false;
                }

                if (currCol == goalCol && currRow == goalRow)
                {
                    // ゴールについた
                    return "GOAL";
                }
            }

            return "NG_OVER";

        }

        private void DrawMap(List<char[]> map, bool drawToImage)
        {
            var rowCnt = map.Count;
            var colCnt = map[0].Length;

            // マップ表示
            Console.WriteLine("");
            for (int row = 0; row < rowCnt; row++)
            {
                for (int col = 0; col < colCnt; col++)
                {
                    Console.Write(map[row][col]);
                }
                Console.WriteLine("");
            }

            picMaze.Image?.Dispose();
            picMaze.Image = null;

            if (drawToImage == false)
            {
                return;
            }

            var bitMap = new Bitmap(picMaze.Width, picMaze.Height);
            var rectWid = bitMap.Width / colCnt;
            var rectHig = bitMap.Height / rowCnt;
            var g = Graphics.FromImage(bitMap);
            g.FillRectangle(Brushes.Red, 0, 0, bitMap.Width, bitMap.Height);
            for (int row = 0; row < rowCnt; row++)
            {
                for (int col = 0; col < colCnt; col++)
                {
                    int x = col * rectWid;
                    int y = row * rectHig;

                    if (map[row][col] == ' ')
                    {
                        g.FillRectangle(Brushes.White, x, y, rectWid, rectHig);
                    }
                }
            }
            g.DrawString("S", this.Font, Brushes.Black, (1) * rectWid + 5, (1) * rectHig + 5);
            g.DrawString("G", this.Font, Brushes.Black, (colCnt - 2) * rectWid + 5, (rowCnt - 2) * rectHig + 5);
            picMaze.Image = bitMap;
        }
    }
}

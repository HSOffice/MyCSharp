using System;
using System.Threading;

namespace TetrisGame
{
    class Program
    {
        static void Main()
        {
            Console.CursorVisible = false;
            var game = new Game();
            game.Run();
        }
    }

    class Game
    {
        private const int Width = 10;
        private const int Height = 20;
        private readonly int[,] _board = new int[Height, Width];
        private Tetromino _current = Tetromino.CreateRandom();
        private readonly Random _random = new Random();

        public void Run()
        {
            _current.X = Width / 2 - 2;
            _current.Y = 0;
            DateTime lastDrop = DateTime.Now;

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    HandleInput(key);
                }

                if ((DateTime.Now - lastDrop).TotalMilliseconds >= 500)
                {
                    if (!Move(0, 1))
                    {
                        FixTetromino();
                        ClearLines();
                        SpawnNewTetromino();
                        if (!IsValidPosition(_current, _current.X, _current.Y))
                        {
                            break;
                        }
                    }
                    lastDrop = DateTime.Now;
                }

                Draw();
                Thread.Sleep(50);
            }

            Console.SetCursorPosition(0, Height + 2);
            Console.WriteLine("Game Over");
        }

        private void HandleInput(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    Move(-1, 0);
                    break;
                case ConsoleKey.RightArrow:
                    Move(1, 0);
                    break;
                case ConsoleKey.DownArrow:
                    Move(0, 1);
                    break;
                case ConsoleKey.UpArrow:
                    Rotate();
                    break;
            }
        }

        private void SpawnNewTetromino()
        {
            _current = Tetromino.CreateRandom();
            _current.X = Width / 2 - 2;
            _current.Y = 0;
        }

        private bool Move(int dx, int dy)
        {
            if (IsValidPosition(_current, _current.X + dx, _current.Y + dy))
            {
                _current.X += dx;
                _current.Y += dy;
                return true;
            }
            return false;
        }

        private void Rotate()
        {
            var rotated = _current.GetRotated();
            if (IsValidPosition(rotated, _current.X, _current.Y))
            {
                _current = rotated;
            }
        }

        private bool IsValidPosition(Tetromino tetromino, int x, int y)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (tetromino.Shape[i, j] == 0) continue;
                    int newX = x + j;
                    int newY = y + i;
                    if (newX < 0 || newX >= Width || newY < 0 || newY >= Height)
                        return false;
                    if (_board[newY, newX] == 1)
                        return false;
                }
            }
            return true;
        }

        private void FixTetromino()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (_current.Shape[i, j] == 1)
                    {
                        int x = _current.X + j;
                        int y = _current.Y + i;
                        if (y >= 0 && y < Height && x >= 0 && x < Width)
                        {
                            _board[y, x] = 1;
                        }
                    }
                }
            }
        }

        private void ClearLines()
        {
            for (int y = Height - 1; y >= 0; y--)
            {
                bool full = true;
                for (int x = 0; x < Width; x++)
                {
                    if (_board[y, x] == 0)
                    {
                        full = false;
                        break;
                    }
                }
                if (full)
                {
                    for (int row = y; row > 0; row--)
                    {
                        for (int col = 0; col < Width; col++)
                        {
                            _board[row, col] = _board[row - 1, col];
                        }
                    }
                    for (int col = 0; col < Width; col++)
                    {
                        _board[0, col] = 0;
                    }
                    y++; // recheck same line
                }
            }
        }

        private void Draw()
        {
            Console.SetCursorPosition(0, 0);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    bool occupied = _board[y, x] == 1 || IsCurrentCell(x, y);
                    Console.Write(occupied ? "#" : ".");
                }
                Console.WriteLine();
            }
        }

        private bool IsCurrentCell(int x, int y)
        {
            int relX = x - _current.X;
            int relY = y - _current.Y;
            if (relX >= 0 && relX < 4 && relY >= 0 && relY < 4)
            {
                return _current.Shape[relY, relX] == 1;
            }
            return false;
        }
    }

    class Tetromino
    {
        public int[,] Shape { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }

        private Tetromino(int[,] shape)
        {
            Shape = shape;
        }

        public static Tetromino CreateRandom()
        {
            int[][][] shapes = new int[][][]
            {
                new int[][] { new[]{1,1,1,1}, new[]{0,0,0,0}, new[]{0,0,0,0}, new[]{0,0,0,0} }, // I
                new int[][] { new[]{1,1}, new[]{1,1}, new[]{0,0}, new[]{0,0} }, // O
                new int[][] { new[]{0,1,0}, new[]{1,1,1}, new[]{0,0,0}, new[]{0,0,0} }, // T
                new int[][] { new[]{0,1,1}, new[]{1,1,0}, new[]{0,0,0}, new[]{0,0,0} }, // S
                new int[][] { new[]{1,1,0}, new[]{0,1,1}, new[]{0,0,0}, new[]{0,0,0} }, // Z
                new int[][] { new[]{1,0,0}, new[]{1,1,1}, new[]{0,0,0}, new[]{0,0,0} }, // J
                new int[][] { new[]{0,0,1}, new[]{1,1,1}, new[]{0,0,0}, new[]{0,0,0} }  // L
            };

            var rand = new Random();
            int index = rand.Next(shapes.Length);
            var grid = new int[4,4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    grid[i,j] = (j < shapes[index][i].Length) ? shapes[index][i][j] : 0;
                }
            }
            return new Tetromino(grid);
        }

        public Tetromino GetRotated()
        {
            var rotated = new int[4,4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    rotated[j, 3 - i] = Shape[i, j];
                }
            }
            return new Tetromino(rotated) { X = this.X, Y = this.Y };
        }
    }
}


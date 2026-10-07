using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToeMinimax
{
    class Program
    {
        static char[,] board = new char[3, 3];
        static char human = 'X';
        static char ai = 'O';
        static int difficulty = 3; // 1-Easy, 2-Medium, 3-Hard
        static Random rnd = new Random();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Крестики-нолики с ИИ (Minimax) ===\n");
            
            while (true)
            {
                ChooseDifficulty();
                InitBoard();
                PlayGame();
                
                Console.Write("\nСыграть ещё раз? (y/n): ");
                string again = Console.ReadLine()?.Trim().ToLower();
                if (again != "y" && again != "д" && again != "yes")
                    break;
                Console.WriteLine();
            }
            
            Console.WriteLine("Спасибо за игру!");
        }

        static void ChooseDifficulty()
        {
            Console.WriteLine("Выберите уровень сложности:");
            Console.WriteLine("1 - Лёгкий (случайные ходы + слабый ИИ)");
            Console.WriteLine("2 - Средний (ограниченная глубина Minimax)");
            Console.WriteLine("3 - Сложный (полный Minimax, непобедимый)");
            Console.Write("Ваш выбор (1-3): ");
            
            while (true)
            {
                string input = Console.ReadLine()?.Trim();
                if (int.TryParse(input, out int d) && d >= 1 && d <= 3)
                {
                    difficulty = d;
                    break;
                }
                Console.Write("Неверный ввод. Введите 1, 2 или 3: ");
            }
            
            string[] names = { "", "Лёгкий", "Средний", "Сложный" };
            Console.WriteLine($"\nУровень: {names[difficulty]}");
            Console.WriteLine("Вы играете крестиками (X), ИИ — ноликами (O)\n");
        }

        static void InitBoard()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    board[i, j] = ' ';
        }

        static void PlayGame()
        {
            bool humanTurn = true;
            
            while (true)
            {
                PrintBoard();
                
                if (CheckWinner(human))
                {
                    Console.WriteLine("🎉 Вы победили!");
                    return;
                }
                if (CheckWinner(ai))
                {
                    Console.WriteLine("🤖 ИИ победил!");
                    return;
                }
                if (IsBoardFull())
                {
                    Console.WriteLine("🤝 Ничья!");
                    return;
                }

                if (humanTurn)
                {
                    HumanMove();
                }
                else
                {
                    Console.WriteLine("Ход ИИ...");
                    AiMove();
                }
                
                humanTurn = !humanTurn;
            }
        }

        static void PrintBoard()
        {
            Console.WriteLine();
            Console.WriteLine("  1 2 3");
            for (int i = 0; i < 3; i++)
            {
                Console.Write($"{i + 1} ");
                for (int j = 0; j < 3; j++)
                {
                    Console.Write(board[i, j]);
                    if (j < 2) Console.Write("|");
                }
                Console.WriteLine();
                if (i < 2) Console.WriteLine("  -+-+-");
            }
            Console.WriteLine();
        }

        static void HumanMove()
        {
            while (true)
            {
                Console.Write("Ваш ход (строка столбец, например 1 2): ");
                string input = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(input)) continue;
                
                string[] parts = input.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int row) &&
                    int.TryParse(parts[1], out int col) &&
                    row >= 1 && row <= 3 && col >= 1 && col <= 3)
                {
                    row--; col--;
                    if (board[row, col] == ' ')
                    {
                        board[row, col] = human;
                        return;
                    }
                    Console.WriteLine("Клетка уже занята!");
                }
                else
                {
                    Console.WriteLine("Неверный ввод. Пример: 2 3");
                }
            }
        }

        static void AiMove()
        {
            if (difficulty == 1)
            {
                // Easy: 70% random, 30% minimax depth 2
                if (rnd.NextDouble() < 0.7)
                {
                    RandomMove();
                    return;
                }
            }

            int bestScore = int.MinValue;
            int bestRow = -1, bestCol = -1;
            int maxDepth = difficulty == 1 ? 2 : (difficulty == 2 ? 4 : 9);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == ' ')
                    {
                        board[i, j] = ai;
                        int score = Minimax(0, false, maxDepth, int.MinValue, int.MaxValue);
                        board[i, j] = ' ';
                        
                        // Для среднего уровня добавляем немного случайности
                        if (difficulty == 2 && rnd.NextDouble() < 0.15)
                            score += rnd.Next(-2, 3);
                        
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestRow = i;
                            bestCol = j;
                        }
                    }
                }
            }

            if (bestRow != -1)
            {
                board[bestRow, bestCol] = ai;
            }
            else
            {
                RandomMove(); // fallback
            }
        }

        static void RandomMove()
        {
            var empty = new List<(int, int)>();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[i, j] == ' ')
                        empty.Add((i, j));
            
            if (empty.Count > 0)
            {
                var move = empty[rnd.Next(empty.Count)];
                board[move.Item1, move.Item2] = ai;
            }
        }

        // Minimax with Alpha-Beta pruning
        static int Minimax(int depth, bool isMaximizing, int maxDepth, int alpha, int beta)
        {
            if (CheckWinner(ai)) return 10 - depth;
            if (CheckWinner(human)) return depth - 10;
            if (IsBoardFull() || depth >= maxDepth) return 0;

            if (isMaximizing)
            {
                int maxEval = int.MinValue;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (board[i, j] == ' ')
                        {
                            board[i, j] = ai;
                            int eval = Minimax(depth + 1, false, maxDepth, alpha, beta);
                            board[i, j] = ' ';
                            maxEval = Math.Max(maxEval, eval);
                            alpha = Math.Max(alpha, eval);
                            if (beta <= alpha) return maxEval;
                        }
                    }
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (board[i, j] == ' ')
                        {
                            board[i, j] = human;
                            int eval = Minimax(depth + 1, true, maxDepth, alpha, beta);
                            board[i, j] = ' ';
                            minEval = Math.Min(minEval, eval);
                            beta = Math.Min(beta, eval);
                            if (beta <= alpha) return minEval;
                        }
                    }
                }
                return minEval;
            }
        }

        static bool CheckWinner(char player)
        {
            // Rows
            for (int i = 0; i < 3; i++)
                if (board[i, 0] == player && board[i, 1] == player && board[i, 2] == player)
                    return true;
            
            // Columns
            for (int j = 0; j < 3; j++)
                if (board[0, j] == player && board[1, j] == player && board[2, j] == player)
                    return true;
            
            // Diagonals
            if (board[0, 0] == player && board[1, 1] == player && board[2, 2] == player)
                return true;
            if (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player)
                return true;
            
            return false;
        }

        static bool IsBoardFull()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[i, j] == ' ')
                        return false;
            return true;
        }
    }
}


using System;
using System.Collections.Generic;

namespace Eight_Puzzle
{
    class Program
    {
        static void Main(string[] args)
        {
            int[,] start_state = {
                {0, 6, 8},
                {7, 4, 2},
                {3, 5, 1}
            };
            Console.Write("Eight Puzzle Problem-\n\nSolved using A* algorithm\n\nInitial State : \n\n");
            File.WriteAllText("output.txt", "Eight Puzzle Problem-\n\nSolved using A* algorithm\n\nInitial State : \n\n");
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Console.Write(start_state[i, j] + " ");
                    File.AppendAllText("output.txt", start_state[i, j] + " ");
                }
                Console.WriteLine();
                File.AppendAllText("output.txt", "\n");
            }
            var clock = System.Diagnostics.Stopwatch.StartNew();
            Generate_soln(start_state);
            clock.Stop();
            Console.WriteLine("\nTotal running time(seconds) : " + clock.Elapsed.TotalSeconds);
            File.AppendAllText("output.txt", "\nTotal running time(seconds) : " + clock.Elapsed.TotalSeconds + "\n");
        }

        static void Generate_soln(int[,] initial)
        {
            //Choose heuristic method to be used(Enter 1,2 or 3)
            int heuristic = 3;
            var initialState = new State(initial);
            var solver = new A_Star(initialState);
            var solution = solver.Solve(heuristic);

            if (solution.soln_path != null)
            {
                if (heuristic == 1)
                {
                    Console.WriteLine("\nHeuristic Variant - Misplaced Tiles");
                    File.AppendAllText("output.txt", "\nHeuristic Variant - Misplaced Tiles\n");
                }
                else if (heuristic == 2)
                {
                    Console.WriteLine("\nHeuristic Variant - Manhattan distance");
                    File.AppendAllText("output.txt", "\nHeuristic Variant - Manhattan distance\n");
                }
                else
                {
                    Console.WriteLine("\nHeuristic Variant - Linear Conflicts");
                    File.AppendAllText("output.txt", "\nHeuristic Variant - Linear Conflicts\n");
                }
                Console.WriteLine("\nGenerating shortest path...\n");
                File.AppendAllText("output.txt", "\nGenerating shortest path...\n");
                foreach (var state in solution.soln_path)
                {
                    state.Print();
                    Console.WriteLine();
                    File.AppendAllText("output.txt", "\n");
                }
                Console.WriteLine("Total number of nodes removed from the frontier : " + solution.nodes_removed);
                File.AppendAllText("output.txt", "Total number of nodes removed from the frontier : " + solution.nodes_removed + "\n");
                Console.WriteLine("\nLength of the solution(shortest_path) is : " + solution.soln_path.Count());
                File.AppendAllText("output.txt", "\nLength of the solution(shortest_path) is : " + solution.soln_path.Count() + "\n");
            }
            else
            {
                Console.WriteLine("No solution found.");
            }
        }
    }

    class A_Star
    {
        private readonly State _initialState;

        public A_Star(State initialState)
        {
            _initialState = initialState;
        }

        public (List<State> soln_path,int nodes_removed) Solve(int Heuristic_method)
        {
            int heuristic_value = 0;
            int nodes_count = 0;
            var frontier = new PriorityQueue<State>();
            var explored = new HashSet<State>();

            frontier.Enqueue(_initialState, _initialState.Cost);

            while (frontier.Count > 0)
            {
                var currentState = frontier.Dequeue();
                nodes_count++;

                if (currentState.IsGoal())
                {
                    return (currentState.GetPath(),nodes_count);
                }

                explored.Add(currentState);

                foreach (var successor in currentState.GetSuccessors())
                {
                    if (Heuristic_method == 1)
                    {
                        heuristic_value = successor.Misplaced();
                    }
                    else if (Heuristic_method == 2)
                    {
                        heuristic_value = successor.Manhattan();
                    }
                    else
                    {
                        heuristic_value = successor.Linear_conflict();
                    }
                    if (!explored.Contains(successor) && !frontier.Contains(successor))
                    {
                        frontier.Enqueue(successor, successor.Cost + heuristic_value);
                    }
                }
            }
            return (null,nodes_count);
        }
    }

    class State
    {
        private readonly int[,] _board;
        public int Cost { get; private set; }
        public State Parent { get; private set; }
        private readonly int _zeroX;
        private readonly int _zeroY;

        public State(int[,] board, int cost = 0, State parent = null)
        {
            _board = board;
            Cost = cost;
            Parent = parent;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (_board[i, j] == 0)
                    {
                        _zeroX = i;
                        _zeroY = j;
                    }
                }
            }
        }

        public bool IsGoal()
        {
            int[,] goal = {
                {0, 4, 6},
                {7, 3, 8},
                {5, 1, 2}
            };

            return CompareBoard(_board, goal);
        }

        private bool CompareBoard(int[,] a, int[,] b)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (a[i, j] != b[i, j])
                        return false;
                }
            }
            return true;
        }

        public IEnumerable<State> GetSuccessors()
        {
            var successors = new List<State>();

            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                int x = _zeroX + dx[i];
                int y = _zeroY + dy[i];

                if (x >= 0 && x < 3 && y >= 0 && y < 3)
                {
                    var newBoard = (int[,])_board.Clone();
                    newBoard[_zeroX, _zeroY] = newBoard[x, y];
                    newBoard[x, y] = 0;

                    successors.Add(new State(newBoard, Cost + 1, this));
                }
            }

            return successors;
        }

        public List<State> GetPath()
        {
            var path = new List<State>();
            var current = this;

            while (current != null)
            {
                path.Add(current);
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }

        public void Print()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Console.Write(_board[i, j] + " ");
                    File.AppendAllText("output.txt", _board[i, j] + " ");
                }
                Console.WriteLine();
                File.AppendAllText("output.txt", "\n");
            }
        }

        public int Misplaced()
        {
            int distance = 0;
            int[,] goal = {
                {0, 4, 6},
                {7, 3, 8},
                {5, 1, 2}
            };

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (_board[i, j] != goal[i, j])
                    {
                        distance++;
                    }
                }
            }

            return distance;
        }

        public int Manhattan()
        {
            int distance = 0;

            for (int i = 0; i < _board.GetLength(0); i++)
            {
                for (int j = 0; j < _board.GetLength(1); j++)
                {
                    int value = _board[i, j];
                    if (value != 0) // Ignore the empty tile
                    {
                        int targetRow = (value - 1) / _board.GetLength(0); // Goal row index
                        int targetCol = (value - 1) % _board.GetLength(1); // Goal column index

                        // Calculate Manhattan distance
                        distance += Math.Abs(i - targetRow) + Math.Abs(j - targetCol);
                    }
                }
            }

            return distance;
        }

        public int Linear_conflict()
        {
            // Calculate Manhattan distance
            int distance = this.Manhattan();

            // Resolve linear conflicts
            for (int i = 0; i < _board.GetLength(0); i++)
            {
                for (int j = 0; j < _board.GetLength(1); j++)
                {
                    int value = _board[i, j];
                    if (value != 0) // Ignore the empty tile
                    {
                        int targetRow = (value - 1) / _board.GetLength(0); // Goal row index
                        int targetCol = (value - 1) % _board.GetLength(1); // Goal column index

                        // Check for linear conflicts along rows
                        if (i == targetRow)
                        {
                            for (int k = j + 1; k < _board.GetLength(1); k++)
                            {
                                int nextValue = _board[i, k];
                                if (nextValue != 0 && (nextValue - 1) / _board.GetLength(0) == i && nextValue < value)
                                {
                                    distance += 2; // Add extra cost for linear conflict
                                }
                            }
                        }

                        // Check for linear conflicts along columns
                        if (j == targetCol)
                        {
                            for (int k = i + 1; k < _board.GetLength(0); k++)
                            {
                                int nextValue = _board[k, j];
                                if (nextValue != 0 && (nextValue - 1) % _board.GetLength(1) == j && nextValue < value)
                                {
                                    distance += 2; // Add extra cost for linear conflict
                                }
                            }
                        }
                    }
                }
            }

            return distance;
        }

    }

    class PriorityQueue<T>
    {
        private readonly SortedDictionary<int, Queue<T>> _dict = new SortedDictionary<int, Queue<T>>();

        public int Count { get; private set; }

        public bool Contains(T item)
        {
            foreach (var kv in _dict)
            {
                if (kv.Value.Contains(item))
                    return true;
            }
            return false;
        }

        public void Enqueue(T item, int priority)
        {
            if (!_dict.ContainsKey(priority))
                _dict[priority] = new Queue<T>();

            _dict[priority].Enqueue(item);
            Count++;
        }

        public T Dequeue()
        {
            var item = _dict[_dict.Keys.Min()].Dequeue();
            Count--;

            if (_dict[_dict.Keys.Min()].Count == 0)
                _dict.Remove(_dict.Keys.Min());

            return item;
        }
    }
}

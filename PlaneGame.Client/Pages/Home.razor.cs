using Microsoft.AspNetCore.Components;
using PlaneGame.Client.Helpers;
using PlaneGame.Client.Models;
using static PlaneGame.Client.Models.MazeModel;
namespace PlaneGame.Client.Pages
{
    public partial class Home : ComponentBase
    {
        DimentionsModel Dimentions { get; set; } = new DimentionsModel();
        MazeCell[,] Maze = null;
        bool IsLoopCompleted;

        /// <summary>
        /// Recursive Backtracking
        /// </summary>
        async void GenerateMaze()
        {
            IsLoopCompleted = false;
            await InvokeAsync(StateHasChanged);

            // Ensure minimum maze dimensions
            if (Dimentions.Width < 5 || Dimentions.Height < 5)
            {
                IsLoopCompleted = true;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Initialize maze with walls
            Maze = new MazeCell[Dimentions.Height, Dimentions.Width];

            // Fill maze with wall cells
            for (int y = 0; y < Dimentions.Height; y++)
            {
                for (int x = 0; x < Dimentions.Width; x++)
                {
                    Maze[y, x] = new MazeCell { Type = CellType.Wall, Visited = false };
                }
            }

            Random random = new Random();
            // Ensure to start from a non-border coordinate
            var xStartCell = random.Next(1, Dimentions.Width - 1);
            var yStartCell = random.Next(1, Dimentions.Height - 1);

            // Start cell
            Maze[yStartCell, xStartCell] = new MazeCell { Type = CellType.Path, Visited = true };

            // Stack for backtracking
            Stack<(int x, int y)> cellStack = new Stack<(int x, int y)>();
            cellStack.Push((xStartCell, yStartCell));

            while (cellStack.Count > 0)
            {
                var (x, y) = cellStack.Peek();

                // Get all unvisited neighbors that are 2 cells away
                List<(int nx, int ny, int wx, int wy)> neighbors = new List<(int, int, int, int)>();

                // Check North
                if (y - 2 >= 1 && y - 2 < Dimentions.Height && x >= 0 && x < Dimentions.Width && !Maze[y - 2, x].Visited)
                    neighbors.Add((x, y - 2, x, y - 1));

                // Check East
                if (x + 2 < Dimentions.Width - 1 && x + 2 >= 0 && y >= 0 && y < Dimentions.Height && !Maze[y, x + 2].Visited)
                    neighbors.Add((x + 2, y, x + 1, y));

                // Check South
                if (y + 2 < Dimentions.Height - 1 && y + 2 >= 0 && x >= 0 && x < Dimentions.Width && !Maze[y + 2, x].Visited)
                    neighbors.Add((x, y + 2, x, y + 1));

                // Check West
                if (x - 2 >= 1 && x - 2 < Dimentions.Width && y >= 0 && y < Dimentions.Height && !Maze[y, x - 2].Visited)
                    neighbors.Add((x - 2, y, x - 1, y));

                if (neighbors.Count > 0)
                {
                    // Choose a random unvisited neighbor
                    var (nx, ny, wx, wy) = neighbors[random.Next(neighbors.Count)];

                    // Double-check bounds before modifying
                    if (wy >= 0 && wy < Dimentions.Height && wx >= 0 && wx < Dimentions.Width)
                    {
                        // Remove the wall between current cell and chosen neighbor
                        Maze[wy, wx] = new MazeCell { Type = CellType.Path, Visited = true };
                    }

                    // Double-check bounds before modifying
                    if (ny >= 0 && ny < Dimentions.Height && nx >= 0 && nx < Dimentions.Width)
                    {
                        // Mark the chosen neighbor as visited
                        Maze[ny, nx] = new MazeCell { Type = CellType.Path, Visited = true };

                        // Push the neighbor to the stack
                        cellStack.Push((nx, ny));
                    }
                }
                else
                {
                    // No unvisited neighbors, backtrack
                    cellStack.Pop();
                }

                await InvokeAsync(StateHasChanged);
                await Task.Delay(5);
            }

            // Add entry and exit to the maze
            AddEntryAndExit();

            IsLoopCompleted = true;
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Add entry and exit points to the maze
        /// </summary>
        private void AddEntryAndExit()
        {
            if (Maze == null || Dimentions.Width < 3 || Dimentions.Height < 3) return;

            Random random = new Random();

            // Find a valid entry point on the top row that connects to a path
            int entryX = -1;
            for (int attempts = 0; attempts < 100 && entryX == -1; attempts++)
            {
                int x = random.Next(1, Dimentions.Width - 1);
                if (1 < Dimentions.Height && x < Dimentions.Width && Maze[1, x].Type == CellType.Path)
                {
                    entryX = x;
                    if (0 < Dimentions.Height && x < Dimentions.Width)
                    {
                        Maze[0, x] = new MazeCell { Type = CellType.Entry, Visited = true };
                    }
                }
            }

            // Find a valid exit point on the bottom row that connects to a path
            int exitX = -1;
            int bottomRow = Dimentions.Height - 1;
            int secondToLastRow = Dimentions.Height - 2;

            if (secondToLastRow >= 0 && bottomRow >= 0)
            {
                for (int attempts = 0; attempts < 100 && exitX == -1; attempts++)
                {
                    int x = random.Next(1, Dimentions.Width - 1);
                    if (x < Dimentions.Width && secondToLastRow < Dimentions.Height &&
                        Maze[secondToLastRow, x].Type == CellType.Path)
                    {
                        exitX = x;
                        if (bottomRow < Dimentions.Height && x < Dimentions.Width)
                        {
                            Maze[bottomRow, x] = new MazeCell { Type = CellType.Exit, Visited = true };
                        }
                    }
                }
            }

            // If we couldn't find valid entry/exit points, we need to create valid connections
            if (entryX == -1 && Dimentions.Width > 2 && Dimentions.Height > 1)
            {
                // Find any column with a path in the second row
                for (int x = 1; x < Dimentions.Width - 1; x++)
                {
                    if (1 < Dimentions.Height && x < Dimentions.Width && Maze[1, x].Type == CellType.Path)
                    {
                        Maze[0, x] = new MazeCell { Type = CellType.Entry, Visited = true };
                        entryX = x;
                        break;
                    }
                }

                // If still no valid entry found, create a pathway
                if (entryX == -1)
                {
                    entryX = random.Next(1, Dimentions.Width - 1);
                    if (0 < Dimentions.Height && entryX < Dimentions.Width)
                    {
                        Maze[0, entryX] = new MazeCell { Type = CellType.Entry, Visited = true };
                    }
                    if (1 < Dimentions.Height && entryX < Dimentions.Width)
                    {
                        Maze[1, entryX] = new MazeCell { Type = CellType.Path, Visited = true };
                        // Connect to nearest path
                        ConnectToNearestPath(1, entryX);
                    }
                }
            }

            if (exitX == -1 && Dimentions.Width > 2 && Dimentions.Height > 2)
            {
                // Find any column with a path in the second-to-last row
                for (int x = 1; x < Dimentions.Width - 1; x++)
                {
                    if (secondToLastRow < Dimentions.Height && secondToLastRow >= 0 && x < Dimentions.Width &&
                        Maze[secondToLastRow, x].Type == CellType.Path)
                    {
                        Maze[bottomRow, x] = new MazeCell { Type = CellType.Exit, Visited = true };
                        exitX = x;
                        break;
                    }
                }

                // If still no valid exit found, create a pathway
                if (exitX == -1)
                {
                    exitX = random.Next(1, Dimentions.Width - 1);
                    if (bottomRow < Dimentions.Height && exitX < Dimentions.Width)
                    {
                        Maze[bottomRow, exitX] = new MazeCell { Type = CellType.Exit, Visited = true };
                    }
                    if (secondToLastRow < Dimentions.Height && secondToLastRow >= 0 && exitX < Dimentions.Width)
                    {
                        Maze[secondToLastRow, exitX] = new MazeCell { Type = CellType.Path, Visited = true };
                        // Connect to nearest path
                        ConnectToNearestPath(secondToLastRow, exitX);
                    }
                }
            }
        }

        // Helper method to connect a cell to the nearest path in the maze
        private void ConnectToNearestPath(int startY, int startX)
        {
            // Simple implementation - search for the nearest path cell and create a path to it

            // Use a breadth-first search to find the nearest path
            Queue<(int y, int x, List<(int y, int x)> path)> queue = new Queue<(int, int, List<(int, int)>)>();
            bool[,] visited = new bool[Dimentions.Height, Dimentions.Width];

            List<(int y, int x)> initialPath = new List<(int y, int x)>();
            queue.Enqueue((startY, startX, initialPath));
            visited[startY, startX] = true;

            int[] dy = { -1, 0, 1, 0 };
            int[] dx = { 0, 1, 0, -1 };

            while (queue.Count > 0)
            {
                var (y, x, currentPath) = queue.Dequeue();

                // Check if this is a path cell (but not our starting point)
                if (Maze[y, x].Type == CellType.Path && !(y == startY && x == startX))
                {
                    // We found a path - now create a path from our starting point
                    foreach (var (py, px) in currentPath)
                    {
                        if (py >= 0 && py < Dimentions.Height && px >= 0 && px < Dimentions.Width &&
                            Maze[py, px].Type != CellType.Entry && Maze[py, px].Type != CellType.Exit)
                        {
                            Maze[py, px] = new MazeCell { Type = CellType.Path, Visited = true };
                        }
                    }
                    return;
                }

                // Check neighbors
                for (int i = 0; i < 4; i++)
                {
                    int ny = y + dy[i];
                    int nx = x + dx[i];

                    if (ny >= 0 && ny < Dimentions.Height && nx >= 0 && nx < Dimentions.Width && !visited[ny, nx])
                    {
                        visited[ny, nx] = true;
                        var newPath = new List<(int, int)>(currentPath);
                        newPath.Add((ny, nx));
                        queue.Enqueue((ny, nx, newPath));
                    }
                }
            }
        }

        /// <summary>
        /// Reset the maze
        /// </summary>
        void ResetMaze()
        {
            Dimentions = new();
            Maze = null;
            IsLoopCompleted = false;
        }
        /// <summary>
        /// Solve the maze
        /// </summary>
        void MazeSolved()
        {
            if (Maze != null)
            {
                MazeLogic mazeLogic = new();
                var solution = mazeLogic.AStarAlgorithm(Maze);
                foreach (var cell in solution)
                {
                    Maze[cell.y, cell.x].Type = CellType.solution;
                }
            }
        }
    }
}
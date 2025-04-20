using Microsoft.AspNetCore.Components;
using PlaneGame.Client.Models;
using static PlaneGame.Client.Models.MazeModel;
namespace PlaneGame.Client.Pages
{
    public partial class Home : ComponentBase
    {
        DimentionsModel Dimentions { get; set; } = new DimentionsModel();
        MazeCell[,] Maze = null;
        bool IsLoopCompleted;
        string? Debug = "<div>";
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
                Debug += "<p>Maze dimensions must be at least 5x5</p>";
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

            // Find a valid entry point on the top row
            int entryX = -1;
            for (int attempts = 0; attempts < 100 && entryX == -1; attempts++)
            {
                int x = random.Next(1, Dimentions.Width - 1);
                if (x < Dimentions.Width && 1 < Dimentions.Height && Maze[1, x].Type == CellType.Path)
                {
                    entryX = x;
                    if (0 < Dimentions.Height && x < Dimentions.Width)
                    {
                        Maze[0, x] = new MazeCell { Type = CellType.Entry, Visited = true };
                    }
                }
            }

            // Find a valid exit point on the bottom row
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

            // If we couldn't find valid entry/exit points, force them
            if (entryX == -1 && Dimentions.Width > 2 && Dimentions.Height > 1)
            {
                int x = random.Next(1, Dimentions.Width - 1);
                if (0 < Dimentions.Height && x < Dimentions.Width)
                {
                    Maze[0, x] = new MazeCell { Type = CellType.Entry, Visited = true };
                }
                if (1 < Dimentions.Height && x < Dimentions.Width)
                {
                    Maze[1, x] = new MazeCell { Type = CellType.Entry, Visited = true };
                }
            }

            if (exitX == -1 && Dimentions.Width > 2 && Dimentions.Height > 2)
            {
                int x = random.Next(1, Dimentions.Width - 1);
                if (bottomRow < Dimentions.Height && x < Dimentions.Width)
                {
                    Maze[bottomRow, x] = new MazeCell { Type = CellType.Exit, Visited = true };
                }
                if (secondToLastRow < Dimentions.Height && secondToLastRow >= 0 && x < Dimentions.Width)
                {
                    Maze[secondToLastRow, x] = new MazeCell { Type = CellType.Exit, Visited = true };
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
            Debug = null;
            IsLoopCompleted = false;
        }
    }
}
using static PlaneGame.Client.Models.MazeModel;

namespace PlaneGame.Client.Helpers
{
    public class MazeLogic
    {
        /// <summary>
        /// Solve a maze using the A* algorithm.
        /// </summary>
        /// <param name="maze"></param>
        public Stack<(int x, int y, int f)> AStarAlgorithm(MazeCell[,] maze)
        {

            // Implementation of A* algorithm
            int[] start = new int[2];
            int[] destination = new int[2];
            //stack for backtracking
            Stack<(int x, int y, int f)> solutionStack = new();
            Stack<(int x, int y, int f)> openStack = new();
            int distanceWeight = 0;
            // Find the start and destination cells
            var StartAndExit = FindEntryAndExitCoords(maze);
            if (StartAndExit != null)
            {
                // Initialize the start and destination cells
                start[0] = StartAndExit[0, 0];
                start[1] = StartAndExit[0, 1];

                destination[0] = StartAndExit[1, 0];
                destination[1] = StartAndExit[1, 1];
            }
            //calculate the cost of the start cell
            int fCost = CalCulateFCost(start[0], start[1], destination, distanceWeight);

            //find neighbors of the start cell
            List<(int x, int y)> neighbors = FindNeighboors(maze, start[0], start[1]);

            //sort the neighbors by cost
            List<(int x, int y, int f)> lowestCostNeighbors = SortNeighborsByCost(neighbors, destination, distanceWeight);

            // Initialize the start cell
            solutionStack.Push((start[0], start[1], fCost));
            //mark the first node as visited 
            maze[start[1], start[0]].Visited = true;

            foreach (var neighbor in lowestCostNeighbors)
            {
                // Add the neighbor to the open stack
                openStack.Push((neighbor.x, neighbor.y, neighbor.f));
            }
            bool isNoNeighbors = false;
            while (openStack.Count > 0)
            {

                var peek = isNoNeighbors ? solutionStack.Pop() : openStack.Pop();
                isNoNeighbors = false;
                if (maze[peek.y, peek.x].Type == CellType.Exit && peek.y == maze.GetLength(0) - 1)
                {
                    solutionStack.Push((peek.x, peek.y, fCost));
                    break;
                }

                // Mark the cell as visited
                maze[peek.y, peek.x].Visited = true;

                //calculate the cost of the start cell
                fCost = CalCulateFCost(peek.y, peek.x, destination, distanceWeight++);

                //find neighbors of the start cell
                neighbors = FindNeighboors(maze, peek.x, peek.y);
                //backtrack if there are no neighbors
                if (neighbors.Count == 0)
                {
                    isNoNeighbors = true;
                    distanceWeight--;
                    continue;
                }
                //sort the neighbors by cost
                lowestCostNeighbors = SortNeighborsByCost(neighbors, destination, distanceWeight);
                foreach (var neighbor in lowestCostNeighbors)
                {
                    // Add the neighbor to the open stack
                    var node = (neighbor.x, neighbor.y, neighbor.f);
                    if (!openStack.Any(cor => cor.x == node.x && cor.y == node.y))
                        openStack.Push(node);
                }
                // Mark the cell as part of the solution temporarily
                solutionStack.Push((peek.x, peek.y, fCost));

            }
            return solutionStack;
        }
        /// <summary>
        /// Find the entry and exit coordinates in the maze.
        /// </summary>
        /// <param name="maze"></param>
        /// <returns></returns>
        private int[,] FindEntryAndExitCoords(MazeCell[,] maze)
        {
            int[,] results = new int[2, 2];
            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    maze[i, j].Visited = false;
                    if (maze[i, j].Type == CellType.Entry && i == 0)
                    {
                        results[0, 0] = j;
                        results[0, 1] = i;
                    }
                    else if (maze[i, j].Type == CellType.Exit && i == maze.GetLength(0) - 1)
                    {
                        results[1, 0] = j;
                        results[1, 1] = i;
                    }

                }
            }
            return results;
        }

        /// <summary>
        /// Find the neighbors of a cell in the maze.
        /// </summary>
        /// <param name="maze"></param>
        /// <param name="xCell"></param>
        /// <param name="yCell"></param>
        /// <returns></returns>
        private List<(int x, int y)> FindNeighboors(MazeCell[,] maze, int xCell, int yCell)
        {
            List<(int x, int y)> neighbors = new();
            int x = xCell;
            int y = yCell;

            // Check North
            if (y > 0 && maze[y - 1, x].Type != CellType.Wall && !maze[y - 1, x].Visited)
                neighbors.Add((x, y - 1));
            // Check South
            if (y < maze.GetLength(0) - 1 && maze[y + 1, x].Type != CellType.Wall && !maze[y + 1, x].Visited)
                neighbors.Add((x, y + 1));
            // Check West
            if (x > 0 && maze[y, x - 1].Type != CellType.Wall && !maze[y, x - 1].Visited)
                neighbors.Add((x - 1, y));
            // Check East
            if (x < maze.GetLength(1) - 1 && maze[y, x + 1].Type != CellType.Wall && !maze[y, x + 1].Visited)
                neighbors.Add((x + 1, y));

            return neighbors;
        }

        /// <summary>
        /// Sort the neighbors of a cell by their cost.
        /// </summary>
        /// <param name="neighbors"></param>
        /// <param name="destination"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        private List<(int x, int y, int f)> SortNeighborsByCost(List<(int x, int y)> neighbors, int[] destination, int step)
        {
            List<(int x, int y, int f)> lowestCostNeighbors = new();
            foreach (var neighbor in neighbors)
            {
                // Calculate the cost of each neighbor
                int fcost = CalCulateFCost(neighbor.x, neighbor.y, destination, step + 1);
                lowestCostNeighbors.Add((neighbor.x, neighbor.y, fcost));
            }
            return lowestCostNeighbors.OrderByDescending(n => n.f).ToList();
        }
        /// <summary>
        /// Calculate the F cost for a cell.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="destination"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        private int CalCulateFCost(int x, int y, int[] destination, int g)
        {
            // Calculate the F cost for the cell
            int h = Math.Abs(x - destination[0]) + Math.Abs(y - destination[1]);
            int fCost = g + h;
            return fCost;
        }
    }
}

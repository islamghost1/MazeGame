namespace PlaneGame.Client.Models
{
    public class MazeModel
    {
        public enum CellType { Wall, Path, Entry, Exit , solution }

        public struct MazeCell
        {
            public MazeCell()
            {
                Type = CellType.Wall;
            }
            public CellType Type;
            public bool Visited;
        }
    }
}

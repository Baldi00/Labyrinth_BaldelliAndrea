namespace DBGA.Common
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        None
    }

    public static class DirectionMethods
    {
        public static Direction GetOppositeDirection(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => Direction.None,
            };
        }
    }
}

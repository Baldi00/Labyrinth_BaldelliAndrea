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
        /// <summary>
        /// Returns the opposite direction of this direction (e.g. the opposite direction of Up is Down)
        /// </summary>
        /// <returns>The opposite direction of this direction</returns>
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

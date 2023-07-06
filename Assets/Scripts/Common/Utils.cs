using UnityEngine;

namespace DBGA.Common
{
    public static class Utils
    {
        /// <summary>
        /// Returns the next position from the current position in the given direction
        /// </summary>
        /// <param name="currentPosition">The current position</param>
        /// <param name="direction">The direction in which you search the next position</param>
        /// <returns>The next position from the current position in the given direction</returns>
        public static Vector2Int GetNextPosition(Vector2Int currentPosition, Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Vector2Int(currentPosition.x, currentPosition.y + 1),
                Direction.Down => new Vector2Int(currentPosition.x, currentPosition.y - 1),
                Direction.Left => new Vector2Int(currentPosition.x - 1, currentPosition.y),
                Direction.Right => new Vector2Int(currentPosition.x + 1, currentPosition.y),
                _ => new Vector2Int(currentPosition.x, currentPosition.y)
            };
        }

        /// <summary>
        /// Checks if the given position is on the grid or not
        /// </summary>
        /// <param name="position">The position to test</param>
        /// <returns>True if the given position is on the grid, false if is outside</returns>
        public static bool IsPositionInsideGrid(Vector2Int position, int gridSize)
        {
            return position.x >= 0 && position.y >= 0 &&
                position.x < gridSize && position.y < gridSize;
        }

        /// <summary>
        /// Converts a generic array into a generic matrix
        /// </summary>
        /// <typeparam name="T">The type of the array to convert into matrix</typeparam>
        /// <param name="array">The input array to convert into matrix</param>
        /// <param name="rows">The number of rows of the matrix</param>
        /// <param name="cols">The number of columns of the matrix</param>
        /// <returns>The matrix representation of the given array</returns>
        public static T[][] ArrayToMatrix<T>(T[] array, int rows, int cols)
        {
            T[][] matrix = new T[rows][];
            for (int i = 0; i < rows; i++)
            {
                matrix[i] = new T[cols];
                for (int j = 0; j < cols; j++)
                    matrix[i][j] = array[i * cols + j];
            }
            return matrix;
        }
    }
}

using UnityEngine;

namespace DBGA.Player
{
    [DisallowMultipleComponent]
    public class Player : MonoBehaviour
    {
        [SerializeField]
        private int initialArrowsCount = 5;

        private int currentArrowsCount;

        private Vector2Int positionOnGrid;

        public Vector2Int PositionOnGrid { get => positionOnGrid; }

        void Awake()
        {
            currentArrowsCount = initialArrowsCount;
        }

        /// <summary>
        /// Sets the position of the player on the grid
        /// </summary>
        /// <param name="positionOnGrid">The position on the grid</param>
        public void SetPositionOnGrid(Vector2Int positionOnGrid)
        {
            this.positionOnGrid = positionOnGrid;
        }

        public void MoveToNextPosition(Vector2Int nextPosition)
        {
            SetPositionOnGrid(nextPosition);
            transform.position = new Vector3(nextPosition.x, 0f, nextPosition.y);
        }
    }
}

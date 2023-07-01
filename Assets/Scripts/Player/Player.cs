using System.Linq;
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

        /// <summary>
        /// Tries to move the player to the next position
        /// </summary>
        /// <param name="nextPosition">The next position the player should be placed to</param>
        /// <returns>True if the movement had success (i.e. no walls were detected), false otherwise</returns>
        public bool TryMoveToNextPosition(Vector2Int nextPosition)
        {
            Vector3 position3d = new(positionOnGrid.x, 0.25f, positionOnGrid.y);
            Vector3 nextPosition3d = new(nextPosition.x, 0.25f, nextPosition.y);

            RaycastHit[] raycastHits = Physics.RaycastAll(position3d, nextPosition3d - position3d, 1f);

            if (raycastHits.Any(hit => !hit.collider.isTrigger))
                return false;

            SetPositionOnGrid(nextPosition);
            transform.position = new Vector3(nextPosition.x, 0f, nextPosition.y);

            return true;
        }
    }
}

using System.Linq;
using UnityEngine;
using DBGA.Common;
using System.Runtime.CompilerServices;

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
        /// <param name="moveDirection">The direction in which the player is trying to move</param>
        /// <returns>True if the movement had success (i.e. no walls were detected), false otherwise</returns>
        public bool TryMoveToNextPosition(Vector2Int nextPosition, Direction moveDirection)
        {
            Vector3 position3d = new(positionOnGrid.x, 0.25f, positionOnGrid.y);
            Vector3 nextPosition3d = new(nextPosition.x, 0.25f, nextPosition.y);

            RaycastHit[] raycastHits = Physics.RaycastAll(position3d, nextPosition3d - position3d, 1f);

            // A wall in front of player detected, no movement allowed
            if (raycastHits.Any<RaycastHit>(hit => !hit.collider.isTrigger))
                return false;

            if (raycastHits.Any<RaycastHit>(hit => hit.collider.CompareTag("Tunnel")))
                return TunnelDetectedManagement(ref nextPosition, moveDirection, raycastHits);
            else
                return MoveToNextTile(nextPosition);
        }

        private bool TunnelDetectedManagement(ref Vector2Int nextPosition, Direction moveDirection, RaycastHit[] raycastHits)
        {
            ITunnel tunnel = raycastHits
                .Where<RaycastHit>(hit => hit.collider.CompareTag("Tunnel"))
                .ElementAt<RaycastHit>(0)
                .collider.GetComponent<ITunnelTrigger>().GetTunnel();

            if (tunnel.CanCross(moveDirection.GetOppositeDirection()))
            {
                nextPosition = tunnel.GetFinalDestination(moveDirection.GetOppositeDirection());
                tunnel.RevealEntireTunnel(moveDirection.GetOppositeDirection());
                SetPositionOnGrid(nextPosition);
                transform.position = new Vector3(nextPosition.x, 0f, nextPosition.y);
                return true;
            }
            else
                return false;
        }

        private bool MoveToNextTile(Vector2Int nextPosition)
        {
            SetPositionOnGrid(nextPosition);
            transform.position = new Vector3(nextPosition.x, 0f, nextPosition.y);
            return true;
        }
    }
}

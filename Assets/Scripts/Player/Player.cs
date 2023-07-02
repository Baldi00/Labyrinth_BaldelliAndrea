using System.Linq;
using UnityEngine;
using DBGA.Common;
using DBGA.EventSystem;
using System.Collections;
using System.Collections.Generic;

namespace DBGA.Player
{
    [DisallowMultipleComponent]
    public class Player : MonoBehaviour
    {
        [SerializeField]
        private int initialArrowsCount = 5;
        [SerializeField]
        private GameObject arrowPrefab;
        [SerializeField]
        private float movementAnimationDuration = 1f;
        [SerializeField]
        private AnimationCurve moveAnimationSmoothing;

        private int currentArrowsCount;

        private Vector2Int positionOnGrid;
        private bool isInMoveAnimation;

        private GameEventsManager gameEventsManager;

        public Vector2Int PositionOnGrid { get => positionOnGrid; }
        public bool IgnoreInputs { set; get; }

        void Awake()
        {
            currentArrowsCount = initialArrowsCount;
            gameEventsManager = GameEventsManager.Instance;
        }

        /// <summary>
        /// Sets the position of the player on the grid
        /// </summary>
        /// <param name="positionOnGrid">The position on the grid</param>
        public void SetPositionOnGrid(Vector2Int positionOnGrid)
        {
            this.positionOnGrid = positionOnGrid;
            gameEventsManager.DispatchGameEvent(new PlayerExploredTileEvent() { positionOnGrid = positionOnGrid });
        }

        public void TeleportToNextPosition(Vector2Int nextPosition)
        {
            StopAllCoroutines();
            isInMoveAnimation = false;
            MoveToNextTile(nextPosition, 0);
        }

        /// <summary>
        /// Tries to move the player to the next position
        /// </summary>
        /// <param name="nextPosition">The next position the player should be placed to</param>
        /// <param name="moveDirection">The direction in which the player is trying to move</param>
        /// <returns>True if the movement had success (i.e. no walls were detected), false otherwise</returns>
        public bool TryMoveToNextPosition(Vector2Int nextPosition, Direction moveDirection)
        {
            if (IgnoreInputs)
                return false;

            Vector3 position3d = new(positionOnGrid.x, 0.25f, positionOnGrid.y);
            Vector3 nextPosition3d = new(nextPosition.x, 0.25f, nextPosition.y);

            RaycastHit[] raycastHits = Physics.RaycastAll(position3d, nextPosition3d - position3d, 1f);

            // A wall in front of player detected, no movement allowed
            if (raycastHits.Any<RaycastHit>(hit => !hit.collider.isTrigger && hit.collider.CompareTag("Wall")))
                return false;

            if (raycastHits.Any<RaycastHit>(hit => hit.collider.CompareTag("Tunnel")))
                return TunnelDetectedManagement(ref nextPosition, moveDirection, raycastHits);
            else
                return MoveToNextTile(nextPosition, movementAnimationDuration);
        }

        public bool ShotArrow(Direction shotDirection)
        {
            if (currentArrowsCount <= 0)
                return false;

            currentArrowsCount--;
            
            Quaternion arrowRotation = GetArrowRotationFromDirection(shotDirection);
            GameObject a = Instantiate(arrowPrefab, transform.position, arrowRotation);

            return true;
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
                return MoveToNextTiles(
                    nextPosition,
                    tunnel.GetAllCrossingPoints(moveDirection.GetOppositeDirection()),
                    movementAnimationDuration);
            }
            else
                return false;
        }

        private bool MoveToNextTile(Vector2Int nextPosition, float animationDuration)
        {
            if (isInMoveAnimation)
                return false;

            SetPositionOnGrid(nextPosition);
            StartCoroutine(AnimateMovementToNextTile(nextPosition, animationDuration));
            return true;
        }

        private bool MoveToNextTiles(Vector2Int finalPosition, List<Vector2Int> crossingPositions, float animationDuration)
        {
            if (isInMoveAnimation)
                return false;

            SetPositionOnGrid(finalPosition);
            StartCoroutine(AnimateMovementToNextTiles(crossingPositions, animationDuration));
            return true;
        }

        private IEnumerator AnimateMovementToNextTile(Vector2Int nextPosition, float animationDuration)
        {
            isInMoveAnimation = true;
            float animationTimer = 0;
            Vector3 startPosition3d = transform.position;
            Vector3 nextPosition3d = new Vector3(nextPosition.x, 0f, nextPosition.y);
            while (animationTimer < animationDuration)
            {
                transform.position = Vector3.Lerp(
                    startPosition3d,
                    nextPosition3d,
                    moveAnimationSmoothing.Evaluate(animationTimer / animationDuration));

                animationTimer += Time.deltaTime;
                yield return null;
            }
            transform.position = nextPosition3d;
            isInMoveAnimation = false;
        }

        private IEnumerator AnimateMovementToNextTiles(List<Vector2Int> crossingPositions, float animationDuration)
        {
            isInMoveAnimation = true;
            foreach (Vector2Int nextPosition in crossingPositions)
            {
                float animationTimer = 0;
                Vector3 startPosition3d = transform.position;
                Vector3 nextPosition3d = new Vector3(nextPosition.x, 0f, nextPosition.y);
                while (animationTimer < animationDuration)
                {
                    transform.position = Vector3.Lerp(
                        startPosition3d,
                        nextPosition3d,
                        moveAnimationSmoothing.Evaluate(animationTimer / animationDuration));

                    animationTimer += Time.deltaTime;
                    yield return null;
                }
                transform.position = nextPosition3d;
            }

            isInMoveAnimation = false;
        }

        private Quaternion GetArrowRotationFromDirection(Direction shotDirection)
        {
            return shotDirection switch
            {
                Direction.Left => Quaternion.LookRotation(Vector3.left, Vector3.up),
                Direction.Right => Quaternion.LookRotation(Vector3.right, Vector3.up),
                Direction.Down => Quaternion.LookRotation(Vector3.back, Vector3.up),
                _ => Quaternion.identity
            };
        }
    }
}

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
        private float arrowSpawnOffset;
        [SerializeField]
        private float movementAnimationDuration = 1f;
        [SerializeField]
        private AnimationCurve moveAnimationSmoothing;

        private int currentArrowsCount;

        private Vector2Int positionOnGrid;
        private bool isInMoveAnimation;

        private GameEventsManager gameEventsManager;

        private Coroutine animateMovementToNextTileCoroutine;
        private Coroutine animateMovementToNextTilesCoroutine;

        public Vector2Int PositionOnGrid { get => positionOnGrid; }
        public bool IgnoreInputs { set; get; }
        public int CurrentArrowsCount { get => currentArrowsCount; }

        void Awake()
        {
            currentArrowsCount = initialArrowsCount;
            gameEventsManager = GameEventsManager.Instance;
        }

        void Start()
        {
            gameEventsManager.DispatchGameEvent(new InitializeArrowCountEvent() { remainingArrows = currentArrowsCount });
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

        /// <summary>
        /// Teleports the player to the given position
        /// </summary>
        /// <param name="nextPosition">The position the player must be set on</param>
        public void TeleportToNextPosition(Vector2Int nextPosition)
        {
            if (animateMovementToNextTileCoroutine != null)
                StopCoroutine(animateMovementToNextTileCoroutine);
            if (animateMovementToNextTilesCoroutine != null)
                StopCoroutine(animateMovementToNextTilesCoroutine);

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
            {
                ITunnel tunnel = raycastHits
                    .Where<RaycastHit>(hit => hit.collider.CompareTag("Tunnel"))
                    .ElementAt<RaycastHit>(0)
                    .collider.GetComponent<ITunnelTrigger>().GetTunnel();
                return HandleTunnelMovement(moveDirection, tunnel);
            }
            else
                return MoveToNextTile(nextPosition, movementAnimationDuration);
        }

        /// <summary>
        /// Shots an arrow in the given direction if player has some remaining arrows
        /// </summary>
        /// <param name="shotDirection">The direction in which the arrow should be shot</param>
        /// <returns>True if the arrow has been shot, false otherwise</returns>
        public bool ShotArrow(Direction shotDirection)
        {
            if (IgnoreInputs)
                return false;

            if (currentArrowsCount <= 0)
                return false;

            currentArrowsCount--;

            ArrowMover arrowMover =
                Instantiate(arrowPrefab, GetArrowSpawnPosition(shotDirection), Quaternion.identity)
                .GetComponent<ArrowMover>();

            arrowMover.SetArrowDirection(shotDirection);
            gameEventsManager.DispatchGameEvent(new ArrowShotEvent() { remainingArrows = currentArrowsCount });

            return true;
        }

        /// <summary>
        /// Handles the movement of the player inside a tunnel.
        /// If the movement is allows, moves the player until the end of the tunnel
        /// </summary>
        /// <param name="moveDirection">The direction in which the player wants to move</param>
        /// <param name="tunnel">The first tunnel tile encountered</param>
        /// <returns>True if movement had success, false otherwise</returns>
        private bool HandleTunnelMovement(Direction moveDirection, ITunnel tunnel)
        {
            if (tunnel.CanCross(moveDirection.GetOppositeDirection()))
            {
                Vector2Int nextPosition = tunnel.GetFinalDestination(moveDirection.GetOppositeDirection());
                tunnel.RevealEntireTunnel(moveDirection.GetOppositeDirection());
                return MoveToNextTiles(
                    nextPosition,
                    tunnel.GetAllCrossingPoints(moveDirection.GetOppositeDirection()),
                    movementAnimationDuration);
            }
            else
                return false;
        }

        /// <summary>
        /// Moves the player to the given position with an animation
        /// </summary>
        /// <param name="nextPosition">The position in which the player should be placed in</param>
        /// <param name="animationDuration">The duration of the animation</param>
        /// <returns>True if movement succeeded, false otherwise</returns>
        private bool MoveToNextTile(Vector2Int nextPosition, float animationDuration)
        {
            if (isInMoveAnimation)
                return false;

            SetPositionOnGrid(nextPosition);
            animateMovementToNextTileCoroutine =
                StartCoroutine(AnimateMovementToNextTile(nextPosition, animationDuration));
            return true;
        }

        /// <summary>
        /// Moves the player through the given list of position with an animation
        /// </summary>
        /// <param name="finalPosition">The final position in which the player should be placed in</param>
        /// <param name="crossingPositions">The list of position the player should pass in</param>
        /// <param name="animationDuration">The duration of the animation</param>
        /// <returns>True if movement succeeded, false otherwise</returns>
        private bool MoveToNextTiles(Vector2Int finalPosition, List<Vector2Int> crossingPositions, float animationDuration)
        {
            if (isInMoveAnimation)
                return false;

            SetPositionOnGrid(finalPosition);
            animateMovementToNextTilesCoroutine =
                StartCoroutine(AnimateMovementToNextTiles(crossingPositions, animationDuration));
            return true;
        }

        /// <summary>
        /// Animates the movement from the start to the end position of the player
        /// </summary>
        /// <param name="nextPosition">The position in which the player should be placed in</param>
        /// <param name="animationDuration">The duration of the animation</param>
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

        /// <summary>
        /// Animates the movement from the start to the end position of the player passing through the crossing positions
        /// </summary>
        /// <param name="crossingPositions">The list of position the player should pass in</param>
        /// <param name="animationDuration">The duration of the animation</param>
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

        /// <summary>
        /// Returns the arrow spawn position based on the given direction and the set offset
        /// </summary>
        /// <param name="shotDirection">The direction in which the arrow should be shot</param>
        /// <returns>The arrow spawn position based on the given direction and the set offset</returns>
        private Vector3 GetArrowSpawnPosition(Direction shotDirection)
        {
            return transform.position + shotDirection switch
            {
                Direction.Left => Vector3.left,
                Direction.Right => Vector3.right,
                Direction.Down => Vector3.back,
                _ => Vector3.forward
            } * arrowSpawnOffset;
        }
    }
}

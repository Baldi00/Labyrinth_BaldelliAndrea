using DBGA.Common;
using DBGA.EventSystem;
using System.Linq;
using UnityEngine;

namespace DBGA.MazePlayer
{
    [DisallowMultipleComponent]
    public class Arrow : MonoBehaviour
    {
        private enum ArrowRaycastOutcome
        {
            NONE,
            TUNNEL_CENTER,
            WALL,
            MONSTER,
            PLAYER
        }

        private const float ARROW_RAYCAST_LENGTH = 0.1f;

        [SerializeField]
        private float speed;

        private Direction currentDirection;
        private GameEventsManager gameEventsManager;

        public int OwnerPlayerNumber { set; get; }

        void Awake()
        {
            gameEventsManager = GameEventsManager.Instance;
        }

        void Update()
        {
            Vector3 movement = speed * Time.deltaTime * transform.forward;

            ArrowRaycastOutcome arrowRaycastOutcome = DoArrowRaycast(out RaycastHit[] raycastHits, movement);
            HandleArrowRaycastOutcome(arrowRaycastOutcome, raycastHits);

            if (arrowRaycastOutcome != ArrowRaycastOutcome.NONE)
                return;

            transform.Translate(movement, Space.World);
        }

        /// <summary>
        /// Sets the arrow direction
        /// </summary>
        /// <param name="direction">The direction the arrow should have</param>
        public void SetArrowDirection(Direction direction)
        {
            currentDirection = direction;

            transform.rotation = direction switch
            {
                Direction.Left => Quaternion.LookRotation(Vector3.left, Vector3.up),
                Direction.Right => Quaternion.LookRotation(Vector3.right, Vector3.up),
                Direction.Down => Quaternion.LookRotation(Vector3.back, Vector3.up),
                _ => Quaternion.identity,
            };
        }

        /// <summary>
        /// Casts a ray in front of the arrow searching for tunnels, walls, monsters and player
        /// </summary>
        /// <param name="raycastHits">Hits output in case of raycast collisions</param>
        /// <param name="movement">
        /// The movement that will be performed (used to predict next position and always detect collision)
        /// </param>
        /// <returns>The outcome of the raycast, or ArrowRaycastOutcome.NONE if there were no collisions</returns>
        private ArrowRaycastOutcome DoArrowRaycast(out RaycastHit[] raycastHits, Vector3 movement)
        {
            float movementDistance = Vector3.Distance(transform.position, transform.position + movement);

            raycastHits = Physics.RaycastAll(
                transform.position,
                transform.forward,
                movementDistance + ARROW_RAYCAST_LENGTH);

            if (raycastHits.Any<RaycastHit>(hit => hit.collider.CompareTag("TunnelCenter")))
                return ArrowRaycastOutcome.TUNNEL_CENTER;
            if (raycastHits.Any<RaycastHit>(hit => hit.collider.CompareTag("Wall")))
                return ArrowRaycastOutcome.WALL;
            if (raycastHits.Any<RaycastHit>(hit => hit.collider.CompareTag("Monster")))
                return ArrowRaycastOutcome.MONSTER;
            if (raycastHits.Any<RaycastHit>(hit => hit.collider.CompareTag("Player")))
                return ArrowRaycastOutcome.PLAYER;

            return ArrowRaycastOutcome.NONE;
        }

        /// <summary>
        /// Handles the arrow raycast outcome managing or dispatching the related event
        /// </summary>
        /// <param name="arrowRaycastOutcome">The outcome of the raycast</param>
        /// <param name="raycastHits">The list of raycast hits output in case of raycast collisions</param>
        private void HandleArrowRaycastOutcome(ArrowRaycastOutcome arrowRaycastOutcome, RaycastHit[] raycastHits)
        {
            switch (arrowRaycastOutcome)
            {
                case ArrowRaycastOutcome.TUNNEL_CENTER:
                    HandleCollisionWithTunnelCenter(raycastHits);
                    break;
                case ArrowRaycastOutcome.WALL:
                    gameEventsManager.DispatchGameEvent(new GameEvent("ArrowCollidedWithWallEvent"));
                    Destroy(gameObject);
                    break;
                case ArrowRaycastOutcome.MONSTER:
                    gameEventsManager.DispatchGameEvent(new GameEvent("ArrowCollidedWithMonsterEvent",
                        new GameEventParameter("PlayerNumber",OwnerPlayerNumber)));
                    Destroy(gameObject);
                    break;
                case ArrowRaycastOutcome.PLAYER:
                    HandleCollisionWithPlayer(raycastHits);
                    Destroy(gameObject);
                    break;
            }
        }

        /// <summary>
        /// Sets the arrow position to the tunnel center and change its direction according to tunnel curve
        /// </summary>
        /// <param name="raycastHits">The list of raycast hits including the one with the tunnel center</param>
        private void HandleCollisionWithTunnelCenter(RaycastHit[] raycastHits)
        {
            GameObject tunnelCenterGameObject = raycastHits
                    .Where<RaycastHit>(hit => hit.collider.CompareTag("TunnelCenter"))
                    .ElementAt<RaycastHit>(0)
                    .collider.gameObject;
            ITunnel tunnel = tunnelCenterGameObject.GetComponent<ITunnelTrigger>().GetTunnel();

            transform.position = new Vector3(
                tunnelCenterGameObject.transform.position.x,
                transform.position.y,
                tunnelCenterGameObject.transform.position.z);

            SetArrowDirection(tunnel.GetOutDirection(currentDirection.GetOppositeDirection()));
        }

        /// <summary>
        /// Sends the arrow hit player event with the informations about the shooter and the hit players
        /// </summary>
        /// <param name="raycastHits">The list of raycast hits including the one with the player</param>
        private void HandleCollisionWithPlayer(RaycastHit[] raycastHits)
        {
            Player hitPlayer = raycastHits
                .Where<RaycastHit>(hit => hit.collider.CompareTag("Player"))
                .ElementAt<RaycastHit>(0)
                .collider.GetComponent<Player>();

            gameEventsManager.DispatchGameEvent(new GameEvent("ArrowHitPlayerEvent",
                new GameEventParameter("ShooterPlayerNumber", OwnerPlayerNumber),
                new GameEventParameter("HitPlayerNumber", hitPlayer.PlayerNumber)));
        }
    }
}

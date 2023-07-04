using DBGA.Common;
using DBGA.EventSystem;
using System.Linq;
using UnityEngine;

namespace DBGA.Player
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
                    gameEventsManager.DispatchGameEvent(new ArrowCollidedWithWallEvent());
                    Destroy(gameObject);
                    break;
                case ArrowRaycastOutcome.MONSTER:
                    gameEventsManager.DispatchGameEvent(new ArrowCollidedWithMonsterEvent());
                    Destroy(gameObject);
                    break;
                case ArrowRaycastOutcome.PLAYER:
                    gameEventsManager.DispatchGameEvent(new ArrowHitPlayerEvent());
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
    }
}

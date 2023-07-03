using DBGA.Common;
using System.Linq;
using UnityEngine;

namespace DBGA.Player
{
    [DisallowMultipleComponent]
    public class ArrowMover : MonoBehaviour
    {
        private const float ARROW_RAYCAST_LENGTH = 0.1f;

        [SerializeField]
        private float speed;

        private Direction currentDirection;

        void Update()
        {
            if (HasCollidedWithTunnelCenter(out RaycastHit[] raycastHits))
            {
                HandleCollisionWithTunnelCenter(raycastHits);
                return;
            }

            transform.Translate(speed * Time.deltaTime * transform.forward, Space.World);
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
        /// Checks if arrow collided with a tunnel center
        /// </summary>
        /// <param name="raycastHits">Hits output in case of raycast collisions</param>
        /// <returns>True if arrow collided with a tunnel center, false otherwise</returns>
        private bool HasCollidedWithTunnelCenter(out RaycastHit[] raycastHits)
        {
            raycastHits = Physics.RaycastAll(transform.position, transform.forward, ARROW_RAYCAST_LENGTH);
            return raycastHits.Any<RaycastHit>(hit => hit.collider.CompareTag("TunnelCenter"));
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

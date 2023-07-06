using UnityEngine;

namespace DBGA.Camera
{
    /// <summary>
    /// Component that follows the player with a smoothing
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraFollowPlayer : MonoBehaviour
    {
        [SerializeField]
        [Range(0f, 0.99f)]
        private float damping;

        private Transform currentPlayerTransform;

        /// <summary>
        /// Updates camera position in order to follow the current player transform with a damping
        /// </summary>
        void FixedUpdate()
        {
            if (currentPlayerTransform == null)
                return;

            transform.position =
                Vector3.Lerp(
                    transform.position,
                    new Vector3(
                        currentPlayerTransform.position.x,
                        transform.position.y,
                        currentPlayerTransform.position.z),
                    1 - damping);
        }

        /// <summary>
        /// Sets the current player transform the camera should follow
        /// </summary>
        /// <param name="playerTransform">The current player transform to follow</param>
        public void SetPlayerTransform(Transform playerTransform)
        {
            this.currentPlayerTransform = playerTransform;
        }

        /// <summary>
        /// Sets the orthographic size of the camera
        /// </summary>
        /// <param name="size"></param>
        public void SetOrthographicCameraSize(float size)
        {
            UnityEngine.Camera.main.orthographicSize = size;
        }
    }
}

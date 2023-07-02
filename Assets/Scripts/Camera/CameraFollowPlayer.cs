using UnityEngine;

namespace DBGA.Camera
{
    [DisallowMultipleComponent]
    public class CameraFollowPlayer : MonoBehaviour
    {
        [SerializeField]
        [Range(0f, 0.99f)]
        private float damping;

        private Transform playerTransform;

        void LateUpdate()
        {
            transform.position = 
                Vector3.Lerp(
                    transform.position,
                    new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z),
                    1 - damping);
        }

        public void SetPlayerTransform(Transform playerTransform)
        {
            this.playerTransform = playerTransform;
        }
    }
}

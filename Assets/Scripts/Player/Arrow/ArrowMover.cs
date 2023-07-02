using UnityEngine;

namespace DBGA.Player
{
    [DisallowMultipleComponent]
    public class ArrowMover : MonoBehaviour
    {
        [SerializeField]
        private float speed;

        void Update()
        {
            transform.Translate(speed * Time.deltaTime * transform.forward, Space.World);
        }
    }
}

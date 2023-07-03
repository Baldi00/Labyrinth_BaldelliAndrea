using DBGA.EventSystem;
using UnityEngine;

namespace DBGA.Player
{
    [DisallowMultipleComponent]
    public class ArrowCollisionDetection : MonoBehaviour
    {
        private GameEventsManager gameEventsManager;

        void Awake()
        {
            gameEventsManager = GameEventsManager.Instance;
        }

        void OnTriggerEnter(Collider other)
        {
            switch (other.gameObject.tag)
            {
                case "Wall":
                    gameEventsManager.DispatchGameEvent(new ArrowCollidedWithWallEvent());
                    Destroy(gameObject);
                    break;
                case "Monster":
                    gameEventsManager.DispatchGameEvent(new ArrowCollidedWithMonsterEvent());
                    Destroy(gameObject);
                    break;
                case "Tunnel":
                    // TODO: Arrow entered a tunnel
                    break;
            }
        }
    }
}
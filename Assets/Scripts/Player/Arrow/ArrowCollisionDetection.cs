using DBGA.Common;
using DBGA.EventSystem;
using System.Collections;
using System.Collections.Generic;
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
                case "Player":
                    gameEventsManager.DispatchGameEvent(new ArrowHitPlayerEvent());
                    Destroy(gameObject);
                    break;
            }
        }
    }
}

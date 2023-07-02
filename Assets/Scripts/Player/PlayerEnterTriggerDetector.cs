using DBGA.EventSystem;
using UnityEngine;

namespace DBGA.Player
{
    [DisallowMultipleComponent]
    public class PlayerEnterTriggerDetector : MonoBehaviour
    {
        private GameEventsManager gameEventsManager;

        void Awake()
        {
            gameEventsManager = GameEventsManager.Instance;
        }

        void OnTriggerEnter(Collider other)
        {
            switch (other.tag)
            {
                case "Monster":
                    gameEventsManager.DispatchGameEvent(new EnteredMonsterTileEvent());
                    break;
                case "AdjacentMonster":
                    gameEventsManager.DispatchGameEvent(new MonsterTileAdjacentEvent() { isPlayerInside = true });
                    break;
                case "Well":
                    gameEventsManager.DispatchGameEvent(new EnteredWellTileEvent());
                    break;
                case "AdjacentWell":
                    gameEventsManager.DispatchGameEvent(new WellTileAdjacentEvent() { isPlayerInside = true });
                    break;
                case "Teleport":
                    gameEventsManager.DispatchGameEvent(new EnteredTeleportTileEvent());
                    break;
                case "AdjacentTeleport":
                    gameEventsManager.DispatchGameEvent(new TeleportTileAdjacentEvent() { isPlayerInside = true });
                    break;
            }
        }

        void OnTriggerExit(Collider other)
        {
            switch (other.tag)
            {
                case "AdjacentMonster":
                    gameEventsManager.DispatchGameEvent(new MonsterTileAdjacentEvent() { isPlayerInside = false });
                    break;
                case "AdjacentWell":
                    gameEventsManager.DispatchGameEvent(new WellTileAdjacentEvent() { isPlayerInside = false });
                    break;
                case "AdjacentTeleport":
                    gameEventsManager.DispatchGameEvent(new TeleportTileAdjacentEvent() { isPlayerInside = false });
                    break;
            }
        }
    }
}

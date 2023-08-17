using DBGA.EventSystem;
using UnityEngine;

namespace DBGA.MazePlayer
{
    [DisallowMultipleComponent]
    public class PlayerEnterTriggerDetector : MonoBehaviour
    {
        private GameEventsManager gameEventsManager;

        public int PlayerNumber { set; get; }

        void Awake()
        {
            gameEventsManager = GameEventsManager.Instance;
        }

        void OnTriggerEnter(Collider other)
        {
            switch (other.tag)
            {
                case "Monster":
                    gameEventsManager.DispatchGameEvent(new GameEvent("EnteredMonsterTileEvent",
                        new GameEventParameter("PlayerNumber", PlayerNumber)));
                    break;
                case "AdjacentMonster":
                    gameEventsManager.DispatchGameEvent(new GameEvent("MonsterTileAdjacentEvent",
                        new GameEventParameter("PlayerNumber", PlayerNumber),
                        new GameEventParameter("IsPlayerInside", true)));
                    break;
                case "Well":
                    gameEventsManager.DispatchGameEvent(new GameEvent("EnteredWellTileEvent",
                        new GameEventParameter("PlayerNumber", PlayerNumber)));
                    break;
                case "AdjacentWell":
                    gameEventsManager.DispatchGameEvent(new GameEvent("WellTileAdjacentEvent",
                        new GameEventParameter("PlayerNumber", PlayerNumber),
                        new GameEventParameter("IsPlayerInside", true)));
                    break;
                case "Teleport":
                    gameEventsManager.DispatchGameEvent(new GameEvent("EnteredTeleportTileEvent",
                        new GameEventParameter("PlayerNumber", PlayerNumber)));
                    break;
                case "AdjacentTeleport":
                    gameEventsManager.DispatchGameEvent(new GameEvent("TeleportTileAdjacentEvent",
                        new GameEventParameter("PlayerNumber", PlayerNumber),
                        new GameEventParameter("IsPlayerInside", true)));
                    break;
            }
        }

        void OnTriggerExit(Collider other)
        {
            switch (other.tag)
            {
                case "AdjacentMonster":
                    gameEventsManager.DispatchGameEvent(new GameEvent("MonsterTileAdjacentEvent",
                        new GameEventParameter("PlayerNumber", PlayerNumber),
                        new GameEventParameter("IsPlayerInside", false)));
                    break;
                case "AdjacentWell":
                    gameEventsManager.DispatchGameEvent(new GameEvent("WellTileAdjacentEvent",
                        new GameEventParameter("PlayerNumber", PlayerNumber),
                        new GameEventParameter("IsPlayerInside", false)));
                    break;
                case "AdjacentTeleport":
                    gameEventsManager.DispatchGameEvent(new GameEvent("TeleportTileAdjacentEvent",
                        new GameEventParameter("PlayerNumber", PlayerNumber),
                        new GameEventParameter("IsPlayerInside", false)));
                    break;
            }
        }
    }
}

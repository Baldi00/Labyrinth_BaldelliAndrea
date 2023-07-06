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
                    gameEventsManager.DispatchGameEvent(new EnteredMonsterTileEvent()
                    { PlayerNumber = PlayerNumber });
                    break;
                case "AdjacentMonster":
                    gameEventsManager.DispatchGameEvent(new MonsterTileAdjacentEvent()
                    { PlayerNumber = PlayerNumber, IsPlayerInside = true });
                    break;
                case "Well":
                    gameEventsManager.DispatchGameEvent(new EnteredWellTileEvent()
                    { PlayerNumber = PlayerNumber });
                    break;
                case "AdjacentWell":
                    gameEventsManager.DispatchGameEvent(new WellTileAdjacentEvent()
                    { PlayerNumber = PlayerNumber, IsPlayerInside = true });
                    break;
                case "Teleport":
                    gameEventsManager.DispatchGameEvent(new EnteredTeleportTileEvent()
                    { PlayerNumber = PlayerNumber });
                    break;
                case "AdjacentTeleport":
                    gameEventsManager.DispatchGameEvent(new TeleportTileAdjacentEvent()
                    { PlayerNumber = PlayerNumber, IsPlayerInside = true });
                    break;
            }
        }

        void OnTriggerExit(Collider other)
        {
            switch (other.tag)
            {
                case "AdjacentMonster":
                    gameEventsManager.DispatchGameEvent(new MonsterTileAdjacentEvent()
                    { PlayerNumber = PlayerNumber, IsPlayerInside = false });
                    break;
                case "AdjacentWell":
                    gameEventsManager.DispatchGameEvent(new WellTileAdjacentEvent()
                    { PlayerNumber = PlayerNumber, IsPlayerInside = false });
                    break;
                case "AdjacentTeleport":
                    gameEventsManager.DispatchGameEvent(new TeleportTileAdjacentEvent()
                    { PlayerNumber = PlayerNumber, IsPlayerInside = false });
                    break;
            }
        }
    }
}

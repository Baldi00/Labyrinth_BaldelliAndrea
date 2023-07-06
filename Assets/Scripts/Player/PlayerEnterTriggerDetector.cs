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
                    { playerNumber = PlayerNumber });
                    break;
                case "AdjacentMonster":
                    gameEventsManager.DispatchGameEvent(new MonsterTileAdjacentEvent()
                    { playerNumber = PlayerNumber, isPlayerInside = true });
                    break;
                case "Well":
                    gameEventsManager.DispatchGameEvent(new EnteredWellTileEvent()
                    { playerNumber = PlayerNumber });
                    break;
                case "AdjacentWell":
                    gameEventsManager.DispatchGameEvent(new WellTileAdjacentEvent() 
                    { playerNumber = PlayerNumber, isPlayerInside = true });
                    break;
                case "Teleport":
                    gameEventsManager.DispatchGameEvent(new EnteredTeleportTileEvent()
                    { playerNumber = PlayerNumber });
                    break;
                case "AdjacentTeleport":
                    gameEventsManager.DispatchGameEvent(new TeleportTileAdjacentEvent()
                    { playerNumber = PlayerNumber, isPlayerInside = true });
                    break;
            }
        }

        void OnTriggerExit(Collider other)
        {
            switch (other.tag)
            {
                case "AdjacentMonster":
                    gameEventsManager.DispatchGameEvent(new MonsterTileAdjacentEvent() 
                    { playerNumber = PlayerNumber, isPlayerInside = false });
                    break;
                case "AdjacentWell":
                    gameEventsManager.DispatchGameEvent(new WellTileAdjacentEvent() 
                    { playerNumber = PlayerNumber, isPlayerInside = false });
                    break;
                case "AdjacentTeleport":
                    gameEventsManager.DispatchGameEvent(new TeleportTileAdjacentEvent() 
                    { playerNumber = PlayerNumber, isPlayerInside = false });
                    break;
            }
        }
    }
}

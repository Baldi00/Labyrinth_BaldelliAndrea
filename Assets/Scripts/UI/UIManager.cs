using DBGA.EventSystem;
using UnityEngine;

namespace DBGA.UI
{
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour, IGameEventsListener
    {
        [SerializeField]
        private GameObject bloodUi;
        [SerializeField]
        private GameObject windUi;
        [SerializeField]
        private GameObject moldUi;

        void Start()
        {
            AddListener();
        }

        public void ReceiveGameEvent(GameEvent gameEvent)
        {
            switch (gameEvent)
            {
                case MonsterTileAdjacentEvent monsterTileAdjacentEvent:
                    bloodUi.SetActive(monsterTileAdjacentEvent.isPlayerInside);
                    break;
                case WellTileAdjacentEvent wellTileAdjacentEvent:
                    moldUi.SetActive(wellTileAdjacentEvent.isPlayerInside);
                    break;
                case TeleportTileAdjacentEvent teleportTileAdjacentEvent:
                    windUi.SetActive(teleportTileAdjacentEvent.isPlayerInside);
                    break;
            }
        }

        private void AddListener()
        {
            GameEventsManager.Instance.AddGameEventListener(this, typeof(MonsterTileAdjacentEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(WellTileAdjacentEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(TeleportTileAdjacentEvent));
        }
    }
}

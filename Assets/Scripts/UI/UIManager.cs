using DBGA.EventSystem;
using UnityEngine;

namespace DBGA.UI
{
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour, IGameEventsListener
    {
        [Header("Win/Lose UI")]
        [SerializeField]
        private GameObject youWinUi;
        [SerializeField]
        private GameObject youLoseUi;

        [Header("Special UI")]
        [SerializeField]
        private GameObject bloodUi;
        [SerializeField]
        private GameObject windUi;
        [SerializeField]
        private GameObject moldUi;

        void Start()
        {
            AddGameEventsListeners();
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
                case EnteredWellTileEvent:
                case EnteredMonsterTileEvent:
                    youLoseUi.SetActive(true);
                    break;
            }
        }

        private void AddGameEventsListeners()
        {
            GameEventsManager.Instance.AddGameEventListener(this, typeof(MonsterTileAdjacentEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(WellTileAdjacentEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(TeleportTileAdjacentEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(EnteredWellTileEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(EnteredMonsterTileEvent));
        }
    }
}

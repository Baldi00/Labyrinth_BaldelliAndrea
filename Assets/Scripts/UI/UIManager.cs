using DBGA.EventSystem;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField]
        private Text youLoseDescription;
        [SerializeField]
        private string youLoseMonsterDescription;
        [SerializeField]
        private string youLoseWellDescription;
        [SerializeField]
        private string youLoseNoArrowDescription;
        [SerializeField]
        private string youLoseArrowHitPlayerDescription;

        [Header("Special UI")]
        [SerializeField]
        private GameObject bloodUi;
        [SerializeField]
        private GameObject windUi;
        [SerializeField]
        private GameObject moldUi;

        [Header("Arrows count")]
        [SerializeField]
        private Text arrowCount;

        [Header("Invalid move")]
        [SerializeField]
        private InvalidMoveUIManager invalidMove;

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
                    youLoseDescription.text = youLoseWellDescription;
                    youLoseUi.SetActive(true);
                    break;
                case EnteredMonsterTileEvent:
                    youLoseDescription.text = youLoseMonsterDescription;
                    youLoseUi.SetActive(true);
                    break;
                case PlayerLostForNoArrowRemainingEvent:
                    youLoseDescription.text = youLoseNoArrowDescription;
                    youLoseUi.SetActive(true);
                    break;
                case ArrowHitPlayerEvent:
                    youLoseDescription.text = youLoseArrowHitPlayerDescription;
                    youLoseUi.SetActive(true);
                    break;
                case ArrowCollidedWithMonsterEvent:
                    youWinUi.SetActive(true);
                    break;
                case ArrowShotEvent arrowShotEvent:
                    arrowCount.text = $"Arrows: {arrowShotEvent.remainingArrows}";
                    break;
                case InitializeArrowCountEvent initializeArrowCountEvent:
                    arrowCount.text = $"Arrows: {initializeArrowCountEvent.remainingArrows}";
                    break;
                case InvalidMoveEvent:
                    invalidMove.Show();
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
            GameEventsManager.Instance.AddGameEventListener(this, typeof(ArrowCollidedWithMonsterEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(ArrowShotEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(InitializeArrowCountEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(PlayerLostForNoArrowRemainingEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(InvalidMoveEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(ArrowHitPlayerEvent));
        }
    }
}

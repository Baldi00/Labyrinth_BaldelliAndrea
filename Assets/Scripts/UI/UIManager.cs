using DBGA.EventSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DBGA.UI
{
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour, IGameEventsListener
    {
        private class CurrentPlayerUIInfo
        {
            public bool bloodUiOn;
            public bool windUiOn;
            public bool moldUiOn;
        }

        [Header("Win/Lose UI")]
        [SerializeField]
        private GameObject youWinUi;
        [SerializeField]
        private GameObject youLoseUi;
        [SerializeField]
        private Text youLoseDescription;
        [SerializeField]
        private Text anotherPlayerLost;
        [SerializeField]
        private string youLoseMonsterDescription;
        [SerializeField]
        private string youLoseWellDescription;
        [SerializeField]
        private string youLoseNoArrowDescription;
        [SerializeField]
        private string youLoseArrowHitPlayerDescription;
        [SerializeField]
        private string anotherPlayerWonDescription;

        [Header("Special UI")]
        [SerializeField]
        private GameObject bloodUi;
        [SerializeField]
        private GameObject windUi;
        [SerializeField]
        private GameObject moldUi;

        [Header("Player info")]
        [SerializeField]
        private Text playerNumber;
        [SerializeField]
        private Text arrowCount;

        [Header("Invalid move")]
        [SerializeField]
        private InvalidMoveUIManager invalidMove;

        private int currentPlayerNumber;
        private Dictionary<int, CurrentPlayerUIInfo> playersUIStates;

        void Awake()
        {
            AddGameEventsListeners();
            playersUIStates = new Dictionary<int, CurrentPlayerUIInfo>();
        }

        public void ReceiveGameEvent(GameEvent gameEvent)
        {
            switch (gameEvent)
            {
                case PlayerAddedEvent playerAddedEvent:
                    AddCurrentPlayerNumberUIInfoState(playerAddedEvent.playerNumber);
                    break;
                case MonsterTileAdjacentEvent monsterTileAdjacentEvent:
                    playersUIStates[monsterTileAdjacentEvent.playerNumber].bloodUiOn = monsterTileAdjacentEvent.isPlayerInside;
                    UpdateSpecialUI();
                    break;
                case WellTileAdjacentEvent wellTileAdjacentEvent:
                    playersUIStates[wellTileAdjacentEvent.playerNumber].moldUiOn = wellTileAdjacentEvent.isPlayerInside;
                    UpdateSpecialUI();
                    break;
                case TeleportTileAdjacentEvent teleportTileAdjacentEvent:
                    playersUIStates[teleportTileAdjacentEvent.playerNumber].windUiOn = teleportTileAdjacentEvent.isPlayerInside;
                    UpdateSpecialUI();
                    break;
                case EnteredWellTileEvent enteredWellTileEvent:
                    HandleLoseEvents(enteredWellTileEvent.playerNumber, youLoseWellDescription);
                    break;
                case EnteredMonsterTileEvent enteredMonsterTileEvent:
                    HandleLoseEvents(enteredMonsterTileEvent.playerNumber, youLoseMonsterDescription);
                    break;
                case PlayerLostForNoArrowRemainingEvent playerLostForNoArrowRemainingEvent:
                    HandleLoseEvents(playerLostForNoArrowRemainingEvent.playerNumber, youLoseNoArrowDescription);
                    break;
                case ArrowHitPlayerEvent arrowHitPlayerEvent:
                    HandleArrowHitPlayerEvent(arrowHitPlayerEvent);
                    break;
                case ArrowCollidedWithMonsterEvent arrowCollidedWithMonsterEvent:
                    HandleWinEvent(arrowCollidedWithMonsterEvent.playerNumber);
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
                case NextPlayerStartTurnEvent nextPlayerStartTurnEvent:
                    currentPlayerNumber = nextPlayerStartTurnEvent.nextPlayerNumber;
                    playerNumber.text = $"Player {currentPlayerNumber + 1}";
                    arrowCount.text = $"Arrows: {nextPlayerStartTurnEvent.currentPlayerArrows}";
                    UpdateSpecialUI();
                    break;
            }
        }

        public void LoadMainMenuScene()
        {
            SceneManager.LoadScene("MainMenuScene");
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
            GameEventsManager.Instance.AddGameEventListener(this, typeof(NextPlayerStartTurnEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(PlayerAddedEvent));
        }

        private void AddCurrentPlayerNumberUIInfoState(int playerNumber)
        {
            if (!playersUIStates.ContainsKey(playerNumber))
                playersUIStates.Add(playerNumber,
                    new CurrentPlayerUIInfo() { bloodUiOn = false, windUiOn = false, moldUiOn = false });
        }

        private void UpdateSpecialUI()
        {
            bloodUi.SetActive(playersUIStates[currentPlayerNumber].bloodUiOn);
            moldUi.SetActive(playersUIStates[currentPlayerNumber].moldUiOn);
            windUi.SetActive(playersUIStates[currentPlayerNumber].windUiOn);
        }

        private void HandleLoseEvents(int playerNumber, string loseDescription)
        {
            if (playerNumber == 0)
            {
                youLoseDescription.text = $"Player 1 {loseDescription}";
                youLoseUi.SetActive(true);
            }
            else
            {
                anotherPlayerLost.text = $"Player {currentPlayerNumber + 1} {loseDescription}";
                anotherPlayerLost.enabled = true;
            }
        }

        private void HandleWinEvent(int playerNumber)
        {
            if (playerNumber == 0)
                youWinUi.SetActive(true);
            else
            {
                youLoseDescription.text = $"Player {currentPlayerNumber + 1} {anotherPlayerWonDescription}";
                youLoseUi.SetActive(true);
            }
        }

        private void HandleArrowHitPlayerEvent(ArrowHitPlayerEvent arrowHitPlayerEvent)
        {
            if (arrowHitPlayerEvent.hitPlayerNumber == 0)
            {
                youLoseDescription.text = $"Player 1 {youLoseArrowHitPlayerDescription}";
                youLoseUi.SetActive(true);
            }
            else
            {
                anotherPlayerLost.text =
                    $"Player {arrowHitPlayerEvent.hitPlayerNumber + 1} {youLoseArrowHitPlayerDescription}";
                anotherPlayerLost.enabled = true;
            }
        }
    }
}

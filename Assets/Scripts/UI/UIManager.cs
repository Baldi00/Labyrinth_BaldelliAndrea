using DBGA.EventSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DBGA.UI
{
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour, IGameEventsListener
    {
        private sealed class PlayerUIInfoState
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
        private Dictionary<int, PlayerUIInfoState> playersUIStates;

        void Awake()
        {
            AddGameEventsListeners();
            playersUIStates = new Dictionary<int, PlayerUIInfoState>();
            playerNumber.color = Color.blue;
        }

        public void ReceiveGameEvent(GameEvent gameEvent)
        {
            switch (gameEvent)
            {
                case PlayerAddedEvent playerAddedEvent:
                    AddCurrentPlayerNumberUIInfoState(playerAddedEvent.playerNumber);
                    break;
                case PlayerStartedTurnEvent nextPlayerStartTurnEvent:
                    HandlePlayerStartedTurnEvent(nextPlayerStartTurnEvent);
                    break;
                case InvalidMoveEvent:
                    invalidMove.Show();
                    break;
            }

            HandleSpecialUIUpdatesEvents(gameEvent);
            HandlePlayerLoseEvents(gameEvent);
            HandleArrowEvents(gameEvent);
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
            GameEventsManager.Instance.AddGameEventListener(this, typeof(InitializeArrowCountEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(ArrowShotEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(ArrowCollidedWithMonsterEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(ArrowHitPlayerEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(PlayerLostForNoArrowRemainingEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(PlayerStartedTurnEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(PlayerAddedEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(InvalidMoveEvent));
        }

        /// <summary>
        /// Adds an empty UI info state for the given player 
        /// </summary>
        /// <param name="playerNumber">The player number for which to add the UI info state</param>
        private void AddCurrentPlayerNumberUIInfoState(int playerNumber)
        {
            if (!playersUIStates.ContainsKey(playerNumber))
                playersUIStates.Add(playerNumber,
                    new PlayerUIInfoState() { bloodUiOn = false, windUiOn = false, moldUiOn = false });
        }

        /// <summary>
        /// Sets up the player number, color, arrows count UI and updates special UI for the current player
        /// </summary>
        /// <param name="playerStartedTurnEvent">The event containing the info about the current player</param>
        private void HandlePlayerStartedTurnEvent(PlayerStartedTurnEvent playerStartedTurnEvent)
        {
            currentPlayerNumber = playerStartedTurnEvent.playerNumber;
            playerNumber.text = $"Player {currentPlayerNumber + 1}";
            playerNumber.color = playerStartedTurnEvent.playerColor;
            arrowCount.text = $"Arrows: {playerStartedTurnEvent.playerArrowsCount}";
            UpdateSpecialUI();
        }

        /// <summary>
        /// Handles the special UI updates events
        /// </summary>
        /// <param name="gameEvent">Triggered game event to evaluate</param>
        private void HandleSpecialUIUpdatesEvents(GameEvent gameEvent)
        {
            switch (gameEvent)
            {
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
            }
        }

        /// <summary>
        /// Updates the blood, wind and mold UI visibility based on current player UI info state
        /// </summary>
        private void UpdateSpecialUI()
        {
            bloodUi.SetActive(playersUIStates[currentPlayerNumber].bloodUiOn);
            moldUi.SetActive(playersUIStates[currentPlayerNumber].moldUiOn);
            windUi.SetActive(playersUIStates[currentPlayerNumber].windUiOn);
        }

        /// <summary>
        /// Handles the player lose conditions events
        /// </summary>
        /// <param name="gameEvent">Triggered game event to evaluate</param>
        private void HandlePlayerLoseEvents(GameEvent gameEvent)
        {
            switch (gameEvent)
            {
                case EnteredWellTileEvent enteredWellTileEvent:
                    HandleLoseEventsUI(enteredWellTileEvent.playerNumber, youLoseWellDescription);
                    break;
                case EnteredMonsterTileEvent enteredMonsterTileEvent:
                    HandleLoseEventsUI(enteredMonsterTileEvent.playerNumber, youLoseMonsterDescription);
                    break;
                case PlayerLostForNoArrowRemainingEvent playerLostForNoArrowRemainingEvent:
                    HandleLoseEventsUI(playerLostForNoArrowRemainingEvent.playerNumber, youLoseNoArrowDescription);
                    break;
            }
        }

        /// <summary>
        /// Handles the events that involves an arrow
        /// </summary>
        /// <param name="gameEvent">Triggered game event to evaluate</param>
        private void HandleArrowEvents(GameEvent gameEvent)
        {
            switch (gameEvent)
            {
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
            }
        }

        /// <summary>
        /// If given player number is player 1 shows win UI, the lose UI is shown otherwise
        /// </summary>
        /// <param name="playerNumber">The player that has won</param>
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

        /// <summary>
        /// Shows the lose UI if the player 1 lost, shows the other player lost message otherwise
        /// </summary>
        /// <param name="playerNumber">The player that has lost</param>
        /// <param name="loseDescription">The lose description to display</param>
        private void HandleLoseEventsUI(int playerNumber, string loseDescription)
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

        /// <summary>
        /// Shows the lose UI for the player that has been hit by an arrow
        /// </summary>
        /// <param name="arrowHitPlayerEvent">The event containing the player that has been hit</param>
        /// <see cref="HandleLoseEventsUI"/>
        private void HandleArrowHitPlayerEvent(ArrowHitPlayerEvent arrowHitPlayerEvent)
        {
            HandleLoseEventsUI(arrowHitPlayerEvent.hitPlayerNumber, youLoseArrowHitPlayerDescription);
        }
    }
}

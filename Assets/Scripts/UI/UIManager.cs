using DBGA.EventSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DBGA.UI
{
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour
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

        public void LoadMainMenuScene()
        {
            SceneManager.LoadScene("MainMenuScene");
        }

        private void AddGameEventsListeners()
        {
            GameEventsManager.Instance.AddEventCallback("MonsterTileAdjacentEvent", HandleSpecialUIUpdatesEvents);
            GameEventsManager.Instance.AddEventCallback("WellTileAdjacentEvent", HandleSpecialUIUpdatesEvents);
            GameEventsManager.Instance.AddEventCallback("TeleportTileAdjacentEvent", HandleSpecialUIUpdatesEvents);
            GameEventsManager.Instance.AddEventCallback("EnteredWellTileEvent", HandlePlayerLoseEvents);
            GameEventsManager.Instance.AddEventCallback("EnteredMonsterTileEvent", HandlePlayerLoseEvents);
            GameEventsManager.Instance.AddEventCallback("PlayerLostForNoArrowRemainingEvent", HandlePlayerLoseEvents);
            GameEventsManager.Instance.AddEventCallback("InitializeArrowCountEvent", HandleArrowEvents);
            GameEventsManager.Instance.AddEventCallback("ArrowShotEvent", HandleArrowEvents);
            GameEventsManager.Instance.AddEventCallback("ArrowCollidedWithMonsterEvent", HandleWinEvent);
            GameEventsManager.Instance.AddEventCallback("ArrowHitPlayerEvent", HandleArrowHitPlayerEvent);
            GameEventsManager.Instance.AddEventCallback("PlayerStartedTurnEvent", HandlePlayerStartedTurnEvent);
            GameEventsManager.Instance.AddEventCallback("PlayerAddedEvent", AddCurrentPlayerNumberUIInfoState);
            GameEventsManager.Instance.AddEventCallback("InvalidMoveEvent", HandleInvalidMoveEvent);
        }

        /// <summary>
        /// Adds an empty UI info state for the given player 
        /// </summary>
        /// <param name="playerNumber">The player number for which to add the UI info state</param>
        private void AddCurrentPlayerNumberUIInfoState(GameEvent playerAddedEvent)
        {
            if (!playerAddedEvent.TryGetParameter("PlayerNumber", out int playerNumber))
                return;

            if (!playersUIStates.ContainsKey(playerNumber))
                playersUIStates.Add(playerNumber,
                    new PlayerUIInfoState() { bloodUiOn = false, windUiOn = false, moldUiOn = false });
        }

        /// <summary>
        /// Sets up the player number, color, arrows count UI and updates special UI for the current player
        /// </summary>
        /// <param name="playerStartedTurnEvent">The event containing the info about the current player</param>
        private void HandlePlayerStartedTurnEvent(GameEvent playerStartedTurnEvent)
        {
            if (!playerStartedTurnEvent.TryGetParameter("PlayerNumber", out int currentPlayerNumber))
                return;
            if (!playerStartedTurnEvent.TryGetParameter("PlayerColor", out Color playerColor))
                return;
            if (!playerStartedTurnEvent.TryGetParameter("PlayerArrowsCount", out int playerArrowsCount))
                return;

            playerNumber.text = $"Player {currentPlayerNumber + 1}";
            playerNumber.color = playerColor;
            arrowCount.text = $"Arrows: {playerArrowsCount}";
            UpdateSpecialUI();
        }

        private void HandleInvalidMoveEvent(GameEvent gameEvent)
        {
            invalidMove.Show();
        }

        /// <summary>
        /// Handles the special UI updates events
        /// </summary>
        /// <param name="gameEvent">Triggered game event to evaluate</param>
        private void HandleSpecialUIUpdatesEvents(GameEvent gameEvent)
        {
            if (!gameEvent.TryGetParameter("PlayerNumber", out int playerNumber))
                return;
            if (!gameEvent.TryGetParameter("IsPlayerInside", out bool isPlayerInside))
                return;

            switch (gameEvent.Name)
            {
                case "MonsterTileAdjacentEvent":
                    playersUIStates[playerNumber].bloodUiOn = isPlayerInside;
                    UpdateSpecialUI();
                    break;
                case "WellTileAdjacentEvent":
                    playersUIStates[playerNumber].moldUiOn = isPlayerInside;
                    UpdateSpecialUI();
                    break;
                case "TeleportTileAdjacentEvent":
                    playersUIStates[playerNumber].windUiOn = isPlayerInside;
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
            if (!gameEvent.TryGetParameter("PlayerNumber", out int playerNumber))
                return;

            switch (gameEvent.Name)
            {
                case "EnteredWellTileEvent":
                    HandleLoseEventsUI(playerNumber, youLoseWellDescription);
                    break;
                case "EnteredMonsterTileEvent":
                    HandleLoseEventsUI(playerNumber, youLoseMonsterDescription);
                    break;
                case "PlayerLostForNoArrowRemainingEvent":
                    HandleLoseEventsUI(playerNumber, youLoseNoArrowDescription);
                    break;
            }
        }

        /// <summary>
        /// Handles the events that involves an arrow
        /// </summary>
        /// <param name="gameEvent">Triggered game event to evaluate</param>
        private void HandleArrowEvents(GameEvent gameEvent)
        {
            if (!gameEvent.TryGetParameter("RemainingArrows", out int remainingArrows))
                return;

            if (gameEvent.Name == "ArrowShotEvent" || gameEvent.Name == "InitializeArrowCountEvent")
                arrowCount.text = $"Arrows: {remainingArrows}";
        }

        /// <summary>
        /// If given player number is player 1 shows win UI, the lose UI is shown otherwise
        /// </summary>
        /// <param name="playerNumber">The player that has won</param>
        private void HandleWinEvent(GameEvent winEvent)
        {
            if (!winEvent.TryGetParameter("PlayerNumber", out int playerNumber))
                return;

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
        private void HandleArrowHitPlayerEvent(GameEvent arrowHitPlayerEvent)
        {
            if (!arrowHitPlayerEvent.TryGetParameter("HitPlayerNumber", out int hitPlayerNumber))
                return;

            HandleLoseEventsUI(hitPlayerNumber, youLoseArrowHitPlayerDescription);
        }
    }
}

using DBGA.Camera;
using DBGA.Common;
using DBGA.EventSystem;
using DBGA.MapGeneration;
using DBGA.MazePlayer;
using DBGA.ThroughScenes;
using DBGA.Tiles;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DBGA.GameManager
{
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour, IGameEventsListener
    {
        private const int PRECOMPUTED_MAP_SIZE = 20;

        [Header("Map generation")]
        [SerializeField]
        private GameObject precomputedMap;
        [SerializeField]
        private bool generateRandomMap;
        [SerializeField]
        private MapGenerator mapGenerator;

        [Header("Player")]
        [SerializeField]
        private Player playerPrefab;

        [Header("Camera")]
        [SerializeField]
        private CameraFollowPlayer mainCamera;
        [SerializeField]
        private float inGameCameraSize = 5.4f;

        [Header("Fog")]
        [SerializeField]
        private GameObject fogElementPrefab;

        private int gridSize;
        private GameObject mapContainer;
        private Tile[][] grid;

        /// <summary>
        /// The player list will contain all added players, also the ones who has lost and are disabled
        /// </summary>
        private List<Player> players;
        private Player currentPlayer;
        private int currentPlayerIndex;
        private Dictionary<int, Color> playerColors;

        private MapElementsList mapElementsList;
        private GameObject monster;
        private Tile monsterTile;

        private Fog[][] fogGrid;
        private GameObject fogContainer;
        private bool isFogVisible;

        private bool isMapGenerated;
        private bool isGameEnded;

        private GameEventsManager gameEventsManager;

        void Awake()
        {
            ThroughScenesParameters.MapGenerationError = false;
            gridSize = generateRandomMap ? ThroughScenesParameters.GridSize : PRECOMPUTED_MAP_SIZE;
            mapElementsList = ThroughScenesParameters.MapElementsList;
            gameEventsManager = GameEventsManager.Instance;
            players = new List<Player>();
            playerColors = new Dictionary<int, Color>();
        }

        void Start()
        {
            AddGameEventListeners();
            PlaceMap((grid) =>
            {
                this.grid = grid;
                isMapGenerated = true;
                ConnectTunnelTiles();
                PlaceFog();
                PlaceMapElements();
                PlacePlayer();
                currentPlayerIndex = 0;
                currentPlayer = players[currentPlayerIndex];
                mainCamera.SetPlayerTransform(currentPlayer.transform);
                mainCamera.SetOrthographicCameraSize(inGameCameraSize);
            });
        }

        void OnDestroy()
        {
            gameEventsManager.RemoveAllGameEventListeners();
        }

        public void ReceiveGameEvent(IGameEvent gameEvent)
        {
            switch (gameEvent)
            {
                case InputMoveEvent inputMoveEvent:
                    HandleInputMoveEvent(inputMoveEvent);
                    break;
                case InputArrowShotEvent inputArrowShotEvent:
                    HandleInputArrowShotEvent(inputArrowShotEvent);
                    break;
                case InputToggleFogVisibilityEvent:
                    HandleToggleFogVisibilityEvent();
                    break;
                case EnteredTeleportTileEvent:
                    TeleportPlayerOntoRandomEmptyTile();
                    break;
                case EnteredWellTileEvent:
                case EnteredMonsterTileEvent:
                case PlayerLostForNoArrowRemainingEvent:
                    HandlePlayerLostEvents(currentPlayerIndex);
                    break;
                case ArrowHitPlayerEvent arrowHitPlayerEvent:
                    HandleArrowHitPlayerEvent(arrowHitPlayerEvent);
                    break;
                case ArrowCollidedWithMonsterEvent:
                    EndGame();
                    break;
                case PlayerExploredTileEvent playerExploredTileEvent:
                    HandlePlayerExploredTileEvent(playerExploredTileEvent);
                    break;
                case ArrowCollidedWithWallEvent:
                    HandleArrowCollidedWithWallEvent();
                    break;
                case PlayerCompletedMovementEvent:
                    ProceedToNextPlayer();
                    break;
            }
        }

        /// <summary>
        /// Instantiates and places a player on a random empty tile, sets its number, color,
        /// and adds it into the players list
        /// </summary>
        /// <see cref="GetRandomPositionOnEmptyTile"/>
        public void PlacePlayer()
        {
            if (!isMapGenerated)
                return;

            // Instantiates and places the player
            Vector2Int randomPosition = GetRandomPositionOnEmptyTile();
            GameObject playerGameObject = InstantiateOnTile(playerPrefab.gameObject, randomPosition, null);
            Player placedPlayer = playerGameObject.GetComponent<Player>();
            placedPlayer.PositionOnGrid = randomPosition;

            // Sets the player number and adds to the player list
            int placedPlayerNumber = players.Count;
            placedPlayer.PlayerNumber = placedPlayerNumber;
            playerGameObject.GetComponent<PlayerEnterTriggerDetector>().PlayerNumber = placedPlayerNumber;

            // Sets a random color (or blue if player 1)
            Color playerColor = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f);
            if (placedPlayerNumber == 0)
                playerColor = Color.blue;
            placedPlayer.GetComponentInChildren<Renderer>().material.color = playerColor;
            playerColors.Add(placedPlayerNumber, playerColor);

            // Adds the player to the players list
            players.Add(placedPlayer);

            gameEventsManager.DispatchGameEvent(new PlayerAddedEvent() { PlayerNumber = placedPlayer.PlayerNumber });
        }

        /// <summary>
        /// Registers the game manager as a listener for some tipe of game events
        /// </summary>
        private void AddGameEventListeners()
        {
            gameEventsManager.AddGameEventListener(this, typeof(InputMoveEvent));
            gameEventsManager.AddGameEventListener(this, typeof(InputArrowShotEvent));
            gameEventsManager.AddGameEventListener(this, typeof(InputToggleFogVisibilityEvent));
            gameEventsManager.AddGameEventListener(this, typeof(EnteredTeleportTileEvent));
            gameEventsManager.AddGameEventListener(this, typeof(EnteredWellTileEvent));
            gameEventsManager.AddGameEventListener(this, typeof(EnteredMonsterTileEvent));
            gameEventsManager.AddGameEventListener(this, typeof(PlayerExploredTileEvent));
            gameEventsManager.AddGameEventListener(this, typeof(PlayerLostForNoArrowRemainingEvent));
            gameEventsManager.AddGameEventListener(this, typeof(PlayerCompletedMovementEvent));
            gameEventsManager.AddGameEventListener(this, typeof(ArrowCollidedWithWallEvent));
            gameEventsManager.AddGameEventListener(this, typeof(ArrowCollidedWithMonsterEvent));
            gameEventsManager.AddGameEventListener(this, typeof(ArrowHitPlayerEvent));
        }

        /// <summary>
        /// Places monsters, teleports and wells on the grid in empty tiles
        /// </summary>
        /// <see cref="IsEmptyTile"/>
        private void PlaceMapElements()
        {
            foreach (MapElementsListItem mapElementsItem in mapElementsList.MapElements)
                for (int i = 0; i < mapElementsItem.Count; i++)
                {
                    Vector2Int randomPosition = GetRandomPositionOnEmptyTile();
                    GameObject mapElement = InstantiateOnTile(mapElementsItem.Prefab, randomPosition, transform);
                    Tile placementTile = GetTileAtPosition(randomPosition);

                    switch (mapElementsItem.MapElementType)
                    {
                        case MapElementType.MONSTER:
                            placementTile.HasMonster = true;
                            monsterTile = placementTile;
                            monster = mapElement;
                            break;
                        case MapElementType.TELEPORT:
                            placementTile.HasTeleport = true;
                            break;
                        case MapElementType.WELL:
                            placementTile.HasWell = true;
                            break;
                    }
                }
        }

        /// <summary>
        /// Generates the map and instantiates it in game, calls the given callback when the map is generated
        /// </summary>
        /// <param name="onMapGeneratedCallback">Callback called after the map is generated</param>
        private void PlaceMap(System.Action<Tile[][]> onMapGeneratedCallback)
        {
            if (mapContainer != null)
                Destroy(mapContainer);

            if (generateRandomMap)
            {
                mapContainer = new("MapContainer");
                StartCoroutine(mapGenerator.GenerateMap(gridSize, mapContainer.transform, onMapGeneratedCallback));
            }
            else
            {
                Tile[][] precomputedGrid = SetupPrecomputedMap();
                onMapGeneratedCallback?.Invoke(precomputedGrid);
            }
            mapContainer.transform.parent = transform;
        }

        /// <summary>
        /// Instantiates the precomputed map and sets up the tiles grid
        /// </summary>
        /// <returns>The prepared tiles grid</returns>
        private Tile[][] SetupPrecomputedMap()
        {
            mapContainer = Instantiate(precomputedMap, Vector3.zero, Quaternion.identity, transform);
            mapContainer.name = "MapContainer";
            Tile[] tiles = new Tile[gridSize * gridSize];
            for (int childIndex = 0; childIndex < mapContainer.transform.childCount; childIndex++)
                tiles[childIndex] = mapContainer.transform.GetChild(childIndex).GetComponent<Tile>();

            Tile[][] precomputedGrid = Utils.ArrayToMatrix<Tile>(tiles, gridSize, gridSize);
            for (int row = 0; row < gridSize; row++)
                for (int col = 0; col < gridSize; col++)
                    precomputedGrid[row][col].PositionOnGrid = new Vector2Int(row, col);
            return precomputedGrid;
        }

        /// <summary>
        /// Instantiates and places the fog tiles on the grid
        /// </summary>
        private void PlaceFog()
        {
            if (fogContainer != null)
                Destroy(fogContainer);

            fogContainer = new("FogContainer");
            fogContainer.transform.parent = transform;

            fogGrid = new Fog[gridSize][];

            for (int row = 0; row < gridSize; row++)
            {
                fogGrid[row] = new Fog[gridSize];
                for (int col = 0; col < gridSize; col++)
                    fogGrid[row][col] =
                        InstantiateOnTile(fogElementPrefab, new Vector2Int(row, col), fogContainer.transform)
                        .GetComponent<Fog>();
            }
            isFogVisible = true;
        }

        /// <summary>
        /// Returns a random position of an empty tile on the grid
        /// </summary>
        /// <returns>A random position of an empty tile on the grid</returns>
        /// <see cref="IsEmptyTile"/>
        /// <exception cref="NoEmptyTilesRemainingException">Thrown if an empty random tile couldn't be find</exception>
        private Vector2Int GetRandomPositionOnEmptyTile()
        {
            int iterations = 0;
            Vector2Int randomPosition;
            do
            {
                iterations++;
                if (iterations > 1000)
                {
                    ThroughScenesParameters.MapGenerationError = true;
                    SceneManager.LoadScene("MainMenuScene");
                    throw new NoEmptyTilesRemainingException("No more empty tiles on the map");
                }

                randomPosition =
                    new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));
            } while (!IsEmptyTile(randomPosition));

            return randomPosition;
        }

        /// <summary>
        /// Instantiates the given game object in the given position of the grid with the given transform as parent
        /// </summary>
        /// <param name="gameObject">The game object to place</param>
        /// <param name="position">The position on the grid on which the game object will be instantiated</param>
        /// <param name="parent">The parent transform under which the game object will be instantiated</param>
        /// <returns>The instantiated game object</returns>
        private GameObject InstantiateOnTile(GameObject gameObject, Vector2Int position, Transform parent)
        {
            return Instantiate(gameObject, new Vector3(position.x, 0, position.y), Quaternion.identity, parent);
        }

        /// <summary>
        /// Checks if the tile is empty and can be used to place things.
        /// Empty means that it is not a Tunnel tile, it has no monsters, teleports, wells or player on it and it's not void
        /// </summary>
        /// <param name="position">The position on the grid of the tile to test</param>
        /// <returns>True if the tile is empty, false otherwise</returns>
        private bool IsEmptyTile(Vector2Int position)
        {
            if (currentPlayer != null && position.Equals(currentPlayer.PositionOnGrid))
                return false;

            Tile tile = grid[position.x][position.y];
            if (tile is Tunnel || tile is VoidTile || tile.HasMonster || tile.HasTeleport || tile.HasWell)
                return false;
            return true;
        }

        /// <summary>
        /// Returns the tile in the given position on the grid, null if position is out of the grid
        /// </summary>
        /// <param name="position">The position of the tile you want to get</param>
        /// <returns>The tile in the given position on the grid, null if position is out of the grid</returns>
        private Tile GetTileAtPosition(Vector2Int position)
        {
            if (Utils.IsPositionInsideGrid(position, gridSize))
                return grid[position.x][position.y];
            return null;
        }

        /// <summary>
        /// Returns the fog game object in the given position on the grid, null if position is out of the grid
        /// </summary>
        /// <param name="position">The position of the fog game object you want to get</param>
        /// <returns>The fog game object in the given position on the grid, null if position is out of the grid</returns>
        private Fog GetFogAtPosition(Vector2Int position)
        {
            if (Utils.IsPositionInsideGrid(position, gridSize))
                return fogGrid[position.x][position.y];
            return null;
        }

        /// <summary>
        /// Teleport the player onto another empty tile
        /// </summary>
        private void TeleportPlayerOntoRandomEmptyTile()
        {
            Vector2Int randomPosition = GetRandomPositionOnEmptyTile();
            currentPlayer.TeleportToNextPosition(randomPosition);
        }

        /// <summary>
        /// Teleport the monster onto another empty tile inside the fog
        /// </summary>
        private void TeleportMonsterOntoRandomEmptyTile()
        {
            Vector2Int randomPosition;
            do
                randomPosition = GetRandomPositionOnEmptyTile();
            while (GetTileAtPosition(randomPosition).PlayerExplored);

            monsterTile.HasMonster = false;
            monster.transform.position = new Vector3(randomPosition.x, 0f, randomPosition.y);
            monsterTile = GetTileAtPosition(randomPosition);
            monsterTile.HasMonster = true;
        }

        /// <summary>
        /// If the movement is allowed moves the player to next tile, otherwise triggers an invalid move event
        /// </summary>
        /// <param name="inputMoveEvent">The event that triggered the movement containing the direction of the movement</param>
        private void HandleInputMoveEvent(InputMoveEvent inputMoveEvent)
        {
            Vector2Int nextPosition = Utils.GetNextPosition(currentPlayer.PositionOnGrid, inputMoveEvent.Direction);

            Player.MoveOutcome moveOutcome = Player.MoveOutcome.FAIL_CANT_PROCEED;
            if (Utils.IsPositionInsideGrid(nextPosition, gridSize))
                moveOutcome = currentPlayer.TryMoveToNextPosition(nextPosition, inputMoveEvent.Direction);

            if (moveOutcome == Player.MoveOutcome.FAIL_CANT_PROCEED)
                gameEventsManager.DispatchGameEvent(new InvalidMoveEvent());
        }

        /// <summary>
        /// Asks the current player to shot an arrow than stops its inputs
        /// </summary>
        /// <param name="inputArrowShotEvent"></param>
        private void HandleInputArrowShotEvent(InputArrowShotEvent inputArrowShotEvent)
        {
            currentPlayer.ShotArrow(inputArrowShotEvent.Direction);
            currentPlayer.IgnoreInputs = true;
        }

        /// <summary>
        /// Sets the tile as explored by the player and disable fog on that tile
        /// </summary>
        /// <param name="playerExploredTileEvent"></param>
        private void HandlePlayerExploredTileEvent(PlayerExploredTileEvent playerExploredTileEvent)
        {
            if (!GetTileAtPosition(playerExploredTileEvent.PositionOnGrid).PlayerExplored)
            {
                GetTileAtPosition(playerExploredTileEvent.PositionOnGrid).PlayerExplored = true;
                GetFogAtPosition(playerExploredTileEvent.PositionOnGrid).PlayerExplored = true;
            }
        }

        /// <summary>
        /// Teleports the monster onto anoter tile,
        /// if the player has no remaining arrows tells the game that the player lost
        /// </summary>
        private void HandleArrowCollidedWithWallEvent()
        {
            TeleportMonsterOntoRandomEmptyTile();
            if (currentPlayer.CurrentArrowsCount <= 0)
                gameEventsManager
                    .DispatchGameEvent(new PlayerLostForNoArrowRemainingEvent() { PlayerNumber = currentPlayerIndex });
            else
                ProceedToNextPlayer();
        }

        /// <summary>
        /// Toggles fog rendering for each tile
        /// </summary>
        private void HandleToggleFogVisibilityEvent()
        {
            isFogVisible = !isFogVisible;
            SetFogVisibility(isFogVisible);
        }

        /// <summary>
        /// Sets fog visibility for each tile
        /// </summary>
        private void SetFogVisibility(bool visible)
        {
            for (int row = 0; row < gridSize; row++)
                for (int col = 0; col < gridSize; col++)
                    if (!visible)
                        fogGrid[row][col].Hide();
                    else if (!grid[row][col].PlayerExplored)
                        fogGrid[row][col].Show();
        }

        /// <summary>
        /// Selects the next available player as current player
        /// </summary>
        private void ProceedToNextPlayer()
        {
            if (isGameEnded)
                return;

            currentPlayer.IgnoreInputs = true;

            do
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            while (players[currentPlayerIndex].HasPlayerLost);

            currentPlayer = players[currentPlayerIndex];
            mainCamera.SetPlayerTransform(currentPlayer.transform);
            currentPlayer.IgnoreInputs = false;
            gameEventsManager
                .DispatchGameEvent(new PlayerStartedTurnEvent()
                {
                    PlayerNumber = currentPlayerIndex,
                    PlayerArrowsCount = currentPlayer.CurrentArrowsCount,
                    PlayerColor = playerColors[currentPlayerIndex]
                });
        }

        /// <summary>
        /// Connects the tunnel tiles by checking if there are adjacent tunnels
        /// </summary>
        private void ConnectTunnelTiles()
        {
            for (int row = 0; row < gridSize; row++)
                for (int col = 0; col < gridSize; col++)
                    if (grid[row][col].gameObject.CompareTag("Tunnel"))
                    {
                        Tunnel tunnelTile = grid[row][col] as Tunnel;
                        foreach (Direction direction in tunnelTile.GetAvailableDirections())
                            switch (direction)
                            {
                                case Direction.Right:
                                    if (row < gridSize - 1)
                                        tunnelTile.AddAdjacentTile(Direction.Right, grid[row + 1][col]);
                                    break;
                                case Direction.Left:
                                    if (row > 0)
                                        tunnelTile.AddAdjacentTile(Direction.Left, grid[row - 1][col]);
                                    break;
                                case Direction.Up:
                                    if (col < gridSize - 1)
                                        tunnelTile.AddAdjacentTile(Direction.Up, grid[row][col + 1]);
                                    break;
                                case Direction.Down:
                                    if (col > 0)
                                        tunnelTile.AddAdjacentTile(Direction.Down, grid[row][col - 1]);
                                    break;
                            }
                    }
        }

        /// <summary>
        /// Removes a player from the game
        /// </summary>
        /// <param name="playerNumber"></param>
        private void RemovePlayer(int playerNumber)
        {
            players[playerNumber].HasPlayerLost = true;
            players[playerNumber].gameObject.SetActive(false);
        }

        /// <summary>
        /// Handles the lost condition events by removing the player who lost in case of guest players
        /// or by ending the game if the player 1 (main) lost
        /// </summary>
        /// <param name="playerNumber">The player that has lost</param>
        /// <see cref="EndGame"/>
        private void HandlePlayerLostEvents(int playerNumber)
        {
            if (playerNumber == 0)
                // My player lost
                EndGame();
            else
            {
                // Other player lost
                RemovePlayer(playerNumber);
                ProceedToNextPlayer();
            }
        }

        /// <summary>
        /// Handle the event of a player that has been hit by an arrow. The hit player loses
        /// </summary>
        /// <param name="arrowHitPlayerEvent">The event containing the arrow hit info</param>
        /// <see cref="HandlePlayerLostEvents"/>
        private void HandleArrowHitPlayerEvent(ArrowHitPlayerEvent arrowHitPlayerEvent)
        {
            HandlePlayerLostEvents(arrowHitPlayerEvent.HitPlayerNumber);
        }

        /// <summary>
        /// Disable all players inputs and reveals fog
        /// </summary>
        private void EndGame()
        {
            isGameEnded = true;
            players.ForEach(player => player.IgnoreInputs = true);
            SetFogVisibility(false);
        }

    }
}

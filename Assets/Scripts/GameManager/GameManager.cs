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

        private MapElementsList mapElementsList;

        private Fog[][] fogGrid;
        private GameObject fogContainer;
        private bool isFogVisible;

        private List<Player> players;
        private Player currentPlayer;
        private int currentPlayerIndex;

        private GameObject monster;
        private Tile monsterTile;

        private GameEventsManager gameEventsManager;

        private bool isGameEnded;

        void Awake()
        {
            ThroughScenesParameters.mapGenerationError = false;
            gridSize = generateRandomMap ? ThroughScenesParameters.gridSize : PRECOMPUTED_MAP_SIZE;
            mapElementsList = ThroughScenesParameters.mapElementsList;
            gameEventsManager = GameEventsManager.Instance;
        }

        void Start()
        {
            AddGameEventListeners();
            PlaceMap((grid) =>
            {
                this.grid = grid;
                ConnectTunnelTiles();
                PlaceFog();
                PlaceMapElements();
                PlacePlayer();
                mainCamera.SetPlayerTransform(currentPlayer.transform);
                mainCamera.SetSize(inGameCameraSize);
            });
        }

        void OnDestroy()
        {
            gameEventsManager.RemoveAllGameEventListeners();
        }

        public void ReceiveGameEvent(GameEvent gameEvent)
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
                case ArrowCollidedWithMonsterEvent:
                case PlayerLostForNoArrowRemainingEvent:
                case ArrowHitPlayerEvent:
                    isGameEnded = true;
                    players.ForEach(player => player.IgnoreInputs = true);
                    SetFogVisibility(false);
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
        /// Places player on a random empty tile
        /// </summary>
        /// <see cref="IsEmptyTile"/>
        private void PlacePlayer()
        {
            Vector2Int randomPosition = GetRandomPositionOnEmptyTile();
            GameObject playerGameObject = InstantiateOnTile(playerPrefab.gameObject, randomPosition, null);
            players = new List<Player>();
            currentPlayer = playerGameObject.GetComponent<Player>();
            currentPlayer.SetPositionOnGrid(randomPosition);
            players.Add(currentPlayer);
            currentPlayerIndex = 0;
        }

        /// <summary>
        /// Places monsters, teleports and wells on the grid in empty tiles
        /// </summary>
        /// <see cref="IsEmptyTile"/>
        private void PlaceMapElements()
        {
            foreach (MapElementsItem mapElementsItem in mapElementsList.mapElements)
                for (int i = 0; i < mapElementsItem.count; i++)
                {
                    Vector2Int randomPosition = GetRandomPositionOnEmptyTile();
                    GameObject mapElement = InstantiateOnTile(mapElementsItem.prefab, randomPosition, transform);
                    Tile placementTile = GetTileAtPosition(randomPosition);

                    switch (mapElementsItem.mapElement)
                    {
                        case MapElement.MONSTER:
                            placementTile.HasMonster = true;
                            monsterTile = placementTile;
                            monster = mapElement;
                            break;
                        case MapElement.TELEPORT:
                            placementTile.HasTeleport = true;
                            break;
                        case MapElement.WELL:
                            placementTile.HasWell = true;
                            break;
                    }
                }
        }

        /// <summary>
        /// Generates the map and instantiates it in game
        /// </summary>
        private void PlaceMap(System.Action<Tile[][]> onMapGenerated)
        {
            if (mapContainer != null)
                Destroy(mapContainer);

            if (generateRandomMap)
            {
                mapContainer = new("MapContainer");
                StartCoroutine(mapGenerator.GenerateMap(gridSize, mapContainer.transform, onMapGenerated));
            }
            else
            {
                mapContainer = Instantiate(precomputedMap, Vector3.zero, Quaternion.identity, transform);
                mapContainer.name = "MapContainer";
                Tile[] tiles = new Tile[gridSize * gridSize];
                for (int childIndex = 0; childIndex < mapContainer.transform.childCount; childIndex++)
                    tiles[childIndex] = mapContainer.transform.GetChild(childIndex).GetComponent<Tile>();

                Tile[][] grid = Utils.ArrayToMatrix<Tile>(tiles, gridSize, gridSize);
                for (int row = 0; row < gridSize; row++)
                    for (int col = 0; col < gridSize; col++)
                        grid[row][col].PositionOnGrid = new Vector2Int(row, col);

                onMapGenerated?.Invoke(grid);
            }
            mapContainer.transform.parent = transform;
        }

        /// <summary>
        /// Places fog on the grid
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
        /// <see cref="IsEmptyTile"/>
        /// <returns>A random position of an empty tile on the grid</returns>
        private Vector2Int GetRandomPositionOnEmptyTile()
        {
            int iterations = 0;
            Vector2Int randomPosition;
            do
            {
                iterations++;
                if (iterations > 1000)
                {
                    ThroughScenesParameters.mapGenerationError = true;
                    SceneManager.LoadScene("MainMenuScene");
                    throw new System.InvalidOperationException("Trying to create map with wrong parameters");
                }

                randomPosition =
                    new Vector2Int(UnityEngine.Random.Range(0, gridSize), UnityEngine.Random.Range(0, gridSize));
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
            Vector2Int nextPosition = Utils.GetNextPosition(currentPlayer.PositionOnGrid, inputMoveEvent.direction);

            Player.MoveOutcome moveOutcome = Player.MoveOutcome.FAIL_CANT_PROCEED;
            if (Utils.IsPositionInsideGrid(nextPosition, gridSize))
                moveOutcome = currentPlayer.TryMoveToNextPosition(nextPosition, inputMoveEvent.direction);

            if (moveOutcome == Player.MoveOutcome.FAIL_CANT_PROCEED)
                gameEventsManager.DispatchGameEvent(new InvalidMoveEvent());
        }

        /// <summary>
        /// Asks player to shot an arrow
        /// </summary>
        /// <param name="inputArrowShotEvent"></param>
        private void HandleInputArrowShotEvent(InputArrowShotEvent inputArrowShotEvent)
        {
            currentPlayer.ShotArrow(inputArrowShotEvent.direction);
            currentPlayer.IgnoreInputs = true;
        }

        /// <summary>
        /// Sets the tile as explored by the player and disable fog on that tile
        /// </summary>
        /// <param name="playerExploredTileEvent"></param>
        private void HandlePlayerExploredTileEvent(PlayerExploredTileEvent playerExploredTileEvent)
        {
            if (!GetTileAtPosition(playerExploredTileEvent.positionOnGrid).PlayerExplored)
            {
                GetTileAtPosition(playerExploredTileEvent.positionOnGrid).PlayerExplored = true;
                GetFogAtPosition(playerExploredTileEvent.positionOnGrid).PlayerExplored = true;
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
                gameEventsManager.DispatchGameEvent(new PlayerLostForNoArrowRemainingEvent());
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
        /// Selects the playing player as current player
        /// </summary>
        private void ProceedToNextPlayer()
        {
            if (isGameEnded)
                return;

            currentPlayer.IgnoreInputs = true;
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            currentPlayer = players[currentPlayerIndex];
            currentPlayer.IgnoreInputs = false;
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
    }
}

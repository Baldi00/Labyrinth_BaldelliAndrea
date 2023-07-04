using DBGA.Camera;
using DBGA.Common;
using DBGA.EventSystem;
using DBGA.MapGeneration;
using DBGA.Tiles;
using UnityEngine;

namespace DBGA.GameManager
{
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour, IGameEventsListener
    {
        [Header("Map generation")]
        [SerializeField]
        private bool generateRandomMap;
        [SerializeField]
        private int gridSize;
        [SerializeField]
        private MapGenerator mapGenerator;

        [Header("Player")]
        [SerializeField]
        private Player.Player playerPrefab;

        [Header("Camera")]
        [SerializeField]
        private CameraFollowPlayer mainCamera;
        [SerializeField]
        private float inGameCameraSize = 5.4f;

        [Header("Map elements")]
        [SerializeField]
        private MapElementsList mapElementsList;
        [SerializeField]
        private GameObject fogElementPrefab;

        private GameObject mapContainer;
        private Tile[][] grid;

        private GameObject[][] fogGrid;
        private GameObject fogContainer;

        private Player.Player currentPlayer;

        private GameObject monster;
        private Tile monsterTile;

        private GameEventsManager gameEventsManager;

        void Awake()
        {
            gameEventsManager = GameEventsManager.Instance;
        }

        void Start()
        {
            AddGameEventListeners();
            PlaceMap((grid) =>
            {
                this.grid = grid;
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
                    currentPlayer.IgnoreInputs = true;
                    break;
                case PlayerExploredTileEvent playerExploredTileEvent:
                    HandlePlayerExploredTileEvent(playerExploredTileEvent);
                    break;
                case ArrowCollidedWithWallEvent:
                    HandleArrowCollidedWithWallEvent();
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
            gameEventsManager.AddGameEventListener(this, typeof(ArrowCollidedWithWallEvent));
            gameEventsManager.AddGameEventListener(this, typeof(ArrowCollidedWithMonsterEvent));
            gameEventsManager.AddGameEventListener(this, typeof(PlayerLostForNoArrowRemainingEvent));
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
            currentPlayer = playerGameObject.GetComponent<Player.Player>();
            currentPlayer.SetPositionOnGrid(randomPosition);
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

            mapContainer = new("MapContainer");
            mapContainer.transform.parent = transform;

            if (generateRandomMap)
                StartCoroutine(mapGenerator.GenerateMap(gridSize, mapContainer.transform, onMapGenerated));
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

            fogGrid = new GameObject[gridSize][];

            for (int row = 0; row < gridSize; row++)
            {
                fogGrid[row] = new GameObject[gridSize];
                for (int col = 0; col < gridSize; col++)
                    fogGrid[row][col] =
                        InstantiateOnTile(fogElementPrefab, new Vector2Int(row, col), fogContainer.transform);
            }
        }

        /// <summary>
        /// Returns a random position of an empty tile on the grid
        /// </summary>
        /// <see cref="IsEmptyTile"/>
        /// <returns>A random position of an empty tile on the grid</returns>
        private Vector2Int GetRandomPositionOnEmptyTile()
        {
            Vector2Int randomPosition;
            do
            {
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
            if (tile is Tunnel || tile.HasMonster || tile.HasTeleport || tile.HasWell || tile.IsVoid)
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
        private GameObject GetFogAtPosition(Vector2Int position)
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
        /// Teleport the monster onto another empty tile
        /// </summary>
        private void TeleportMonsterOntoRandomEmptyTile()
        {
            Vector2Int randomPosition = GetRandomPositionOnEmptyTile();
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

            bool successfulMove = false;
            if (Utils.IsPositionInsideGrid(nextPosition, gridSize))
                successfulMove = currentPlayer.TryMoveToNextPosition(nextPosition, inputMoveEvent.direction);

            if (!successfulMove)
                gameEventsManager.DispatchGameEvent(new InvalidMoveEvent());
        }

        /// <summary>
        /// Asks player to shot an arrow
        /// </summary>
        /// <param name="inputArrowShotEvent"></param>
        private void HandleInputArrowShotEvent(InputArrowShotEvent inputArrowShotEvent)
        {
            currentPlayer.ShotArrow(inputArrowShotEvent.direction);
        }

        /// <summary>
        /// Sets the tile as explored by the player and disable fog on that tile
        /// </summary>
        /// <param name="playerExploredTileEvent"></param>
        private void HandlePlayerExploredTileEvent(PlayerExploredTileEvent playerExploredTileEvent)
        {
            GetTileAtPosition(playerExploredTileEvent.positionOnGrid).PlayerExplored = true;
            GetFogAtPosition(playerExploredTileEvent.positionOnGrid).SetActive(false);
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
        }

        /// <summary>
        /// Enables or disables the fog rendering
        /// </summary>
        private void HandleToggleFogVisibilityEvent()
        {
            if (fogContainer != null)
                fogContainer.SetActive(!fogContainer.activeSelf);
        }
    }
}

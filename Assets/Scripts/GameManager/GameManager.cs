using DBGA.Common;
using DBGA.EventSystem;
using DBGA.MapGeneration;
using DBGA.Tiles;
using DBGA.Camera;
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
        private TilesList tilesList;
        [SerializeField]
        private MapGenerator mapGenerator;

        [Header("Player")]
        [SerializeField]
        private Player.Player playerPrefab;

        [Header("Camera")]
        [SerializeField]
        private CameraFollowPlayer mainCamera;

        [Header("Map elements")]
        [SerializeField]
        private MapElementsList mapElementsList;
        [SerializeField]
        private GameObject fogElementPrefab;

        private Tile[][] grid;
        private GameObject[][] fogGrid;
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

            if (generateRandomMap)
                grid = mapGenerator.GenerateMap(gridSize, tilesList);

            PlaceFog();
            PlaceMapElements();
            PlacePlayer();
            mainCamera.SetPlayerTransform(currentPlayer.transform);
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
                case EnteredTeleportTileEvent:
                    TeleportPlayerOntoRandomEmptyTile();
                    break;
                case EnteredWellTileEvent:
                case EnteredMonsterTileEvent:
                case ArrowCollidedWithMonsterEvent:
                case PlayerLostForNoArrowRemainingEvent:
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
            gameEventsManager.AddGameEventListener(this, typeof(EnteredTeleportTileEvent));
            gameEventsManager.AddGameEventListener(this, typeof(EnteredWellTileEvent));
            gameEventsManager.AddGameEventListener(this, typeof(EnteredMonsterTileEvent));
            gameEventsManager.AddGameEventListener(this, typeof(PlayerExploredTileEvent));
            gameEventsManager.AddGameEventListener(this, typeof(ArrowCollidedWithWallEvent));
            gameEventsManager.AddGameEventListener(this, typeof(ArrowCollidedWithMonsterEvent));
            gameEventsManager.AddGameEventListener(this, typeof(PlayerLostForNoArrowRemainingEvent));
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
        /// Places fog on the grid
        /// </summary>
        private void PlaceFog()
        {
            fogGrid = new GameObject[gridSize][];

            for (int row = 0; row < gridSize; row++)
            {
                fogGrid[row] = new GameObject[gridSize];
                for (int col = 0; col < gridSize; col++)
                    fogGrid[row][col] = InstantiateOnTile(fogElementPrefab, new Vector2Int(row, col), transform);
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

        private GameObject InstantiateOnTile(GameObject gameObject, Vector2Int position, Transform parent)
        {
            return Instantiate(gameObject, new Vector3(position.x, 0, position.y), Quaternion.identity, parent);
        }

        /// <summary>
        /// Checks if the tile is empty and can be used to place things.
        /// Empty means that it is not a Tunnel tile, it has no monsters, teleports, wells or player on it
        /// </summary>
        /// <param name="position">The position on the grid of the tile to test</param>
        /// <returns>True if the tile is empty, false otherwise</returns>
        private bool IsEmptyTile(Vector2Int position)
        {
            if (currentPlayer != null && position.Equals(currentPlayer.PositionOnGrid))
                return false;

            Tile tile = grid[position.x][position.y];
            if (tile is Tunnel || tile.HasMonster || tile.HasTeleport || tile.HasWell)
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
            if (IsPositionInsideGrid(position))
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
            if (IsPositionInsideGrid(position))
                return fogGrid[position.x][position.y];
            return null;
        }

        /// <summary>
        /// Returns the next position on the grid from the current position on the grid in the given direction
        /// </summary>
        /// <param name="currentPosition">The current position on the grid</param>
        /// <param name="direction">The direction in which you search the next position</param>
        /// <returns>The next position on the grid from the current position on the grid in the given direction</returns>
        private Vector2Int GetNextPosition(Vector2Int currentPosition, Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Vector2Int(currentPosition.x, currentPosition.y + 1),
                Direction.Down => new Vector2Int(currentPosition.x, currentPosition.y - 1),
                Direction.Left => new Vector2Int(currentPosition.x - 1, currentPosition.y),
                Direction.Right => new Vector2Int(currentPosition.x + 1, currentPosition.y),
                _ => new Vector2Int(currentPosition.x, currentPosition.y)
            };
        }

        /// <summary>
        /// Checks if the given position is on the grid or not
        /// </summary>
        /// <param name="position">The position to test</param>
        /// <returns>True if the given position is on the grid, false if is outside</returns>
        private bool IsPositionInsideGrid(Vector2Int position)
        {
            return position.x >= 0 && position.y >= 0 &&
                position.x < gridSize && position.y < gridSize;
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
            Vector2Int nextPosition = GetNextPosition(currentPlayer.PositionOnGrid, inputMoveEvent.direction);

            bool successfulMove = false;
            if (IsPositionInsideGrid(nextPosition))
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
    }
}

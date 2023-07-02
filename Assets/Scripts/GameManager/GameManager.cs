using DBGA.Common;
using DBGA.EventSystem;
using DBGA.MapGeneration;
using DBGA.Player;
using DBGA.Tiles;
using DBGA.Camera;
using System;
using System.Collections.Generic;
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

        private Tile[][] grid;
        private Player.Player currentPlayer;

        void Start()
        {
            AddGameEventListeners();

            if (generateRandomMap)
                grid = mapGenerator.GenerateMap(gridSize, tilesList);

            PlacePlayer();
            mainCamera.SetPlayerTransform(currentPlayer.transform);

            PlaceMapElements();
        }

        void OnDestroy()
        {
            GameEventsManager.Instance.RemoveAllGameEventListeners();
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
            }
        }

        /// <summary>
        /// Registers the game manager as a listener for some tipe of game events
        /// </summary>
        private void AddGameEventListeners()
        {
            GameEventsManager.Instance.AddGameEventListener(this, typeof(InputMoveEvent));
            GameEventsManager.Instance.AddGameEventListener(this, typeof(InputArrowShotEvent));
        }

        /// <summary>
        /// Places player on a random empty tile
        /// </summary>
        private void PlacePlayer()
        {
            Vector2Int randomPosition = GetRandomPositionOnEmptyTile();
            GameObject playerGameObject = InstantiateOnTile(playerPrefab.gameObject, randomPosition);
            currentPlayer = playerGameObject.GetComponent<Player.Player>();
            currentPlayer.SetPositionOnGrid(randomPosition);
        }

        /// <summary>
        /// Places monsters, teleports and wells on the grid
        /// </summary>
        private void PlaceMapElements()
        {
            foreach (MapElementsItem mapElementsItem in mapElementsList.mapElements)
                for (int i = 0; i < mapElementsItem.count; i++)
                {
                    Vector2Int randomPosition = GetRandomPositionOnEmptyTile();
                    InstantiateOnTile(mapElementsItem.prefab, randomPosition);
                    Tile placementTile = GetTileAtPosition(randomPosition);

                    switch (mapElementsItem.mapElement)
                    {
                        case MapElement.MONSTER:
                            placementTile.HasMonser = true;
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

        private GameObject InstantiateOnTile(GameObject gameObject, Vector2Int position)
        {
            return Instantiate(gameObject, new Vector3(position.x, 0, position.y), Quaternion.identity);
        }

        /// <summary>
        /// Checks if the tile is empty and can be used to place things.
        /// Empty means that it is not a Tunnel tile, it has no monsters, teleports or wells
        /// </summary>
        /// <param name="position">The position on the grid of the tile to test</param>
        /// <returns>True if the tile is empty, false otherwise</returns>
        private bool IsEmptyTile(Vector2Int position)
        {
            Tile tile = grid[position.x][position.y];
            if (tile is Tunnel || tile.HasMonser || tile.HasTeleport || tile.HasWell)
                return false;
            return true;
        }

        private void HandleInputMoveEvent(InputMoveEvent inputMoveEvent)
        {
            Vector2Int nextPosition = GetNextPosition(currentPlayer.PositionOnGrid, inputMoveEvent.direction);

            if (IsPositionInsideGrid(nextPosition))
            {
                bool successfulMove = currentPlayer.TryMoveToNextPosition(nextPosition, inputMoveEvent.direction);
            }
            else
            {
                // TODO: Manage invalid move
            }

            // TODO: Manage invalid move
        }

        private void HandleInputArrowShotEvent(InputArrowShotEvent inputArrowShotEvent)
        {
            throw new NotImplementedException();
        }

        private Tile GetPlayerTile()
        {
            return GetTileAtPosition(currentPlayer.PositionOnGrid);
        }

        private Tile GetTileAtPosition(Vector2Int position)
        {
            if (IsPositionInsideGrid(position))
                return grid[position.x][position.y];
            return null;
        }

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

        private bool IsPositionInsideGrid(Vector2Int position)
        {
            return position.x >= 0 && position.y >= 0 &&
                position.x < gridSize && position.y < gridSize;
        }

    }
}

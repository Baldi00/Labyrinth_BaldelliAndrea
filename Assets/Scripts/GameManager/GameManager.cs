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
        private CameraFollowPlayer camera;

        private Tile[][] grid;
        private Player.Player currentPlayer;

        void Start()
        {
            AddGameEventListeners();

            if (generateRandomMap)
                grid = mapGenerator.GenerateMap(gridSize, tilesList);

            SpawnPlayer();
            camera.SetPlayerTransform(currentPlayer.transform);
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
        /// Spawns player on a random valid tile
        /// </summary>
        private void SpawnPlayer()
        {
            Vector2Int randomPosition;
            do
            {
                randomPosition = RandomizePosition();
            } while (!IsPlayerSpawnedOnValidTile(randomPosition));

            GameObject playerGameObject = Instantiate(
                playerPrefab.gameObject,
                new Vector3(randomPosition.x, 0, randomPosition.y),
                Quaternion.identity);

            currentPlayer = playerGameObject.GetComponent<Player.Player>();

            currentPlayer.SetPositionOnGrid(randomPosition);
        }

        private Vector2Int RandomizePosition()
        {
            return new Vector2Int(UnityEngine.Random.Range(0, gridSize), UnityEngine.Random.Range(0, gridSize));
        }

        private bool IsPlayerSpawnedOnValidTile(Vector2Int position)
        {
            Tile tile = grid[position.x][position.y];
            if (tile is Tunnel || tile.HasMonser || tile.HasTeleport || tile.HasWell)
                return false;
            return true;
        }

        private void HandleInputMoveEvent(InputMoveEvent inputMoveEvent)
        {
            List<Direction> availableDirections = GetPlayerTile().GetAvailableDirections();
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
            return grid[currentPlayer.PositionOnGrid.x][currentPlayer.PositionOnGrid.y];
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

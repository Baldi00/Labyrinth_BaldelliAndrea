using DBGA.Common;
using DBGA.Tiles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DBGA.MapGeneration
{
    [DisallowMultipleComponent]
    public class MapGenerator : MonoBehaviour
    {
        public struct ProbabilityRange
        {
            public float min;
            public float max;
        }

        private class State
        {
            public Tile[][] grid;
            public List<Tile> domain;
            public Vector2Int position;

            public State(Tile[][] grid, List<Tile> domain, Vector2Int position)
            {
                int gridSize = grid.Length;
                this.grid = new Tile[gridSize][];

                for (int row = 0; row < gridSize; row++)
                {
                    this.grid[row] = new Tile[gridSize];
                    for (int col = 0; col < gridSize; col++)
                        this.grid[row][col] = grid[row][col];
                }

                this.domain = new List<Tile>(domain);
                this.position = position;
            }

            public Tile GetAvailableTile()
            {
                Tile chosenTile = null;
                if (domain.Count > 0)
                {
                    chosenTile = domain[Random.Range(0, domain.Count)];
                    domain.Remove(chosenTile);
                }
                return chosenTile;
            }
        }

        [SerializeField]
        private GameObject blockTilePrefab;

        private int gridSize;
        private TilesList tilesList;
        private Tile[][] grid;
        private Dictionary<ProbabilityRange, GameObject> availableTilesWithProbability;

        /// <summary>
        /// Generates a random map of tiles with the given size and tiles from the given list
        /// </summary>
        /// <param name="gridSize">The size of the resulting grid map (e.g. if 20 the result is a 20x20 grid)</param>
        /// <param name="tileList">The list of available tiles</param>
        /// <returns>A random grid of tiles with the given size and tiles</returns>
        public Tile[][] GenerateMap(int gridSize, TilesList tilesList, Transform parent)
        {
            this.gridSize = gridSize;
            this.tilesList = tilesList;

            InitializeTilesAndProbability();

            grid = new Tile[gridSize][];

            for (int row = 0; row < gridSize; row++)
            {
                grid[row] = new Tile[gridSize];
                for (int col = 0; col < gridSize; col++)
                {
                    GameObject randomTileGameObject = GetRandomTileFromAvailable();
                    GameObject tileInstanceGameObject =
                        Instantiate(randomTileGameObject, new Vector3(row, 0f, col), Quaternion.identity, parent);

                    grid[row][col] = tileInstanceGameObject.GetComponent<Tile>();
                    grid[row][col].PositionOnGrid = new Vector2Int(row, col);
                }
            }

            SetupTunnelAdjacentTiles();

            return grid;
        }

        /// <summary>
        /// Initializes the tiles probabilities in order to pick them in a weighted way
        /// </summary>
        private void InitializeTilesAndProbability()
        {
            availableTilesWithProbability = new Dictionary<ProbabilityRange, GameObject>();

            float totalTilesProbabilityWeight = 0;
            foreach (TileListItem tileListItem in tilesList.availableTiles)
                totalTilesProbabilityWeight += tileListItem.probabilityWeight;

            float previousRangeMin = 0f;
            foreach (TileListItem tileListItem in tilesList.availableTiles)
            {
                float rangeMax = previousRangeMin + tileListItem.probabilityWeight / totalTilesProbabilityWeight;
                ProbabilityRange probabilityRange = new() { min = previousRangeMin, max = rangeMax };

                if (!availableTilesWithProbability.ContainsKey(probabilityRange))
                    availableTilesWithProbability.Add(probabilityRange, tileListItem.tilePrefab);

                previousRangeMin = rangeMax;
            }
        }

        /// <summary>
        /// Returns a weighted random tile between the available ones
        /// </summary>
        /// <returns>A weighted random tile between the available ones</returns>
        private GameObject GetRandomTileFromAvailable()
        {
            float randomNumber = Random.Range(0f, 1f);
            foreach (KeyValuePair<ProbabilityRange, GameObject> tile in availableTilesWithProbability)
                if (randomNumber >= tile.Key.min && randomNumber < tile.Key.max)
                    return tile.Value;

            return null;
        }

        private void SetupTunnelAdjacentTiles()
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

        public Tile[][] GenerateMapWFC(int gridSize, TilesList tilesList, Transform parent)
        {
            this.gridSize = gridSize;
            this.tilesList = tilesList;

            // Initialize matrices
            grid = new Tile[gridSize][];
            List<Tile>[][] domains = new List<Tile>[gridSize][];

            for (int row = 0; row < gridSize; row++)
            {
                grid[row] = new Tile[gridSize];
                domains[row] = new List<Tile>[gridSize];
            }

            // Initialize domains (every domain contains all tiles)
            List<Tile> allTilesPrefab =
                tilesList.availableTiles
                .Select<TileListItem, Tile>(tli => tli.tile)
                .ToList<Tile>();

            for (int row = 0; row < gridSize; row++)
                for (int col = 0; col < gridSize; col++)
                    domains[row][col] = new List<Tile>(allTilesPrefab);

            // Remove borders
            for (int row = 0; row < gridSize; row++)
                for (int col = 0; col < gridSize; col++)
                {
                    if (row == 0)
                        RemoveTilesWithOutDirectionFromDomain(Direction.Down, domains[col][row]);
                    if (row == gridSize - 1)
                        RemoveTilesWithOutDirectionFromDomain(Direction.Up, domains[col][row]);
                    if (col == 0)
                        RemoveTilesWithOutDirectionFromDomain(Direction.Left, domains[col][row]);
                    if (col == gridSize - 1)
                        RemoveTilesWithOutDirectionFromDomain(Direction.Right, domains[col][row]);
                }

            // Select random position
            Vector2Int randomPosition = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));

            Queue<Vector2Int> positionsToFill = new Queue<Vector2Int>();
            positionsToFill.Enqueue(randomPosition);

            Stack<State> states = new Stack<State>();
            State currentState = null;

            // Generate map
            while (positionsToFill.Count > 0)
            {
                Vector2Int positionToFill = positionsToFill.Dequeue();

                // Choose a random tile from the available and instantiate it
                List<Tile> domain = domains[positionToFill.x][positionToFill.y];

                currentState = new State(grid, domain, positionToFill);

                Tile chosenTile = null;
                do
                {
                    chosenTile = currentState.GetAvailableTile();
                    if (chosenTile == null)
                    {
                        currentState = states.Pop();
                        grid = currentState.grid;
                        positionToFill = currentState.position;
                        positionsToFill.Clear();
                    }
                } while (chosenTile == null);

                states.Push(currentState);
                grid[positionToFill.x][positionToFill.y] = chosenTile;

                // Add next tiles
                List<Direction> availableDirections = chosenTile.OutDirections;

                foreach (Direction direction in availableDirections)
                {
                    Vector2Int nextPosition = GetNextPosition(positionToFill, direction);
                    if (!positionsToFill.Contains(nextPosition) &&
                        IsPositionInsideGrid(nextPosition) &&
                        grid[nextPosition.x][nextPosition.y] == null)
                    {
                        // Remove tiles from domain
                        RemoveTilesWithoutOutDirectionFromDomain(
                            direction.GetOppositeDirection(),
                            domains[nextPosition.x][nextPosition.y]);

                        // Enqueue next position
                        positionsToFill.Enqueue(nextPosition);
                    }
                }
            }

            grid = currentState.grid;

            // Instantiate game objects
            for (int row = 0; row < gridSize; row++)
                for (int col = 0; col < gridSize; col++)
                {
                    GameObject tilePrefab = blockTilePrefab;

                    if (grid[row][col] != null)
                        tilePrefab = tilesList.availableTiles
                                .Where<TileListItem>(tli => tli.tile == grid[row][col])
                                .Select<TileListItem, GameObject>(tli => tli.tilePrefab)
                                .ElementAt<GameObject>(0);

                    GameObject tileInstanceGameObject = Instantiate(
                        tilePrefab,
                        new Vector3(row, 0f, col),
                        Quaternion.identity,
                        parent);

                    grid[row][col] = tileInstanceGameObject.GetComponent<Tile>();
                    grid[row][col].PositionOnGrid = new Vector2Int(row, col);
                }

            SetupTunnelAdjacentTiles();

            return grid;
        }

        private void RemoveTilesWithOutDirectionFromDomain(Direction direction, List<Tile> domain)
        {
            domain.RemoveAll(tile => tile.OutDirections.Contains(direction));
        }

        private void RemoveTilesWithoutOutDirectionFromDomain(Direction direction, List<Tile> domain)
        {
            domain.RemoveAll(tile => !tile.OutDirections.Contains(direction));
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
    }
}

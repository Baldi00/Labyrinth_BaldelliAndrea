using DBGA.Common;
using DBGA.Tiles;
using System.Collections;
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

        [SerializeField]
        private TilesList tilesList;
        [SerializeField]
        private bool animateGeneration;
        [SerializeField]
        [Range(0f, 1f)]
        private float generationTime;
        [SerializeField]
        private GameObject voidTilePrefab;

        private int gridSize;
        private Tile[][] grid;

        /// <summary>
        /// Initializes the tiles probabilities in order to pick them in a weighted way
        /// </summary>
        private Dictionary<ProbabilityRange, Tile> GetTilesProbability(List<Tile> domain)
        {
            Dictionary<ProbabilityRange, Tile> tilesProbability = new Dictionary<ProbabilityRange, Tile>();

            float totalTilesProbabilityWeight = 0;
            foreach (Tile tile in domain)
                totalTilesProbabilityWeight += tilesList.GetProbabilityWeight(tile);

            float previousRangeMin = 0f;
            foreach (Tile tile in domain)
            {
                float tileProbabilityWeight = tilesList.GetProbabilityWeight(tile);
                float rangeMax = previousRangeMin + tileProbabilityWeight / totalTilesProbabilityWeight;
                ProbabilityRange probabilityRange = new() { min = previousRangeMin, max = rangeMax };

                if (!tilesProbability.ContainsKey(probabilityRange))
                    tilesProbability.Add(probabilityRange, tile);

                previousRangeMin = rangeMax;
            }

            return tilesProbability;
        }

        /// <summary>
        /// Returns a weighted random tile between the available ones
        /// </summary>
        /// <returns>A weighted random tile between the available ones</returns>
        private Tile GetWeightedRandomTileFromAvailable(Dictionary<ProbabilityRange, Tile> availableTilesProbability)
        {
            float randomNumber = Random.Range(0f, 1f);
            foreach (KeyValuePair<ProbabilityRange, Tile> tile in availableTilesProbability)
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

        public IEnumerator GenerateMapWFC(int gridSize, Transform parent, System.Action<Tile[][]> onMapGenerated)
        {
            this.gridSize = gridSize;

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

            // Generate map
            while (positionsToFill.Count > 0)
            {
                Vector2Int positionToFill = positionsToFill.Dequeue();

                // Choose a random tile from the available and instantiate it
                List<Tile> domain = domains[positionToFill.x][positionToFill.y];

                Tile chosenTile = GetAvailableTile(domain);
                grid[positionToFill.x][positionToFill.y] = chosenTile;

                // Instantiate game object
                if (grid[positionToFill.x][positionToFill.y] != null)
                {
                    GameObject tilePrefab = tilesList.availableTiles
                            .Where<TileListItem>(tli => tli.tile == grid[positionToFill.x][positionToFill.y])
                            .Select<TileListItem, GameObject>(tli => tli.tilePrefab)
                            .ElementAt<GameObject>(0);

                    GameObject tileInstanceGameObject = Instantiate(
                        tilePrefab,
                        new Vector3(positionToFill.x, 0f, positionToFill.y),
                        Quaternion.identity,
                        parent);

                    grid[positionToFill.x][positionToFill.y] = tileInstanceGameObject.GetComponent<Tile>();
                    grid[positionToFill.x][positionToFill.y].PositionOnGrid = new Vector2Int(positionToFill.x, positionToFill.y);

                    if (animateGeneration)
                        yield return new WaitForSecondsRealtime(generationTime);
                }

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

            // Fill empty tiles
            for (int row = 0; row < gridSize; row++)
                for (int col = 0; col < gridSize; col++)
                    if (grid[row][col] == null)
                    {
                        GameObject tileInstanceGameObject = Instantiate(
                            voidTilePrefab,
                            new Vector3(row, 0f, col),
                            Quaternion.identity,
                            parent);

                        grid[row][col] = tileInstanceGameObject.GetComponent<Tile>();
                        grid[row][col].PositionOnGrid = new Vector2Int(row, col);
                        grid[row][col].IsVoid = true;
                    }

            SetupTunnelAdjacentTiles();

            onMapGenerated?.Invoke(grid);
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

        private Tile GetAvailableTile(List<Tile> domain)
        {
            Tile chosenTile = null;
            if (domain.Count > 0)
            {
                chosenTile = GetWeightedRandomTileFromAvailable(GetTilesProbability(domain));
                domain.Remove(chosenTile);
            }
            return chosenTile;
        }
    }
}

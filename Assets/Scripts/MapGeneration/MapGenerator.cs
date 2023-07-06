using DBGA.Common;
using DBGA.ThroughScenes;
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
        private struct ProbabilityRange
        {
            public float min;
            public float max;
        }

        [SerializeField]
        private GameObject voidTilePrefab;
        [SerializeField]
        [Range(0.01f, 1f)]
        private float generationAnimationTime = 0.01f;

        private int gridSize;
        private Tile[][] grid;
        private TilesList currentTilesList;

        /// <summary>
        /// Generates and instantiates a maze-like grid of tiles using a modified version of
        /// Constraint Satisfaction Problem (CSP) without backtraking
        /// </summary>
        /// <param name="gridSize">The size of the grid (result grid will be size x size)</param>
        /// <param name="parent">The transform under which the map will be instantiated</param>
        /// <param name="onMapGenerated">Callback called at the end of the generation</param>
        public IEnumerator GenerateMap(int gridSize, Transform parent, System.Action<Tile[][]> onMapGenerated)
        {
            bool animateGeneration = ThroughScenesParameters.AnimateGeneration;
            currentTilesList = ThroughScenesParameters.TilesList;

            this.gridSize = gridSize;

            InitializeGrid();
            List<Tile>[][] domains = InitializeDomains();

            RemoveInvalidTilesFromBorderDomains(domains);

            // Select random start position
            Vector2Int randomStartPosition = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));

            // Setup positions queue
            Queue<Vector2Int> positionsToFill = new Queue<Vector2Int>();
            positionsToFill.Enqueue(randomStartPosition);

            // Fill positions
            while (positionsToFill.Count > 0)
            {
                Vector2Int positionToFill = positionsToFill.Dequeue();

                Tile chosenTile = ChooseTileForThisPosition(domains, ref positionToFill);

                if (chosenTile != null)
                {
                    InstantiateAndPlaceTile(chosenTile, positionToFill, parent);

                    List<Direction> availableDirectionsFromPlacedTile = chosenTile.OutDirections;

                    foreach (Direction direction in availableDirectionsFromPlacedTile)
                    {
                        Vector2Int nextPosition = Utils.GetNextPosition(positionToFill, direction);
                        if (IsNextPositionToFillValid(ref nextPosition, positionsToFill))
                        {
                            // Remove tiles from domain
                            RemoveTilesWithoutOutDirectionFromDomain(
                                direction.GetOppositeDirection(),
                                domains[nextPosition.x][nextPosition.y]);

                            // Enqueue next position
                            positionsToFill.Enqueue(nextPosition);
                        }
                    }

                    // Wait for the animation
                    if (animateGeneration)
                        yield return new WaitForSecondsRealtime(generationAnimationTime);
                }
            }

            // Not all tile can be reached, this fills the remaining ones with a void tile
            FillVoidTiles(parent);

            onMapGenerated?.Invoke(grid);
        }

        /// <summary>
        /// Instantiates the grid and initializes it with null values
        /// </summary>
        private void InitializeGrid()
        {
            grid = new Tile[gridSize][];

            for (int row = 0; row < gridSize; row++)
                grid[row] = new Tile[gridSize];
        }

        /// <summary>
        /// Instantiates and initializes all grid domains with all available tiles
        /// </summary>
        /// <returns>The grid of initialized domains</returns>
        private List<Tile>[][] InitializeDomains()
        {
            List<Tile>[][] domains = new List<Tile>[gridSize][];
            for (int row = 0; row < gridSize; row++)
                domains[row] = new List<Tile>[gridSize];

            List<Tile> allTilesPrefab =
                currentTilesList.availableTiles
                .Select<TileListItem, Tile>(tli => tli.tile)
                .ToList<Tile>();

            for (int row = 0; row < gridSize; row++)
                for (int col = 0; col < gridSize; col++)
                    domains[row][col] = new List<Tile>(allTilesPrefab);
            return domains;
        }

        /// <summary>
        /// Removes all invalid tiles from the domains on the grid border
        /// (e.g. T tile can't be placed on the bottom border because the player can't go any lower)
        /// </summary>
        /// <param name="domains">The grid of domains</param>
        private void RemoveInvalidTilesFromBorderDomains(List<Tile>[][] domains)
        {
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
        }

        /// <summary>
        /// Removes all the tiles in the domain that have the given direction as an out direction
        /// </summary>
        /// <param name="direction">Tiles with this direction as out direction will be removed from domain</param>
        /// <param name="domain">The domain on which to remove tiles</param>
        private void RemoveTilesWithOutDirectionFromDomain(Direction direction, List<Tile> domain)
        {
            domain.RemoveAll(tile => tile.OutDirections.Contains(direction));
        }

        /// <summary>
        /// Removes all the tiles in the domain that don't have the given direction as an out direction
        /// </summary>
        /// <param name="direction">Tiles without this direction as out direction will be removed from domain</param>
        /// <param name="domain">The domain on which to remove tiles</param>
        private void RemoveTilesWithoutOutDirectionFromDomain(Direction direction, List<Tile> domain)
        {
            domain.RemoveAll(tile => !tile.OutDirections.Contains(direction));
        }

        /// <summary>
        /// Chooses a tile for the given position based on given probabilities for each tile
        /// </summary>
        /// <param name="domains">The grid of domains</param>
        /// <param name="positionToFill">The position in which the grid should be placed</param>
        /// <returns>The chosen tile, null if the domain for the given position is empty</returns>
        private Tile ChooseTileForThisPosition(List<Tile>[][] domains, ref Vector2Int positionToFill)
        {
            List<Tile> domain = domains[positionToFill.x][positionToFill.y];
            Tile chosenTile = null;
            if (domain.Count > 0)
                chosenTile = GetWeightedRandomTileFromAvailable(GetTilesProbabilityRanges(domain));
            return chosenTile;
        }

        /// <summary>
        /// Calculates and return the probability ranges for the tiles in the given domain
        /// </summary>
        /// <param name="domain">The domain on which to calculate the tiles probability ranges</param>
        /// <returns>The probability ranges for the tiles in the given domain</returns>
        private Dictionary<ProbabilityRange, Tile> GetTilesProbabilityRanges(List<Tile> domain)
        {
            Dictionary<ProbabilityRange, Tile> tilesProbability = new Dictionary<ProbabilityRange, Tile>();

            float totalTilesProbabilityWeight = 0;
            foreach (Tile tile in domain)
                totalTilesProbabilityWeight += currentTilesList.GetProbabilityWeight(tile);

            float previousRangeMin = 0f;
            foreach (Tile tile in domain)
            {
                float tileProbabilityWeight = currentTilesList.GetProbabilityWeight(tile);
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
        /// <param name="availableTilesProbability">The probability ranges for each available tile</param>
        /// <returns>A weighted random tile between the available ones</returns>
        private Tile GetWeightedRandomTileFromAvailable(Dictionary<ProbabilityRange, Tile> availableTilesProbability)
        {
            float randomNumber = Random.Range(0f, 1f);
            foreach (KeyValuePair<ProbabilityRange, Tile> tile in availableTilesProbability)
                if (randomNumber >= tile.Key.min && randomNumber < tile.Key.max)
                    return tile.Value;

            return null;
        }

        /// <summary>
        /// Instantiates the given tile in scene in the given position with the given parent
        /// and places it on the tiles grid in the given position
        /// </summary>
        /// <param name="tileToPlace">The chosen tile to place</param>
        /// <param name="positionToFill">The position of the grid and scene in which the tile will be placed in</param>
        /// <param name="parent">The parent transform under which the game object will be instantiated</param>
        private void InstantiateAndPlaceTile(Tile tileToPlace, Vector2Int positionToFill, Transform parent)
        {
            GameObject tilePrefab = currentTilesList.availableTiles
                .Where<TileListItem>(tli => tli.tile == tileToPlace)
                .Select<TileListItem, GameObject>(tli => tli.tilePrefab)
                .ElementAt<GameObject>(0);

            GameObject tileInstanceGameObject = Instantiate(
                tilePrefab,
                new Vector3(positionToFill.x, 0f, positionToFill.y),
                Quaternion.identity,
                parent);

            grid[positionToFill.x][positionToFill.y] = tileInstanceGameObject.GetComponent<Tile>();
            grid[positionToFill.x][positionToFill.y].PositionOnGrid = new Vector2Int(positionToFill.x, positionToFill.y);
        }

        /// <summary>
        /// Checks if the given next position to fill is valid.
        /// A next position to fill is valid if it isn't already in the next positions queue,
        /// if it is inside the grid bounds and there isn't already a tile placed in that position
        /// </summary>
        /// <param name="nextPosition">The next position to check</param>
        /// <param name="positionsToFill">The queue of next positions to fill</param>
        /// <returns>True if the next position to fill is valid, false otherwise</returns>
        private bool IsNextPositionToFillValid(ref Vector2Int nextPosition, Queue<Vector2Int> positionsToFill)
        {
            return
                !positionsToFill.Contains(nextPosition) &&
                Utils.IsPositionInsideGrid(nextPosition, gridSize) &&
                grid[nextPosition.x][nextPosition.y] == null;
        }

        /// <summary>
        /// Fills and sets the uninitialized tiles with a void tile.
        /// </summary>
        /// <param name="parent">The parent transform under which the game object will be instantiated</param>
        private void FillVoidTiles(Transform parent)
        {
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
                    }
        }
    }
}

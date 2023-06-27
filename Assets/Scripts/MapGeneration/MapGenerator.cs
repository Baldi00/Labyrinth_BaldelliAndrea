using DBGA.Tiles;
using System.Collections.Generic;
using UnityEngine;

namespace DBGA.MapGeneration
{
    [DisallowMultipleComponent]
    public class MapGenerator
    {
        public struct ProbabilityRange
        {
            public float min;
            public float max;
        }

        /// <summary>
        /// Generates a random map of tiles with the given size and tiles from the given list
        /// </summary>
        /// <param name="gridSize">The size of the resulting grid map (e.g. if 20 the result is a 20x20 grid)</param>
        /// <param name="tileList">The list of available tiles</param>
        /// <returns>A random grid of tiles with the given size and tiles</returns>
        public static Tile[][] GenerateMap(int gridSize, TilesList tileList)
        {
            Dictionary<ProbabilityRange, Tile> availableTilesWithProbability = InitializeTilesAndProbability(tileList);

            Tile[][] grid = new Tile[gridSize][];

            for (int row = 0; row < gridSize; row++)
            {
                grid[row] = new Tile[gridSize];
                for (int col = 0; col < gridSize; col++)
                {
                    grid[row][col] = GetRandomTileFromAvailable(availableTilesWithProbability);
                    grid[row][col].SetPositionOnGrid(new Vector2Int(row, col));
                }
            }

            return grid;
        }

        /// <summary>
        /// Initializes the tiles probabilities in order to pick them in a weighted way
        /// </summary>
        /// <param name="tileList">The list of available tiles with their probability weights</param>
        /// <returns>The initialized list of tiles with probability ranges</returns>
        private static Dictionary<ProbabilityRange, Tile> InitializeTilesAndProbability(TilesList tileList)
        {
            float totalTilesProbabilityWeight = 0;
            foreach (TileListItem tileListItem in tileList.availableTiles)
                totalTilesProbabilityWeight += tileListItem.probabilityWeight;

            Dictionary<ProbabilityRange, Tile> availableTilesWithProbability = new Dictionary<ProbabilityRange, Tile>();
            float previousRangeMin = 0f;
            foreach (TileListItem tileListItem in tileList.availableTiles)
            {
                float rangeMax = previousRangeMin + tileListItem.probabilityWeight / totalTilesProbabilityWeight;
                ProbabilityRange probabilityRange = new() { min = previousRangeMin, max = rangeMax };

                if (!availableTilesWithProbability.ContainsKey(probabilityRange))
                    availableTilesWithProbability.Add(probabilityRange, tileListItem.tilePrefab);

                previousRangeMin = rangeMax;
            }

            return availableTilesWithProbability;
        }

        /// <summary>
        /// Returns a weighted random tile between the available ones
        /// </summary>
        /// <param name="availableTilesWithProbability">The list of available tiles with probability ranges</param>
        /// <returns>A weighted random tile between the available ones</returns>
        private static Tile GetRandomTileFromAvailable(Dictionary<ProbabilityRange, Tile> availableTilesWithProbability)
        {
            float randomNumber = Random.Range(0f, 1f);
            foreach (KeyValuePair<ProbabilityRange, Tile> tile in availableTilesWithProbability)
                if (randomNumber >= tile.Key.min && randomNumber < tile.Key.max)
                    return tile.Value;

            return null;
        }
    }
}

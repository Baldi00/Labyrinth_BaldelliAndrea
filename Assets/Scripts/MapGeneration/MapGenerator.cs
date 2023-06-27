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

        public static Tile[][] GenerateMap(int gridSize, TilesList tileList)
        {
            Dictionary<ProbabilityRange, Tile> availableTilesWithProbability = InitializeTilesAndProbability(tileList);

            Tile[][] grid = new Tile[gridSize][];

            for (int row = 0; row < gridSize; row++)
            {
                grid[row] = new Tile[gridSize];
                for(int col = 0; col < gridSize; col++)
                {
                    grid[row][col] = GetRandomTileFromAvailable(availableTilesWithProbability);
                    grid[row][col].SetPosition(row, col);
                }
            }

            return grid;
        }

        private static Dictionary<ProbabilityRange, Tile> InitializeTilesAndProbability(TilesList tileList)
        {
            float totalTilesProbabilityWeight = 0;
            foreach(TileListItem tileListItem in tileList.availableTiles)
                totalTilesProbabilityWeight += tileListItem.probabilityWeight;

            Dictionary<ProbabilityRange, Tile> availableTilesWithProbability = new Dictionary<ProbabilityRange, Tile>();
            float previousRangeMin = 0f;
            foreach(TileListItem tileListItem in tileList.availableTiles)
            {
                float rangeMax = previousRangeMin + tileListItem.probabilityWeight / totalTilesProbabilityWeight;
                availableTilesWithProbability
                    .Add(new ProbabilityRange() { min = previousRangeMin, max = rangeMax }, tileListItem.tilePrefab);

                previousRangeMin = rangeMax;
            }

            return availableTilesWithProbability;
        }

        private static Tile GetRandomTileFromAvailable(Dictionary<ProbabilityRange, Tile> availableTilesWithProbability)
        {
            float randomNumber = Random.Range(0f, 1f);
            foreach(KeyValuePair<ProbabilityRange, Tile> tile in availableTilesWithProbability)
                if (randomNumber >= tile.Key.min && randomNumber < tile.Key.max)
                    return tile.Value;

            return null;
        }
    }
}

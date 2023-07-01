using DBGA.Tiles;
using DBGA.Common;
using System.Collections.Generic;
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
        public Tile[][] GenerateMap(int gridSize, TilesList tilesList)
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
                        Instantiate(randomTileGameObject, new Vector3(row, 0f, col), Quaternion.identity, transform);

                    grid[row][col] = tileInstanceGameObject.GetComponent<Tile>();
                    grid[row][col].SetPositionOnGrid(new Vector2Int(row, col));
                }
            }
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
    }
}

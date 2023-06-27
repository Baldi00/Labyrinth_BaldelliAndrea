using DBGA.Tiles;
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

        [SerializeField]
        private int gridSize;
        [SerializeField]
        private TilesList tileList;

        private Tile[][] grid;

        private Dictionary<ProbabilityRange, Tile> availableTilesWithProbability;

        void Awake()
        {
            InitializeTilesAndProbability();
        }

        void Start()
        {
            grid = new Tile[gridSize][];
            for (int row = 0; row < gridSize; row++)
            {
                grid[row] = new Tile[gridSize];
                for(int col = 0; col < gridSize; col++)
                {
                    grid[row][col] = GetRandomTileFromAvailable();
                    grid[row][col].SetPosition(row, col);
                    Instantiate(grid[row][col].gameObject, new Vector3(row, 0f, col), Quaternion.identity, transform);
                }
            }
        }

        private void InitializeTilesAndProbability()
        {
            float totalTilesProbabilityWeight = 0;
            foreach(TileListItem tileListItem in tileList.availableTiles)
                totalTilesProbabilityWeight += tileListItem.probabilityWeight;

            availableTilesWithProbability = new Dictionary<ProbabilityRange, Tile>();
            float previousRangeMin = 0f;
            foreach(TileListItem tileListItem in tileList.availableTiles)
            {
                float rangeMax = previousRangeMin + tileListItem.probabilityWeight / totalTilesProbabilityWeight;
                availableTilesWithProbability
                    .Add(new ProbabilityRange() { min = previousRangeMin, max = rangeMax }, tileListItem.tilePrefab);

                previousRangeMin = rangeMax;
            }
        }

        private Tile GetRandomTileFromAvailable()
        {
            float randomNumber = Random.Range(0f, 1f);
            foreach(KeyValuePair<ProbabilityRange, Tile> tile in availableTilesWithProbability)
                if (randomNumber >= tile.Key.min && randomNumber < tile.Key.max)
                    return tile.Value;

            return null;
        }
    }
}

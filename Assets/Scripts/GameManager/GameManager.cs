using DBGA.MapGeneration;
using DBGA.Tiles;
using UnityEngine;

namespace DBGA.GameManager
{
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        [Header("Map generation")]
        [SerializeField]
        private bool generateRandomMap;
        [SerializeField]
        private int gridSize;
        [SerializeField]
        private TilesList tilesList;

        private Tile[][] grid;

        void Awake()
        {
            if (generateRandomMap)
                GenerateAndInstantiateRandomMap();
        }

        /// <summary>
        /// Generates and instantiates a random grid map
        /// </summary>
        private void GenerateAndInstantiateRandomMap()
        {
            grid = MapGenerator.GenerateMap(gridSize, tilesList);
            for (int row = 0; row < gridSize; row++)
                for (int col = 0; col < gridSize; col++)
                    Instantiate(
                        grid[row][col].gameObject,
                        new Vector3(row, 0f, col),
                        Quaternion.identity,
                        transform);
        }
    }
}

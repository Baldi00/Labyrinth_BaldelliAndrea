using DBGA.Common;
using DBGA.ThroughScenes;
using DBGA.Tiles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DBGA.UI
{
    [DisallowMultipleComponent]
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField]
        private Toggle animateGenerationToggle;

        [SerializeField]
        private InputField gridSizeInputField;

        [Header("Tiles")]
        [SerializeField]
        private InputField xTilesProbability;
        [SerializeField]
        private InputField tTilesProbability;
        [SerializeField]
        private InputField iTilesProbability;
        [SerializeField]
        private InputField lTilesProbability;
        [SerializeField]
        private InputField doubleLTilesProbability;
        [SerializeField]
        private TilesList initialTilesList;

        [Header("Map Elements")]
        [SerializeField]
        private InputField teleportsInputField;
        [SerializeField]
        private InputField wellsInputField;
        [SerializeField]
        private MapElementsList initialMapElementsList;

        [Header("Errors")]
        [SerializeField]
        private GameObject mapGenerationError;

        void Awake()
        {
            SetUpMainMenuUI();
        }

        /// <summary>
        /// Sets custom map elements parameters then loads the precomputed maze scene
        /// </summary>
        public void LoadPrecomputedMazeScene()
        {
            SetMapElementsInThroughSceneParameters();
            SceneManager.LoadScene("PrecomputedMazeScene");
        }

        /// <summary>
        /// Sets custom tiles weights and map elements parameters then loads the maze generation scene
        /// </summary>
        public void LoadGeneratedMazeScene()
        {
            SetTilesItemListInThroughSceneParameters();
            SetMapElementsInThroughSceneParameters();
            ThroughScenesParameters.GridSize = Mathf.Clamp(int.Parse(gridSizeInputField.text), 5, 50);
            ThroughScenesParameters.AnimateGeneration = animateGenerationToggle.isOn;
            SceneManager.LoadScene("GeneratedMazeScene");
        }

        /// <summary>
        /// Quits the game
        /// </summary>
        public void QuitGame()
        {
            Application.Quit();
        }

        /// <summary>
        /// Initializes the main menu UI with default values for grid size, tiles probabilities and map elements
        /// </summary>
        private void SetUpMainMenuUI()
        {
            if (ThroughScenesParameters.MapGenerationError)
                mapGenerationError.SetActive(true);

            gridSizeInputField.text = "20";

            xTilesProbability.text = GetListOfTiles("X_Tile")[0].ProbabilityWeight.ToString();
            tTilesProbability.text = GetListOfTiles("T_Tile")[0].ProbabilityWeight.ToString();
            iTilesProbability.text = GetListOfTiles("I_Tile")[0].ProbabilityWeight.ToString();
            lTilesProbability.text = GetListOfTiles("L_Tile", "Double_L_Tile")[0].ProbabilityWeight.ToString();
            doubleLTilesProbability.text = GetListOfTiles("Double_L_Tile")[0].ProbabilityWeight.ToString();

            teleportsInputField.text = initialMapElementsList.MapElements
                .Find(element => element.MapElementType == MapElementType.TELEPORT).Count.ToString();
            wellsInputField.text = initialMapElementsList.MapElements
                .Find(element => element.MapElementType == MapElementType.WELL).Count.ToString();
        }

        /// <summary>
        /// Returns a list of tiles from the available ones that contains or not contains the given strings in the name
        /// </summary>
        /// <param name="contains">String contained in the name of the tiles</param>
        /// <param name="notContains">String not contained in the name of the tiles</param>
        /// <returns>A list of tiles from the available ones that contains or not contains the given strings in the name</returns>
        private List<TileListItem> GetListOfTiles(string contains, string notContains = "###")
        {
            return initialTilesList.AvailableTiles
                .Where<TileListItem>(
                tile => tile.TilePrefab.name.Contains(contains) &&
                !tile.TilePrefab.name.Contains(notContains))
                .ToList<TileListItem>();
        }

        /// <summary>
        /// Sets the tiles probabilities in the through scenes parameters
        /// </summary>
        private void SetTilesItemListInThroughSceneParameters()
        {
            List<TileListItem> xTilesList = GetListOfTiles("X_Tile");
            List<TileListItem> tTilesList = GetListOfTiles("T_Tile");
            List<TileListItem> iTilesList = GetListOfTiles("I_Tile");
            List<TileListItem> lTilesList = GetListOfTiles("L_Tile", "Double_L_Tile");
            List<TileListItem> doubleLTilesList = GetListOfTiles("Double_L_Tile");

            xTilesList.ForEach(tile => tile.ProbabilityWeight = Mathf.Max(0, float.Parse(xTilesProbability.text)));
            tTilesList.ForEach(tile => tile.ProbabilityWeight = Mathf.Max(0, float.Parse(tTilesProbability.text)));
            iTilesList.ForEach(tile => tile.ProbabilityWeight = Mathf.Max(0, float.Parse(iTilesProbability.text)));
            lTilesList.ForEach(tile => tile.ProbabilityWeight = Mathf.Max(0, float.Parse(lTilesProbability.text)));
            doubleLTilesList.ForEach(tile => tile.ProbabilityWeight = Mathf.Max(0, float.Parse(doubleLTilesProbability.text)));

            List<TileListItem> tilesListItems = new();
            tilesListItems.AddRange(xTilesList);
            tilesListItems.AddRange(tTilesList);
            tilesListItems.AddRange(iTilesList);
            tilesListItems.AddRange(lTilesList);
            tilesListItems.AddRange(doubleLTilesList);

            TilesList tilesList = ScriptableObject.CreateInstance<TilesList>();
            tilesList.AvailableTiles = tilesListItems;

            ThroughScenesParameters.TilesList = tilesList;
        }

        /// <summary>
        /// Sets the map elements count in the through scenes parameters
        /// </summary>
        private void SetMapElementsInThroughSceneParameters()
        {
            MapElementsList mapElementsList = ScriptableObject.CreateInstance<MapElementsList>();
            mapElementsList.MapElements = new List<MapElementsListItem>()
            {
                new MapElementsListItem()
                {
                    MapElementType = MapElementType.MONSTER,
                    Prefab = initialMapElementsList.MapElements
                        .Find(element => element.MapElementType == MapElementType.MONSTER).Prefab,
                    Count = 1
                },
                new MapElementsListItem()
                {
                    MapElementType = MapElementType.TELEPORT,
                    Prefab = initialMapElementsList.MapElements
                        .Find(element => element.MapElementType == MapElementType.TELEPORT).Prefab,
                    Count = Mathf.Max(0, int.Parse(teleportsInputField.text))
                },
                new MapElementsListItem()
                {
                    MapElementType = MapElementType.WELL,
                    Prefab = initialMapElementsList.MapElements
                        .Find(element => element.MapElementType == MapElementType.WELL).Prefab,
                    Count = Mathf.Max(0, int.Parse(wellsInputField.text))
                },
            };

            ThroughScenesParameters.MapElementsList = mapElementsList;
        }
    }
}

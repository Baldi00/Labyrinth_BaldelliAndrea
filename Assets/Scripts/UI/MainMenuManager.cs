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
            if (ThroughScenesParameters.mapGenerationError)
                mapGenerationError.SetActive(true);

            gridSizeInputField.text = "20";

            xTilesProbability.text = GetListOfTiles("X_Tile")[0].probabilityWeight.ToString();
            tTilesProbability.text = GetListOfTiles("T_Tile")[0].probabilityWeight.ToString();
            iTilesProbability.text = GetListOfTiles("I_Tile")[0].probabilityWeight.ToString();
            lTilesProbability.text = GetListOfTiles("L_Tile", "Double_L_Tile")[0].probabilityWeight.ToString();
            doubleLTilesProbability.text = GetListOfTiles("Double_L_Tile")[0].probabilityWeight.ToString();

            teleportsInputField.text = initialMapElementsList.mapElements
                .Find(element => element.mapElement == MapElement.TELEPORT).count.ToString();
            wellsInputField.text = initialMapElementsList.mapElements
                .Find(element => element.mapElement == MapElement.WELL).count.ToString();
        }

        public void LoadPrecomputedMazeScene()
        {
            SetMapElementsInThroughSceneParameters();
            SceneManager.LoadScene("PrecomputedMazeScene");
        }

        public void LoadGeneratedMazeScene()
        {
            SetTilesItemListInThroughSceneParameters();
            SetMapElementsInThroughSceneParameters();
            ThroughScenesParameters.gridSize = Mathf.Clamp(int.Parse(gridSizeInputField.text), 5, 50);
            ThroughScenesParameters.animateGeneration = animateGenerationToggle.isOn;
            SceneManager.LoadScene("GeneratedMazeScene");
        }

        private List<TileListItem> GetListOfTiles(string contains, string notContains = "###")
        {
            return initialTilesList.availableTiles
                .Where<TileListItem>(
                tile => tile.tilePrefab.name.Contains(contains) &&
                !tile.tilePrefab.name.Contains(notContains))
                .ToList<TileListItem>();
        }

        private void SetTilesItemListInThroughSceneParameters()
        {
            List<TileListItem> xTilesList = GetListOfTiles("X_Tile");
            List<TileListItem> tTilesList = GetListOfTiles("T_Tile");
            List<TileListItem> iTilesList = GetListOfTiles("I_Tile");
            List<TileListItem> lTilesList = GetListOfTiles("L_Tile", "Double_L_Tile");
            List<TileListItem> doubleLTilesList = GetListOfTiles("Double_L_Tile");

            xTilesList.ForEach(tile => tile.probabilityWeight = Mathf.Max(0, float.Parse(xTilesProbability.text)));
            tTilesList.ForEach(tile => tile.probabilityWeight = Mathf.Max(0, float.Parse(tTilesProbability.text)));
            iTilesList.ForEach(tile => tile.probabilityWeight = Mathf.Max(0, float.Parse(iTilesProbability.text)));
            lTilesList.ForEach(tile => tile.probabilityWeight = Mathf.Max(0, float.Parse(lTilesProbability.text)));
            doubleLTilesList.ForEach(tile => tile.probabilityWeight = Mathf.Max(0, float.Parse(doubleLTilesProbability.text)));

            List<TileListItem> tilesListItems = new List<TileListItem>();
            tilesListItems.AddRange(xTilesList);
            tilesListItems.AddRange(tTilesList);
            tilesListItems.AddRange(iTilesList);
            tilesListItems.AddRange(lTilesList);
            tilesListItems.AddRange(doubleLTilesList);

            TilesList tilesList = ScriptableObject.CreateInstance<TilesList>();
            tilesList.availableTiles = tilesListItems;

            ThroughScenesParameters.tilesList = tilesList;
        }

        private void SetMapElementsInThroughSceneParameters()
        {
            MapElementsList mapElementsList = ScriptableObject.CreateInstance<MapElementsList>();
            mapElementsList.mapElements = new List<MapElementsItem>()
            {
                new MapElementsItem()
                {
                    mapElement = MapElement.MONSTER,
                    prefab = initialMapElementsList.mapElements
                        .Find(element => element.mapElement == MapElement.MONSTER).prefab,
                    count = 1
                },
                new MapElementsItem()
                {
                    mapElement = MapElement.TELEPORT,
                    prefab = initialMapElementsList.mapElements
                        .Find(element => element.mapElement == MapElement.TELEPORT).prefab,
                    count = Mathf.Max(0, int.Parse(teleportsInputField.text))
                },
                new MapElementsItem()
                {
                    mapElement = MapElement.WELL,
                    prefab = initialMapElementsList.mapElements
                        .Find(element => element.mapElement == MapElement.WELL).prefab,
                    count = Mathf.Max(0, int.Parse(wellsInputField.text))
                },
            };

            ThroughScenesParameters.mapElementsList = mapElementsList;
        }
    }
}

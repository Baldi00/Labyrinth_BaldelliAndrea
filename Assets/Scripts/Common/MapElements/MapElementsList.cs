using System.Collections.Generic;
using UnityEngine;

namespace DBGA.Common
{
    /// <summary>
    /// A list of map elements that should be placed on the map
    /// </summary>
    [CreateAssetMenu(fileName = "MapElementsList", menuName = "Labyrinth/Create Map Elements List", order = 1)]
    public class MapElementsList : ScriptableObject
    {
        [SerializeField]
        private List<MapElementsListItem> mapElements;

        public List<MapElementsListItem> MapElements { set => mapElements = value; get => mapElements; }
    }
}

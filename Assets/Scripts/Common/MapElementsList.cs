using System.Collections.Generic;
using UnityEngine;

namespace DBGA.Common
{
    [CreateAssetMenu(fileName = "MapElementsList", menuName = "Labyrinth/Create Map Elements List", order = 1)]
    public class MapElementsList : ScriptableObject
    {
        public List<MapElementsItem> mapElements;
    }
}

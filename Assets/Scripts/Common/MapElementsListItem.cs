using System;
using UnityEngine;

namespace DBGA.Common
{
    /// <summary>
    /// An item of the map elements list.
    /// Contains the map element type along with its game object prefab and
    /// the number of elements that should be placed in the map
    /// </summary>
    [Serializable]
    public class MapElementsListItem
    {
        public MapElementType mapElement;
        public GameObject prefab;
        public int count;
    }
}

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
        [SerializeField]
        private MapElementType mapElement;
        [SerializeField]
        private GameObject prefab;
        [SerializeField]
        private int count;

        public MapElementType MapElementType { set => mapElement = value; get => mapElement; }
        public GameObject Prefab { set => prefab = value; get => prefab; }
        public int Count { set => count = value; get => count; }
    }
}

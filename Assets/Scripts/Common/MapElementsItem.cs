using System;
using UnityEngine;

namespace DBGA.Common
{
    [Serializable]
    public struct MapElementsItem
    {
        public MapElement mapElement;
        public GameObject prefab;
        public int count;
    }
}

using DBGA.Common;
using System.Collections.Generic;
using UnityEngine;

namespace DBGA.Tiles
{
    [DisallowMultipleComponent]
    public class Tile : MonoBehaviour
    {
        [SerializeField]
        private GameObject thisTileTrigger;
        [SerializeField]
        private GameObject adjacentTriggers;
        [SerializeField]
        private List<Direction> outDirections;

        private bool hasMonster;
        private bool hasWell;
        private bool hasTeleport;

        public List<Direction> OutDirections { get => outDirections; }

        public bool HasMonster
        {
            set
            {
                hasMonster = value;
                if (value)
                {
                    thisTileTrigger.tag = "Monster";
                    adjacentTriggers.tag = "AdjacentMonster";
                }
                else
                {
                    thisTileTrigger.tag = "Untagged";
                    adjacentTriggers.tag = "Untagged";
                }
            }

            get => hasMonster;
        }

        public bool HasWell
        {
            set
            {
                hasWell = value;
                if (value)
                {
                    thisTileTrigger.tag = "Well";
                    adjacentTriggers.tag = "AdjacentWell";
                }
                else
                {
                    thisTileTrigger.tag = "Untagged";
                    adjacentTriggers.tag = "Untagged";
                }
            }

            get => hasWell;
        }

        public bool HasTeleport
        {
            set
            {
                hasTeleport = value;
                if (value)
                {
                    thisTileTrigger.tag = "Teleport";
                    adjacentTriggers.tag = "AdjacentTeleport";
                }
                else
                {
                    thisTileTrigger.tag = "Untagged";
                    adjacentTriggers.tag = "Untagged";
                }
            }

            get => hasTeleport;
        }

        public bool IsVoid { get; set; }

        public bool PlayerExplored { set; get; }

        public Vector2Int PositionOnGrid { set; get; }
    }
}

using DBGA.Common;
using UnityEngine;

namespace DBGA.Tiles
{
    [DisallowMultipleComponent]
    public class TunnelTrigger : MonoBehaviour, ITunnelTrigger
    {
        [SerializeField]
        private Tunnel tunnel;

        public ITunnel GetTunnel() => tunnel;
    }
}
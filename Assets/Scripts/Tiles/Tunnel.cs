using DBGA.Common;
using DBGA.EventSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DBGA.Tiles
{
    [DisallowMultipleComponent]
    public class Tunnel : Tile, ITunnel
    {
        [Serializable]
        public struct AdjacentTile
        {
            public Direction direction;
            public Tile tile;
        }

        [SerializeField]
        private Cross[] availableCrossings;

        private List<AdjacentTile> adjacentTiles;

        private GameEventsManager gameEventsManager;

        void Awake()
        {
            adjacentTiles = new List<AdjacentTile>();
            gameEventsManager = GameEventsManager.Instance;
        }

        public void AddAdjacentTile(Direction direction, Tile tile)
        {
            adjacentTiles.Add(new AdjacentTile() { direction = direction, tile = tile });
        }

        /// <summary>
        /// Checks if tunnel can be crossed from the enter direction till the end
        /// </summary>
        /// <param name="enterDirection">The entrance direction in the tunnel.
        /// (e.g. if player is under a tunnel and goes Up, it is entering in the tunnel from Down, then you should pass Down)
        /// </param>
        /// <returns>True if tunnel can be crossed till the end, false otherwise</returns>
        public bool CanCross(Direction enterDirection)
        {
            Direction outDirection = GetOutDirection(enterDirection);

            // Something went wrong, can't cross the tile
            if (outDirection == Direction.None)
                return false;

            AdjacentTile nextTile = GetAdjacentTile(outDirection);

            // Something went wrong, can't cross the tile
            if (nextTile.direction == Direction.None)
                return false;


            // Next tile is a tunnel, check if can cross till the end
            if (nextTile.tile is Tunnel nextTunnelTile)
                return nextTunnelTile.CanCross(nextTile.direction.GetOppositeDirection());

            // Next tile is a normal tile, check if can traverse (i.e. no wall on the exit)
            Vector3 position3d = new(transform.position.x, 0.25f, transform.position.z);
            Vector3 nextPosition3d = new(nextTile.tile.transform.position.x, 0.25f, nextTile.tile.transform.position.z);

            RaycastHit[] raycastHits = Physics.RaycastAll(position3d, nextPosition3d - position3d, 1f);

            if (raycastHits.Any(hit => !hit.collider.isTrigger))
                return false;

            return true;
        }

        /// <summary>
        /// Returns the final destination of the tunnel
        /// </summary>
        /// <param name="enterDirection">The entrance direction in the tunnel.
        /// (e.g. if player is under a tunnel and goes Up, it is entering in the tunnel from Down, then you should pass Down)
        /// </param>
        /// <returns>The final destination of the tunnel if tunnel can be crossed, -inf position otherwise</returns>
        public Vector2Int GetFinalDestination(Direction enterDirection)
        {
            Direction outDirection = GetOutDirection(enterDirection);

            // Something went wrong, can't cross the tile
            if (outDirection == Direction.None)
                return Vector2Int.one * int.MinValue;

            AdjacentTile nextTile = GetAdjacentTile(outDirection);

            // Something went wrong, can't cross the tile
            if (nextTile.direction == Direction.None)
                return Vector2Int.one * int.MinValue;

            // Next tile is a tunnel, check if can cross till the end
            if (nextTile.tile is Tunnel nextTunnelTile)
                return nextTunnelTile.GetFinalDestination(nextTile.direction.GetOppositeDirection());

            // Next tile is a normal tile, check if can traverse (i.e. no wall on the exit)
            Vector3 position3d = new(transform.position.x, 0.25f, transform.position.z);
            Vector3 nextPosition3d = new(nextTile.tile.transform.position.x, 0.25f, nextTile.tile.transform.position.z);

            RaycastHit[] raycastHits = Physics.RaycastAll(position3d, nextPosition3d - position3d, 1f);

            if (raycastHits.Any(hit => !hit.collider.isTrigger))
                return Vector2Int.one * int.MinValue;

            return nextTile.tile.PositionOnGrid;
        }

        /// <summary>
        /// Returns the list of all crossing points in the tunnel
        /// </summary>
        /// <param name="enterDirection">The entrance direction in the tunnel.
        /// (e.g. if player is under a tunnel and goes Up, it is entering in the tunnel from Down, then you should pass Down)
        /// </param>
        /// <returns>The list of all crossing points in the tunnel</returns>
        public List<Vector2Int> GetAllCrossingPoints(Direction enterDirection)
        {
            Direction outDirection = GetOutDirection(enterDirection);
            List<Vector2Int> crossingPoints = new List<Vector2Int>();

            // Something went wrong, can't cross the tile
            if (outDirection == Direction.None)
                return crossingPoints;

            AdjacentTile nextTile = GetAdjacentTile(outDirection);

            // Something went wrong, can't cross the tile
            if (nextTile.direction == Direction.None)
                return crossingPoints;

            // Next tile is a tunnel, check if can cross till the end
            crossingPoints.Add(PositionOnGrid);

            if (nextTile.tile is Tunnel nextTunnelTile)
            {
                crossingPoints.AddRange(nextTunnelTile.GetAllCrossingPoints(nextTile.direction.GetOppositeDirection()));
                return crossingPoints;
            }

            // Next tile is a normal tile, check if can traverse (i.e. no wall on the exit)
            Vector3 position3d = new(transform.position.x, 0.25f, transform.position.z);
            Vector3 nextPosition3d = new(nextTile.tile.transform.position.x, 0.25f, nextTile.tile.transform.position.z);

            RaycastHit[] raycastHits = Physics.RaycastAll(position3d, nextPosition3d - position3d, 1f);

            if (raycastHits.Any(hit => !hit.collider.isTrigger))
                return crossingPoints;

            crossingPoints.Add(nextTile.tile.PositionOnGrid);
            return crossingPoints;
        }

        /// <summary>
        /// Reveals this tunnel and all adjacent crossed tunnel tiles
        /// </summary>
        /// <param name="enterDirection">The entrance direction in the tunnel.
        /// (e.g. if player is under a tunnel and goes Up, it is entering in the tunnel from Down, then you should pass Down)
        /// </param>
        public void RevealEntireTunnel(Direction enterDirection)
        {
            Direction outDirection = GetOutDirection(enterDirection);

            // Something went wrong, can't cross the tile
            if (outDirection == Direction.None)
                return;

            AdjacentTile nextTile = GetAdjacentTile(outDirection);

            // Something went wrong, can't cross the tile
            if (nextTile.direction == Direction.None)
                return;

            gameEventsManager.DispatchGameEvent(new PlayerExploredTileEvent() { positionOnGrid = PositionOnGrid });

            // Next tile is a tunnel, reveal next tile
            if (nextTile.tile is Tunnel nextTunnelTile)
                nextTunnelTile.RevealEntireTunnel(nextTile.direction.GetOppositeDirection());
        }

        /// <summary>
        /// Returns the list of available directions from this tile
        /// </summary>
        /// <returns>The list of available directions from this tile</returns>
        public List<Direction> GetAvailableDirections()
        {
            HashSet<Direction> availableDirections = new HashSet<Direction>();
            foreach (Cross cross in availableCrossings)
            {
                availableDirections.Add(cross.direction1);
                availableDirections.Add(cross.direction2);
            }
            return availableDirections.ToList<Direction>();
        }

        /// <summary>
        /// Returns the out direction of the tunnel if entering from the given direction
        /// </summary>
        /// <param name="enterDirection">The entrance direction in the tunnel.
        /// (e.g. if player is under a tunnel and goes Up, it is entering in the tunnel from Down, then you should pass Down)
        /// </param>
        /// <returns>The out direction of the tunnel if entering from the given direction</returns>
        public Direction GetOutDirection(Direction enterDirection)
        {
            foreach (Cross cross in availableCrossings)
            {
                if (cross.direction1 == enterDirection)
                    return cross.direction2;
                else if (cross.direction2 == enterDirection)
                    return cross.direction1;
            }
            return Direction.None;
        }

        /// <summary>
        /// Returns the adjacent tile for the given direction
        /// </summary>
        /// <param name="enterDirection">The entrance direction in the tunnel.
        /// (e.g. if player is under a tunnel and goes Up, it is entering in the tunnel from Down, then you should pass Down)
        /// </param>
        /// <returns>The adjacent tile for the given direction</returns>
        private AdjacentTile GetAdjacentTile(Direction enterDirection)
        {
            foreach (AdjacentTile adjacentTile in adjacentTiles)
                if (adjacentTile.direction == enterDirection)
                    return adjacentTile;

            return new AdjacentTile() { direction = Direction.None };
        }

    }
}

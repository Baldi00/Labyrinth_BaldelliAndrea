using UnityEngine;
using DBGA.Common;
using System.Collections.Generic;
using System;
using System.Linq;

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

        private List<AdjacentTile> adjacentTiles;

        void Awake()
        {
            adjacentTiles = new List<AdjacentTile>();
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
            List<Direction> outDirections = GetOutDirections(enterDirection);

            // Something went wrong, can't cross the tile
            if (outDirections.Count == 0)
                return false;

            AdjacentTile nextTile = GetAdjacentTile(outDirections[0]);

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
            List<Direction> outDirections = GetOutDirections(enterDirection);

            // Something went wrong, can't cross the tile
            if (outDirections.Count == 0)
                return Vector2Int.one * int.MinValue;

            AdjacentTile nextTile = GetAdjacentTile(outDirections[0]);

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
            List<Direction> outDirections = GetOutDirections(enterDirection);
            List<Vector2Int> crossingPoints = new List<Vector2Int>();

            // Something went wrong, can't cross the tile
            if (outDirections.Count == 0)
                return crossingPoints;

            AdjacentTile nextTile = GetAdjacentTile(outDirections[0]);

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
            List<Direction> outDirections = GetOutDirections(enterDirection);

            // Something went wrong, can't cross the tile
            if (outDirections.Count == 0)
                return;

            AdjacentTile nextTile = GetAdjacentTile(outDirections[0]);

            // Something went wrong, can't cross the tile
            if (nextTile.direction == Direction.None)
                return;

            // TODO: Reveal this tile

            // Next tile is a tunnel, reveal next tile
            if (nextTile.tile is Tunnel nextTunnelTile)
                nextTunnelTile.RevealEntireTunnel(nextTile.direction.GetOppositeDirection());
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

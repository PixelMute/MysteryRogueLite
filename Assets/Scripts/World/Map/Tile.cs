using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Roguelike
{
    // The BattleGrid stores a 2d array of these.
    public class Tile
    {
        // This is null if there's nothing here but a floor.
        private TileEntity entityOnTile = null;

        public enum TileEntityType { empty, wall, enemy, player };
        [HideInInspector] public TileEntityType tileEntityType = TileEntityType.empty;

        public Tile()
        {
            entityOnTile = null;
        }

        public void SetEntityOnTile(TileEntity entity)
        {
            entityOnTile = entity;

            if (entityOnTile == null)
                tileEntityType = TileEntityType.empty;
            else if (entityOnTile is Wall)
                tileEntityType = TileEntityType.wall;
            else if (entityOnTile is GenericEnemy)
                tileEntityType = TileEntityType.enemy;
            else if (entityOnTile is PlayerController)
                tileEntityType = TileEntityType.player;
        }

        public TileEntity GetEntityOnTile()
        {
            return entityOnTile;
        }

        public float GetPathfindingCost()
        {
            if (entityOnTile == null)
            {
                return 1f; // Empty tile.
            }
            else
            {
                return entityOnTile.GetPathfindingCost();
            }
        }

        // Returns wether the player can walk on this or not.
        public bool GetPlayerWalkability()
        {
            if (entityOnTile == null)
            {
                return true; // Empty tile.
            }
            else
            {
                return entityOnTile.GetPlayerWalkable();
            }
        }

        // Returns true if there's a moving character on this tile
        public bool IsCreatureOnTile()
        {
            if (entityOnTile == null)
            {
                return false;
            }
            else
            {
                if (tileEntityType == TileEntityType.enemy || tileEntityType == TileEntityType.player)
                    return true;
                else
                    return false;
            }
        }
    }
}

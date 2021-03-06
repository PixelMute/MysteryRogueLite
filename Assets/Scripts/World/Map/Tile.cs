﻿using System;
using UnityEngine;
namespace Roguelike
{
    [Serializable]
    // The BattleGrid stores a 2d array of these.
    public class Tile
    {
        public enum TileEntityType { empty, wall, enemy, player, boss };
        [HideInInspector] public TileEntityType tileEntityType = TileEntityType.empty;
        // This is null if there's nothing here but a floor.
        [NonSerialized]
        private TileEntity entityOnTile = null;

        public enum TileItemType { empty, money, smallChest };
        public TileItemType tileItemType = TileItemType.empty;
        [NonSerialized]
        private TileItem itemOnTile;

        public enum TileTerrainType { floor, stairsUp, stairsDown, trap };
        [NonSerialized]
        public TileTerrain terrainOnTile = null;
        public TileTerrainType tileTerrainType = TileTerrainType.floor;

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
            else if (entityOnTile is EnemyBody && !(((EnemyBody)entityOnTile).AI is BossBrain))
                tileEntityType = TileEntityType.enemy;
            else if (entityOnTile is PlayerController)
                tileEntityType = TileEntityType.player;
            //else if (entityOnTile is Stairs)
            //tileEntityType = ((Stairs)entityOnTile).IsUp ? TileEntityType.stairsUp : TileEntityType.stairsDown;
        }

        public void SetItemOnTile(TileItem item)
        {
            itemOnTile = item;
            if (itemOnTile == null)
                tileItemType = TileItemType.empty;
            else if (itemOnTile is DroppedMoney)
                tileItemType = TileItemType.money;
            else if (itemOnTile is TreasureChest)
            {
                switch ((itemOnTile as TreasureChest).ChestType)
                {
                    case TreasureChest.TreasureChestTypeEnum.small:
                        tileItemType = TileItemType.smallChest;
                        break;
                }
            }
        }

        public TileItem GetItemOnTile()
        {
            return itemOnTile;
        }

        public TileEntity GetEntityOnTile()
        {
            if (tileEntityType == TileEntityType.boss)
            {
                return BossRoom.Boss;
            }
            return entityOnTile;
        }

        public void SetTerrainOnTile(TileTerrain terrain)
        {
            terrainOnTile = terrain;

            if (terrain == null)
                tileTerrainType = TileTerrainType.floor;
            else if (terrain is Stairs)
            {
                Stairs s = terrain as Stairs;
                if (s.IsUp)
                    tileTerrainType = TileTerrainType.stairsUp;
                else
                    tileTerrainType = TileTerrainType.stairsDown;
            }
            else if (terrain is Trap)
            {
                tileTerrainType = TileTerrainType.trap;
            }
        }

        public float GetPathfindingCost()
        {
            if (entityOnTile == null)
            {
                if (tileEntityType == TileEntityType.boss)
                {
                    return 0f;
                }
                return 1f; // Empty tile.
            }
            else
            {
                float mult = 1f; // multiplier from terrain
                if (terrainOnTile != null)
                    mult = terrainOnTile.GetPathfindingCost();
                return entityOnTile.GetPathfindingCost() * mult;
            }
        }

        // Returns wether the player can walk on this or not.
        public bool GetPlayerWalkability()
        {
            if (entityOnTile == null)
            {
                if (tileEntityType == TileEntityType.boss)
                {
                    return false;
                }
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
            if (tileEntityType == TileEntityType.enemy || tileEntityType == TileEntityType.player || tileEntityType == TileEntityType.boss)
                return true;
            else
                return false;
        }
    }
}

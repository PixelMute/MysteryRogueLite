using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : TileItem
{
    public enum TreasureChestTypeEnum { small }; // This currently is unused, but this is to add more types of chests.
    public TreasureChestTypeEnum ChestType;
}

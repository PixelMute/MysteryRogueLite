using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedMoney : TileItem
{
    private int value;

    public int Value { get => value; private set => this.value = value; }

    public void Initialize(int amount)
    {
        Value = amount;
    }
}

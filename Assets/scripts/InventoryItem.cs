using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class InventoryItem
{
    public string Type { get; set; }
    public int Amount { get; set; }
    public float Health { get; set; }
    public int Position { get; set; }
    public bool IsEquipped { get; set; }
    public float X { get; set; }
    public float Y { get; set; }

    public InventoryItem()
    {
        Amount = 0;
        Health = 100f;
        IsEquipped = false;
    }
}


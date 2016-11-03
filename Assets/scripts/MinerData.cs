﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class MinerData {

    public static int XCOUNT = 26;
    public static int YCOUNT = 115;

    public static int CANDLERANGE = 8;
    public static int SUNRANGE = 15;

    public static int LVL2 = 50;
    public static int LVL3 = 150;
    public static int LVL4 = 300;
    public static int LVL5 = 500;
    public static int LVL6 = 750;
    public static int LVL7 = 1050;
    public static int LVL8 = 1400;
    public static int LVL9 = 1800;

    public static int MAXLVL = 9;

    public DateTime? SaveDate { get; set; }

    public float Health { get; set; }
    public float FoodLevel { get; set; }
    public float Moral { get; set; }

    public int Money { get; set; }
    public int Experience { get; set; }
    public float DayTime { get; set; }
    public int Day { get; set; }
    public int Level { get; set; }

    public float Speed { get; set; }
    public float MaxHealth { get; set; }
    public int NextLevelExperience { get; set; }

    public Rock[,] Rocks { get; set; }

    public bool Loaded { get; set; }

    public bool HasData { get; set; }

    public int Slot { get; set; }

    public float X { get; set; }
    public float Y { get; set; }

    public float ElevatorX { get; set; }
    public float ElevatorY { get; set; }

    public List<InventoryItem> Inventory { get; set; }

    public List<InventoryItem> Candles { get; set; }

    public MinerData(int slot)
    {
        Slot = slot;
        Reset();
    }

    public void Reset()
    {
        Health = 100;
        FoodLevel = 100;
        Moral = 100;
        Money = 100;
        Experience = 0;
        Day = 1;
        DayTime = 0;
        Rocks = new Rock[XCOUNT, YCOUNT];
        HasData = false;
        Loaded = false;
        X = 307.5f;
        Y = 10f;
        ElevatorX = 337.5f;
        ElevatorY = 10f;
        Speed = 3.5f;
        MaxHealth = 100f;
        Level = 1;
        NextLevelExperience = MinerData.LVL2;
        Debug.Log("Resetting");
        SaveDate = null;
        FillInventory();
    }

    public int GetExperienceByLevel(int level)
    {
        switch(level)
        {
            case 1: return 0;
            case 2: return LVL2;
            case 3: return LVL3;
            case 4: return LVL4;
            case 5: return LVL5;
            case 6: return LVL6;
            case 7: return LVL7;
            case 8: return LVL8;
            case 9: return LVL9;
            default: return 0;
        }
    }

    private void FillInventory()
    {
        Inventory = new List<InventoryItem>();
        Candles = new List<InventoryItem>();
        AddInventoryItem("shovel", true);
        AddInventoryItem("pick", true);
        AddInventoryItem("torch", true);
        for (var ii = 1; ii <= 5; ii++)
        {
            AddInventoryItem("apple", false);
        }
        for (var ii = 1; ii <= 30; ii++)
        {
            AddInventoryItem("candle", false);
        }
    }

    public void AddInventoryItem(string type, bool equip = false)
    {
        InventoryItem newItem = null;
        newItem = (from item in Inventory where item.Type == type select item).FirstOrDefault();
        if (newItem == null)
        {
            newItem = new InventoryItem();
            newItem.Type = type;
            if (equip)
                Equip(newItem);
            else
                UnEquip(newItem);
            Inventory.Add(newItem);
        }
        //Debug.Log("Adding Item " + newItem.Type);
        ++newItem.Amount;
    }

    public void Equip(InventoryItem itemToEquip)
    {
        itemToEquip.IsEquipped = true;
        itemToEquip.Position = -1;
    }

    public void UnEquip(InventoryItem itemToEquip)
    {
        itemToEquip.IsEquipped = false;
        SetInventoryPosition(itemToEquip);
    }

    public void SetInventoryPosition(InventoryItem itemToPosition)
    {
        var arrFilled = from item in Inventory where !item.IsEquipped && item != itemToPosition select item.Position;
        for(int ii = 0; ii < 20; ii++)
        {
            if (!arrFilled.Contains(ii))
            {
                itemToPosition.Position = ii;
                break;
            }
        }
    }

    public void setRock(int x, int y, Rock rock)
    {
        Rocks[x, y] = rock;
    }

    [Serializable]
    public class Rock
    {
        public string Type { get; set; }
        public string AfterType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Health { get; set; }
        public EnemyType EnemyType { get; set; }
        public int EnemyHealth { get; set; }
        public EnemyState EnemyState { get; set; }
        public float EnemyX { get; set; }
        public float EnemyY { get; set; }
        public float EnemyTargetX { get; set; }
        public float EnemyTargetY { get; set; }
        public float CounterStart { get; set; }
        public float CounterInterval { get; set; }
    }
}

public enum EnemyState
{
    None, Eyes, Walking, Hidden
}

public enum EnemyType
{
    None, MudGolem
}

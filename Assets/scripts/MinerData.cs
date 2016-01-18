using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class MinerData {

    public static int XCOUNT = 26;
    public static int YCOUNT = 115;

    public float Health { get; set; }
    public float FoodLevel { get; set; }
    public float O2 { get; set; }

    public int Money { get; set; }
    public int Experience { get; set; }
    public float DayTime { get; set; }
    
    public Rock[,] Rocks { get; set; }

    public bool Loaded { get; set; }

    public bool HasData { get; set; }

    public MinerData()
    {
        Health = 100;
        FoodLevel = 100;
        O2 = 100;
        Money = 100;
        Experience = 0;
        DayTime = 0;
        Rocks = new Rock[XCOUNT, YCOUNT];
        Loaded = true;
    }

    public void setRock(int x, int y, Rock rock)
    {
        Rocks[x, y] = rock;
    }

    [Serializable]
    public class Rock
    {
        public string Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Health { get; set; }
    }
}

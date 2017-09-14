using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class DatabaseItem
{
    public string Name { get; set; }
    public int BuyValue { get; set; }
    public int SellValue { get; set; }
    public int Stack { get; set; }
    public int MinBuyValue { get; set; }
    public int MaxBuyValue { get; set; }
    public int MinSellValue { get; set; }
    public int MaxSellValue { get; set; }
    public float ShopChance { get; set; }
    public int MinShopAmount { get; set; }
    public int MaxShopAmount { get; set; }
}

public class DropChance
{
    public string ItemName { get; set; }
    public int Chance { get; set; }
}

public class DatabaseEnemy
{
    public string Name { get; set; }
    public int Life { get; set; }
    public int Walk { get; set; }
    public int Run { get; set; }
    public int DamageMin { get; set; }
    public int DamageMax { get; set; }
    public int SleepMin { get; set; }
    public int SleepMax { get; set; }
    public int SleepChance { get; set; }
    public int ViewDay { get; set; }
    public int ViewNight { get; set; }
    public int LevelMin { get; set; }
    public int LevelMax { get; set; }
    public int NoiseRadius { get; set; }
    public int EarRadius { get; set; }
    public bool Poison { get; set; }
    public List<DropChance> DropChances { get; set; }
    public int Experience { get; set; }
}

public class Database
{
    public static Dictionary<string, DatabaseItem> ItemList = new Dictionary<string, DatabaseItem>()
    {
        { "apple", new DatabaseItem() {
            Name = "Apple",
            MinBuyValue = 18,
            MaxBuyValue = 35,
            BuyValue = 15,
            MinSellValue = 8,
            MaxSellValue = 12,
            SellValue = 5,
            Stack = 5
            }
        },
        { "apple golden", new DatabaseItem() {
            Name = "Golden Apple",
            MinBuyValue = 350,
            BuyValue = 400,
            MaxBuyValue = 490,
            MinSellValue = 130,
            SellValue = 150,
            MaxSellValue = 180,
            ShopChance = 5,
            Stack = 5,
            MinShopAmount = 1,
            MaxShopAmount = 5
            }
        },
        { "candle", new DatabaseItem() {
            Name = "Candle",
            MinBuyValue = 20,
            MaxBuyValue = 35,
            BuyValue = 25,
            MinSellValue = 8,
            MaxSellValue = 17,
            SellValue = 5,
            Stack = 20
            }
        },
        { "torch", new DatabaseItem() {
            Name = "Torch",
            BuyValue = 150,
            SellValue = 35,
            Stack = 5
            }
        },
        { "shovel", new DatabaseItem() {
            Name = "Shovel",
            BuyValue = 0,
            SellValue = 0,
            Stack = 1
            }
        },
        { "pick", new DatabaseItem() {
            Name = "Pick",
            BuyValue = 0,
            SellValue = 0,
            Stack = 1
            }
        },
        { "copper", new DatabaseItem() {
            Name = "Copper",
            SellValue = 25,
            MinSellValue = 6,
            MaxSellValue = 12,
            Stack = 30
            }
        },
        { "gem", new DatabaseItem() {
            Name = "Gem",
            SellValue = 250,
            MinSellValue = 45,
            MaxSellValue = 62,
            Stack = 20
            }
        },
        { "gold", new DatabaseItem() {
            Name = "Gold",
            SellValue = 60,
            MinSellValue = 32,
            MaxSellValue = 38,
            Stack = 30
            }
        },
        { "silver", new DatabaseItem() {
            Name = "Silver",
            SellValue = 45,
            MinSellValue = 15,
            MaxSellValue = 22,
            Stack = 30
            }
        },
        { "platinum", new DatabaseItem() {
            Name = "Platinum",
            SellValue = 90,
            MinSellValue = 24,
            MaxSellValue = 30,
            Stack = 30
            }
        }
    };
    public static Dictionary<string, DatabaseEnemy> EnemyList = new Dictionary<string, DatabaseEnemy>()
    {
        { "mud golem", new DatabaseEnemy() {
            Name = "Mud Golem",
            Life = 2,
            Walk = 3,
            Run = 4,
            DamageMin = 10,
            DamageMax = 12,
            SleepMin = 180,
            SleepMax = 240,
            SleepChance = 1,
            ViewDay = 5,
            ViewNight = 3,
            LevelMin = 5,
            LevelMax = 30,
            NoiseRadius = 4,
            EarRadius = 2,
            Poison = true,
            Experience = 50,
            DropChances = new List<DropChance>()
            {
                new DropChance()
                {
                    Chance = 25,
                    ItemName = "geode"
                }
            }
            }
        },
        { "copper golem", new DatabaseEnemy() {
            Name = "Copper Golem",
            Life = 5,
            Walk = 2,
            Run = 4,
            DamageMin = 17,
            DamageMax = 20,
            SleepMin = 180,
            SleepMax = 360,
            SleepChance = 2,
            ViewDay = 6,
            ViewNight = 4,
            LevelMin = 5,
            LevelMax = 40,
            NoiseRadius = 6,
            EarRadius = 2,
            Poison = false,
            Experience = 80,
            DropChances = new List<DropChance>()
            {
                new DropChance()
                {
                    Chance = 100,
                    ItemName = "copper"
                }
            }
            }
        }
    };
}

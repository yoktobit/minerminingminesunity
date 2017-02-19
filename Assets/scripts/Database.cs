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
}

public class Database
{
    public static Dictionary<string, DatabaseItem> ItemList = new Dictionary<string, DatabaseItem>()
    {
        { "apple", new DatabaseItem() {
            Name = "Apple",
            BuyValue = 15,
            SellValue = 5,
            Stack = 5
            }
        },
        { "candle", new DatabaseItem() {
            Name = "Candle",
            BuyValue = 25,
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
            Stack = 30
            }
        },
        { "gem", new DatabaseItem() {
            Name = "Gem",
            SellValue = 250,
            Stack = 20
            }
        },
        { "gold", new DatabaseItem() {
            Name = "Gold",
            SellValue = 60,
            Stack = 30
            }
        },
        { "silver", new DatabaseItem() {
            Name = "Silver",
            SellValue = 45,
            Stack = 30
            }
        },
        { "platinum", new DatabaseItem() {
            Name = "Platinum",
            SellValue = 90,
            Stack = 30
            }
        }
    };
}

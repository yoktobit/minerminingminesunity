using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RockGroupBehaviour : MonoBehaviour {

    public List<Sprite> spriteCollection;
    public Transform invisibleWallTemplate;
    public Transform enemyTemplate;

    void setRocks()
    {
        var topRockTemplate = this.gameObject.transform.GetChild(0);

        for (var xx = 0; xx < MinerData.XCOUNT; ++xx)
        {
            var topRockCopy = Instantiate(topRockTemplate);
            topRockCopy.transform.position = new Vector3(xx * 15, 0);
            string spriteCount = ("" + ((xx % 6) + 1)).PadLeft(2, '0');
            var spriteName = "rocks/world top rocks ";
            if (xx == 22)
            {
                spriteName += "empty";
            }
            else
            {
                spriteName += spriteCount;
            }
            topRockCopy.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(spriteName);
        }
        topRockTemplate.gameObject.SetActive(false);

        var template = this.gameObject.transform.GetChild(1);
        var hardFactor = 10.0;
        var graniteFactor = 98.0;
        var graniteFactorOriginal = 98.0;
        var graniteFactorOriginal1 = 77.0;
        var endStoneFactor = 101.0;
        var arrTypes = new string[MinerData.XCOUNT];
        Random rnd = new Random();
        for (var xx = 0; xx < MinerData.XCOUNT; ++xx)
        {
            arrTypes[xx] = "light";
        }

        for (var yy = 0; yy < MinerData.YCOUNT; yy++)
        {
            var invisibleWall = Instantiate(invisibleWallTemplate, new Vector3(180, yy * -20 - 30), Quaternion.identity) as Transform;
            hardFactor = 10;
            graniteFactor = graniteFactorOriginal - (yy / 10.0);
            int xCave = Random.Range(0, MinerData.XCOUNT);
            int yRnd = Random.Range(0, 101);
            bool hasCave = yRnd < yy;
            for (var xx = 0; xx < MinerData.XCOUNT; xx++)
            {
                string type = "light";
                string afterType = "light empty";
                var rock = Instantiate(template);
                rock.name = "Rock_" + xx + "_" + yy;
                rock.SetParent(this.gameObject.transform);
                rock.transform.position = new Vector3(xx * 15, (yy * -20) - 10);

                if (!MinerSaveGame.Instance.Current.Loaded) // isFresh (also nicht aus Savegame)
                {
                    var randomNumber = Mathf.FloorToInt(Random.Range(0, 100f));
                    if (arrTypes[xx] == "hard")
                    {
                        hardFactor = 60;
                    }
                    if (arrTypes[xx] == "granite")
                    {
                        graniteFactor = graniteFactorOriginal1 - (yy / 10.0);
                    }
                    if (randomNumber < hardFactor)
                    {
                        type = "hard";
                        hardFactor = 60;
                    }
                    else
                    {
                        hardFactor = 10;
                    }

                    if (randomNumber > graniteFactor)
                    {
                        type = "granite";
                        graniteFactor = graniteFactorOriginal1 - (yy / 10.0);
                    }
                    else
                    {
                        graniteFactor = graniteFactorOriginal - (yy / 10.0);
                    }

                    if (yy == 96) endStoneFactor = 75;
                    if (yy == 97) endStoneFactor = 50;
                    if (yy == 98) endStoneFactor = 25;

                    if (randomNumber > endStoneFactor)
                    {
                        type = "endstone";
                    }

                    arrTypes[xx] = type;

                    if (xx == 22)
                    {
                        type = "light empty";
                    }
                    if (yy >= 99)
                    {
                        type = "endstone";
                    }

                    if (type == "light empty")
                    {
                        afterType = "light empty";
                    }
                    else
                    {
                        if (hasCave && xCave == xx)
                        {
                            afterType = "cave " + type;
                        }
                        else
                        {
                            afterType = type + " empty";
                        }
                    }
                    // Im Savegame speíchern
                    MinerData.Rock r = new MinerData.Rock();
                    r.Type = type;
                    r.AfterType = afterType;
                    r.X = xx;
                    r.Y = yy;
                    r.CounterInterval = 0;
                    r.CounterStart = 0;
                    r.EnemyHealth = 0;
                    r.EnemyState = EnemyState.None;
                    r.EnemyType = EnemyType.None;
                    MinerSaveGame.Instance.Current.setRock(xx, yy, r);
                }
                else
                {
                    type = MinerSaveGame.Instance.Current.Rocks[xx, yy].Type;
                }
                if (type.Contains("cave"))
                {
                    Transform enemy = CastEnemy(MinerSaveGame.Instance.Current.Rocks[xx, yy], true);
                }
                int randomImage = Random.Range(1, type.Contains("cave") ? 5 : 6);
                string strRandomImage = "" + randomImage;
                strRandomImage = strRandomImage.PadLeft(2, '0');
                string spriteName = "rocks/rock " + type;
                if (type.IndexOf("empty") < 0)
                {
                    spriteName += " " + strRandomImage;
                }
                rock.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(spriteName);
            } // xx
        } // yy
        MinerSaveGame.Instance.Current.Loaded = true;
        template.gameObject.SetActive(false);
    }

    public static void GetGridPosition(Vector3 position, bool plusOne, out int x, out int y)
    {
        x = -1;
        y = -1;
        x = (int)position.x / 15;
        y = (int)position.y == 10 ? -1 : Mathf.Abs((int)position.y / 20 + (plusOne ? 1 : 0));
    }

    public static Vector3 GetPosition(int x, int y, bool centerPivot = false)
    {
        Vector3 position = new Vector3(x * 15 + (centerPivot ? 7.5f : 0), (y * -20) - 10 - (centerPivot ? 10 : 0));
        return position;
    }

    public Transform CastEnemy(MinerData.Rock rock, bool fromSaveGame = false)
    {
        var enemy = Instantiate<Transform>(enemyTemplate);
        enemy.SetParent(this.transform.parent, false);
        enemy.GetComponent<SpriteRenderer>().sortingOrder = 10;
        enemy.GetComponent<EnemyBehaviour>().rock = rock;
        if (fromSaveGame)
        {
            Debug.Log("Enemy Cast from SaveGame into State " + rock.EnemyState);
            enemy.transform.position = new Vector3(rock.EnemyX, rock.EnemyY);
            enemy.GetComponent<EnemyBehaviour>().target = new Vector3(rock.EnemyTargetX, rock.EnemyTargetY);
            enemy.GetComponent<EnemyBehaviour>().SetState(rock.EnemyState);
        }
        else
        {
            Debug.Log("Enemy Cast New");
            enemy.transform.position = GetPosition(rock.X, rock.Y, true);
            enemy.GetComponent<EnemyBehaviour>().SetState(EnemyState.None);
        }
        return enemy;
    }

    // Use this for initialization
    void Start () {
        setRocks();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

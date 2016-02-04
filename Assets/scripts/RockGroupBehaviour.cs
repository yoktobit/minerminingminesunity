using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RockGroupBehaviour : MonoBehaviour {

    public List<Sprite> spriteCollection;
    public Transform invisibleWallTemplate;

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
            for (var xx = 0; xx < MinerData.XCOUNT; xx++)
            {
                string type = "light";
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
                    // Im Savegame speíchern
                    MinerData.Rock r = new MinerData.Rock();
                    r.Type = type;
                    r.X = xx;
                    r.Y = yy;
                    MinerSaveGame.Instance.Current.setRock(xx, yy, r);
                }
                else
                {
                    type = MinerSaveGame.Instance.Current.Rocks[xx, yy].Type;
                }
                int randomImage = Random.Range(1, 6);
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

	// Use this for initialization
	void Start () {
        setRocks();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

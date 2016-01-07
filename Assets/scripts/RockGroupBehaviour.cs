﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RockGroupBehaviour : MonoBehaviour {

    public List<Sprite> spriteCollection;

    void setRocks()
    {
        var topRockTemplate = this.gameObject.transform.GetChild(0);

        for (var xx = 0; xx < 26; ++xx)
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

        var template = this.gameObject.transform.GetChild(1);
        var hardFactor = 10.0;
        var graniteFactor = 98.0;
        var graniteFactorOriginal = 98.0;
        var graniteFactorOriginal1 = 77.0;
        var endStoneFactor = 101.0;
        var arrTypes = new string[26];
        Random rnd = new Random();
        for (var xx = 0; xx < 26; ++xx)
        {
            arrTypes[xx] = "light";
        }

        for (var yy = 0; yy < 115; yy++)
        {
            hardFactor = 10;
            graniteFactor = graniteFactorOriginal - (yy / 10.0);
            for (var xx = 0; xx < 26; xx++)
            {
                string type = "light";
                var rock = Instantiate(template);
                rock.name = "Rock_" + xx + "_" + yy;
                rock.SetParent(this.gameObject.transform);
                rock.transform.position = new Vector3(xx * 15, (yy * -20) - 10);

                if (true) // isFresh (also nicht aus Savegame)
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
                    int randomImage = Random.Range(1, 6);
                    string strRandomImage = ""+randomImage;
                    strRandomImage = strRandomImage.PadLeft(2, '0');
                    string spriteName = "rocks/rock " + type;
                    if (type.IndexOf("empty") < 0)
                    {
                        spriteName += " " + strRandomImage;
                    }
                    rock.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(spriteName);
                    Debug.Log("SpriteName " + spriteName);
                }
                //Input.register(xx, yy, newRock);
            }
        }
        template.gameObject.SetActive(false);
    }

	// Use this for initialization
	void Start () {
        /*var template = this.gameObject.transform.GetChild(0);
        for (var xx = 0; xx < 24; ++xx)
        {
            for (var yy = 0; yy < 100; ++yy)
            {
                var rock = Instantiate(template);
                rock.SetParent(this.gameObject.transform);
                rock.transform.position = new Vector3(xx * 15, yy * -20);
                if (xx == 22)
                {
                    rock.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("rocks/rock light empty");//spriteCollection[spriteCollection.Count - 1];
                }
            }
        }*/
        setRocks();

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

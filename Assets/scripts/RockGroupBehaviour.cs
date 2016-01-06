using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RockGroupBehaviour : MonoBehaviour {

    public List<Sprite> spriteCollection;

	// Use this for initialization
	void Start () {
        var template = this.gameObject.transform.GetChild(0);
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
        }

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

using UnityEngine;
using System.Collections;

public class InventoryBehaviour : MonoBehaviour {

    public GameObject inventoryTemplate;

	// Use this for initialization
	void Start () {
        inventoryTemplate = GameObject.Find("Inventory0") as GameObject;
        var background = GameObject.Find("InventoryBackground");
        inventoryTemplate.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        inventoryTemplate.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        Debug.Log(inventoryTemplate);
        int count = 0;
        for (int xx = 0; xx < 4; xx++)
        {
            for (int yy = 0; yy < 4; yy++)
            {
                ++count;
                if (xx == 0 && yy == 0) continue;
                GameObject newObject = Instantiate(inventoryTemplate, inventoryTemplate.transform.position, inventoryTemplate.transform.rotation) as GameObject;
                newObject.name = "Inventory" + count;
                Debug.Log(newObject);
                newObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.1f, 0.1f);// new Vector2(0.2f + 0.2f * xx, 0.85f - 0.2f * yy);
                newObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.1f, 0.1f);
                //newObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

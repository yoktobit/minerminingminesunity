using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class InventoryBehaviour : MonoBehaviour {

    public Transform inventoryTemplate;
    public Transform[] InventorySlots;

	// Use this for initialization
	void Start () {
        //inventoryTemplate = GameObject.Find("Inventory0") as GameObject;
        var background = transform;//.GetChild(0);
        //Debug.Log(background);
        int count = -1;
        InventorySlots = new Transform[20];
        for (int yy = 0; yy < 4; yy++)
        {
            for (int xx = 0; xx < 5; xx++) 
            {
                ++count;
                var newObject = Instantiate(inventoryTemplate) as Transform;
                newObject.SetParent(background, false);
                newObject.name = "Inventory" + count;
                newObject.GetComponent<RectTransform>().localPosition = new Vector2(-240f + 120f * xx, 180f - 120f * yy);
                //Debug.Log(newObject);
                //newObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.2f + 0.15f * xx, 0.87f - 0.15f * yy);
                //newObject.GetComponent<RectTransform>().anchorMax = newObject.GetComponent<RectTransform>().anchorMin;//new Vector2(0.2f + 0.2f * xx, 0.65f - 0.2f * yy);
                InventorySlots[count] = newObject;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        
    }
}

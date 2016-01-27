using UnityEngine;
using System.Collections;

public class InventoryBehaviour : MonoBehaviour {

    public Transform inventoryTemplate;

	// Use this for initialization
	void Start () {
        //inventoryTemplate = GameObject.Find("Inventory0") as GameObject;
        var background = transform;//.GetChild(0);
        Debug.Log(background);
        int count = -1;
        for (int yy = 0; yy < 4; yy++)
        {
            for (int xx = 0; xx < 5; xx++) 
            {
                ++count;
                var newObject = Instantiate(inventoryTemplate) as Transform;
                newObject.SetParent(background, false);
                newObject.name = "Inventory" + count;
                Debug.Log(newObject);
                //newObject.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 20f, 100f);
                //newObject.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0f, 10f);
                //newObject.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0f, 10f);
                //newObject.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0.2f, 10f);
                //newObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100f);
                //newObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100f);
                //new Vector2(0.2f + 0.2f * xx, 0.85f - 0.2f * yy);
                newObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.2f + 0.15f * xx, 0.85f - 0.15f * yy);
                newObject.GetComponent<RectTransform>().anchorMax = newObject.GetComponent<RectTransform>().anchorMin;//new Vector2(0.2f + 0.2f * xx, 0.65f - 0.2f * yy);
                Vector3[] vec = new Vector3[4];
                newObject.GetComponent<RectTransform>().GetLocalCorners(vec);
                Debug.Log(vec[0]);
                Debug.Log(vec[1]);
                Debug.Log(vec[2]);
                Debug.Log(vec[3]);

            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        
    }
}

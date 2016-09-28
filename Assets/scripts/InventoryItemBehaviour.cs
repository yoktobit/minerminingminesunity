using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class InventoryItemBehaviour : MonoBehaviour, IPointerClickHandler {

    public InventoryItem inventoryItem;

    public void OnPointerClick(PointerEventData eventData)
    {
        var rico = GameObject.FindGameObjectWithTag("Player");
        Debug.Log("InventoryItemClicked " + this.transform.name);
        rico.GetComponent<MinerRicoBehavior>().SelectInventoryItem(this.transform);
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

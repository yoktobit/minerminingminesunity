using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;

public class InventoryItemBehaviour : MonoBehaviour, IPointerClickHandler {

    public InventoryItem inventoryItem;

    public void OnPointerClick(PointerEventData eventData)
    {
        var rico = GameObject.FindGameObjectWithTag("Player");
        if (this.inventoryItem == null) return;
        Debug.Log("InventoryItemClicked " + this.transform.name);
        if (SceneManager.GetActiveScene().name == "game")
        {
            rico.GetComponent<MinerRicoBehavior>().SelectInventoryItem(this.transform);
        }
        else
        {
            rico.GetComponent<MinerRicoShopBehaviour>().SetSelection(this.transform);
        }
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

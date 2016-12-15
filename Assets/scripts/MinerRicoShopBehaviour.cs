﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class MinerRicoShopBehaviour : MonoBehaviour {

    public MinerData Data;

    public Transform ShopUi;
    public Transform LeftFrame;
    public Transform RightFrame;

    public Vector3 target;

    public Transform SelectedItem;

	// Use this for initialization
	void Start () {
        Data = MinerSaveGame.Instance.Current;
        target = transform.position;
	}

    private void FixedUpdate()
    {
        bool handled = false;
        UpdateInventory(ref handled);
        if (!handled)
        {
            UpdateWalk(ref handled);
        }
    }

    private void UpdateWalk(ref bool handled)
    {
        bool left = false, right = false, up = false, down = false;
        if (Input.GetAxis("Horizontal") < -0.1)
        {
            left = true;
        }
        else if (Input.GetAxis("Horizontal") > 0.1)
        {
            right = true;
        }
        else if (Input.GetAxis("Vertical") < -0.1)
        {
            down = true;
        }
        else if (Input.GetAxis("Vertical") > 0.1)
        {
            up = true;
        }
        if (right)
        {
            target = new Vector3(20, transform.position.y);//transform.position + new Vector3(15f, 0);
            transform.position = Vector3.MoveTowards(transform.position, target, 0.5f);
            if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("walking"))
            {
                GetComponent<Animator>().Play("walking");
            }
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else if (left)
        {
            target = new Vector3(-70, transform.position.y);//transform.position - new Vector3(15f, 0);
            transform.position = Vector3.MoveTowards(transform.position, target, 0.5f);
            if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("walking"))
            {
                GetComponent<Animator>().Play("walking");
            }
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            GetComponent<Animator>().Play("idle");
        }
    }

    public int selectedX, selectedY;
    public string selectedFrame = "Left";
    float lastInventoryHorz = 0, lastInventoryVert = 0;
    private void UpdateInventory(ref bool handled)
    {
        bool left, right, up, down;
        left = right = up = down = false;
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        if (Input.GetButtonUp("Submit"))
        {
            ShowShop();
            handled = true;
        }
        if (horz < 0 && (Mathf.Sign(horz) != Mathf.Sign(lastInventoryHorz) || lastInventoryHorz == 0))
        {
            left = true;
        }
        else if (horz > 0 && (Mathf.Sign(horz) != Mathf.Sign(lastInventoryHorz) || lastInventoryHorz == 0))
        {
            right = true;
        }
        else if (vert < 0 && (Mathf.Sign(vert) != Mathf.Sign(lastInventoryVert) || lastInventoryVert == 0))
        {
            down = true;
        }
        else if (vert > 0 && (Mathf.Sign(vert) != Mathf.Sign(lastInventoryVert) || lastInventoryVert == 0))
        {
            up = true;
        }
        else
        {
            // rest doesn't matter
        }
        if (left)
        {
            SetSelection(-1, 0);
            handled = true;
        }
        else if (right)
        {
            SetSelection(1, 0);
            handled = true;
        }
        else if (up)
        {
            SetSelection(0, -1);
            handled = true;
        }
        else if (down)
        {
            SetSelection(0, 1);
            handled = true;
        }
        lastInventoryHorz = horz;
        lastInventoryVert = vert;
        if (ShopUi.gameObject.activeSelf)
        {
            handled = true;
        }
    }

    private void SetSelection(int x, int y)
    {
        selectedX += x;
        selectedY += y;
        if (selectedX < 0)
        {
            if (selectedFrame == "Left")
            {
                selectedX = 0;
            }
            else
            {
                selectedX = 4;
                selectedFrame = "Left";
            }
        }
        if (selectedX > 4)
        {
            if (selectedFrame == "Left")
            {
                selectedX = 0;
                selectedFrame = "Right";
            }
            else
            {
                selectedX = 4;
            }
        }
        if (selectedY > 3) selectedY = 3;
        if (selectedY < 0) selectedY = 0;
        var go = GameObject.Find(string.Format("Inventory{2}_{0}_{1}", selectedX, selectedY, selectedFrame));
        if (go == null)
        {
            Debug.Log(string.Format("InventoryLeft_{0}_{1}", selectedX, selectedY) + " nicht gefunden");
        }
        SelectedItem = go == null ? null : go.transform;
        UpdateItemSelection();
    }

    private void UpdateItemSelection()
    {
        for (var ii = 0; ii < LeftFrame.childCount; ii++)
        {
            var child = LeftFrame.GetChild(ii);
            UpdateItemSelectionChild(child);
        }
        for (var ii = 0; ii < RightFrame.childCount; ii++)
        {
            var child = RightFrame.GetChild(ii);
            UpdateItemSelectionChild(child);
        }
    }

    void UpdateItemSelectionChild(Transform child)
    {
        if (child.name == "SellButton" || child.name == "BuyButton") return;
        if (child == SelectedItem)
        {
            if (child.GetComponent<Image>().sprite.name != "borderlargefilledselected")
                child.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilledselected");
        }
        else
        {
            if (child.GetComponent<Image>().sprite.name != "borderlargefilled")
                child.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilled");
        }
    }

    private void ShowShop()
    {
        ShopUi.gameObject.SetActive(true);
        SelectedItem = LeftFrame.Find("InventoryLeft_0_0");
        UpdateItemSelection();
    }

    // Update is called once per frame
    void Update () {
        
	}
}

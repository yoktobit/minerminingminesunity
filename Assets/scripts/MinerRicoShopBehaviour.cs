using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class MinerRicoShopBehaviour : MonoBehaviour {

    public enum Action
    {
        None, Leave, OpenShop
    }

    public MinerData Data;

    public Transform ShopUi;
    public Transform LeftFrame;
    public Transform RightFrame;
    public Transform SellButton;
    public Transform BuyButton;
    public Transform ActionButton;
    public Transform ConfirmDialogName;
    public Transform ConfirmDialog;

    public Vector3 target;

    public Transform SelectedItem;

	// Use this for initialization
	void Start () {
        Data = MinerSaveGame.Instance.Current;
        target = transform.position;
        FillShopInventory();
	}

    private void FixedUpdate()
    {
        bool handled = false;
        UpdateActionButton(ref handled);
        UpdateInventory(ref handled);
        if (!handled)
        {
            UpdateWalk(ref handled);
        }
    }

    public Action possibleAction = Action.None;
    private void UpdateActionButton(ref bool handled)
    {
        if (ShopUi.gameObject.activeSelf) return;
        if (transform.position.x >= 20 || transform.position.x <= -60)
        {
            ActionButton.gameObject.SetActive(true);
            if (transform.position.x >= 20)
            {
                possibleAction = Action.OpenShop;
                ActionButton.GetComponent<Animator>().Play("ui interact coin");
            }
            else if (transform.position.x <= -60)
            {
                possibleAction = Action.Leave;
                ActionButton.GetComponent<Animator>().Play("ui interact door");
            }
        }
        else
        {
            ActionButton.gameObject.SetActive(false);
        }
    }

    public void Submit()
    {
        if (possibleAction == Action.OpenShop)
        {
            ActionButton.GetComponent<Animator>().Stop();
            ActionButton.gameObject.SetActive(false);
            ShowShop();
        }
        else if (possibleAction == Action.Leave)
        {
            SceneManager.LoadScene("game");
        }
    }

    private void UpdateWalk(ref bool handled)
    {
        bool left = false, right = false, up = false, down = false, action = false, cancel = false;
        if (Input.GetAxis("Horizontal") < -0.1)
        {
            left = true;
        }
        else if (Input.GetAxis("Horizontal") > 0.1)
        {
            right = true;
        }
        else if (Input.GetMouseButton(0) && Input.mousePosition.x < Screen.width * 0.25)
        {
            left = true;
        }
        else if (Input.GetMouseButton(0) && Input.mousePosition.x > Screen.width * 0.75)
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
        else if (Input.GetButtonUp("Submit"))
        {
            action = true;
        }
        else if (Input.GetButtonUp("Cancel"))
        {
            cancel = true;
        }
        if (right)
        {
            target = new Vector3(20, transform.position.y);//transform.position + new Vector3(15f, 0);
            transform.position = Vector3.MoveTowards(transform.position, target, 0.6f);
            if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("walking"))
            {
                GetComponent<Animator>().Play("walking");
            }
            GetComponent<SpriteRenderer>().flipX = true;
            handled = true;
        }
        else if (left)
        {
            target = new Vector3(-70, transform.position.y);//transform.position - new Vector3(15f, 0);
            transform.position = Vector3.MoveTowards(transform.position, target, 0.6f);
            if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("walking"))
            {
                GetComponent<Animator>().Play("walking");
            }
            GetComponent<SpriteRenderer>().flipX = false;
            handled = true;
        }
        else if (action)
        {
            Submit();
            handled = true;
        }
        else if (cancel)
        {
            handled = true;
        }
        else
        {
            GetComponent<Animator>().Play("idle");
        }
    }

    public int selectedX, selectedY;
    public string selectedFrame = "Left";
    float lastInventoryHorz = 0, lastInventoryVert = 0;
    public string state = "";
    public string selectedAction = "";
    public InventoryItem selectedInventoryItem;
    private void UpdateInventory(ref bool handled)
    {
        if (!ShopUi.gameObject.activeSelf) return;
        bool left, right, up, down, action, cancel;
        left = right = up = down = action = cancel = false;
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
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
        else if (Input.GetButtonUp("Cancel"))
        {
            cancel = true;
        }
        else if (Input.GetButtonUp("Submit"))
        {
            action = true;
        }
        else
        {
            // rest doesn't matter
        }
        if (state == "")
        {
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
            else if (cancel)
            {
                HideShop();
                handled = true;
            }
            else if (action)
            {
                Debug.Log("ItemSelected, going to Button");
                if (SelectedItem.GetComponent<InventoryItemBehaviour>().inventoryItem != null)
                {
                    if (selectedFrame == "Left")
                    {
                        UpdateButtonSelection(SellButton);
                    }
                    else
                    {
                        UpdateButtonSelection(BuyButton);
                    }
                    state = "ButtonSelected";
                }
            }
        }
        else
        {
            if (cancel)
            {
                UpdateButtonSelection(null);
                state = "";
            }
            else if (action)
            {
                HandleButton(selectedAction);
            }
        }
        lastInventoryHorz = horz;
        lastInventoryVert = vert;
        if (ShopUi.gameObject.activeSelf)
        {
            handled = true;
        }
    }

    public void HandleButton(string action)
    {
        if (action == "Sell")
        {
            TrySell();
        }
        else
        {
            TryBuy();
        }
        UpdateButtonSelection(null);
        state = "";
    }

    private void TryBuy()
    {
        throw new NotImplementedException();
    }

    private void TrySell()
    {
        ConfirmDialog.gameObject.SetActive(true);
        ConfirmDialogName.GetComponent<Text>().text = SelectedItem.GetComponent<InventoryItemBehaviour>().inventoryItem.Type;
    }

    private void FillInventory(string inventoryType = "Inventory")
    {
        int position = 0;
        var inventory = (inventoryType == "Inventory") ? MinerSaveGame.Instance.Current.Inventory : MinerSaveGame.Instance.Current.ShopInventory;
        var frameName = (inventoryType == "Inventory") ? "Left" : "Right";
        var frame = (inventoryType == "Inventory") ? LeftFrame : RightFrame;
        for (int yy = 0; yy < 4; yy++)
        {
            for (int xx = 0; xx < 5; xx++)
            {
                var slot = frame.FindChild("Inventory" + frameName + "_" + xx + "_" + yy);
                var text = slot.GetChild(0);
                var image = slot.GetChild(1);
                var item = (from i in inventory where i.Position == position select i).FirstOrDefault();
                if (item != null && item.Amount > 0)
                {
                    text.GetComponent<Text>().text = item.Amount.ToString();
                    image.GetComponent<Image>().sprite = Resources.Load<Sprite>("items/item " + item.Type);
                    slot.GetComponent<InventoryItemBehaviour>().inventoryItem = item;
                }
                else if (item != null && item.Amount <= 0)
                {
                    text.GetComponent<Text>().text = "";
                    image.GetComponent<Image>().sprite = Resources.Load("") as Sprite;
                    slot.GetComponent<InventoryItemBehaviour>().inventoryItem = null;
                }
                else
                {
                    text.GetComponent<Text>().text = "";
                    image.GetComponent<Image>().sprite = Resources.Load("") as Sprite;
                    slot.GetComponent<InventoryItemBehaviour>().inventoryItem = null;
                }
                ++position;
            }
        }
    }

    public void FillShopInventory()
    {
        if (MinerSaveGame.Instance.Current.ShopInventoryDay != MinerSaveGame.Instance.Current.Day)
        {
            MinerSaveGame.Instance.Current.FillShopInventory();
        }
    }

    public void SetSelection(Transform child)
    {
        string name = child.name;
        name = name.Replace("Inventory", "").Replace("Right_", "").Replace("Left_", "");
        string x = name.Substring(0, 1);
        string y = name.Substring(2, 1);
        selectedX = int.Parse(x);
        selectedY = int.Parse(y);
        SelectedItem = child;
        UpdateItemSelection();
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
            if (!child.name.Contains("Line"))
            {
                UpdateItemSelectionChild(child);
            }
        }
        for (var ii = 0; ii < RightFrame.childCount; ii++)
        {
            var child = RightFrame.GetChild(ii);
            if (!child.name.Contains("Line"))
            {
                UpdateItemSelectionChild(child);
            }
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

    public void UpdateButtonSelection(Transform child)
    {
        SellButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/barfilled");
        BuyButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/barfilled");
        if (child != null)
        {
            child.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/barfilledselected");
        }
        if (child == BuyButton)
        {
            selectedAction = "Buy";
        }
        else
        {
            selectedAction = "Sell";
        }
    }

    private void ShowShop()
    {
        ShopUi.gameObject.SetActive(true);
        ActionButton.gameObject.SetActive(false);
        SelectedItem = LeftFrame.Find("InventoryLeft_0_0");
        selectedX = 0;
        selectedY = 0;
        FillInventory();
        FillInventory("ShopInventory");
        UpdateItemSelection();
    }

    public void HideShop()
    {
        ShopUi.gameObject.SetActive(false);
        ActionButton.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update () {
        
	}
}

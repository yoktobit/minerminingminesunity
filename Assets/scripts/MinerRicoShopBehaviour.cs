using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using SmartLocalization;

public class MinerRicoShopBehaviour : MonoBehaviour {

    public enum Action
    {
        None, Leave, OpenShop
    }

    public MinerData Data;

    public Transform ShopUi;
    public Transform InGameMenu;
    public Transform LeftFrame;
    public Transform RightFrame;
    public Transform SellBuyButton;
    public Transform CancelButton;
    public Transform ConfirmSellBuyButton;
    public Transform ConfirmCancelButton;
    public Transform ActionButton;
    public Transform ConfirmDialog;
    public Transform SellBuyButtonText;
    public Transform ConfirmSellBuyButtonText;
    public Transform ConfirmSellBuyText;
    public Transform ConfirmCountText;
    public Transform ConfirmMoneyText;
    public Transform ConfirmImage;
    public Transform DetailCaption;
    public Transform DetailText;
    public Transform DetailImage;
    public Transform DetailMoney;
    public Transform DetailCoin;
    public Transform MinerMoneyText;
    public Transform PlusButton;
    public Transform MinusButton;

    public Vector3 target;

    public string cheatCode = "";

    public Transform SelectedItem;
    public Transform SelectedButton;

    public string sellorBuy;

	// Use this for initialization
	void Start () {
        Data = MinerSaveGame.Instance.Current;
        target = transform.position;
        LanguageManager languageManager = LanguageManager.Instance;
        SmartCultureInfo deviceCulture = languageManager.GetDeviceCultureIfSupported();
        //languageManager.OnChangeLanguage += OnChangeLanguage;
        languageManager.ChangeLanguage(deviceCulture);
        FillShopInventory();
        InvokeRepeating("save", 60, 60);
    }

    public void save()
    {
        MinerSaveGame.Save();
    }

    private void Update()
    {
        if (SceneManager.sceneCount > 1) return;
        if (InGameMenu.gameObject.activeSelf) return;
        bool handled = false;
        if (!handled)
            UpdateInGameMenu(ref handled);
        if (!handled)
            UpdateActionButton(ref handled);
        if (!handled)
            UpdateInventory(ref handled);
        if (!handled)
        {
            UpdateWalk(ref handled);
        }
        if (!handled)
        {
            UpdateCheatCodes(ref handled);
        }
    }

    private void UpdateInGameMenu(ref bool handled)
    {
        if (ShopUi.gameObject.activeSelf) return;
#if UNITY_ANDROID
        if (Input.GetKeyUp(KeyCode.Menu) || Input.GetButtonUp("Menu"))
#else
        if (Input.GetButtonUp("Menu"))
#endif
        {
            SceneManager.LoadSceneAsync("InGameMenu", LoadSceneMode.Additive);
            handled = true;
        }
        if (InGameMenu.gameObject.activeSelf) handled = true; // damit nur das Update vom Menü ausgewertet wird
    }

    void UpdateCheatCodes(ref bool handled)
    {
        if (Input.inputString != null && Input.inputString.Length > 0 && (cheatCode.Length == 0 || cheatCode.Substring(cheatCode.Length - 1, 1) != Input.inputString.Substring(0, 1)))
        {
            cheatCode += Input.inputString.Substring(0, 1);
            handled = true;
            Debug.Log(cheatCode);
        }
        CheckCheatCode();
    }

    void CheckCheatCode()
    {
        if (cheatCode.Contains("moneymoneymoney"))
        {
            MinerSaveGame.Instance.Current.Money += 50;
            MinerMoneyText.GetComponent<Text>().text = MinerSaveGame.Instance.Current.Money.ToString();
            cheatCode = "";
        }
        if (cheatCode.Contains("foster"))
        {
            Time.timeScale = 5f;
            cheatCode = "";
        }
        if (cheatCode.Contains("slower"))
        {
            Time.timeScale = 1f;
            cheatCode = "";
        }
        if (cheatCode.Length > 2000) cheatCode = "";
    }

    public Action possibleAction = Action.None;
    private void UpdateActionButton(ref bool handled)
    {
        if (ShopUi.gameObject.activeSelf) return;
        if (Time.timeSinceLevelLoad < 0.5f) return;
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
            possibleAction = Action.None;
            ActionButton.gameObject.SetActive(false);
        }
    }

    public void Submit()
    {
        if (possibleAction == Action.OpenShop)
        {
            ActionButton.GetComponent<Animator>().StopPlayback();
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
        if (ShopUi.gameObject.activeSelf) return;
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
            transform.position = Vector3.MoveTowards(transform.position, target, 25f * Time.smoothDeltaTime);
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
            transform.position = Vector3.MoveTowards(transform.position, target, 25f * Time.smoothDeltaTime);
            if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("walking"))
            {
                GetComponent<Animator>().Play("walking");
            }
            GetComponent<SpriteRenderer>().flipX = false;
            handled = true;
        }
        else if (action || up)
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
    public float repeatStart = 0.0f;
    public float keyStart = 0.0f;
    public float repeatTime = 0.4f;

    private void ResetKeyRepeat()
    {
        keyStart = Time.unscaledTime;
        repeatTime = 0.4f;
        repeatStart = Time.unscaledTime;
    }

    private void HandleKeyRepeat()
    {
        // Wiederholung
        if (Time.unscaledTime - repeatStart > repeatTime)
        {
            repeatStart = Time.unscaledTime;
            if (Time.unscaledTime - keyStart > 1)
            {
                repeatTime = 0.15f;
            }
            if (Time.unscaledTime - keyStart > 2)
            {
                repeatTime = 0.05f;
            }
            if (Time.unscaledTime - keyStart > 4)
            {
                repeatTime = 0.02f;
            }
        }
        // erster Tastendruck
        else
        {
            keyStart = Time.unscaledTime;
            repeatStart = Time.unscaledTime;
            repeatTime = 0.4f;
        }
    }

    private void UpdateInventory(ref bool handled)
    {
        if (!ShopUi.gameObject.activeSelf) return;
        bool left, right, up, down, action, cancel;
        left = right = up = down = action = cancel = false;
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        if (horz < 0 && ((Mathf.Sign(horz) != Mathf.Sign(lastInventoryHorz) || lastInventoryHorz == 0) || Time.unscaledTime - repeatStart > repeatTime))
        {
            if ((Mathf.Sign(horz) != Mathf.Sign(lastInventoryHorz) || lastInventoryHorz == 0)) ResetKeyRepeat();
            HandleKeyRepeat();
            left = true;
        }
        if (horz > 0 && ((Mathf.Sign(horz) != Mathf.Sign(lastInventoryHorz) || lastInventoryHorz == 0) || Time.unscaledTime - repeatStart > repeatTime))
        {
            if ((Mathf.Sign(horz) != Mathf.Sign(lastInventoryHorz) || lastInventoryHorz == 0)) ResetKeyRepeat();
            HandleKeyRepeat();
            right = true;
        }
        if (vert < 0 && ((Mathf.Sign(vert) != Mathf.Sign(lastInventoryVert) || lastInventoryVert == 0) || Time.unscaledTime - repeatStart > repeatTime))
        {
            if ((Mathf.Sign(vert) != Mathf.Sign(lastInventoryVert) || lastInventoryVert == 0)) ResetKeyRepeat();
            HandleKeyRepeat();
            down = true;
        }
        if (vert > 0 && ((Mathf.Sign(vert) != Mathf.Sign(lastInventoryVert) || lastInventoryVert == 0) || Time.unscaledTime - repeatStart > repeatTime))
        {
            if ((Mathf.Sign(vert) != Mathf.Sign(lastInventoryVert) || lastInventoryVert == 0)) ResetKeyRepeat();
            HandleKeyRepeat();
            up = true;
        }
        if (Input.GetButtonUp("Cancel"))
        {
            cancel = true;
        }
        if (Input.GetButtonUp("Submit"))
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
                var inventoryItem = SelectedItem.GetComponent<InventoryItemBehaviour>().inventoryItem;
                if (inventoryItem != null)
                {
                    DatabaseItem di = Database.ItemList[inventoryItem.Type];
                    if ((sellorBuy == "buy" && inventoryItem.BuyValue > 0) || (sellorBuy == "sell" && inventoryItem.SellValue > 0) )
                    {
                        ShowDetailDialog(true);
                    }
                    handled = true;
                }
            }
        }
        else if (state == "DetailDialog")
        {
            if (cancel)
            {
                ShowDetailDialog(false);
                handled = true;
            }
            else if (left || right)
            {
                SetButtonSelection(SelectedButton == SellBuyButton ? CancelButton : SellBuyButton);
                handled = true;
            }
            else if (action)
            {
                HandleButton(SelectedButton);
                handled = true;
            }
        }
        else if (state == "ConfirmDialog")
        {
            if (cancel)
            {
                ShowConfirmDialog(false);
                ShowDetailDialog(false);
                handled = true;
            }
            else if (left || right)
            {
                SetButtonSelection(SelectedButton == ConfirmSellBuyButton ? ConfirmCancelButton : ConfirmSellBuyButton);
                handled = true;
            }
            else if (up)
            {
                SetConfirmCountText(++confirmCount);
                handled = true;
            }
            else if (down)
            {
                SetConfirmCountText(--confirmCount);
                handled = true;
            }
            else if (action)
            {
                HandleButton(SelectedButton);
                handled = true;
            }
        }
        lastInventoryHorz = horz;
        lastInventoryVert = vert;
    }

    public void HandleButton(Transform selectedButton)
    {
        Debug.Log("Button Pressed: " + selectedButton.name);
        if (selectedButton == SellBuyButton)
        {
            ShowConfirmDialog(true);
        }
        else if (selectedButton == CancelButton)
        {
            state = "";
            SetButtonSelection(null);
            UpdateItemSelection();
        }
        else if (selectedButton == ConfirmCancelButton)
        {
            state = "";
            ShowConfirmDialog(false);
            UpdateItemSelection();
        }
        else if (selectedButton == ConfirmSellBuyButton)
        {
            if (TrySellBuy())
            {
                state = "";
                ShowConfirmDialog(false);
                UpdateItemSelection();
            }
        }
        else if (selectedButton == PlusButton)
        {
            SetConfirmCountText(++confirmCount);
        }
        else if (selectedButton == MinusButton)
        {
            SetConfirmCountText(--confirmCount);
        }
    }

    public bool IsSellBuyPossible()
    {
        var item = SelectedItem.GetComponent<InventoryItemBehaviour>().inventoryItem;
        var type = item.Type;
        var databaseItem = Database.ItemList[type];
        var total = this.confirmCount;
        var inventory = (sellorBuy == "sell") ? MinerSaveGame.Instance.Current.Inventory : MinerSaveGame.Instance.Current.ShopInventory;
        var otherInventory = (sellorBuy == "sell") ? MinerSaveGame.Instance.Current.ShopInventory : MinerSaveGame.Instance.Current.Inventory;

        // 1. zu teuer (nur beim Kaufen)
        Debug.Log(String.Format("{0} * {1} > {2}?", total, item.BuyValue, Data.Money));
        if (sellorBuy == "buy" && total * item.BuyValue > Data.Money) return false;
        
        // 2. kein Platz
        // Freie Plätze in belegten Slots
        var sumFreeInExisting = (from i in otherInventory where i.Type == type && i.Position >= 0 && i.Position < MinerSaveGame.Instance.Current.InventorySize select databaseItem.Stack - i.Amount).Sum();
        // Freie Plätze generell
        var sumFreeInEmpty = (MinerSaveGame.Instance.Current.InventorySize - (from i in otherInventory where i.Position >= 0 && i.Position < MinerSaveGame.Instance.Current.InventorySize && i.Amount > 0 select i).Count()) * databaseItem.Stack;
        var sumFree = sumFreeInExisting + sumFreeInEmpty;
        Debug.Log(String.Format("{0} + {1} = {2}", sumFreeInExisting, sumFreeInEmpty, sumFree));
        if (sumFree < total) return false;

        return true;
    }

    public bool TrySellBuy()
    {
        if (!IsSellBuyPossible()) return false;
        var item = SelectedItem.GetComponent<InventoryItemBehaviour>().inventoryItem;
        var type = item.Type;
        var databaseItem = Database.ItemList[item.Type];
        var total = this.confirmCount;
        if (sellorBuy == "buy" && total * item.BuyValue > Data.Money) return false;
        var sourceAmount = item.Amount - this.confirmCount;
        var inventory = (sellorBuy == "sell") ? MinerSaveGame.Instance.Current.Inventory : MinerSaveGame.Instance.Current.ShopInventory;
        var tmpItem = item;
        while (true)
        {
            this.confirmCount = Mathf.Abs(sourceAmount);
            tmpItem.Amount = Mathf.Clamp(sourceAmount, 0, 99);
            if (tmpItem.Amount <= 0)
            {
                tmpItem.Position = -1;
            }
            if (sourceAmount >= 0) break;
            tmpItem = (from i in inventory where i != tmpItem && i.Position >= 0 && i.Type == type select i).FirstOrDefault();
            if (tmpItem == null) break;
            sourceAmount = tmpItem.Amount - this.confirmCount;
        }
        
        if (sellorBuy == "sell")
        {
            var shopItem = MinerSaveGame.Instance.Current.ShopInventory.Where(ii => ii.Type == item.Type).FirstOrDefault();
            if (shopItem == null)
            {
                MinerSaveGame.Instance.Current.AddInventoryItem(item.Type, false, "ShopInventory");
                shopItem = MinerSaveGame.Instance.Current.ShopInventory.Where(ii => ii.Type == item.Type).FirstOrDefault();
                shopItem.Amount--;
            }
            var targetAmount = shopItem.Amount + total;
            targetAmount = Mathf.Clamp(targetAmount, 0, 99);
            shopItem.Amount = targetAmount;
            if (shopItem.Amount <= 0)
            {
                shopItem.Position = -1;
            }
            MinerSaveGame.Instance.Current.Money += total * item.SellValue;
        }
        else
        {
            for (int ii = 0; ii < total; ii++)
            {
                MinerSaveGame.Instance.Current.AddInventoryItem(item.Type, false);
            }
            MinerSaveGame.Instance.Current.Money -= total * item.BuyValue;
        }
        FillInventory();
        FillInventory("ShopInventory");
        return true;
    }

    public void ShowDetailDialog(bool show)
    {
        if (show)
        {
            SetButtonSelection(SellBuyButton);
            state = "DetailDialog";
        }
        else
        {
            SetButtonSelection(null);
            SetSelection(SelectedItem);
            state = "";
        }
    }

    public void SetSellOrBuy(string sellOrBuy)
    {
        CancelButton.GetChild(0).GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("Cancel");
        ConfirmCancelButton.GetChild(0).GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("Cancel");
        SellBuyButtonText.GetComponent<Text>().text = LanguageManager.Instance.GetTextValue(sellorBuy);
        ConfirmSellBuyButtonText.GetComponent<Text>().text = LanguageManager.Instance.GetTextValue(sellorBuy);
        ConfirmSellBuyText.GetComponent<Text>().text = LanguageManager.Instance.GetTextValue(sellorBuy);
    }

    int confirmCount;
    void SetConfirmCountText(int confirmCount)
    {
        var type = SelectedItem.GetComponent<InventoryItemBehaviour>().inventoryItem.Type;
        var inventory = (sellorBuy == "sell") ? MinerSaveGame.Instance.Current.Inventory : MinerSaveGame.Instance.Current.ShopInventory;
        var sumAmount = (from item in inventory where item.Type == type select item.Amount).Sum();
        this.confirmCount = Mathf.Clamp(confirmCount, 1, sumAmount);
        ConfirmCountText.GetComponent<Text>().text = this.confirmCount.ToString();
        var databaseItem = Database.ItemList[SelectedItem.GetComponent<InventoryItemBehaviour>().inventoryItem.Type];
        var money = sellorBuy == "sell" ? SelectedItem.GetComponent<InventoryItemBehaviour>().inventoryItem.SellValue : SelectedItem.GetComponent<InventoryItemBehaviour>().inventoryItem.BuyValue;
        var totalMoney = money * this.confirmCount;
        ConfirmMoneyText.GetComponent<Text>().text = totalMoney.ToString();
        if (!IsSellBuyPossible()/*sellorBuy == "buy" && totalMoney > Data.Money*/)
        {
            ConfirmMoneyText.GetComponent<Text>().color = Color.red;
            ConfirmCountText.GetComponent<Text>().color = Color.red;
        }
        else
        {
            ConfirmMoneyText.GetComponent<Text>().color = Color.black;
            ConfirmCountText.GetComponent<Text>().color = Color.black;
        }
    }

    public void ShowConfirmDialog(bool show)
    {
        if (show)
        {
            ConfirmDialog.gameObject.SetActive(true);
            ConfirmImage.GetComponent<Image>().sprite = DetailImage.GetComponent<Image>().sprite;
            SetConfirmCountText(1);
            state = "ConfirmDialog";
            SetButtonSelection(ConfirmSellBuyButton);
        }
        else
        {
            ConfirmDialog.gameObject.SetActive(false);
            state = "";
        }
    }
    
    private void FillInventory(string inventoryType = "Inventory")
    {
        int position = 0;
        MinerMoneyText.GetComponent<Text>().text = MinerSaveGame.Instance.Current.Money.ToString();
        var inventory = (inventoryType == "Inventory") ? MinerSaveGame.Instance.Current.Inventory : MinerSaveGame.Instance.Current.ShopInventory;
        var frameName = (inventoryType == "Inventory") ? "Left" : "Right";
        var frame = (inventoryType == "Inventory") ? LeftFrame : RightFrame;
        for (int yy = 0; yy < 4; yy++)
        {
            for (int xx = 0; xx < 5; xx++)
            {
                var slot = frame.Find("Inventory" + frameName + "_" + xx + "_" + yy);
                var text = slot.GetChild(0);
                var image = slot.GetChild(1);
                var item = (from i in inventory where i.Position == position select i).FirstOrDefault();
                DatabaseItem dbItem = null;
                if (item != null && Database.ItemList.ContainsKey(item.Type))
                {
                    dbItem = Database.ItemList[item.Type];
                }
                if (item != null && item.Amount > 0)
                {
                    text.GetComponent<Text>().text = item.Amount.ToString();
                    if (dbItem != null && item.Amount == dbItem.Stack && inventoryType == "Inventory")
                    {
                        text.GetComponent<Text>().color = new Color(0, 0.7f, 0);
                    }
                    else
                    {
                        text.GetComponent<Text>().color = Color.black;
                    }
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

    public void SetButtonSelection(Transform child)
    {
        SelectedButton = child;
        UpdateItemSelection(true);
        UpdateButtonSelection(child);
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
        ShowDetailDialog(true);
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

    private void UpdateItemSelection(bool deselectAll = false)
    {
        var temp = SelectedItem;
        if (deselectAll)
        {
            SelectedItem = null;
        }
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
        SelectedItem = temp;
        if (SelectedItem.name.Contains("Left"))
        {
            sellorBuy = "sell";
        }
        else
        {
            sellorBuy = "buy";
        }
        SetSellOrBuy(sellorBuy);
        SetDetails(SelectedItem);
    }

    public void SetDetails(Transform selectedItem)
    {
        bool found = false;
        if (selectedItem != null)
        {
            var ii = selectedItem.GetComponent<InventoryItemBehaviour>().inventoryItem;
            if (ii != null)
            {
                var di = Database.ItemList[ii.Type];
                Debug.Log("Type: " + ii.Type);
                if (di != null)
                {
                    Debug.Log("BuyValue " + ii.BuyValue);
                    var caption = LanguageManager.Instance.GetTextValue(di.Name);
                    DetailCaption.GetComponent<Text>().text = caption;
                    var desc = LanguageManager.Instance.GetTextValue(di.Name + "Desc");
                    //Debug.Log("Caption: " + caption + ", Desc: " + desc);
                    DetailText.GetComponent<Text>().text = desc;
                    DetailMoney.GetComponent<Text>().text = (sellorBuy == "sell" ? ii.SellValue.ToString() : (ii.BuyValue == 0 ? "-" : ii.BuyValue.ToString()));
                    if ((ii.BuyValue == 0 && sellorBuy == "buy") || (ii.SellValue == 0 && sellorBuy == "sell"))
                    {
                        SellBuyButton.gameObject.SetActive(false);
                        CancelButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        SellBuyButton.gameObject.SetActive(true);
                        CancelButton.gameObject.SetActive(true);
                    }
                    DetailImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("items/item " + ii.Type);
                    DetailImage.gameObject.SetActive(true);
                    found = true;
                }
            }
        }
        if (!found)
        {
            DetailCaption.GetComponent<Text>().text = "";
            DetailText.GetComponent<Text>().text = "";
            DetailMoney.GetComponent<Text>().text = "";
            DetailImage.GetComponent<Image>().sprite = null;
            DetailImage.gameObject.SetActive(false);
            SellBuyButton.gameObject.SetActive(false);
            CancelButton.gameObject.SetActive(false);
            DetailCoin.gameObject.SetActive(false);
        }
    }

    void UpdateItemSelectionChild(Transform child)
    {
        //if (child.name == "SellButton" || child.name == "BuyButton") return;
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
        SellBuyButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/barfilled");
        CancelButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/barfilled");
        ConfirmSellBuyButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/barfilled");
        ConfirmCancelButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/barfilled");
        if (child != null)
        {
            child.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/barfilledselected");
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

    void OnDestroy()
    {
        save();
    }
}

using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using SmartLocalization;

public class MinerRicoBehavior : MonoBehaviour {

    enum Action
    {
        Empty, Move, Pick, Shovel, Idle, Collect, NeedsWork, UseItem, EnterShop, EnterHome, ActionAborted
    }

    public const float BASIC_SPEED = 9.375f;
    public const float DAY_DURATION = 1200.0f;

    public bool isAnimated = false;
    bool moveElevator = false;
    bool inElevator = false;
    Vector3 target = Vector3.zero;

    public MinerData Data;

    Action oldAction = Action.Idle;
    Action requestedAction = Action.Empty;
    string oldOrientation;

    float workStartedTime;
    float estimatedWorkTime;
    MinerData.Rock workingRock;
    bool itemUseHandled = false;
    InventoryItem itemToUse;
    bool inventoryWasClosed = false;
    private float oldTimeScale;

    public GameObject foodBarInner;
    public GameObject foodBarText;
    public GameObject healthBarInner;
    public GameObject healthBarText;
    public GameObject moralBarInner;
    public GameObject moralBarText;
    public GameObject moral;
    public GameObject gameOver;
    public GameObject timeBarInner;
    public GameObject timeBarText;
    public GameObject sun;
    public GameObject inventory;
    public GameObject inventoryTop;
    public GameObject inventoryBackground;
    public GameObject inGameMenu;
    public GameObject elevatorLabel;
    public GameObject experienceBarInner;
    public GameObject experienceBarText;
    public GameObject moneyBarText;
    public GameObject level;
    public GameObject dayCount;
    public GameObject cuCount;
    public GameObject auCount;
    public GameObject agCount;
    public GameObject ptCount;
    public GameObject gemCount;
    public GameObject inventoryText;
    public Transform elevator;
    public GameObject[] arrSky;
    public List<Transform> resourcesToCollect;
    public Transform rockGroup;
    public Transform ActionButton;
    public Transform DetailImage;
    public Transform DetailText;
    public Transform DetailCaption;

    // Inventory
    float lastInventoryHorz = 0;
    float lastInventoryVert = 0;
    string inventoryState = "";
    public Transform activeInventoryItem;
    public int activeInventoryNumber = 0;
    string inventoryAction = "";

    void Start () {
        Data = MinerSaveGame.Instance.Current;
        Data.Migrate();
        Data.Paused = false;
        healthBarInner = GameObject.Find("HealthBarInner");
        healthBarText = GameObject.Find("HealthBarText");
        foodBarInner = GameObject.Find("FoodBarInner");
        foodBarText = GameObject.Find("FoodBarText");
        moralBarInner = GameObject.Find("MoralBarInner");
        moralBarText = GameObject.Find("MoralBarText");
        moral = GameObject.Find("Moral");
        gameOver = GameObject.Find("GameOver");
        timeBarInner = GameObject.Find("TimeBarInner");
        experienceBarInner = GameObject.Find("ExperienceBarInner");
        experienceBarText = GameObject.Find("ExperienceBarText");
        moneyBarText = GameObject.Find("MoneyBarText");
        sun = GameObject.Find("Sun");
        inventory = GameObject.Find("Inventory");
        inventoryTop = GameObject.Find("InventoryTop");
        inventoryBackground = GameObject.Find("InventoryBottom");
        inGameMenu = GameObject.Find("InGameMenu");
        elevatorLabel = GameObject.Find("ElevatorLabel");
        level = GameObject.Find("Level");
        dayCount = GameObject.Find("DayCount");
        /*cuCount = GameObject.Find("CuCount");
        agCount = GameObject.Find("AgCount");
        auCount = GameObject.Find("AuCount");
        ptCount = GameObject.Find("PtCount");
        gemCount = GameObject.Find("GemCount");*/
        inventoryText = GameObject.Find("InventoryText");

        arrSky = new GameObject[4];
        for (int ii = 0; ii < 4; ii++)
        {
            arrSky[ii] = GameObject.Find("world sky" + ii);
        }

        gameOver.SetActive(false);
        inventory.SetActive(false);
        inGameMenu.SetActive(false);

        UpdateBars();
        UpdateSun(true);
        HandleInventorySelection();
        UpdateExperienceBar();
        UpdateMoneyBar();
        UpdateInventoryText();

        transform.position = new Vector3(Data.X, Data.Y, transform.position.z);
        target = transform.position;
        elevator.transform.position = new Vector3(Data.ElevatorX, Data.ElevatorY, elevator.transform.position.z);

        InvokeRepeating("reduceFood", 12, 12);
        InvokeRepeating("UpdateHealth", 1, 1);
        InvokeRepeating("UpdateMoral", 1, 1);
        InvokeRepeating("save", 60, 60);

        LanguageManager languageManager = LanguageManager.Instance;
        SmartCultureInfo deviceCulture = languageManager.GetDeviceCultureIfSupported();
        //languageManager.OnChangeLanguage += OnChangeLanguage;
        languageManager.ChangeLanguage(deviceCulture);
    }

    void OnChangeLanguage(LanguageManager languageManager)
    {
        // braucht eigentlich gar nichts tun erstmal
    }

    void OnDestroy()
    {
        if (LanguageManager.HasInstance)
            LanguageManager.Instance.OnChangeLanguage -= OnChangeLanguage;
        save();
    }

    public void save()
    {
        if (Data.Paused) return;
        MinerSaveGame.Save();
    }

    public void UpdateBars()
    {
        //foodBarInner.GetComponent<RectTransform>().anchorMax = new Vector2(Mathf.Lerp(0.02f, 0.98f, Data.FoodLevel / 100.0f), 0.9f);
        foodBarInner.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Lerp(0f, 276f, Data.FoodLevel / 100.0f), 35f);
        string text = Math.Round(Data.FoodLevel) + "/100";
        if (foodBarText.GetComponent<Text>().text != text)
            foodBarText.GetComponent<Text>().text = text;
        healthBarInner.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Lerp(0f, 276f, Data.Health / Data.MaxHealth), 35f);
        text = Math.Round(Data.Health) + "/" + Data.MaxHealth;
        if (healthBarText.GetComponent<Text>().text != text)
            healthBarText.GetComponent<Text>().text = text;
        moralBarInner.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Lerp(0f, 276f, Data.Moral / 100.0f), 35f);
        text = Math.Round(Data.Moral) + "/100";
        if (moralBarText.GetComponent<Text>().text != text)
            moralBarText.GetComponent<Text>().text = text;
        if (Data.Moral < 20)
        {
            if (moral.GetComponent<Image>().sprite.name != "ui pic moral bad")
                moral.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/ui pic moral bad");
        }
        else if (Data.Moral < 50)
        {
            if (moral.GetComponent<Image>().sprite.name != "ui pic moral medium")
                moral.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/ui pic moral medium");
        }
        else
        {
            if (moral.GetComponent<Image>().sprite.name != "ui pic moral good")
                moral.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/ui pic moral good");
        }
    }

    public void reduceFood()
    {
        if (Data.Paused) return;
        --Data.FoodLevel;
        Data.FoodLevel = Mathf.Clamp(Data.FoodLevel, 0, 100);
        UpdateBars();
    }

    public void UpdateHealth()
    {
        if (Data.Paused) return;
        if (Data.FoodLevel <= 0)
        {
            --Data.Health;
            Data.Health = Mathf.Clamp(Data.Health, 0, Data.MaxHealth);
        }
        else if (Data.FoodLevel >= 80)
        {
            var regenerationRate = Mathf.Max(3.0f - 1.0f/(Data.MaxHealth / 3f) * Data.Health, 0.0f);
            Data.Health += regenerationRate / 2.0f;
            Data.Health = Mathf.Clamp(Data.Health, 0, Data.MaxHealth);
        }
        UpdateBars();
    }

    void UpdateSun(bool first = false)
    {
        Color preColor = timeBarInner.GetComponent<Image>().color;
        timeBarInner.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Lerp(0f, 276f, (Data.DayTime / 100.0f)), 35f);
        timeBarInner.GetComponent<Image>().color = Data.DayTime > 50.0f ? (Color)new Vector4(0.02f, 0, 0.2f, 1f) : Color.white;
        if (preColor != timeBarInner.GetComponent<Image>().color || first)
        {
            sun.GetComponent<SpriteRenderer>().sprite = Data.DayTime > 50.0f ? Resources.Load<Sprite>("world/world moon") : Resources.Load<Sprite>("world/world sun");
        }
        string dayTime = "" + Data.Day;
        if (dayCount.GetComponent<Text>().text != dayTime)
            dayCount.GetComponent<Text>().text = dayTime;
        // Durchsichtigkeit Nachthimmel
        float opacity = 0;
        float dauer = 12.5f;
        float anstieg = 100f / dauer;
        float stufe1 = dauer;
        float stufe2 = 100f - dauer;
        if (Data.DayTime < stufe1)
        {
            opacity = (Data.DayTime * -anstieg + 50) / 100f;
        }
        else if (Data.DayTime < stufe2)
        {
            opacity = (Data.DayTime * anstieg - (anstieg * (50 - dauer/2))) / 100f;
        }
        else
        {
            opacity = (Data.DayTime * -anstieg + (100 + anstieg * (100 - dauer/2))) / 100f;
        }
        var nightSky = GameObject.FindGameObjectsWithTag("SkyNight");
        foreach (GameObject nightSkySingle in nightSky)
        {
            nightSkySingle.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, Mathf.Clamp(opacity, 0, 1));
        }

        // elyptische Bahn der Sonne
        var x = Mathf.Lerp(-15f, 390f, (((Data.DayTime * 2f) % 100) / 100.0f));
        var y = (-1.0f / 450.0f) * Mathf.Pow(x - 180.0f, 2f) + 80f;
        sun.transform.position = new Vector2(x, y);
    }

    void UpdateExperienceBar()
    {
        if (Data.Level < MinerData.MAXLVL)
        {
            float newValue = Mathf.Lerp(0f, 276f, (float)(Data.Experience - Data.GetExperienceByLevel(Data.Level)) / (float)(Data.NextLevelExperience - Data.GetExperienceByLevel(Data.Level)));
            experienceBarInner.GetComponent<RectTransform>().sizeDelta = new Vector2(newValue, 35f);
            experienceBarText.GetComponent<Text>().text = (Data.Experience - Data.GetExperienceByLevel(Data.Level)) + "/" + (Data.NextLevelExperience - Data.GetExperienceByLevel(Data.Level));
            level.GetComponent<Text>().text = Data.Level.ToString();
        }
        else
        {
            experienceBarInner.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 35f);
            experienceBarText.GetComponent<Text>().text = "MAX";
            level.GetComponent<Text>().text = Data.Level.ToString();
        }
        /*cuCount.GetComponent<Text>().text = (from inv in Data.Inventory where inv.Type == "copper" select inv.Amount).Sum().ToString();
        auCount.GetComponent<Text>().text = (from inv in Data.Inventory where inv.Type == "gold" select inv.Amount).Sum().ToString();
        agCount.GetComponent<Text>().text = (from inv in Data.Inventory where inv.Type == "silver" select inv.Amount).Sum().ToString();
        ptCount.GetComponent<Text>().text = (from inv in Data.Inventory where inv.Type == "platinum" select inv.Amount).Sum().ToString();
        gemCount.GetComponent<Text>().text = (from inv in Data.Inventory where inv.Type == "gem" select inv.Amount).Sum().ToString();*/
    }

    void UpdateMoneyBar()
    {
        moneyBarText.GetComponent<Text>().text = Data.Money.ToString();
    }
	
    void UpdateDayTime()
    {
        if (Data.Day == 0) Data.Day = 1;
        Data.DayTime += Time.deltaTime * (100/(DAY_DURATION));
        if (Data.DayTime > 100)
        {
            Data.DayTime = 0.0f;
            Data.Day++;
        }
        UpdateSun(false);        
    }

    public void SelectInventoryItem(Transform inventoryItem)
    {
        var inventoryNumberString = inventoryItem.name.Replace("Inventory", "");
        var inventoryNumber = int.Parse(inventoryNumberString);
        activeInventoryNumber = inventoryNumber;
        if (inventoryTop.transform.GetChild(activeInventoryNumber + 1).transform.GetChild(1).GetComponent<Image>().sprite != null)
        {
            inventoryState = "selected";
            HandleInventorySelection();
        }
    }

    void HandleInventorySelection()
    {
        activeInventoryItem = inventoryTop.transform.GetChild(activeInventoryNumber + 1);
        for (int ii = 1; ii < inventoryTop.transform.childCount; ii++)
        {
            var current = inventoryTop.transform.GetChild(ii);
            if (current == activeInventoryItem)
            {
                current.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilledselected");
            }
            else
            {
                current.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilled");
            }
        }
        SetDetail(activeInventoryItem);
        if (inventoryState == "selected")
        {
            if (inventoryAction == "use")
            {
                inventoryBackground.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilledselectedgame");
                inventoryBackground.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilledgame");
            }
            else
            {
                inventoryBackground.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilledgame");
                inventoryBackground.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilledselectedgame");
            }
        }
        else
        {
            inventoryBackground.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilledgame");
            inventoryBackground.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilledgame");
        }
    }

    private void SetDetail(Transform activeInventoryItem)
    {
        bool found = false;
        if (activeInventoryItem != null)
        {
            var ii = activeInventoryItem.GetComponent<InventoryItemBehaviour>().inventoryItem;
            if (ii != null && ii.Type != null)
            {
                var di = Database.ItemList[ii.Type];
                Debug.Log("Type: " + ii.Type);
                if (di != null)
                {
                    Debug.Log("BuyValue " + di.BuyValue);
                    var caption = LanguageManager.Instance.GetTextValue(di.Name);
                    DetailCaption.GetComponent<Text>().text = caption;
                    var desc = LanguageManager.Instance.GetTextValue(di.Name + "Desc");
                    //Debug.Log("Caption: " + caption + ", Desc: " + desc);
                    DetailText.GetComponent<Text>().text = desc;
                    DetailImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("items/item " + ii.Type);
                    DetailImage.gameObject.SetActive(true);
                    found = true;
                }
                inventoryBackground.transform.GetChild(0).gameObject.SetActive(true); // USE
                inventoryBackground.transform.GetChild(1).gameObject.SetActive(true); // DROP
            }
            else
            {
                inventoryBackground.transform.GetChild(0).gameObject.SetActive(false); // USE
                inventoryBackground.transform.GetChild(1).gameObject.SetActive(false); // DROP
            }
        }
        if (!found)
        {
            DetailCaption.GetComponent<Text>().text = "";
            DetailText.GetComponent<Text>().text = "";
            DetailImage.GetComponent<Image>().sprite = null;
            DetailImage.gameObject.SetActive(false);
        }
    }

    void HandleInventory(ref bool isHandled)
    {
        if (Input.GetButtonUp("Inventory"))
        {
            SwitchInventory(false);
            isHandled = true;
            return;
        }
        bool left, right, up, down;
        left = right = up = down = false;
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        if (inventory.activeSelf)
        {
            if (Input.GetButtonUp("Cancel"))
            {
                if (inventoryState == "")
                {
                    SwitchInventory(false);
                }
                else if (inventoryState == "selected")
                {
                    inventoryState = "";
                }
                isHandled = true;
                return;
            }
            if (Input.GetButtonUp("Submit"))
            {
                if (inventoryState == "")
                {
                    if (activeInventoryItem.transform.GetChild(1).GetComponent<Image>().sprite != null)
                    {
                        inventoryState = "selected";
                        inventoryAction = "use";
                    }
                }
                else if (inventoryState == "selected")
                {
                    if (inventoryAction == "use")
                    {
                        HandleInventoryUse(false);
                    }
                    else if (inventoryAction == "drop")
                    {
                        HandleInventoryDrop(false);
                    }
                }
                isHandled = true;
                return;
            }
            if (horz < 0 && (Mathf.Sign(horz) != Mathf.Sign(lastInventoryHorz) || lastInventoryHorz == 0))
            {
                left = true;
            }
            if (horz > 0 && (Mathf.Sign(horz) != Mathf.Sign(lastInventoryHorz) || lastInventoryHorz == 0))
            {
                right = true;
            }
            if (vert < 0 && (Mathf.Sign(vert) != Mathf.Sign(lastInventoryVert) || lastInventoryVert == 0))
            {
                down = true;
            }
            if (vert > 0 && (Mathf.Sign(vert) != Mathf.Sign(lastInventoryVert) || lastInventoryVert == 0))
            {
                up = true;
            }
            else
            {
                // rest doesn't matter
            }
            if (inventoryState == "")
            {
                if (left) activeInventoryNumber--;
                else if (right) activeInventoryNumber++;
                else if (up) activeInventoryNumber -= 5;
                else if (down) activeInventoryNumber += 5;
                if (activeInventoryNumber < 0) activeInventoryNumber = 20 + activeInventoryNumber;
                activeInventoryNumber %= 20;
            }
            else if (inventoryState == "selected")
            {
                if (left) inventoryAction = inventoryAction == "use" ? "drop" : "use";
                else if (right) inventoryAction = inventoryAction == "use" ? "drop" : "use";
            }
            HandleInventorySelection();
        }
        lastInventoryHorz = horz;
        lastInventoryVert = vert;
    }

    public void HandleInventoryDrop(bool mouseUsed)
    {
        var item = (from i in Data.Inventory where i.Position == activeInventoryNumber select i).FirstOrDefault();
        if (item != null)
        {
            item.Amount = Math.Max(item.Amount - 1, 0);
            if (item.Amount <= 0)
            {
                item.Position = -1;
                inventoryState = "";
            }
        }
        UpdateExperienceBar();
        SwitchInventory(mouseUsed);
    }

    public void HandleInventoryUse(bool mouseUsed)
    {
        itemToUse = (from i in Data.Inventory where i.Position == activeInventoryNumber select i).FirstOrDefault();
        if (itemToUse != null && !isAnimated)
        {
            itemUseHandled = false;
            requestedAction = Action.UseItem;
            isAnimated = true;
            if (itemToUse.Type == "apple")
            {
                GetComponent<Animator>().Play("apple");
            }
            else if (itemToUse.Type == "apple golden")
            {
                GetComponent<Animator>().Play("apple golden");
            }
            else if (itemToUse.Type == "candle")
            {
                GetComponent<Animator>().Play("placing");
            }
            inventoryState = "";
        }
        SwitchInventory(mouseUsed);
    }

    public void SwitchInventory(bool mouseUsed)
    {
        inventory.SetActive(!inventory.activeSelf);
        inventoryState = "";
        UpdateInventory();
        if (mouseUsed)
            inventoryWasClosed = true;
    }

    private void UpdateInventoryText()
    {
        //inventoryText.GetComponent<Text>().text = "20/20";
        inventoryText.GetComponent<Text>().text = Data.Inventory.Count(i => i.Amount > 0 && i.Position >= 0).ToString() +  "/20";
    }

    private void UpdateInventory()
    {
        var slots = inventoryTop.GetComponent<InventoryBehaviour>().InventorySlots;
        
        for (int ii = 0; ii < 20; ii++)
        {
            var slot = slots[ii];
            var text = slot.GetChild(0);
            var image = slot.GetChild(1);
            var item = (from i in Data.Inventory where i.Position == ii select i).FirstOrDefault();
            DatabaseItem dbItem = null;
            if (item != null)
            {
                dbItem = Database.ItemList[item.Type];
            }
            if (item != null && item.Amount > 0)
            {
                text.GetComponent<Text>().text = item.Amount.ToString();
                image.GetComponent<Image>().sprite = Resources.Load<Sprite>("items/item " + item.Type);
                if (dbItem != null)
                {
                    if (item.Amount >= dbItem.Stack)
                    {
                        text.GetComponent<Text>().color = new Color(0, 0.7f, 0);
                    }
                    else
                    {
                        text.GetComponent<Text>().color = Color.black;
                    }
                }
            }
            else if (item != null && item.Amount <= 0)
            {
                text.GetComponent<Text>().text = "";
                image.GetComponent<Image>().sprite = Resources.Load("") as Sprite;
            }
            else
            {
                text.GetComponent<Text>().text = "";
                image.GetComponent<Image>().sprite = Resources.Load("") as Sprite;
            }
            slot.GetComponent<InventoryItemBehaviour>().inventoryItem = item;
        }
        UpdateInventoryText();
    }

    bool IsWalkable(int xx, int yy)
    {
        if (Data.Rocks[xx, yy].Type.Contains("empty") || Data.Rocks[xx, yy].Type.Contains("cave"))
        {
            return true;
        }
        return false;
    }

    public bool mouseOverRucksack = false;
    public void MouseEnterRucksack()
    {
        mouseOverRucksack = true;
    }
    public void MouseExitRucksack()
    {
        mouseOverRucksack = false;
    }

    public void Unpause()
    {
        Debug.Log("Unpause");
        Data.Paused = false;
    }

    // Update is called once per frame
    void Update () {

        if (Data.Paused) return;
        
        CheckIfAlive();
        if (gameOver.activeSelf) return;

        bool isHandled = false;

        HandleInGameMenu();
        HandleInventory(ref isHandled);
        if (isHandled) return;
        HandleActionButton();
        UpdateDayTime();
        CheckLevel();

        float step = Data.Speed * BASIC_SPEED * Time.smoothDeltaTime; // Movement Speed
        bool left = false, right = false, up = false, down = false, action = false;
        Vector3 targetElevator = elevator.transform.position;
        Vector3 currentTarget = target;
        bool arrived = false;
        bool workDone = false;
        bool itemUseDone = false;
        bool shouldWalk = false;
        bool hasWorked = (oldAction == Action.Pick || oldAction == Action.Shovel);//workingRock != null;
        bool hasUsedItem = oldAction == Action.UseItem;
        MinerData.Rock oldWorkingRock = workingRock;

        int pos = elevator.transform.position.y >= 0 ? 0 : Mathf.Abs((int)(elevator.transform.position.y + 10) / 20) + 1;
        if (elevatorLabel.GetComponent<UnityEngine.UI.Text>().text != pos.ToString())
            elevatorLabel.GetComponent<UnityEngine.UI.Text>().text = pos.ToString();
#if UNITY_EDITOR
        if (HasLineCave(pos - 1))
        {
            elevatorLabel.GetComponent<UnityEngine.UI.Text>().color = Color.yellow;
        }
        else
        {
            elevatorLabel.GetComponent<UnityEngine.UI.Text>().color = Color.red;
        }
#endif

        GetComponent<Animator>().SetFloat("walkingSpeed", (Data.Speed / 3.5f) * 0.7f);

        if (!inventory.activeSelf && !inventoryWasClosed && !inGameMenu.activeSelf)
        {
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
            else if (Input.GetButtonUp("Submit"))
            {
                action = true;
            }

            if (Input.GetMouseButton(0) && Input.mousePosition.x < Screen.width * 0.25 && !mouseOverRucksack)
            {
                left = true;
            }
            else if (Input.GetMouseButton(0) && Input.mousePosition.x > Screen.width * 0.75 && !mouseOverRucksack)
            {
                right = true;
            }
            else if (Input.GetMouseButton(0) && Input.mousePosition.y < Screen.height * 0.25 && !mouseOverRucksack)
            {
                down = true;
            }
            else if (Input.GetMouseButton(0) && Input.mousePosition.y > Screen.height * 0.75 && !mouseOverRucksack)
            {
                up = true;
            }
            if (Input.GetButtonUp("Torch"))
            {
                /*
                var torches = GameObject.FindGameObjectsWithTag("Torch");
                if (torches.Count() == 0)
                {
                    var torch = Instantiate(Resources.Load<Transform>("prefabs/Torch"));
                    torch.SetParent(this.transform, false);
                    //Debug.Log("Instantiating new torch " + torch.name);
                }
                else
                {
                    foreach(var torch in torches)
                    {
                        if (torch.name.Contains("Torch"))
                        {
                            Destroy(torch);
                        }
                    }
                }
                */
            }
        }
        if (isAnimated) // gucken, ob er mit irgendwas fertig ist
        {
            // Am Ziel angekommen?
            if (transform.position == target && oldAction == Action.Move)
            {
                //Debug.Log("arrived");
                arrived = true;
                Data.X = transform.position.x;
                Data.Y = transform.position.y;
                Data.ElevatorX = elevator.position.x;
                Data.ElevatorY = elevator.position.y;
            }

            // Fertig mit der Arbeit?
            if (Time.time - workStartedTime > estimatedWorkTime && hasWorked)
            {
                //Debug.Log("work done");
                workDone = true;
                workingRock = null;
            }

            // Kerze
            if (hasUsedItem && !itemUseHandled &&
                this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("placing") &&
                this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > (0.5f * this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).speed))
            {
                HandleItemUse();
            }

            // Apfel
            if (hasUsedItem && !itemUseHandled &&
                (this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("apple") 
                || this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("apple golden")
                ) &&
                this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > (0.6f * this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).speed))
            {
                HandleItemUse();
            }

            if (hasUsedItem &&
                !((this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("apple")
                ||(this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("apple golden")))
                || this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("placing")))
            {
                itemUseDone = true;
            }

            if (arrived || (workDone && hasWorked) || (hasUsedItem && itemUseDone))
            {
                isAnimated = false;
                if (moveElevator)
                {
                    elevator.transform.position = transform.position;
                    moveElevator = false;
                }
            }
        }
        if (!isAnimated)
        {
            moveElevator = false;
            if (left && transform.position.x > 8)
            {
                target = gameObject.transform.position + new Vector3(-15, 0);
                GetComponent<SpriteRenderer>().flipX = false;
                shouldWalk = true;
            }
            if (right && transform.position.x < (elevator.transform.position.y == transform.position.y ? 330 : 315))
            {
                target = transform.position + new Vector3(15, 0);
                GetComponent<SpriteRenderer>().flipX = true;
                shouldWalk = true;
            }
            if (down && transform.position.y > -2000 && (transform.position.y != 10 || inElevator))
            {
                target = transform.position + new Vector3(0, -20);
                if (transform.position.y == 10)
                {
                    target += new Vector3(0, -10);
                }
                if (inElevator)
                {
                    moveElevator = true;
                }
                shouldWalk = true;
            }
            if (up && transform.position.y < 10 && (transform.position.y != 10 || inElevator) && (transform.position.y != -20 || inElevator))
            {
                target = transform.position + new Vector3(0, 20);
                if (transform.position.y == -20)
                {
                    target += new Vector3(0, 10);
                }
                if (inElevator)
                {
                    moveElevator = true;
                }
                shouldWalk = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            inventoryWasClosed = false;
        }

        //int targetGridX = (int)target.x / 15;
        int targetGridX, targetGridY;
        RockGroupBehaviour.GetGridPosition(target, true, out targetGridX, out targetGridY);

        Action newAction = oldAction;

        MinerData.Rock newWorkingRock = null;

        if (arrived || workDone || itemUseDone)
        {
            //Debug.Log("Done, idling");
            newAction = Action.Idle;
        }

        if ((action || up) && transform.position.x == 7.5f + 15 * 10 && transform.position.y > 0)
        {
            newAction = Action.EnterShop;
        }

        if (shouldWalk) // kein else if, weil er sonst er stoppen würde und dann weiterlaufen, er soll aber direkt weiterlaufen
        {
            newAction = Action.Move;
            requestedAction = Action.Empty;
            CancelInvoke("SetNextActionCollect");
            Debug.Log("CancelInvoke wegen shouldwalk");
        }

        if (workDone)
        {
            HandleWorkDone(oldWorkingRock);
            HandleArrived();
        }
        if (arrived)
        {
            requestedAction = Action.Empty;
            CancelInvoke("SetNextActionCollect");
            Debug.Log("CancelInvoke wegen arrived");
            HandleArrived();
        }

        if (newAction == Action.Idle && requestedAction != Action.Empty)
        {
            newAction = requestedAction;
            requestedAction = Action.Empty;
        }
        
        // Wenn er laufen soll, schauen, ob er buddeln müsste
        if (newAction == Action.Move && targetGridX < MinerData.XCOUNT && targetGridY < MinerData.YCOUNT && targetGridY >= 0 && targetGridX >= 0 && Data.Rocks[targetGridX, targetGridY] != null)
        {
            newWorkingRock = Data.Rocks[targetGridX, targetGridY];
            if (!IsWalkable(targetGridX, targetGridY))
            {
                newAction = Action.NeedsWork;
            }
        }
        string orientation = oldOrientation;
        // Richtung ermitteln
        if (target.x < transform.position.x)
        {
            orientation = "side";
        }
        else if (target.x > transform.position.x)
        {
            orientation = "side";
        }
        else if (target.y < transform.position.y)
        {
            orientation = "down";
        }
        else if (target.y > transform.position.y)
        {
            orientation = "up";
        }

        if (newAction == Action.NeedsWork)
        {
            // Arbeitstyp ermitteln
            if (newWorkingRock.Type == "hard")
            {
                newAction = Action.Pick;
            }
            else if (newWorkingRock.Type == "light")
            {
                newAction = Action.Shovel;
            }
            else
            {
                // Da kann ich nicht hinlaufen, daher so tun, als komme ich jetzt am Ziel an
                // Erstmal jetzt nix tun, aber im nächsten Frame dann
                // issue #102
                newAction = Action.Idle;
                requestedAction = Action.ActionAborted;
            }
        }

        if (newAction == Action.ActionAborted)
        {
            // So tun, als kam ich gerade an, also eventuell das Einsammeln der Gegenstände planen
            CancelInvoke("SetNextActionCollect");
            HandleArrived();
            newAction = Action.Idle;
        }
        else if (newAction == Action.Collect)
        {
            if (newAction != oldAction)
            {
                GetComponent<Animator>().Play("placing");
                isAnimated = true;
                //Debug.Log("Playing placing animation");
            }
        }
        else if (newAction == Action.Idle || newAction == Action.NeedsWork)
        {
            target = currentTarget;
            if (newAction != oldAction)
            {
                //Debug.Log("Playing idle, Action = " + newAction);
                GetComponent<Animator>().Play("idle");
            }
        }
        else if (newAction == Action.Move)
        {
            // speed up elevator
            if (moveElevator) step *= 1.25f;
            transform.position = Vector3.MoveTowards(transform.position, target, step);

            if (moveElevator)
            {
                targetElevator = target;
                elevator.transform.position = Vector3.MoveTowards(elevator.transform.position, targetElevator, step);
            }
            if (moveElevator)
            {
                GetComponent<Animator>().Play("idle");
            }
            else if (newAction != oldAction || oldOrientation != orientation)
            {
                //Debug.Log("Playing walking");
                GetComponent<Animator>().Play("walking");
            }
            isAnimated = true;
        }
        else if (newAction == Action.Pick && (newAction != oldAction || oldOrientation != orientation))
        {
            target = currentTarget;
            //Debug.Log("Playing pick");
            GetComponent<Animator>().Play("pick " + orientation);
            workStartedTime = Time.time;
            estimatedWorkTime = 4.5f;
            workingRock = newWorkingRock;
            //Debug.Log(workingRock);
            isAnimated = true;
        }
        else if (newAction == Action.Shovel && (newAction != oldAction || oldOrientation != orientation))
        {
            target = currentTarget;
            //Debug.Log("Playing spade " + orientation);
            GetComponent<Animator>().Play("spade " + orientation);
            workStartedTime = Time.time;
            estimatedWorkTime = 2.25f;
            workingRock = newWorkingRock;
            //Debug.Log(workingRock);
            isAnimated = true;
        }
        else if (newAction == Action.EnterShop)
        {
            EnterShop();
        }
        oldAction = newAction;
        oldOrientation = orientation;
    }

    private void HandleActionButton()
    {
        if (ActionButton.gameObject.activeSelf && !ActionButton.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("ui interact door"))
        {
            ActionButton.GetComponent<Animator>().Play("ui interact door");
        }
        if (transform.position.x == 7.5f + 15 * 10 && transform.position.y > 0)
        {
            if (!ActionButton.gameObject.activeSelf)
            {
                ActionButton.gameObject.SetActive(true);
            }
        }
        else if (ActionButton.gameObject.activeSelf)
        {
            ActionButton.gameObject.SetActive(false);
        }
    }

    public void ActionButtonClicked()
    {
        if (Time.timeSinceLevelLoad < 0.5f) return;
        EnterShop();
    }

    public void EnterShop()
    {
        SceneManager.LoadScene("shop");
    }

    private void HandleItemUse()
    {
        itemUseHandled = true;
        if (itemToUse != null && itemToUse.Type == "candle")
        {
            if (rockGroup.GetComponent<RockGroupBehaviour>().CastCandle(this.transform, null))
            {
                itemToUse.Amount = Math.Max(itemToUse.Amount - 1, 0);
            }
        }
        else if (itemToUse != null && itemToUse.Type == "apple")
        {
            Data.FoodLevel = Math.Min(Data.FoodLevel + 60, 100);
            Data.Health = Math.Min(Data.Health + 10, Data.MaxHealth);
            itemToUse.Amount = Math.Max(itemToUse.Amount - 1, 0);
        }
        else if (itemToUse != null && itemToUse.Type == "apple golden")
        {
            Data.FoodLevel = Math.Min(Data.FoodLevel + 50, 100);
            Data.Health = Math.Min(Data.Health + 50, Data.MaxHealth);
            itemToUse.Amount = Math.Max(itemToUse.Amount - 1, 0);
        }
        if (itemToUse.Amount <= 0)
        {
            itemToUse.Position = -1;
        }
        itemToUse = null;
        UpdateInventory();
    }

    void CheckLightSource(int checkLightX, int checkLightY, int range, ref float looseMoral)
    {
        int lightX, lightY, x, y;
        //lightX = checkLightX;
        //lightY = checkLightY;
        RockGroupBehaviour.GetGridPosition(new Vector3((int)checkLightX, (int)checkLightY), false, out lightX, out lightY);
        RockGroupBehaviour.GetGridPosition(transform.position, false, out x, out y);
        //float distance = Vector2.Distance(new Vector2((float)x, (float)y / (2 / 3)), new Vector2((float)lightX, (float)lightY / (2 / 3)));
        float a2 = ((float)x - (float)lightX) * ((float)x - (float)lightX);
        float b2 = 3f/2f * (((float)y - (float)lightY) * ((float)y - (float)lightY));
        float distance = Mathf.Sqrt(a2 + b2);
        //if (range == MinerData.CANDLERANGE)
        //    Debug.Log(String.Format("{0};{1};{2}", a2, b2, distance));

        //Debug.Log(String.Format("Distance from {0},{1}/{3},{4}: {2}", checkLightX, checkLightY, distance, x, y));

        if (distance > range)
        {
            looseMoral = Mathf.Max(-2 - (y / MinerData.YCOUNT * 0.25f * 2f), looseMoral);
        }
        else if (distance > range - 2)
        {
            looseMoral = Mathf.Max(-1 - (y / MinerData.YCOUNT * 0.25f * 1f), looseMoral);
        }
        else if (distance < range * 0.5f)
        {
            looseMoral = Mathf.Max(3, looseMoral);
        }
        else
        {
            looseMoral = Mathf.Max(0, looseMoral);
        }
    }

    private void UpdateMoral()
    {
        if (Data.Paused) return;
        var lights = GameObject.FindGameObjectsWithTag("LightSource");
        float looseMoral = -5;
        if (transform.position.y > 0)
        {
            looseMoral = 3;
        }
        else
        {
            foreach (var light in Data.Candles)
            {
                CheckLightSource((int)light.X, (int)light.Y, MinerData.CANDLERANGE, ref looseMoral);
            }
            CheckLightSource(345, 0, MinerData.SUNRANGE, ref looseMoral);
        }
        Data.Moral += looseMoral;
        Data.Moral = Mathf.Clamp(Data.Moral, 0, 100);
        if (Data.Moral < 20)
        {
            Data.FoodLevel--;
            Data.FoodLevel = Mathf.Clamp(Data.FoodLevel, 0, 100);
        }
    }

    private void HandleCollect()
    {
        isAnimated = false;
        int gridX, gridY;
        RockGroupBehaviour.GetGridPosition(transform.position, true, out gridX, out gridY);
        var resourcesToCollectNow = (from r in resourcesToCollect where IsCollectable(r.GetComponent<ResourceBehaviour>(), gridX, gridY) select r).Take(2).ToList();
        foreach (var resource in resourcesToCollectNow)
        {
            var resB = resource.GetComponent<ResourceBehaviour>();
            Data.AddInventoryItem(resB.type, false);
            resourcesToCollect.Remove(resource);
            Destroy(resource.gameObject);
        }
        oldAction = Action.Idle;
        TryPlanCollect(0.5f);
        UpdateExperienceBar();
        UpdateInventoryText();
    }
    
    private void HandleInGameMenu()
    {
        if (inventory.activeSelf) return;
#if UNITY_ANDROID
        if (Input.GetKeyUp(KeyCode.Menu) || Input.GetButtonUp("Menu"))
#else
        if (Input.GetButtonUp("Menu"))
#endif
        {
            SceneManager.LoadScene("InGameMenu", LoadSceneMode.Additive);
            //inGameMenu.GetComponent<InGameMenuBehaviour>().oldTimeScale = Time.timeScale;
            //Time.timeScale = 0;
            Data.Paused = true;
            //inGameMenu.SetActive(!inGameMenu.activeSelf);
        }
    }

    public void HandleSubmitInGameMenuContinue()
    {
        //inGameMenu.SetActive(false);
    }

    public void HandleSubmitInGameMenuQuit()
    {
        Application.Quit();
    }

    public void GotoMainMenu()
    {
        MinerSaveGame.Save();
        SceneManager.LoadSceneAsync("MainMenu");
    }

    private int CalculateValue()
    {
        int value = 0;
        Data.Inventory.Where(ii => ii.Type == "copper").ToList().ForEach(ii => value += ii.Amount * 1);
        Data.Inventory.Where(ii => ii.Type == "silver").ToList().ForEach(ii => value += ii.Amount * 2);
        Data.Inventory.Where(ii => ii.Type == "gold").ToList().ForEach(ii => value += ii.Amount * 3);
        Data.Inventory.Where(ii => ii.Type == "platinum").ToList().ForEach(ii => value += ii.Amount * 4);
        Data.Inventory.Where(ii => ii.Type == "gem").ToList().ForEach(ii => value += ii.Amount * 6);
        return value;
    }

    private void CheckIfAlive()
    {
        if (gameOver == null) return;
        if (Data.Health <= 0)
        {
            if (!gameOver.activeSelf)
            {
                gameOver.SetActive(true);
                //Debug.Log(LanguageManager.Instance.CurrentlyLoadedCulture.nativeName);
                gameOver.transform.Find("SurvivedText").GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("GameOverSurvived").Replace("{DAYS}", Data.Day.ToString());
                gameOver.transform.Find("ValueText").GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("GameOverValue").Replace("{VALUE}", CalculateValue().ToString());
            }
        }
        else
        {
            if (gameOver.activeSelf)
                gameOver.SetActive(false);
        }
    }

    public void CheckLevel()
    {
        if (Data.Experience >= Data.NextLevelExperience && Data.Level < MinerData.MAXLVL)
        {
            Data.Level++;
            if (Data.Level == 2)
            {
                Data.MaxHealth = 110f;
                Data.Speed += Data.Speed * 0.05f;
                Data.NextLevelExperience = MinerData.LVL3;
            }
            else if (Data.Level == 3)
            {
                Data.MaxHealth = 120f;
                Data.Speed += Data.Speed * 0.05f;
                Data.NextLevelExperience = MinerData.LVL4;
            }
            else if (Data.Level == 4)
            {
                Data.MaxHealth = 130f;
                Data.Speed += Data.Speed * 0.04f;
                Data.NextLevelExperience = MinerData.LVL5;
            }
            else if (Data.Level == 5)
            {
                Data.MaxHealth = 140f;
                Data.Speed += Data.Speed * 0.04f;
                Data.NextLevelExperience = MinerData.LVL6;
            }
            else if (Data.Level == 6)
            {
                Data.MaxHealth = 150f;
                Data.Speed += Data.Speed * 0.03f;
                Data.NextLevelExperience = MinerData.LVL7;
            }
            else if (Data.Level == 7)
            {
                Data.MaxHealth = 160f;
                Data.Speed += Data.Speed * 0.03f;
                Data.NextLevelExperience = MinerData.LVL8;
            }
            else if (Data.Level == 8)
            {
                Data.MaxHealth = 170f;
                Data.Speed += Data.Speed * 0.02f;
                Data.NextLevelExperience = MinerData.LVL9;
            }
            else if (Data.Level == 9)
            {
                Data.MaxHealth = 180f;
                Data.Speed += Data.Speed * 0.02f;
                Data.NextLevelExperience = 999999999;
            }
        }

    }

    void EnterElevator()
    {
        inElevator = true;
    }
    void ExitElevator()
    {
        inElevator = false;
    }

    public void AddResourceToCollect(Transform resource)
    {
        if (!resourcesToCollect.Contains(resource))
        {
            resourcesToCollect.Add(resource);
        }
    }

    void SetNextActionCollect()
    {
        if (!isAnimated)
        {
            requestedAction = Action.Collect;
        }
    }

    bool IsCollectable(ResourceBehaviour rb, int gridX, int gridY)
    {
        if (rb.gridX != gridX) return false;
        if (rb.gridY != gridY) return false;
        HashSet<int> arrUsedPos = new HashSet<int>();
        foreach (var ii in Data.Inventory)
        {
            arrUsedPos.Add(ii.Position);
            //Debug.Log("Belegte Inventory Position " + ii.Position);
        }
        bool noSpace = false;
        //Debug.Log("Belegte Inventory Positionen " + arrUsedPos.Count + " von " + Data.InventorySize);
        if (arrUsedPos.Count > Data.InventorySize) noSpace = true;
        else
        {
            foreach (var pos in arrUsedPos)
            {
                if (pos > Data.InventorySize)
                {
                    noSpace = true;
                    break;
                }
            }
        }
        Debug.Log("NoSpace " + noSpace);
        if (noSpace)
        {
            bool hasSpace = false;
            foreach (var ii in Data.Inventory)
            {
                if (ii.Type == rb.type)
                {
                    if (ii.Amount < Database.ItemList[ii.Type].Stack)
                    {
                        hasSpace = true;
                        break;
                    }
                }
            }
            return hasSpace;
        }
        return true;
    }

    void TryPlanCollect(float inSeconds)
    {
        int gridX, gridY;
        RockGroupBehaviour.GetGridPosition(transform.position, true, out gridX, out gridY);
        var count = resourcesToCollect.Count(r => IsCollectable(r.GetComponent<ResourceBehaviour>(), gridX, gridY));
        if (count > 0)
        {
            Invoke("SetNextActionCollect", inSeconds);
            Debug.Log("SetNextActionCollect");
        }
    }

    void HandleArrived()
    {
        //Debug.Log("Found " + resourcesToCollect.Count + " resources to collect");
        requestedAction = Action.Empty;
        if (resourcesToCollect.Count > 0)
        {
            TryPlanCollect(0.5f);
        }
    }

    bool HasLineCave(int yy)
    {
        if (yy < 0) return false;
        for (var ii = 0; ii < 25; ++ii)
        {
            if (Data.Rocks != null && Data.Rocks[ii, yy] != null && Data.Rocks[ii,yy].AfterType.Contains("cave"))
            {
                return true;
            }
        }
        return false;
    }

    void HandleWorkDone(MinerData.Rock rock)
    {
        //Debug.Log("Begin HandleWorkDone");
        if (rock.Type == "light")
        {
            Data.Experience += 1;
        }
        else if (rock.Type == "hard")
        {
            Data.Experience += 2;
        }
        else if (rock.Type == "granite")
        {
            Data.Experience += 10;
        }

        rock.Type = rock.AfterType;
        if (rock.Type.Contains("cave"))
        {
            rock.CounterStart = Time.time;
            rock.EnemyState = EnemyState.None;
            rock.EnemyHealth = 100;
            rock.EnemyType = EnemyType.MudGolem;
            rockGroup.GetComponent<RockGroupBehaviour>().CastEnemy(rock);
        }
        
        var rockObject = GameObject.Find("Rock_" + rock.X + "_" + rock.Y);
        string spriteName = "rock " + rock.Type;
        if (rock.Type.Contains("cave"))
        {
            int rnd = UnityEngine.Random.Range(1, 4);
            spriteName += " " + rnd.ToString().PadLeft(2, '0');
        }
        //Debug.Log(spriteName);
        var spriteAtlasIndex = rockGroup.GetComponent<RockGroupBehaviour>().SpriteAtlas[spriteName];
        rockObject.GetComponent<SpriteRenderer>().sprite = rockGroup.GetComponent<RockGroupBehaviour>().spriteCollection[spriteAtlasIndex];//Resources.Load<Sprite>(spriteName);
        UpdateExperienceBar();

        TryCastMinerals(rock);
        //Debug.Log("End HandleWorkDone");
    }

    private void TryCastMinerals(MinerData.Rock rock)
    {
        // check if there is anything at all
        float x = rock.Y;
        float y = (8.0f / 10.0f) * x + 10.0f;
        int random = UnityEngine.Random.Range(0, 100);
        if (random < y)
        {
            int countMinerals = UnityEngine.Random.Range(0, 7);
            for (int ii = 0; ii < countMinerals; ii++)
            {
                CastMineral(rock);
            }
        }
    }

    public Transform resourceTemplate;

    void CastMineral(MinerData.Rock rock)
    {
        var xPos = UnityEngine.Random.Range(1, 15);
        var yPos = UnityEngine.Random.Range(1, 6);
        var what = UnityEngine.Random.Range(0, 101);
        var type = "gold";
        if (rock.Y < 11)
        {
            if (what < 75)
            {
                type = "copper";
            }
            else
            {
                type = "silver";
            }
        } 
        else if (rock.Y < 29)
        {
            if (what < 50)
            {
                type = "copper";
            }
            else if (what < 80)
            {
                type = "silver";
            }
            else
            {
                type = "gold";
            }
        }
        else if (rock.Y < 44)
        {
            if (what < 40)
            {
                type = "copper";
            }
            else if (what < 70)
            {
                type = "silver";
            }
            else if (what < 90)
            {
                type = "gold";
            }
            else
            {
                type = "platinum";
            }
        }
        else
        {
            if (what < 35)
            {
                type = "copper";
            }
            else if (what < 65)
            {
                type = "gold";
            }
            else if (what < 80)
            {
                type = "silver";
            }
            else if (what < 90)
            {
                type = "platinum";
            }
            else
            {
                type = "gem";
            }
        }
        var variant = UnityEngine.Random.Range(1, type == "gem" ? 8 : 5);
        var targetPos = new Vector3(rock.X * 15 + xPos, rock.Y * -20 + yPos - 20);
        //Debug.Log(String.Format("targetPos = {0}", targetPos));
        var newResource = Instantiate(resourceTemplate, targetPos, Quaternion.identity) as Transform;
        newResource.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("resources/resources " + type + " " + variant.ToString().PadLeft(2, '0'));
        newResource.GetComponent<ResourceBehaviour>().type = type;
        RockGroupBehaviour.GetGridPosition(targetPos, false, out newResource.GetComponent<ResourceBehaviour>().gridX, out newResource.GetComponent<ResourceBehaviour>().gridY);
        //Debug.Log("Resource " + type + " added with gridX = " + newResource.GetComponent<ResourceBehaviour>().gridX + " and gridY = " + newResource.GetComponent<ResourceBehaviour>().gridY);
        AddResourceToCollect(newResource);
    }
}

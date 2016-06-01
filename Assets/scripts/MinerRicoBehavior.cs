using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;

public class MinerRicoBehavior : MonoBehaviour {

    enum Action
    {
        Empty, Move, Pick, Shovel, Idle, Collect, NeedsWork
    }

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
    public GameObject foodBarInner;
    public GameObject healthBarInner;
    public GameObject moralBarInner;
    public GameObject moral;
    public GameObject gameOver;
    public GameObject timeBarInner;
    public GameObject sun;
    public GameObject inventory;
    public GameObject inventoryBackground;
    public GameObject inGameMenu;
    public GameObject elevatorLabel;
    public GameObject experienceBarInner;
    public GameObject level;
    public GameObject cuCount;
    public GameObject auCount;
    public GameObject agCount;
    public GameObject ptCount;
    public GameObject gemCount;
    public Transform elevator;
    public GameObject[] arrSky;
    public List<Transform> resourcesToCollect;
    public Transform rockGroup;

    // Inventory
    float lastInventoryHorz = 0;
    float lastInventoryVert = 0;
    string inventoryState = "";
    public Transform activeInventoryItem;
    public int activeInventoryNumber = 0;
    string inventoryAction = "";

    void Start () {
        Data = MinerSaveGame.Instance.Current;

        healthBarInner = GameObject.Find("HealthBarInner");
        foodBarInner = GameObject.Find("FoodBarInner");
        moralBarInner = GameObject.Find("MoralBarInner");
        moral = GameObject.Find("Moral");
        gameOver = GameObject.Find("GameOver");
        timeBarInner = GameObject.Find("TimeBarInner");
        experienceBarInner = GameObject.Find("ExperienceBarInner");
        sun = GameObject.Find("Sun");
        inventory = GameObject.Find("Inventory");
        inventoryBackground = GameObject.Find("InventoryBackground");
        inGameMenu = GameObject.Find("InGameMenu");
        elevatorLabel = GameObject.Find("ElevatorLabel");
        level = GameObject.Find("Level");
        cuCount = GameObject.Find("CuCount");
        agCount = GameObject.Find("AgCount");
        auCount = GameObject.Find("AuCount");
        ptCount = GameObject.Find("PtCount");
        gemCount = GameObject.Find("GemCount");

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

        transform.position = new Vector3(Data.X, Data.Y, transform.position.z);
        target = transform.position;
        elevator.transform.position = new Vector3(Data.ElevatorX, Data.ElevatorY, elevator.transform.position.z);

        InvokeRepeating("reduceFood", 12, 12);
        InvokeRepeating("reduceHealth", 1, 1);
        InvokeRepeating("UpdateMoral", 1, 1);
        InvokeRepeating("save", 60, 60);
	}

    void OnDestroy()
    {
        save();
    }

    public void save()
    {
        MinerSaveGame.Save();
    }

    public void UpdateBars()
    {
        foodBarInner.GetComponent<RectTransform>().anchorMax = new Vector2(Mathf.Lerp(0.02f, 0.98f, Data.FoodLevel / 100.0f), 0.9f);
        healthBarInner.GetComponent<RectTransform>().anchorMax = new Vector2(Mathf.Lerp(0.02f, 0.98f, Data.Health / Data.MaxHealth), 0.9f);
        moralBarInner.GetComponent<RectTransform>().anchorMax = new Vector2(Mathf.Lerp(0.02f, 0.98f, Data.Moral / 100.0f), 0.9f);
        if (Data.Moral < 20)
        {
            moral.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/ui pic moral bad");
        }
        else if (Data.Moral < 50)
        {
            moral.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/ui pic moral medium");
        }
        else
        {
            moral.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/ui pic moral good");
        }
    }

    public void reduceFood()
    {
        --Data.FoodLevel;
        if (Data.FoodLevel < 0)
        {
            Data.FoodLevel = 0;
        }
        UpdateBars();
    }

    public void reduceHealth()
    {
        if (Data.FoodLevel <= 0)
        {
            --Data.Health;
            if (Data.Health < 0) Data.Health = 0;
        }
        UpdateBars();
    }

    void UpdateSun(bool first = false)
    {
        Color preColor = timeBarInner.GetComponent<Image>().color;
        timeBarInner.GetComponent<RectTransform>().anchorMax = new Vector2(Mathf.Lerp(0.02f, 0.98f, (Data.DayTime / 100.0f)), 0.9f);
        timeBarInner.GetComponent<Image>().color = Data.DayTime > 50.0f ? Color.black : Color.white;
        if (preColor != timeBarInner.GetComponent<Image>().color || first)
        {
            sun.GetComponent<SpriteRenderer>().sprite = Data.DayTime > 50.0f ? Resources.Load<Sprite>("world/world moon") : Resources.Load<Sprite>("world/world sun");
            for (int ii = 0; ii < 4; ii++)
            {
                //arrSky[ii].GetComponent<SpriteRenderer>().sprite = Data.DayTime > 50.0f ? Resources.Load<Sprite>("world/world sky night") : Resources.Load<Sprite>("world/world sky day");
            }
        }
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
        float newValue = Mathf.Lerp(0.02f, 0.98f, (float)Data.Experience / (float)Data.NextLevelExperience);
        experienceBarInner.GetComponent<RectTransform>().anchorMax = new Vector2(newValue, 0.9f);
        level.GetComponent<Text>().text = Data.Level.ToString();
        cuCount.GetComponent<Text>().text = (from inv in Data.Inventory where inv.Type == "copper" select inv.Amount).Sum().ToString();
        auCount.GetComponent<Text>().text = (from inv in Data.Inventory where inv.Type == "gold" select inv.Amount).Sum().ToString();
        agCount.GetComponent<Text>().text = (from inv in Data.Inventory where inv.Type == "silver" select inv.Amount).Sum().ToString();
        ptCount.GetComponent<Text>().text = (from inv in Data.Inventory where inv.Type == "platinum" select inv.Amount).Sum().ToString();
        gemCount.GetComponent<Text>().text = (from inv in Data.Inventory where inv.Type == "gem" select inv.Amount).Sum().ToString();
    }
	
    void UpdateDayTime()
    {
        Data.DayTime += Time.deltaTime;
        Data.DayTime %= 100.0f;
        UpdateSun(false);        
    }

    void HandleInventorySelection()
    {
        activeInventoryItem = inventory.transform.GetChild(activeInventoryNumber + 1);
        for (int ii = 1; ii < inventory.transform.childCount; ii++)
        {
            var current = inventory.transform.GetChild(ii);
            if (current == activeInventoryItem)
            {
                current.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilledselected");
            }
            else
            {
                current.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilled");
            }
        }
        if (inventoryState == "selected")
        {
            if (inventoryAction == "use")
            {
                inventoryBackground.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilledselected");
                inventoryBackground.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilled");
            }
            else
            {
                inventoryBackground.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilled");
                inventoryBackground.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilledselected");
            }
        }
        else
        {
            inventoryBackground.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilled");
            inventoryBackground.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/borderlargefilled");
        }
    }

    void HandleInventory()
    {
        if (Input.GetButtonUp("Inventory"))
        {
            inventory.SetActive(!inventory.activeSelf);
            UpdateInventory();
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
                    inventory.SetActive(!inventory.activeSelf);
                }
                else if (inventoryState == "selected")
                {
                    inventoryState = "";
                }
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
                        HandleInventoryUse();
                    }
                    else if (inventoryAction == "drop")
                    {
                        HandleInventoryDrop();
                    }
                    inventory.SetActive(!inventory.activeSelf);
                    inventoryState = "";
                }
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

    private void HandleInventoryDrop()
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
        UpdateInventory();
    }

    private void HandleInventoryUse()
    {
        var item = (from i in Data.Inventory where i.Position == activeInventoryNumber select i).FirstOrDefault();
        if (item != null)
        {
            if (item.Type == "apple")
            {
                Data.FoodLevel = Math.Min(Data.FoodLevel + 40, 100);
                item.Amount = Math.Max(item.Amount - 1, 0);
            }
            else if (item.Type == "candle")
            {
                if (rockGroup.GetComponent<RockGroupBehaviour>().CastCandle(this.transform, null))
                {
                    item.Amount = Math.Max(item.Amount - 1, 0);
                }
            }
            else
            {
                Debug.Log(item.Type);
            }
            if (item.Amount <= 0)
            {
                item.Position = -1;
                inventoryState = "";
            }
        }
        UpdateInventory();
    }

    private void UpdateInventory()
    {
        var slots = inventory.GetComponent<InventoryBehaviour>().InventorySlots;
        
        for (int ii = 0; ii < 20; ii++)
        {
            var slot = slots[ii];
            var text = slot.GetChild(0);
            var image = slot.GetChild(1);
            var item = (from i in Data.Inventory where i.Position == ii select i).FirstOrDefault();
            if (item != null && item.Amount > 0)
            {
                text.GetComponent<Text>().text = item.Amount.ToString();
                image.GetComponent<Image>().sprite = Resources.Load<Sprite>("items/item " + item.Type);
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
        }
    }

    bool IsWalkable(int xx, int yy)
    {
        if (Data.Rocks[xx, yy].Type.Contains("empty") || Data.Rocks[xx, yy].Type.Contains("cave"))
        {
            return true;
        }
        return false;
    }

    // Update is called once per frame
    void Update () {
        HandleReset();
        CheckIfAlive();
        if (gameOver.activeSelf) return;

        HandleInGameMenu();
        HandleInventory();
        UpdateDayTime();
        CheckLevel();

        float step = Data.Speed * 5 * Time.deltaTime; // Movement Speed
        bool left = false, right = false, up = false, down = false;
        Vector3 targetElevator = elevator.transform.position;
        Vector3 currentTarget = target;
        bool arrived = false;
        bool workDone = false;
        bool shouldWalk = false;
        bool hasWorked = (oldAction == Action.Pick || oldAction == Action.Shovel);//workingRock != null;
        MinerData.Rock oldWorkingRock = workingRock;

        int pos = elevator.transform.position.y >= 0 ? 0 : Mathf.Abs((int)(elevator.transform.position.y + 10) / 20) + 1;
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

        if (!inventory.activeSelf && !inGameMenu.activeSelf)
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

            if (Input.GetMouseButton(0) && Input.mousePosition.x < Screen.width * 0.25)
            {
                left = true;
            }
            else if (Input.GetMouseButton(0) && Input.mousePosition.x > Screen.width * 0.75)
            {
                right = true;
            }
            else if (Input.GetMouseButton(0) && Input.mousePosition.y < Screen.height * 0.25)
            {
                down = true;
            }
            else if (Input.GetMouseButton(0) && Input.mousePosition.y > Screen.height * 0.75)
            {
                up = true;
            }
            if (Input.GetButtonUp("Submit"))
            {
                var lights = GameObject.FindGameObjectsWithTag("LightSource");
                if (lights.Count() <= 1)
                {
                    var torch = Instantiate(Resources.Load<Transform>("prefabs/Torch"));
                    torch.SetParent(this.transform, false);
                    Debug.Log("Instantiating new torch " + torch.name);
                }
                else
                {
                    foreach(var light in lights)
                    {
                        if (light.name.Contains("Torch"))
                        {
                            Destroy(light);
                        }
                    }
                }
            }
        }
        if (isAnimated)
        {
            // Am Ziel angekommen?
            if (transform.position == target && oldAction == Action.Move)
            {
                Debug.Log("arrived");
                arrived = true;
                Data.X = transform.position.x;
                Data.Y = transform.position.y;
                Data.ElevatorX = elevator.position.x;
                Data.ElevatorY = elevator.position.y;
            }

            // Fertig mit der Arbeit?
            if (Time.time - workStartedTime > estimatedWorkTime && hasWorked)
            {
                Debug.Log("work done");
                workDone = true;
                workingRock = null;
            }

            if (arrived || (workDone && hasWorked))
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

        //int targetGridX = (int)target.x / 15;
        int targetGridX, targetGridY;
        RockGroupBehaviour.GetGridPosition(target, true, out targetGridX, out targetGridY);

        if (shouldWalk)
        {
            Debug.Log("Target " + target.ToString());
        } 

        Action newAction = oldAction;

        MinerData.Rock newWorkingRock = null;

        if (arrived || workDone)
        {
            Debug.Log("Done, idling");
            newAction = Action.Idle;
        }

        if (shouldWalk) // kein else if, weil er sonst er stoppen würde und dann weiterlaufen, er soll aber direkt weiterlaufen
        {
            newAction = Action.Move;
        }

        if (workDone)
        {
            HandleWorkDone(oldWorkingRock);
        }
        if (arrived)
        {
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
                newAction = Action.Idle;
            }
        }

        if (newAction == Action.Collect)
        {
            if (newAction != oldAction)
            {
                GetComponent<Animator>().Play("placing");
                isAnimated = true;
                Debug.Log("Playing placing animation");
            }
        }
        else if (newAction == Action.Idle || newAction == Action.NeedsWork)
        {
            target = currentTarget;
            if (newAction != oldAction)
            {
                Debug.Log("Playing idle, Action = " + newAction);
                GetComponent<Animator>().Play("idle");
            }
        }
        else if (newAction == Action.Move)
        {
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
                Debug.Log("Playing walking");
                GetComponent<Animator>().Play("walking");
            }
            isAnimated = true;
        }
        else if (newAction == Action.Pick && (newAction != oldAction || oldOrientation != orientation))
        {
            target = currentTarget;
            Debug.Log("Playing pick");
            GetComponent<Animator>().Play("pick " + orientation);
            workStartedTime = Time.time;
            estimatedWorkTime = 6f;
            workingRock = newWorkingRock;
            Debug.Log(workingRock);
            isAnimated = true;
        }
        else if (newAction == Action.Shovel && (newAction != oldAction || oldOrientation != orientation))
        {
            target = currentTarget;
            Debug.Log("Playing spade " + orientation);
            GetComponent<Animator>().Play("spade " + orientation);
            workStartedTime = Time.time;
            estimatedWorkTime = 3f;
            workingRock = newWorkingRock;
            Debug.Log(workingRock);
            isAnimated = true;
        }
        oldAction = newAction;
        oldOrientation = orientation;
    }

    private void UpdateMoral()
    {
        var lights = GameObject.FindGameObjectsWithTag("LightSource");
        float looseMoral = -5;
        foreach (var light in lights)
        {
            float range = light.GetComponent<Light>().range;
            float z = light.transform.position.z;
            float lightRange = (range - Math.Abs(z)) * 2.5f;
            float distance = Vector3.Distance(transform.position, new Vector3(light.transform.position.x, light.transform.position.y));
            //Debug.Log("distance " + distance + " " + );
            if (distance > lightRange * 1.0f)
            {
                looseMoral = Mathf.Max(-2, looseMoral);
            }
            // Reihenfolge ist wichtig
            else if (distance > lightRange * 0.75f)
            {
                looseMoral = Mathf.Max(-1, looseMoral);
            }
            else if (distance < lightRange * 0.5f)
            {
                looseMoral = Mathf.Max(3, looseMoral);
            }
            else
            {
                looseMoral = Mathf.Max(0, looseMoral);
            }
        }
        Data.Moral += looseMoral;
        Data.Moral = Mathf.Clamp(Data.Moral, 0, 100);
    }

    private void HandleCollect()
    {
        isAnimated = false;
        int gridX, gridY;
        RockGroupBehaviour.GetGridPosition(transform.position, true, out gridX, out gridY);
        var resourcesToCollectNow = (from r in resourcesToCollect where r.GetComponent<ResourceBehaviour>().gridX == gridX && r.GetComponent<ResourceBehaviour>().gridY == gridY select r).Take(2).ToList();
        Debug.Log("Collect Now: " + resourcesToCollectNow.Count );
        foreach (var resource in resourcesToCollectNow)
        {
            var resB = resource.GetComponent<ResourceBehaviour>();
            Debug.Log("Collecting " + resB.type);
            Data.AddInventoryItem(resB.type, false);
            resourcesToCollect.Remove(resource);
            Destroy(resource.gameObject);
        }
        oldAction = Action.Idle;
        TryPlanCollect(0.5f);
        UpdateExperienceBar();
    }

    public Transform activeInGameMenuItem;
    public Transform continueItem;
    public Transform mainMenuItem;
    public Transform quitItem;
    private void HandleInGameMenu()
    {
        bool freshActivated = false;
        if (inventory.activeSelf) return;
        if (Input.GetButtonUp("Cancel"))
        {
            freshActivated = true;
            inGameMenu.SetActive(!inGameMenu.activeSelf);
        }
        if (!inGameMenu.activeSelf) return;
        bool up, down, submit, cancel;
        up = down = submit = cancel = false;
        if (Input.GetButtonUp("Up"))
        {
            up = true;
        }
        if (Input.GetButtonUp("Down"))
        {
            down = true;
        }
        if (Input.GetButtonUp("Submit"))
        {
            submit = true;
        }
        if (Input.GetButtonUp("Cancel") && !freshActivated)
        {
            cancel = true;
        }

        if (up)
        {
            if (activeInGameMenuItem == continueItem) activeInGameMenuItem = quitItem;
            else if (activeInGameMenuItem == quitItem) activeInGameMenuItem = mainMenuItem;
            else if (activeInGameMenuItem == mainMenuItem) activeInGameMenuItem = continueItem;
        }
        else if (down)
        {
            if (activeInGameMenuItem == continueItem) activeInGameMenuItem = mainMenuItem;
            else if (activeInGameMenuItem == quitItem) activeInGameMenuItem = continueItem;
            else if (activeInGameMenuItem == mainMenuItem) activeInGameMenuItem = quitItem;
        }
        else if (submit)
        {
            if (activeInGameMenuItem == continueItem) inGameMenu.SetActive(false);
            if (activeInGameMenuItem == mainMenuItem) GotoMainMenu();
            if (activeInGameMenuItem == quitItem) Application.Quit();
        }
        else if (cancel)
        {
            inGameMenu.SetActive(false);
        }
        quitItem.GetComponent<Text>().color = Color.black;
        mainMenuItem.GetComponent<Text>().color = Color.black;
        continueItem.GetComponent<Text>().color = Color.black;
        activeInGameMenuItem.GetComponent<Text>().color = Color.red;
    }

    void GotoMainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    private void HandleReset()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            MinerSaveGame.Instance.Current.Reset();
            GotoMainMenu();
        }
    }

    private void CheckIfAlive()
    {
        if (Data.Health <= 0)
        {
            gameOver.SetActive(true);
        }
        else
        {
            gameOver.SetActive(false);
        }
    }

    public void CheckLevel()
    {
        if (Data.Level == 0 && Data.Experience >= 100)
        {
            Data.MaxHealth = 105f;
            Data.Speed = 3.75f;
            Data.NextLevelExperience = 200;
            Data.Level++;
        }
        else if (Data.Level == 1 && Data.Experience >= 200)
        {
            Data.MaxHealth = 110f;
            Data.Speed = 4.00f;
            Data.NextLevelExperience = 500;
            Data.Level++;
        }
        else if (Data.Level == 2 && Data.Experience >= 500)
        {
            Data.MaxHealth = 115f;
            Data.Speed = 4.25f;
            Data.NextLevelExperience = 1000;
            Data.Level++;
        }
        else if (Data.Level == 3 && Data.Experience >= 1000)
        {
            Data.MaxHealth = 120f;
            Data.Speed = 4.5f;
            Data.NextLevelExperience = 2500;
            Data.Level++;
        }
        else if (Data.Level == 4 && Data.Experience >= 2500)
        {
            Data.MaxHealth = 125f;
            Data.Speed = 4.75f;
            Data.NextLevelExperience = 3000;
            Data.Level++;
        }
        else if (Data.Level == 5 && Data.Experience >= 3000)
        {
            Data.MaxHealth = 130f;
            Data.Speed = 5.0f;
            Data.NextLevelExperience = 3500;
            Data.Level++;
        }
        else if (Data.Level == 6 && Data.Experience >= 3500)
        {
            Data.MaxHealth = 135f;
            Data.Speed = 5.25f;
            Data.NextLevelExperience = 4000;
            Data.Level++;
        }
        else if (Data.Level == 7 && Data.Experience >= 4000)
        {
            Data.MaxHealth = 140f;
            Data.Speed = 5.5f;
            Data.NextLevelExperience = 5000;
            Data.Level++;
        }
        else if (Data.Level == 8 && Data.Experience >= 5000)
        {
            Data.MaxHealth = 150f;
            Data.Speed = 5.75f;
            Data.NextLevelExperience = 10000;
            Data.Level++;
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
        requestedAction = Action.Collect;
    }

    void TryPlanCollect(float inSeconds)
    {
        int gridX, gridY;
        RockGroupBehaviour.GetGridPosition(transform.position, true, out gridX, out gridY);
        var count = (from r in resourcesToCollect where r.GetComponent<ResourceBehaviour>().gridX == gridX && r.GetComponent<ResourceBehaviour>().gridY == gridY select r).Count();
        Debug.Log("Found " + count + " resources to collect here");
        if (count > 0)
        {
            Invoke("SetNextActionCollect", inSeconds);
        }
    }

    void HandleArrived()
    {
        Debug.Log("Found " + resourcesToCollect.Count + " resources to collect");
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
            if (Data.Rocks[ii,yy].AfterType.Contains("cave"))
            {
                return true;
            }
        }
        return false;
    }

    void HandleWorkDone(MinerData.Rock rock)
    {
        Debug.Log("Begin HandleWorkDone");
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
        string spriteName = "rocks/rock " + rock.Type;
        if (rock.Type.Contains("cave"))
        {
            int rnd = UnityEngine.Random.Range(1, 4);
            spriteName += " " + rnd.ToString().PadLeft(2, '0');
        }
        Debug.Log(spriteName);
        rockObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(spriteName);
        UpdateExperienceBar();

        TryCastMinerals(rock);
        Debug.Log("End HandleWorkDone");
    }

    private void TryCastMinerals(MinerData.Rock rock)
    {
        int countMinerals = UnityEngine.Random.Range(0, 7);
        for (int ii = 0; ii < countMinerals; ii++)
        {
            CastMineral(rock);
        }
    }

    public Transform resourceTemplate;
    void CastMineral(MinerData.Rock rock)
    {
        var xPos = UnityEngine.Random.Range(1, 15);
        var yPos = UnityEngine.Random.Range(1, 6);
        var what = UnityEngine.Random.Range(0, 101);
        var type = "gold";
        if (what < 20)
        {
            type = "copper";
        }
        else if (what < 40)
        {
            type = "gold";
        }
        else if (what < 60)
        {
            type = "silver";
        }
        else if (what < 80)
        {
            type = "platinum";
        }
        else if (what < 100)
        {
            type = "gem";
        }
        var variant = UnityEngine.Random.Range(1, type == "gem" ? 8 : 5);
        var targetPos = new Vector3(rock.X * 15 + xPos, rock.Y * -20 + yPos - 20);
        Debug.Log(String.Format("targetPos = {0}", targetPos));
        var newResource = Instantiate(resourceTemplate, targetPos, Quaternion.identity) as Transform;
        newResource.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("resources/resources " + type + " " + variant.ToString().PadLeft(2, '0'));
        newResource.GetComponent<ResourceBehaviour>().type = type;
        RockGroupBehaviour.GetGridPosition(targetPos, false, out newResource.GetComponent<ResourceBehaviour>().gridX, out newResource.GetComponent<ResourceBehaviour>().gridY);
        Debug.Log("Resource " + type + " added with gridX = " + newResource.GetComponent<ResourceBehaviour>().gridX + " and gridY = " + newResource.GetComponent<ResourceBehaviour>().gridY);
        AddResourceToCollect(newResource);
    }

    void OnMouseDown()
    {
        UpdateInventory();
        inventory.SetActive(true);
    }
}

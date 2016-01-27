using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MinerRicoBehavior : MonoBehaviour {

    enum Action
    {
        Move, Pick, Shovel, Idle, NeedsWork
    }

    bool isAnimated = false;
    bool moveElevator = false;
    bool inElevator = false;
    Vector3 target = Vector3.zero;

    public MinerData Data;

    Action oldAction = Action.Idle;
    string oldOrientation;

    float workStartedTime;
    float estimatedWorkTime;
    MinerData.Rock workingRock;
    public GameObject foodBarInner;
    public GameObject healthBarInner;
    public GameObject gameOver;
    public GameObject timeBarInner;
    public GameObject sun;
    public GameObject inventory;
    public GameObject inGameMenu;
    public Transform elevator;
    public GameObject[] arrSky;

    void Start () {
        Data = MinerSaveGame.Instance.Current;

        healthBarInner = GameObject.Find("HealthBarInner");
        foodBarInner = GameObject.Find("FoodBarInner");
        gameOver = GameObject.Find("GameOver");
        timeBarInner = GameObject.Find("TimeBarInner");
        sun = GameObject.Find("Sun");
        inventory = GameObject.Find("Inventory");
        inGameMenu = GameObject.Find("InGameMenu");

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

        transform.position = new Vector3(Data.X, Data.Y, transform.position.z);
        elevator.transform.position = new Vector3(Data.ElevatorX, Data.ElevatorY, elevator.transform.position.z);

        InvokeRepeating("reduceFood", 1, 1);
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
        healthBarInner.GetComponent<RectTransform>().anchorMax = new Vector2(Mathf.Lerp(0.02f, 0.98f, Data.Health / 100.0f), 0.9f);
    }

    public void reduceFood()
    {
        --Data.FoodLevel;
        if (Data.FoodLevel < 0)
        {
            Data.FoodLevel = 0;
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
                arrSky[ii].GetComponent<SpriteRenderer>().sprite = Data.DayTime > 50.0f ? Resources.Load<Sprite>("world/world sky night") : Resources.Load<Sprite>("world/world sky day");
            }
        }
        sun.transform.position = new Vector2(Mathf.Lerp(-15f, 390f, (((Data.DayTime * 2f) % 100) / 100.0f)), sun.transform.position.y);
    }
	
    void UpdateDayTime()
    {
        Data.DayTime += Time.deltaTime;
        Data.DayTime %= 100.0f;
        UpdateSun(false);        
    }

    public Transform activeInventoryItem;
    public int activeInventoryNumber = 0;
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
    }

    void HandleInventory()
    {
        if (Input.GetKeyUp(KeyCode.I))
        {
            inventory.SetActive(!inventory.activeSelf);
        }
        bool left, right, up, down;
        left = right = up = down = false;
        if (inventory.activeSelf)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                inventory.SetActive(!inventory.activeSelf);
            }
            if (Input.GetButtonUp("Left"))
            {
                left = true;
            }
            else if (Input.GetButtonUp("Right"))
            {
                right = true;
            }
            else if (Input.GetButtonUp("Down"))
            {
                down = true;
            }
            else if (Input.GetButtonUp("Up"))
            {
                up = true;
            }
            else
            {
                // rest doesn't matter
                return;
            }
            if (left) activeInventoryNumber--;
            else if (right) activeInventoryNumber++;
            else if (up) activeInventoryNumber -= 5;
            else if (down) activeInventoryNumber += 5;
            if (activeInventoryNumber < 0) activeInventoryNumber = 20 + activeInventoryNumber;
            activeInventoryNumber %= 20;
            HandleInventorySelection();
        }
    }

	// Update is called once per frame
	void Update () {
        HandleReset();
        CheckIfAlive();
        if (gameOver.activeSelf) return;

        HandleInventory();
        UpdateDayTime();
        HandleInGameMenu();

        float step = 30 * Time.deltaTime; // Movement Speed
        bool left = false, right = false, up = false, down = false;
        Vector3 targetElevator = elevator.transform.position;
        Vector3 currentTarget = target;
        bool arrived = false;
        bool workDone = false;
        bool shouldWalk = false;
        bool hasWorked = workingRock != null;
        MinerData.Rock oldWorkingRock = workingRock;

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
            if (up && transform.position.y < 10 && (transform.position.y != 10 || inElevator))
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
        if (target == Vector3.zero) return;

        int targetGridX = (int)target.x / 15;
        int targetGridY = (int)target.y == 10 ? -1 : Mathf.Abs((int)target.y / 20 + 1);
        //Debug.Log(string.Format("targetX/Y {0} {1} TargetGridX/Y {2} {3}", target.x, target.y, targetGridX, targetGridY));
        Action newAction = oldAction;

        MinerData.Rock newWorkingRock = null;

        if (workDone)
        {
            workDoneAction(oldWorkingRock);
        }

        if (arrived || workDone)
        {
            Debug.Log("Done, idling");
            newAction = Action.Idle;
        }
        if (shouldWalk) // kein else if, weil er sonst er stoppen würde und dann weiterlaufen, er soll aber direkt weiterlaufen
        {
            newAction = Action.Move;
        }
        // Wenn er laufen soll, schauen, ob er buddeln müsste
        if (newAction == Action.Move && targetGridX < MinerData.XCOUNT && targetGridY < MinerData.YCOUNT && targetGridY >= 0 && targetGridX >= 0 && Data.Rocks[targetGridX, targetGridY] != null)
        {
            newWorkingRock = Data.Rocks[targetGridX, targetGridY];
            if (newWorkingRock.Type.IndexOf("empty") < 0)
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

        if (newAction == Action.Idle || newAction == Action.NeedsWork)
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
                var elevatorLabel = GameObject.Find("ElevatorLabel");
                int pos = elevator.transform.position.y >= 0 ? 0 : Mathf.Abs((int)(elevator.transform.position.y + 10) / 20) + 1;
                elevatorLabel.GetComponent<UnityEngine.UI.Text>().text = pos.ToString();
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
            if ((newAction != oldAction || oldOrientation != orientation))
            {
                Debug.Log("Playing pick");
                GetComponent<Animator>().Play("pick " + orientation);
                workStartedTime = Time.time;
                estimatedWorkTime = 6f;
                workingRock = newWorkingRock;
                Debug.Log(workingRock);
                isAnimated = true;
            }
        }
        else if (newAction == Action.Shovel)
        {
            target = currentTarget;
            if ((newAction != oldAction || oldOrientation != orientation))
            {
                Debug.Log("Playing spade");
                GetComponent<Animator>().Play("spade " + orientation);
                workStartedTime = Time.time;
                estimatedWorkTime = 3f;
                workingRock = newWorkingRock;
                Debug.Log(workingRock);
                isAnimated = true;
            }
        }
        oldAction = newAction;
        oldOrientation = orientation;
    }

    public Transform activeInGameMenuItem;
    public Transform continueItem;
    public Transform mainMenuItem;
    public Transform quitItem;
    private void HandleInGameMenu()
    {
        bool freshActivated = false;
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

    void EnterElevator()
    {
        inElevator = true;
    }
    void ExitElevator()
    {
        inElevator = false;
    }

    void workDoneAction(MinerData.Rock rock)
    {
        rock.Type += " empty";
        var rockObject = GameObject.Find("Rock_" + rock.X + "_" + rock.Y);
        string spriteName = "rocks/rock " + rock.Type;
        Debug.Log(spriteName);
        rockObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(spriteName);
    }
}

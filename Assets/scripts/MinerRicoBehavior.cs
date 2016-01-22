﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

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
    public Transform elevator;
    public GameObject[] arrSky;

    void Start () {
        Data = MinerSaveGame.Instance.Current;

        healthBarInner = GameObject.Find("HealthBarInner");
        foodBarInner = GameObject.Find("FoodBarInner");
        gameOver = GameObject.Find("GameOver");
        timeBarInner = GameObject.Find("TimeBarInner");
        sun = GameObject.Find("Sun");

        arrSky = new GameObject[4];
        for (int ii = 0; ii < 4; ii++)
        {
            arrSky[ii] = GameObject.Find("world sky" + ii);
        }

        gameOver.SetActive(false);

        UpdateBars();
        UpdateSun(true);

        InvokeRepeating("reduceFood", 1, 1);
        InvokeRepeating("save", 60, 60);
	}

    void OnDestroy()
    {
        MinerSaveGame.Save();
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

	// Update is called once per frame
	void Update () {
        if (gameOver.activeSelf) return;
        float step = 30 * Time.deltaTime; // Movement Speed
        bool left = false, right = false, up = false, down = false;
        Vector3 targetElevator = elevator.transform.position;
        Vector3 current = target;
        bool arrived = false;
        bool workDone = false;
        bool shouldWalk = false;
        bool hasWorked = workingRock != null;
        MinerData.Rock oldWorkingRock = workingRock;

        UpdateDayTime();
        CheckIfAlive();

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
        if (isAnimated)
        {
            // Am Ziel angekommen?
            if (transform.position == target && oldAction == Action.Move)
            {
                arrived = true;
            }

            // Fertig mit der Arbeit?
            if (Time.time - workStartedTime > estimatedWorkTime && hasWorked)
            {
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
        if (targetGridX < MinerData.XCOUNT && targetGridY < MinerData.YCOUNT && targetGridY >= 0 && targetGridX >= 0 && Data.Rocks[targetGridX, targetGridY] != null)
        {
            newWorkingRock = Data.Rocks[targetGridX, targetGridY];
            if (newWorkingRock.Type.IndexOf("empty") < 0)
            {
                newAction = Action.NeedsWork;
            }
        }
        string orientation = "";
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
            target = current;
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
        else if (newAction == Action.Pick && newAction != oldAction)
        {
            target = current;
            Debug.Log("Playing pick");
            GetComponent<Animator>().Play("pick " + orientation);
            isAnimated = true;
            workStartedTime = Time.time;
            estimatedWorkTime = 6f;
            workingRock = newWorkingRock;
        }
        else if (newAction == Action.Shovel && newAction != oldAction)
        {
            target = current;
            Debug.Log("Playing spade");
            GetComponent<Animator>().Play("spade " + orientation);
            isAnimated = true;
            workStartedTime = Time.time;
            estimatedWorkTime = 3f;
            workingRock = newWorkingRock;
        }
        oldAction = newAction;
        oldOrientation = orientation;
    }

    private void CheckIfAlive()
    {
        if (Data.Health <= 0)
        {
            gameOver.SetActive(true);
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

using UnityEngine;
using System.Collections;

public class MinerRicoBehavior : MonoBehaviour {

    enum Action
    {
        Move, Pick, Shovel, Idle
    }

    void Start () {
        Data = MinerSaveGame.Instance.Current;
        InvokeRepeating("reduceFood", 1, 1);
	}

    public Transform elevator;

    public void reduceFood()
    {
        --Data.FoodLevel;
        if (Data.FoodLevel < 0)
        {
            Data.FoodLevel = 0;
            --Data.Health;
            if (Data.Health < 0) Data.Health = 0;
        }
        GameObject.Find("FoodBarInner").transform.localScale = new Vector3(Data.FoodLevel / 100.0f, 1, 1);
        GameObject.Find("HealthBarInner").transform.localScale = new Vector3(Data.Health / 100.0f, 1, 1);
    }

    bool isMoving = false;
    bool moveElevator = false;
    Vector3 target = Vector3.zero;

    public MinerData Data;
	
	// Update is called once per frame
	void Update () {
        float step = 30 * Time.deltaTime; // Movement Speed
        bool left = false, right = false, up = false, down = false;
        Vector3 targetElevator = elevator.transform.position;
        Vector3 current = target;
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
        if (isMoving)
        {
            if (transform.position == target)
            {
                isMoving = false;
                if (moveElevator)
                {
                    elevator.transform.position = transform.position;
                    moveElevator = false;
                }
            }
        }
        if (!isMoving)
        {
            moveElevator = false;
            if (left && transform.position.x > 8)
            {
                target = gameObject.transform.position + new Vector3(-15, 0);
                GetComponent<Animator>().Play("walking");
                GetComponent<SpriteRenderer>().flipX = false;
                isMoving = true;
            }
            if (right && transform.position.x < 330)
            {
                target = transform.position + new Vector3(15, 0);
                GetComponent<SpriteRenderer>().flipX = true;
                GetComponent<Animator>().Play("walking");
                isMoving = true;
            }
            if (down && transform.position.y > -2000)
            {
                target = transform.position + new Vector3(0, -20);
                if (transform.position.y == 10)
                {
                    target += new Vector3(0, -10);
                }
                if (!inElevator)
                {
                    GetComponent<Animator>().Play("walking");
                }
                else
                {
                    moveElevator = true;
                }
                isMoving = true;
            }
            if (up && transform.position.y < 10)
            {
                target = transform.position + new Vector3(0, 20);
                if (transform.position.y == -20)
                {
                    target += new Vector3(0, 10);
                }
                if (!inElevator)
                {
                    GetComponent<Animator>().Play("walking");
                }
                else
                {
                    moveElevator = true;
                }
                isMoving = true;
            }
        }
        if (!isMoving)
            GetComponent<Animator>().Play("idle");
        if (target == Vector3.zero) return;

        int targetGridX = (int)target.x / 15;
        int targetGridY = (int)target.y == 10 ? -1 : Mathf.Abs((int)target.y / 20 + 1);
        Debug.Log(string.Format("targetX/Y {0} {1} TargetGridX/Y {2} {3}", target.x, target.y, targetGridX, targetGridY));
        Action newAction = Action.Move;

        if (targetGridX < MinerData.XCOUNT && targetGridY < MinerData.YCOUNT && targetGridY >= 0 && targetGridX >= 0 && Data.Rocks[targetGridX, targetGridY] != null)
        {
            if (Data.Rocks[targetGridX, targetGridY].Type.IndexOf("empty") < 0) newAction = Action.Idle;
        }

        if (newAction == Action.Idle)
        {
            target = current;
            GetComponent<Animator>().Play("idle");
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
        }
    }

    bool inElevator = false;
    void EnterElevator()
    {
        inElevator = true;
    }
    void ExitElevator()
    {
        inElevator = false;
    }
}

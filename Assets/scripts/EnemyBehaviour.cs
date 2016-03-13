using UnityEngine;
using System.Collections;

public class EnemyBehaviour : MonoBehaviour {

    public MinerData.Rock rock;
    public Vector3 target;
    public bool isAnimated = false;

    void Start () {
        GetComponent<Renderer>().enabled = false;
        float time = UnityEngine.Random.Range(0.0f, 10.0f);
        StartCoroutine(SetEnemyNext(time));
        Debug.Log("Enemy instantiated");
        Init();
	}

    void Init()
    {
        target = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (rock.EnemyState == EnemyState.Walking)
        {
            float step = 3 * 5 * Time.deltaTime;
            if (this.transform.position == target)
            {
                isAnimated = false;
                Debug.Log("Stopping animation");
                //GetComponent<Animator>().Stop();
                if (!IsInvoking("SetNewTarget"))
                {
                    Invoke("SetNewTarget", 5);
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, target, step);
                if (!isAnimated)
                {
                    Debug.Log("Playing animation");
                    GetComponent<Animator>().Play("walking");
                }
                isAnimated = true;
            }
        }
        else
        {
            Debug.Log("Stopping animation");
            GetComponent<Animator>().Stop();
        }
    }



    Sprite GetSprite()
    {
        string type = "";
        string spriteName = "";
        Debug.Log(rock.EnemyType);
        if (rock.EnemyType == EnemyType.MudGolem)
        {
            type = "mud";
        }
        else
        {
            type = "mud";
        }

        spriteName = "golems/golem " + type;
        return Resources.Load<Sprite>(spriteName);

    }

    IEnumerator SetEnemyNext(float time)
    {
        yield return new WaitForSeconds(time);
        rock.CounterInterval = 0;
        if (rock.EnemyState == EnemyState.None)
        {
            rock.EnemyState = EnemyState.Hidden;
        }
        if (rock.EnemyState == EnemyState.Hidden)
        {
            Debug.Log("Hidden Enemy turns on THE EYES");
            SetState(EnemyState.Eyes);
        } 
        else if (rock.EnemyState == EnemyState.Eyes)
        {
            SetState(EnemyState.Walking);
        }
        if (rock.CounterInterval > 0)
        {
            StartCoroutine(SetEnemyNext(rock.CounterInterval));
        }
    }

    void SetNewTarget()
    {
        Debug.Log("Setting Target");
        int xx, yy;
        RockGroupBehaviour.GetGridPosition(this.transform.position, false, out xx, out yy);
        int newXX = xx;
        int count = 0;
        while (newXX == xx && count < 100)
        {
            newXX = Random.Range(0, 23);
            ++count;
        }
        target = RockGroupBehaviour.GetPosition(newXX, yy);
    }

    void SetState(EnemyState state)
    {
        rock.EnemyState = state;
        if (state == EnemyState.Eyes)
        {
            GetComponent<Renderer>().enabled = true;
            int randomTimer = UnityEngine.Random.Range(5, 15);
            rock.CounterInterval = randomTimer;
        }
        else if (state == EnemyState.Hidden)
        {
            GetComponent<Renderer>().enabled = false;
            int randomTimer = UnityEngine.Random.Range(1, 10);
            rock.CounterInterval = randomTimer;
        }
        else if (state == EnemyState.Walking)
        {
            GetComponent<Renderer>().enabled = true;
            target = this.transform.position;
            GetComponent<SpriteRenderer>().sprite = GetSprite();
        }
    }
}

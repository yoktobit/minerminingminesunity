using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        //Debug.Log(GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length);
        /*foreach(var aci in GetComponent<Animator>().GetCurrentAnimatorClipInfo(0))
        {
            Debug.Log(aci.clip.name);
        }*/
        
        if (rock.EnemyState == EnemyState.Walking)
        {
            float step = 3 * 5 * Time.deltaTime;
            if (this.transform.position == target)
            {
                isAnimated = false;
                GetComponent<Animator>().Play("mud idle");
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
                    GetComponent<SpriteRenderer>().flipX = (target - transform.position).normalized == Vector3.right;
                    GetComponent<Animator>().Play("mud walking");
                }
                isAnimated = true;
            }
        }
        else
        {
            GetComponent<Animator>().Play("mud idle");
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
        RockGroupBehaviour.GetGridPosition(this.transform.position, true, out xx, out yy);
        
        var freeX = new List<int>();
        for (var currentXX = xx; currentXX < 22; currentXX++)
        {
            MinerData.Rock rock = MinerSaveGame.Instance.Current.Rocks[currentXX, yy];
            if (rock.Type.Contains("cave") || rock.Type.Contains("empty"))
            {
                freeX.Add(currentXX);
            }
            else
            {
                break;
            }
        }
        for (var currentXX = xx; currentXX >= 0; currentXX--)
        {
            MinerData.Rock rock = MinerSaveGame.Instance.Current.Rocks[currentXX, yy];
            if (rock.Type.Contains("cave") || rock.Type.Contains("empty"))
            {
                freeX.Add(currentXX);
            }
            else
            {
                break;
            }
        }
        int count = 0;
        int newXX = xx;
        while (newXX == xx && count < 100)
        {
            newXX = freeX[Random.Range(0, freeX.Count)];
            ++count;
        }
        if (count < 100)
        {
            target = RockGroupBehaviour.GetPosition(newXX, yy, true);
        }
        else
        {
            target = RockGroupBehaviour.GetPosition(xx, yy, true);
        }
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

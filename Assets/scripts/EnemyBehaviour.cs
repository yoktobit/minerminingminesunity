using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyBehaviour : MonoBehaviour {

    public MinerData.Rock rock;
    public Vector3 target;
    public bool isAnimated = false;

    void Awake()
    {
        GetComponent<Renderer>().enabled = false;
        //Debug.Log("Enemy instantiated");
    }

    void Start () {
        Init();
        //Debug.Log("Enemy Started");
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

        if (MinerSaveGame.Instance.Current.Paused) return;
        
        if (rock.EnemyState == EnemyState.Walking)
        {
            float step = 3 * 5 * Time.deltaTime;
            if (this.transform.position == target)
            {
                var wasAnimated = isAnimated;
                isAnimated = false;
                int xPos, yPos;
                RockGroupBehaviour.GetGridPosition(this.transform.position, true, out xPos, out yPos);
                if (wasAnimated && rock.X == xPos && rock.Y == yPos)
                {
                    //Debug.Log("Entering Cave");
                    SetState(EnemyState.Eyes);
                }
                else
                {
                    GetComponent<Animator>().Play(GetEnemyType(rock.EnemyType) + " idle");
                    if (!IsInvoking("SetNewTarget"))
                    {
                        Invoke("SetNewTarget", 5);
                    }
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, target, step);
                rock.EnemyX = transform.position.x;
                rock.EnemyY = transform.position.y;
                if (!isAnimated)
                {
                    GetComponent<SpriteRenderer>().flipX = (target - transform.position).normalized == Vector3.right;
                    GetComponent<Animator>().Play(GetEnemyType(rock.EnemyType) + " walking");
                }
                isAnimated = true;
            }
        }
        else
        {
            //GetComponent<Animator>().Play(GetEnemyType(rock.EnemyType) + " idle");
        }
    }

    public void InvokeNext(float time = 0)
    {
        if (time <= 0)
        {
            time = UnityEngine.Random.Range(0.0f, 10.0f);
        }
        StartCoroutine(SetEnemyNext(time));
    }

    string GetEnemyType(EnemyType type)
    {
        switch(type)
        {
            case EnemyType.MudGolem:
                return "mud";
            default:
                return "";
        }
    }

    Sprite GetSprite()
    {
        string type = "";
        string spriteName = "";
        //Debug.Log(rock.EnemyType);
        type = GetEnemyType(rock.EnemyType);

        spriteName = "golems/golem " + type;
        return Resources.Load<Sprite>(spriteName);

    }

    public IEnumerator SetEnemyNext(float time)
    {
        yield return new WaitForSeconds(time);
        if (rock.EnemyState == EnemyState.None)
        {
            rock.EnemyState = EnemyState.Hidden;
        }
        if (rock.EnemyState == EnemyState.Hidden)
        {
            //Debug.Log("Hidden Enemy turns on THE EYES");
            SetState(EnemyState.Eyes);
        } 
        else if (rock.EnemyState == EnemyState.Eyes)
        {
            SetState(EnemyState.Walking);
        }
    }

    public void SetNewTarget()
    {
        int xx, yy;
        RockGroupBehaviour.GetGridPosition(this.transform.position, true, out xx, out yy);

        bool backHome = false;
        if (xx != this.rock.X || yy != this.rock.Y)
        {
            var rnd = Random.Range(0, 100);
            if (rnd < 20)
            {
                backHome = true;
            }
        }
        if (backHome)
        {
            target = RockGroupBehaviour.GetPosition(rock.X, rock.Y, true);
        }
        else
        {
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
            if (freeX.Count > 0)
            {
                while (newXX == xx && count < 100)
                {
                    newXX = freeX[Random.Range(0, freeX.Count)];
                    ++count;
                }
            }
            else
            {
                count = 100;
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
        this.rock.EnemyTargetX = target.x;
        this.rock.EnemyTargetY = target.y;
    }

    public void SetState(EnemyState state)
    {
        rock.EnemyState = state;
        //Debug.Log(rock.EnemyState);
        if (state == EnemyState.None)
        {
            //Debug.Log("SetState None");
            InvokeNext(0.1f);
        }
        else if (state == EnemyState.Eyes)
        {
            //Debug.Log("SetState Eyes");
            GetComponent<Renderer>().enabled = true;
            string nextAnimation = GetEnemyType(rock.EnemyType) + " eyes";
            //Debug.Log(nextAnimation);
            GetComponent<Animator>().Play(nextAnimation);
            int randomTimer = UnityEngine.Random.Range(5, 15);
            StartCoroutine(SetEnemyNext(randomTimer));
        }
        else if (state == EnemyState.Hidden)
        {
            //Debug.Log("SetState Hidden");
            GetComponent<Renderer>().enabled = false;
            int randomTimer = UnityEngine.Random.Range(1, 10);
            StartCoroutine(SetEnemyNext(randomTimer));
        }
        else if (state == EnemyState.Walking)
        {
            //Debug.Log("SetState Walking");
            GetComponent<Renderer>().enabled = true;
            target = this.transform.position;
            //GetComponent<SpriteRenderer>().sprite = GetSprite();
            int randomTimer = UnityEngine.Random.Range(1, 5);
            if (!IsInvoking("SetNewTarget"))
            {
                Invoke("SetNewTarget", randomTimer);
            }
        }
    }

    void StartAttackPlayer()
    {
        GetComponent<Animator>().Play(GetEnemyType(rock.EnemyType) + " angry");
    }

    public void AttackPlayer()
    {
        if (playerToBite != null)
        {
            var player = playerToBite.GetComponent<MinerRicoBehavior>();
            player.Data.Health -= 20;
        }
    }

    Transform playerToBite;
    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("PlayerToByte " + other);
        if (other.tag == "Player")
        {
            playerToBite = other.transform;
            StartAttackPlayer();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform == playerToBite)
        {
            //Debug.Log("PlayerToByte null");
            playerToBite = null;
        }
    }
}

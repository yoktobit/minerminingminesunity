using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System;

public class EnemyBehaviour : MonoBehaviour {

    public MinerData.Rock rock;
    public bool hasToFindNewTarget = true;
    public Vector3 target;
    public Node currentInterimNode;
    public Stack<Node> path;
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
        path = new Stack<Node>();
        nodes = new Node[MinerData.XCOUNT,MinerData.YCOUNT];
        for (int xx = 0; xx < MinerData.XCOUNT; xx++)
        {
            for (int yy = 0; yy < MinerData.YCOUNT; yy++)
            {
                nodes[xx, yy] = new Node(xx, yy);
            }
        }
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
                    SetState(EnemyState.Hidden);
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
                if (!isAnimated) // dann macht er es nur beim ersten Mal, also wenn er sich losbewegt
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
        Node startingPoint = nodes[xx, yy];
        if (path.Count == 0 || targetNode == null) // entweder noch kein Ziel festgelegt oder schon erreicht
        {
            bool backHome = false;
            if (xx != this.rock.X || yy != this.rock.Y) // wenn er nicht daheim ist
            {
                var rnd = UnityEngine.Random.Range(0, 100);
                if (rnd < 25) // in 25% der Fälle heimkehren
                {
                    backHome = true;
                }
            }
            if (backHome)
            {
                RefreshNodes();
                startingPoint = nodes[xx, yy];
                targetNode = nodes[rock.X, rock.Y];
                FindWay(startingPoint, targetNode);
                BuildPathFromHereToTarget(startingPoint, targetNode);
                currentInterimNode = path.Pop(); // erstes Zwischenziel, wird wohl irgendwie gehen, irgendwie ist er ja auch hier her gekommen
                target = RockGroupBehaviour.GetPosition(currentInterimNode.x, currentInterimNode.y, true); // Zwischenziel anpeilen
                Debug.Log(String.Format("Ziel (Backhome): {0};{1} ({2};{3})", currentInterimNode.x, currentInterimNode.y, target.x, target.y));
            }
            else
            {
                RefreshNodes();
                startingPoint = nodes[xx, yy];
                List<Node> lstPotentialTargets = GetFreeNodesInDistance(xx, yy, 15);
                Shuffle(lstPotentialTargets);
                Node reachableTargetNode = null;
                foreach (Node n in lstPotentialTargets)
                {
                    if (FindWay(startingPoint, n))
                    {
                        reachableTargetNode = n;
                        Debug.Log(String.Format("Weg zu {0};{1} gefunden.", n.x, n.y));
                        break;
                    }
                }
                if (reachableTargetNode == null)
                {
                    Debug.Log("Kein Weg gefunden");
                    reachableTargetNode = startingPoint;
                }
                BuildPathFromHereToTarget(startingPoint, reachableTargetNode);
                targetNode = reachableTargetNode; // Finales Ziel
                currentInterimNode = path.Pop(); // erstes Zwischenziel
                target = RockGroupBehaviour.GetPosition(currentInterimNode.x, currentInterimNode.y, true); // Zwischenziel anpeilen
                Debug.Log(String.Format("Ziel (erstes Zwischenziel): {0};{1} ({2};{3})", currentInterimNode.x, currentInterimNode.y, target.x, target.y));
            }
        }
        else
        {
            if (path.Count > 0)
            {
                currentInterimNode = path.Pop(); // nächstes Zwischenziel
                target = RockGroupBehaviour.GetPosition(currentInterimNode.x, currentInterimNode.y, true); // Zwischenziel anpeilen
                Debug.Log(String.Format("Ziel (nächstes Zwischenziel): {0};{1} ({2};{3})", currentInterimNode.x, currentInterimNode.y, target.x, target.y));

            }
            else
            {
                target = RockGroupBehaviour.GetPosition(targetNode.x, targetNode.y, true);
                Debug.Log(String.Format("Ziel (letztes Zwischenziel): {0};{1} ({2};{3})", targetNode.x, targetNode.y, target.x, target.y));
            }
        }
        this.rock.EnemyTargetX = target.x;
        this.rock.EnemyTargetY = target.y;
        
        
    }

    private void BuildPathFromHereToTarget(Node startingPoint, Node reachableTargetNode)
    {
        var currentNode = reachableTargetNode;
        int count = 0;
        while (currentNode != null)
        {
            path.Push(currentNode);
            currentNode = currentNode.predecessor;
            Debug.Log(String.Format("Pfad: {0};{1}", path.Peek().x, path.Peek().y));
            if (++count > 1000) break;
        }
    }

    private List<Node> GetFreeNodesInDistance(int x, int y, int distance)
    {
        List<Node> lstReturn = new List<Node>();
        for (var xx = Math.Max(x - distance, 0); xx < Math.Min(x + distance, MinerData.XCOUNT - 4); xx++)
        {
            for (var yy = Math.Max(y - distance, 0); yy < Math.Min(y + distance, MinerData.YCOUNT); yy++)
            {
                if (xx == x && yy == y) continue;
                if (MinerSaveGame.Instance.Current.Rocks[xx, yy].Type == MinerSaveGame.Instance.Current.Rocks[xx, yy].AfterType)
                {
                    lstReturn.Add(nodes[xx, yy]);
                }
            }
        }
        return lstReturn;
    }

    /** List Randomisierer */
    public static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    /** Ende List Randomisierer */

    public void SetState(EnemyState state)
    {
        rock.EnemyState = state;
        //Debug.Log(rock.EnemyState);
        if (state == EnemyState.None)
        {
            //Debug.Log("SetState None");
            float x = Mathf.Abs(this.transform.position.y);
            float y = (-177 / 2000) * x + 180;
            int randomTimer = UnityEngine.Random.Range(30, (int)y);
            InvokeNext(randomTimer);
        }
        else if (state == EnemyState.Eyes)
        {
            //Debug.Log("SetState Eyes");
            GetComponent<Renderer>().enabled = true;
            string nextAnimation = GetEnemyType(rock.EnemyType) + " eyes";
            //Debug.Log(nextAnimation);
            GetComponent<Animator>().Play(nextAnimation);
            int randomTimer = UnityEngine.Random.Range(5, 15);
            InvokeNext(randomTimer);
        }
        else if (state == EnemyState.Hidden)
        {
            //Debug.Log("SetState Hidden");
            GetComponent<Renderer>().enabled = false;
            //int randomTimer = UnityEngine.Random.Range(1, 10);
            //InvokeNext(randomTimer);
            float x = Mathf.Abs(this.transform.position.y);
            float y = (-177 / 2000) * x + 180;
            int randomTimer = UnityEngine.Random.Range(30, (int)y);
            InvokeNext(randomTimer);
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
        if (rock.EnemyState == EnemyState.Walking && other.tag == "Player")
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

    void RefreshNodes()
    {
        for (int xx = 0; xx < MinerData.XCOUNT; xx++)
        {
            for (int yy = 0; yy < MinerData.YCOUNT; yy++)
            {
                nodes[xx, yy] = new Node(xx, yy);
            }
        }
    }

    Node[,] nodes;
    List<Node> listOpen;
    HashSet<Node> listClosed;
    Node targetNode;
    bool FindWay(Node start, Node end)
    {
        targetNode = end;
        listOpen = new List<Node>();
        listClosed = new HashSet<Node>();
        listOpen.Add(start);
        int count = 0;
        do
        {
            Node current = listOpen.OrderBy(node => node.f).First();
            listOpen.Remove(current);
            if (current.Equals(end)) return true;
            listClosed.Add(current);
            ExpandNode(current);
            if (++count > 10000) break;
        } while (listOpen.Count > 0);
        return false;
    }

    bool IsFree(Node node)
    {
        if (MinerSaveGame.Instance.Current.Rocks[node.x, node.y].Type == MinerSaveGame.Instance.Current.Rocks[node.x, node.y].AfterType)
        {
            return true;
        }
        return false;
    }

    List<Node> GetSurroundings(Node node)
    {
        List<Node> listSurroundings = new List<Node>();
        if (node.y > 0)
        {
            Node n = nodes[node.x, node.y - 1];
            n.c = 1.0f;//new Node(node, node.x, node.y - 1, 1.0f);
            if (IsFree(n)) listSurroundings.Add(n);
        }
        if (node.x > 0)
        {
            Node n = nodes[node.x - 1, node.y];
            n.c = 1.0f;//new Node(node, node.x - 1, node.y, 1.0f);
            if (IsFree(n)) listSurroundings.Add(n);
        }
        if (node.x < MinerData.XCOUNT - 4)
        {
            Node n = nodes[node.x + 1, node.y];
            n.c = 1.0f;//new Node(node, node.x + 1, node.y, 1.0f);
            if (IsFree(n)) listSurroundings.Add(n);
        }
        if (node.y < MinerData.YCOUNT)
        {
            Node n = nodes[node.x, node.y + 1];
            n.c = 1.0f; //new Node(node, node.x, node.y + 1, 1.0f);
            if (IsFree(n)) listSurroundings.Add(n);
        }
        return listSurroundings;
    }

    void ExpandNode(Node node)
    {
        var nodes = GetSurroundings(node);
        foreach (var otherNode in nodes)
        {
            if (listClosed.Contains(otherNode)) continue;
            var g = node.g + otherNode.c;
            if (listOpen.Contains(otherNode) && g >= otherNode.g)
            {
                continue;
            }
            otherNode.predecessor = node;
            otherNode.g = g;
            otherNode.h = Vector2Int.Distance(new Vector2Int(otherNode.x, otherNode.y), new Vector2Int(targetNode.x, targetNode.y));
            var f = g + otherNode.h;
            if (listOpen.Contains(otherNode))
            {
                otherNode.f = f;
            }
            else
            {
                otherNode.f = f;
                listOpen.Add(otherNode);
            }
        }
    }
}

public class Node
{
    public int x;
    public int y;
    public float f = 0;
    public float g = 0;
    public float c = 0;
    public float h;
    public Node predecessor;

    public Node(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.c = 0;
    }

    public override bool Equals(object obj)
    {
        Node n = (Node)obj;
        return n.x == x && n.y == y;
    }
}

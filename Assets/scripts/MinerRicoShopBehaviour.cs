using UnityEngine;
using System.Collections;

public class MinerRicoShopBehaviour : MonoBehaviour {

    public MinerData Data;

    public Transform ShopUi;
    public Transform LeftFrame;
    public Transform RightFrame;

    public Vector3 target;

    public Transform SelectedItem;

	// Use this for initialization
	void Start () {
        Data = MinerSaveGame.Instance.Current;
        target = transform.position;
	}

    private void FixedUpdate()
    {
        bool left = false, right = false, up = false, down = false;
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
        if (right)
        {
            target = new Vector3(70, transform.position.y);//transform.position + new Vector3(15f, 0);
            transform.position = Vector3.MoveTowards(transform.position, target, 0.3f);
            if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("walking"))
            { 
                GetComponent<Animator>().Play("walking");
            }
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else if (left)
        {
            target = new Vector3(-70, transform.position.y);//transform.position - new Vector3(15f, 0);
            transform.position = Vector3.MoveTowards(transform.position, target, 0.3f);
            if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("walking"))
            {
                GetComponent<Animator>().Play("walking");
            }
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            GetComponent<Animator>().Play("idle");
        }
        Debug.Log(target);
    }

    // Update is called once per frame
    void Update () {
        
	}
}

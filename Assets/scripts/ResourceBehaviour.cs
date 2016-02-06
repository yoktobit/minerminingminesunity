using UnityEngine;
using System.Collections;

public class ResourceBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var player = GameObject.FindGameObjectWithTag("Player");
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), player.GetComponent<Collider2D>());
	}
	
	// Update is called once per frame
	void Update () {
        /*var player = GameObject.FindGameObjectWithTag("Player");
        var collider = GetComponent<Collider2D>();
        Collider2D[] colTarget = Physics2D.OverlapAreaAll(collider.bounds.min, collider.bounds.max);
        foreach (var col in colTarget)
        {
            if (col == player.GetComponent<Collider2D>())
            {
                player.GetComponent<MinerRicoBehavior>().AddResourceToCollect(gameObject);
                break;
            }
        }*/
    }

    public int gridX;
    public int gridY;

    public string type;

}

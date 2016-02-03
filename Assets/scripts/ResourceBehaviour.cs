using UnityEngine;
using System.Collections;

public class ResourceBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public string type;

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.GetComponent<MinerRicoBehavior>() != null)
            coll.gameObject.GetComponent<MinerRicoBehavior>().AddResourceToCollect(gameObject);
    }
}

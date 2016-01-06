using UnityEngine;
using System.Collections;

public class ElevatorRepeatBehavior : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var original = GameObject.Find("world elevator 02");
        for (var yy = 1; yy < 100; ++yy)
        {
            Instantiate(original, new Vector3(original.transform.position.x, yy * -20), original.transform.rotation);
        }
    }
	
	// Update is called once per frame
	void Update () {
        
	}
}

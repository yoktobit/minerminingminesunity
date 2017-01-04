using UnityEngine;
using System.Collections;

public class CameraRoomBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float aspect = (float)Screen.height / (float)Screen.width;
        this.GetComponent<Camera>().orthographicSize = (42 * aspect) / (9f / 16f);
	}
}

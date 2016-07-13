using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PrereleaseNoticeBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void HandleClick()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
}

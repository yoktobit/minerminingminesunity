using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour {

    public Transform playButton;
    public Transform quitButton;

    public Transform selected;

    float lastPressTime;

	// Use this for initialization
	void Start () {
        selected = playButton;
        MinerSaveGame.Instance.Load();
	}

    float lastVert;
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetButtonUp("Submit"))
        {
            if (selected == playButton)
                HandlePlayButton();
            else
                HandleQuitButton();
        }
        float vert = Input.GetAxis("Vertical");
        if (vert != 0 && Mathf.Sign(vert) != Mathf.Sign(lastVert))
        {
            lastVert = Input.GetAxis("Vertical");
            selected = (selected == playButton) ? quitButton : playButton;
        }
        playButton.GetComponent<Text>().color = Color.black;
        quitButton.GetComponent<Text>().color = Color.black;
        selected.GetComponent<Text>().color = Color.red;
    }

    public void HandlePlayButton()
    {
        SceneManager.LoadScene("game");
    }

    public void HandleQuitButton()
    {
        Application.Quit();
    }
}

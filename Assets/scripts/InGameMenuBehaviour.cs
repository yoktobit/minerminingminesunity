using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenuBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        handled = false;
        HandleInGameMenu();
	}

    public Transform activeInGameMenuItem;
    public Transform continueItem;
    public Transform mainMenuItem;
    public Transform quitItem;
    float lastInGameVert = 0;
    bool freshActivated;
    public bool handled = false;
    public float oldTimeScale = 1;

    private void OnEnable()
    {
        freshActivated = true;
        activeInGameMenuItem = continueItem;
        handled = false;
    }

    private void OnDisable()
    {
        handled = false;
    }

    private void HandleInGameMenu()
    {
        if (!gameObject.activeSelf) return;
        bool up, down, submit, cancel;
        up = down = submit = cancel = false;
        float vert = Input.GetAxis("Vertical");
        /*if (Input.GetButtonUp("Up"))
        {
            up = true;
        }
        if (Input.GetButtonUp("Down"))
        {
            down = true;
        }*/
        if (vert < 0 && (Mathf.Sign(vert) != Mathf.Sign(lastInGameVert) || lastInGameVert == 0))
        {
            down = true;
        }
        if (vert > 0 && (Mathf.Sign(vert) != Mathf.Sign(lastInGameVert) || lastInGameVert == 0))
        {
            up = true;
        }
        if (Input.GetButtonUp("Submit"))
        {
            submit = true;
        }
        if (Input.GetButtonUp("Cancel") && !freshActivated)
        {
            cancel = true;
        }

        if (up)
        {
            if (activeInGameMenuItem == continueItem) activeInGameMenuItem = quitItem;
            else if (activeInGameMenuItem == quitItem) activeInGameMenuItem = mainMenuItem;
            else if (activeInGameMenuItem == mainMenuItem) activeInGameMenuItem = continueItem;
            handled = true;
        }
        else if (down)
        {
            if (activeInGameMenuItem == continueItem) activeInGameMenuItem = mainMenuItem;
            else if (activeInGameMenuItem == quitItem) activeInGameMenuItem = continueItem;
            else if (activeInGameMenuItem == mainMenuItem) activeInGameMenuItem = quitItem;
            handled = true;
        }
        else if (submit)
        {
            if (activeInGameMenuItem == continueItem) HandleSubmitInGameMenuContinue();
            if (activeInGameMenuItem == mainMenuItem) GotoMainMenu();
            if (activeInGameMenuItem == quitItem) Application.Quit();
            handled = true;
        }
        else if (cancel)
        {
            gameObject.SetActive(false);
            handled = true;
        }
        quitItem.GetComponent<Text>().color = Color.black;
        mainMenuItem.GetComponent<Text>().color = Color.black;
        continueItem.GetComponent<Text>().color = Color.black;
        activeInGameMenuItem.GetComponent<Text>().color = Color.red;
        lastInGameVert = vert;
        freshActivated = false;
    }

    public void HandleSubmitInGameMenuContinue()
    {
        //Destroy(transform.parent.gameObject);
        //Time.timeScale = oldTimeScale;
        MinerSaveGame.Instance.Current.Paused = false;
        //gameObject.SetActive(false);
        //Debug.Log("UnloadSceneAsync");
        SceneManager.UnloadSceneAsync("InGameMenu");
    }

    public void HandleSubmitInGameMenuQuit()
    {
        Application.Quit();
    }

    public void GotoMainMenu()
    {
        MinerSaveGame.Save();
        SceneManager.LoadSceneAsync("MainMenu");
    }
}

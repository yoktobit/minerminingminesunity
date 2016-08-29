using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class MainMenuScript : MonoBehaviour {

    public GameObject _selected;
    public GameObject Selected
    {
        get
        {
            return _selected;
        }
        set
        {
            _selected = value;
            playButton.GetComponent<Text>().color = Color.black;
            quitButton.GetComponent<Text>().color = Color.black;
            slot1.GetComponent<Text>().color = Color.black;
            slot2.GetComponent<Text>().color = Color.black;
            slot3.GetComponent<Text>().color = Color.black;

            _selected.GetComponent<Text>().color = Color.red;
        }
    }

    float lastPressTime;

    string _state = "mainMenu";
    string State
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
            if (_state == "mainMenu")
            {
                playButton.SetActive(true);
                quitButton.SetActive(true);
                saveGameSelector.SetActive(false);
            }
            else
            {
                playButton.SetActive(false);
                quitButton.SetActive(false);
                saveGameSelector.SetActive(true);
            }
        }
    }

    GameObject playButton;
    GameObject quitButton;
    GameObject slot1;
    GameObject slot2;
    GameObject slot3;
    GameObject slot1Info;
    GameObject slot2Info;
    GameObject slot3Info;
    GameObject saveGameSelector;

    // Use this for initialization
    void Start () {

        playButton = GameObject.Find("PlayButton");
        quitButton = GameObject.Find("QuitButton");
        slot1 = GameObject.Find("Slot1");
        slot2 = GameObject.Find("Slot2");
        slot3 = GameObject.Find("Slot3");
        slot1Info = GameObject.Find("Slot1Info");
        slot2Info = GameObject.Find("Slot2Info");
        slot3Info = GameObject.Find("Slot3Info");
        saveGameSelector = GameObject.Find("SaveGameSelector");

        Selected = playButton;
        MinerSaveGame saveGame = MinerSaveGame.Instance;

        RefreshDateTimeOfSlots();

        saveGameSelector.SetActive(false);
    }

    void RefreshDateTimeOfSlots()
    {
        slot1Info.GetComponent<Text>().text = MinerSaveGame.Instance.minerData[0].SaveDate.HasValue ? MinerSaveGame.Instance.minerData[0].SaveDate.Value.ToShortDateString() + " " + MinerSaveGame.Instance.minerData[0].SaveDate.Value.ToShortTimeString() : "EMPTY";
        slot2Info.GetComponent<Text>().text = MinerSaveGame.Instance.minerData[1].SaveDate.HasValue ? MinerSaveGame.Instance.minerData[1].SaveDate.Value.ToShortDateString() + " " + MinerSaveGame.Instance.minerData[1].SaveDate.Value.ToShortTimeString() : "EMPTY";
        slot3Info.GetComponent<Text>().text = MinerSaveGame.Instance.minerData[2].SaveDate.HasValue ? MinerSaveGame.Instance.minerData[2].SaveDate.Value.ToShortDateString() + " " + MinerSaveGame.Instance.minerData[2].SaveDate.Value.ToShortTimeString() : "EMPTY";
    }

    float lastVert;
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetButtonUp("Submit"))
        {
            if (Selected == playButton) HandlePlayButton();
            else if (Selected == quitButton) HandleQuitButton();
            else if (Selected == slot1) HandleSlot1();
            else if (Selected == slot2) HandleSlot2();
            else if (Selected == slot3) HandleSlot3();
        }
        if (Input.GetButtonUp("Delete"))
        {
            if (Selected == slot1) HandleSlot1Delete();
            else if (Selected == slot2) HandleSlot2Delete();
            else if (Selected == slot3) HandleSlot3Delete();
        }
        if (Input.GetButtonUp("Cancel"))
        {
            if (State != "mainMenu")
            {
                State = "mainMenu";
                Selected = playButton;
            }
        }
        float vert = Input.GetAxis("Vertical");
        if (vert != 0 && (Mathf.Sign(vert) != Mathf.Sign(lastVert) || lastVert == 0))
        {
            if (State == "mainMenu")
            {
                Selected = (Selected == playButton) ? quitButton : playButton;
            }
            else
            {
                if (vert < 0)
                {
                    Selected = (Selected == slot1 ? slot2 : (Selected == slot2 ? slot3 : slot1));
                }
                else
                {
                    Selected = (Selected == slot1 ? slot3 : (Selected == slot2 ? slot1 : slot2));
                }
            }
        }
        lastVert = Input.GetAxis("Vertical");
    }

    private void DeleteSlot(int v)
    {
        MinerSaveGame.Instance.minerData[v] = new MinerData(v);
    }

    public void HandlePlayButton()
    {
        Debug.Log("Select Slots");
        State = "SlotSelect";
        Selected = slot1;
    }

    public void HandleQuitButton()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }

    public void HandleSlot1()
    {
        Debug.Log("Loading Slot 1");
        MinerSaveGame.Instance.Current = MinerSaveGame.Instance.minerData[0];
        LoadSlot();
    }
    public void HandleSlot2()
    {
        Debug.Log("Loading Slot 2");
        MinerSaveGame.Instance.Current = MinerSaveGame.Instance.minerData[1];
        LoadSlot();
    }
    public void HandleSlot3()
    {
        Debug.Log("Loading Slot 3");
        MinerSaveGame.Instance.Current = MinerSaveGame.Instance.minerData[2];
        LoadSlot();
    }

    public void HandleSlot1Delete()
    {
        Debug.Log("Deleting Slot 1");
        DeleteSlot(0);
        RefreshDateTimeOfSlots();
    }
    public void HandleSlot2Delete()
    {
        Debug.Log("Deleting Slot 2");
        DeleteSlot(1);
        RefreshDateTimeOfSlots();
    }
    public void HandleSlot3Delete()
    {
        Debug.Log("Deleting Slot 3");
        DeleteSlot(2);
        RefreshDateTimeOfSlots();
    }
    public void LoadSlot()
    {
        MinerSaveGame.Instance.Current.HasData = true;
        SceneManager.LoadSceneAsync("game");
    }
}

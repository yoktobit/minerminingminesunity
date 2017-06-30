using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using SmartLocalization;

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
            deletePanelYes.GetComponent<Text>().color = Color.black;
            deletePanelNo.GetComponent<Text>().color = Color.black;

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
            string previous = _state;
            _state = value;
            if (_state == "mainMenu")
            {
                playButton.SetActive(true);
                quitButton.SetActive(true);
                saveGameSelector.SetActive(false);
                deletePanel.SetActive(false);
            }
            else if (_state == "SlotSelect")
            {
                playButton.SetActive(false);
                quitButton.SetActive(false);
                saveGameSelector.SetActive(true);
                deletePanel.SetActive(false);
                Selected = slot1;
                if (previous == "Delete")
                {
                    Selected = slotToDelete == 0 ? slot1 : slotToDelete == 1 ? slot2 : slot3;
                }
            }
            else
            {
                Selected = deletePanelNo;
                deletePanelText.GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("DeletePanelText").Replace("{SLOT}", (slotToDelete + 1).ToString());
                deletePanel.SetActive(true);
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
    GameObject deletePanel;
    GameObject deletePanelText;
    GameObject deletePanelYes;
    GameObject deletePanelNo;

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
        deletePanel = GameObject.Find("DeletePanel");
        deletePanelText = GameObject.Find("DeletePanelText");
        deletePanelYes = GameObject.Find("YES");
        deletePanelNo = GameObject.Find("NO");

        deletePanel.SetActive(false);

        Selected = playButton;
        MinerSaveGame saveGame = MinerSaveGame.Instance;

        RefreshDateTimeOfSlots();

        LanguageManager languageManager = LanguageManager.Instance;
        languageManager.OnChangeLanguage += OnLanguageChanged;
        SmartCultureInfo deviceCultureInfo = languageManager.GetDeviceCultureIfSupported();
        languageManager.ChangeLanguage(deviceCultureInfo);

        saveGameSelector.SetActive(false);
    }

    void OnLanguageChanged(LanguageManager languageManager)
    {

    }

    void RefreshDateTimeOfSlots()
    {
        Debug.Log(MinerSaveGame.Instance.minerData[0].SaveDate);
        slot1Info.GetComponent<Text>().text = MinerSaveGame.Instance.minerData[0].SaveDate.HasValue ? MinerSaveGame.Instance.minerData[0].SaveDate.Value.ToShortDateString() + " " + MinerSaveGame.Instance.minerData[0].SaveDate.Value.ToShortTimeString() : "EMPTY";
        slot2Info.GetComponent<Text>().text = MinerSaveGame.Instance.minerData[1].SaveDate.HasValue ? MinerSaveGame.Instance.minerData[1].SaveDate.Value.ToShortDateString() + " " + MinerSaveGame.Instance.minerData[1].SaveDate.Value.ToShortTimeString() : "EMPTY";
        slot3Info.GetComponent<Text>().text = MinerSaveGame.Instance.minerData[2].SaveDate.HasValue ? MinerSaveGame.Instance.minerData[2].SaveDate.Value.ToShortDateString() + " " + MinerSaveGame.Instance.minerData[2].SaveDate.Value.ToShortTimeString() : "EMPTY";
    }

    float lastVert;
    float lastHorz;
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetButtonUp("Submit"))
        {
            if (Selected == playButton) HandlePlayButton();
            else if (Selected == quitButton) HandleQuitButton();
            else if (Selected == slot1) HandleSlot1();
            else if (Selected == slot2) HandleSlot2();
            else if (Selected == slot3) HandleSlot3();
            else if (Selected == deletePanelYes) DeleteSlotConfirm();
            else if (Selected == deletePanelNo) DeleteSlotAbort();
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
        if (Input.GetButtonUp("X"))
        {
            OpenFacebook();
        }
        float vert = Input.GetAxis("Vertical");
        float horz = Input.GetAxis("Horizontal");
        if (vert != 0 && (Mathf.Sign(vert) != Mathf.Sign(lastVert) || lastVert == 0))
        {
            if (State == "mainMenu")
            {
                Selected = (Selected == playButton) ? quitButton : playButton;
            }
            else if (State == "SlotSelect")
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
            else
            {
                Selected = (Selected == deletePanelYes) ? deletePanelNo: deletePanelYes;
                Debug.Log("Selected " + Selected.name);
            }
        }
        if (horz != 0 && (Mathf.Sign(horz) != Mathf.Sign(lastHorz) || lastHorz == 0))
        {
            if (State == "Delete")
            {
                Selected = (Selected == deletePanelYes) ? deletePanelNo : deletePanelYes;
            }
        }
        lastVert = Input.GetAxis("Vertical");
        lastHorz = Input.GetAxis("Horizontal");
    }

    private void DeleteSlot(int v)
    {
        MinerSaveGame.Instance.minerData[v] = new MinerData(v);
    }

    public void HandlePlayButton()
    {
        //Debug.Log("Select Slots");
        State = "SlotSelect";
        RefreshDateTimeOfSlots();
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

    int slotToDelete;
    public void HandleSlot1Delete()
    {
        Debug.Log("Deleting Slot 1");
        slotToDelete = 0;
        State = "Delete";
        RefreshDateTimeOfSlots();
    }
    public void HandleSlot2Delete()
    {
        Debug.Log("Deleting Slot 2");
        slotToDelete = 1;
        State = "Delete";
        RefreshDateTimeOfSlots();
    }
    public void HandleSlot3Delete()
    {
        Debug.Log("Deleting Slot 3");
        slotToDelete = 2;
        State = "Delete";
        RefreshDateTimeOfSlots();
    }

    public void DeleteSlotConfirm()
    {
        DeleteSlot(slotToDelete);
        State = "SlotSelect";
        RefreshDateTimeOfSlots();
        Debug.Log("Deleted Slot " + slotToDelete);
    }
    public void DeleteSlotAbort()
    {
        State = "SlotSelect";
    }
    public void LoadSlot()
    {
        MinerSaveGame.Instance.Current.Migrate();
        MinerSaveGame.Instance.Current.HasData = true;
        MinerSaveGame.Instance.Current.Paused = false;
        SceneManager.LoadSceneAsync("game");
    }
    public void OpenFacebook()
    {
        Application.OpenURL("https://www.facebook.com/minerminingmines/");
    }

    public void OnDestroy()
    {
        if (LanguageManager.HasInstance)
            LanguageManager.Instance.OnChangeLanguage -= OnLanguageChanged;
        save();
    }

    public void save()
    {
        MinerSaveGame.Save();
    }
}

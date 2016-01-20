using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void HandleGameOverClick()
    {
        MinerSaveGame.Instance.minerData[MinerSaveGame.Instance.Current.Slot] = new MinerData(MinerSaveGame.Instance.Current.Slot);
        SceneManager.LoadSceneAsync("MainMenu");
    }
}

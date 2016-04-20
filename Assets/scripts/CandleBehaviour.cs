using UnityEngine;
using System.Collections;

public class CandleBehaviour : MonoBehaviour {

    public InventoryItem candle;

	// Use this for initialization
	void Start () {
        InvokeRepeating("ReduceHealth", 3f, 3f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ReduceHealth()
    {
        candle.Health--;
        if (candle.Health < 0)
        {
            MinerSaveGame.Instance.Current.Candles.Remove(candle);
            Destroy(this.gameObject);
        }
    }
}

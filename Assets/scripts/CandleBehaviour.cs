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
            int x = 0, y = 0;
            RockGroupBehaviour.GetGridPosition(new Vector3(candle.X, candle.Y), false, out x, out y);
            GameObject.Find("RockGroup").GetComponent<RockGroupBehaviour>().SetRocksLight(x, y-1, 0f, MinerData.CANDLERANGE);
            GameObject.Find("RockGroup").GetComponent<RockGroupBehaviour>().UpdateLights();
            Destroy(this.gameObject);
        }
    }
}

using UnityEngine;
using System.Collections;

public class CandleScaleBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Invoke("Flicker", Random.Range(10, 20));
	}
	
	// Update is called once per frame
	void Update () {
        float health = this.transform.parent.GetComponent<CandleBehaviour>().candle.Health / 100;
        this.transform.localPosition = new Vector3(0, 2f * health + 1f, 0);
        Debug.Log("Candle Scale: " + health);
        this.transform.localScale = new Vector3(1, health, 1);
        var flame = this.transform.parent.GetChild(0);
        flame.transform.localPosition = new Vector3(0, 4f * health + 3.5f, 0);
	}

    void Flicker()
    {
        var flame = this.transform.parent.GetChild(0);
        flame.GetComponent<Animator>().Play("item candle flame flickering");
        Invoke("Flicker", Random.Range(10, 20));
    }
}

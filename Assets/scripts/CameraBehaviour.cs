using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {


    public float dampTime = 0.15f;
    private Vector3 velocity = Vector3.zero;
    public Transform target;


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        var camera = GetComponent<Camera>();
        float vertExtent = camera.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        /*float leftBound = bounds.min.x;
        float rightBound = bounds.max.x + horzExtent / 2;
        float bottomBound = bounds.min.y;
        float topBound = bounds.max.y - vertExtent / 2;*/

        float leftBound = 180 - (180 - horzExtent);
        float rightBound = 180 + (180 - horzExtent);
        float bottomBound = -2000 - (2000 - vertExtent);
        float topBound = 200;

        if (target)
        {
            float camX = Mathf.Clamp(target.transform.position.x, leftBound, rightBound);
            float camY = Mathf.Clamp(target.transform.position.y, bottomBound, topBound);
            Vector3 point = camera.WorldToViewportPoint(target.position);
            Vector3 targetPosition = target.position;
            targetPosition.x = camX;
            targetPosition.y = camY;
            Vector3 delta = targetPosition - camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
            Vector3 destination = transform.position + delta;
            //Vector3 destination = new Vector3(camX, camY, transform.position.z);

            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        }
    }
}

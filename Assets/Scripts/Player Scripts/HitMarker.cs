using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitMarker : MonoBehaviour
{
    public float returnTime = 0.01f; //Length of time hitmarker remains on screen before deactivating
    private float hitTime = 0;

    // Update is called once per frame
    void Update()
    {
        if (hitTime <= hitTime + returnTime) //checking time elapsed from hit time to return time
        {
            gameObject.SetActive(false);
        }
    }

    public void IndicateHit()
    {
        hitTime = Time.time;
        gameObject.SetActive(true);

    }
}

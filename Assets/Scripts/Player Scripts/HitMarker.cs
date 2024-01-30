using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitMarker : MonoBehaviour
{
    public float returnTime = 0.01f; //Length of time hitmarker remains on screen before deactivating
    private float hitTime = 0;
    private Image hitMarker;


    private void Start()
    {
        hitMarker = gameObject.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= hitTime + returnTime) //checking time elapsed from hit time to return time
        {
            hitMarker.enabled = false;
        }
    }

    public void IndicateHit()
    {

        hitMarker.enabled = true;
        hitTime = Time.time;

    }
}

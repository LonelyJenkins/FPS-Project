﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public bool isOpen;
    public GameObject[] lights;
    public GameObject[] spawnPoints;
    public int health = 200;
    public bool isBroken = false;
    private Animator anim;
    private GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("isOpen", isOpen);
    }

    public void TakeDamage(int damageValue)
    {
        health -= damageValue;
        if (health <= 0)
        {
            Break();
        }
    }

    private void Break()
    {
        isBroken = true;
        gameManager.doorsRemaining--;

        foreach (GameObject light in lights)
        {
            light.SetActive(false);
        }
        foreach (GameObject spawnPoint in spawnPoints)
        {
            spawnPoint.SetActive(false);
        }

        gameObject.SetActive(false);
    }
}

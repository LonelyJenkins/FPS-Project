using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TDMManager : MonoBehaviour
{
    public GameObject[] friendlySpawnPoints;
    public GameObject[] enemySpawnPoints;
    public GameObject[] ammoPickups;
    public GameObject[] enemies;
    public GameObject[] friendlies;
    public GameObject player;
    public int killValue = 1; //score earned for each kill
    public int maxScore = 70; //score to win
    public int friendlyScore = 0;
    public int enemyScore = 0;
    public int friendlyCount = 0; //max friendlies to spawn/have in game
    public int enemyCount = 0; //max enemies to spawn/have in game
    public bool matchOver;
    public Text friendlyScoreText;
    public Text enemyScoreText;
    public Text deathText;
    public Text matchEnding;


    private PlayerController playerController;

    void Start()
    {
        matchOver = false;
        playerController = player.GetComponent<PlayerController>();
        playerController.isSurvival = false;
        playerController.isTDM = true; //Assigning unique gamemode settings to player controller

        //spawning all NPCs at start of the game
        for (int i = 0; i < enemyCount; i++)
        {
            int enemyIndex = Random.Range(0, enemies.Length);
            int spawnIndex = Random.Range(0, enemySpawnPoints.Length);
            Instantiate(enemies[enemyIndex], enemySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
            enemies[enemyIndex].GetComponent<HumanController>().isAlerted = true;
            enemies[enemyIndex].GetComponent<HumanController>().isTDM = true;
        }

        for (int i = 0; i < friendlyCount; i++)
        {
            int enemyIndex = Random.Range(0, friendlies.Length);
            int spawnIndex = Random.Range(0, friendlySpawnPoints.Length);
            Instantiate(enemies[enemyIndex], friendlySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
        }
    }

    void Update()
    {
        //This will keep track of all data for HUD
        friendlyScoreText.text = ("Survivors: " + friendlyScore);
        enemyScoreText.text = ("Hostiles: " + enemyScore);

        //Check for player death.
        if (playerController.isDead == true && !matchOver)
        {
            deathText.enabled = true;
            deathText.text = ("YOU'VE BEEN KILLED. PRESS SPACE TO RESPAWN");
            return;
        }

        //Ends match based on team scores
        if (friendlyScore >= maxScore || enemyScore >= maxScore)
        {
            EndMatch();
        }
    }

    void EndMatch()
    {
        matchOver = true;
        if (enemyScore > friendlyScore)
        {
            //announce winning team w/ score matchEnding text
        }
        else if (enemyScore < friendlyScore)
        {
            //announce winning team w/ score matchEnding text
        }

        else
        {
            //announce tie matchEnding text
        }

    }

    public void SpawnNext(bool isEnemy)
    {
        if (isEnemy)
        {
            //spawn random enemy prefab at random enemy spawn point
        }

        else
        {
            //spawn friendly prefab at random friendly spawn point
        }
    }
}

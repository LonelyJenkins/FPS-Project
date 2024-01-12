using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TDMManager : MonoBehaviour
{
    [Header("Game Mode Settings")]
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
    [Space]

    [Header("UI Settings")]
    public Text friendlyScoreText;
    public Text enemyScoreText;
    public Text deathText;
    public Text matchEnding;
    public Text losingStatus;
    public Text winningStatus;
    public Text tiedStatus;


    private PlayerController playerController;

    void Start()
    {
        matchOver = false;
        playerController = player.GetComponentInChildren<PlayerController>();
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
            int friendlyIndex = Random.Range(0, friendlies.Length);
            int spawnIndex = Random.Range(0, friendlySpawnPoints.Length);
            Instantiate(friendlies[friendlyIndex], friendlySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
        }
    }

    void Update()
    {
        //This will keep track of all data for HUD
        friendlyScoreText.text = "" + friendlyScore;
        enemyScoreText.text = "" + enemyScore;
        UIStatus(); //UI status switcher

        if (!matchOver) //continues gamemode logic until game is over
        {

            //Check for player death.
            if (playerController.isDead == true)
            {
                deathText.enabled = true;
                return;
            }

            //Ends match based on team scores
            if (friendlyScore >= maxScore || enemyScore >= maxScore)
            {
                EndMatch();
            }
        }
    }

    void EndMatch()
    {
        matchOver = true;
        if (enemyScore > friendlyScore)
        {
            matchEnding.text = "HOSTILES WIN! " + enemyScore + " > " + friendlyScore;
        }
        else if (enemyScore < friendlyScore)
        {
            matchEnding.text = "SURVIVORS WIN! " + enemyScore + " < " + friendlyScore;
        }

        else
        {
            matchEnding.text = "NOBODY WINS IN WAR " + enemyScore + " = " + friendlyScore;
        }

    }

    public void SpawnNext(bool isEnemy)
    {
        if (isEnemy)
        {
            int enemyIndex = Random.Range(0, enemies.Length);
            int spawnIndex = Random.Range(0, enemySpawnPoints.Length);
            Instantiate(enemies[enemyIndex], enemySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
            enemies[enemyIndex].GetComponent<HumanController>().isAlerted = true;
            enemies[enemyIndex].GetComponent<HumanController>().isTDM = true;
        }

        else
        {
            int friendlyIndex = Random.Range(0, friendlies.Length);
            int spawnIndex = Random.Range(0, friendlySpawnPoints.Length);
            Instantiate(friendlies[friendlyIndex], friendlySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
        }
    }

    void UIStatus() //This switches the UI text indicating if player's team is winning, losing, or tied, while match is active
    {
        if (matchOver)
        {
            if (enemyScore > friendlyScore)
            {
                losingStatus.enabled = true;
                winningStatus.enabled = false;
                tiedStatus.enabled = false;
            }
            else if (enemyScore < friendlyScore)
            {
                winningStatus.enabled = true;
                losingStatus.enabled = false;
                tiedStatus.enabled = false;
            }
            else
            {
                tiedStatus.enabled = true;
                winningStatus.enabled = false;
                losingStatus.enabled = false;
            }
        }

        else
        {
            tiedStatus.enabled = false;
            winningStatus.enabled = false;
            losingStatus.enabled = false;
        }
    }
}

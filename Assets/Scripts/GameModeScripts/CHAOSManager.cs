using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CHAOSManager : MonoBehaviour
{
    //logic for chaos mode will go here.
    //Very similar to TDM but with zombie spawner continually spawning in zombies and occasionally boss monsters. Zombies will count towards player kills, however overall team score will remain unaffected.
    //NOT FINAL: Winning game mode may be dependent on how many deaths occur on opposing teams, rather than killcounts. This will be determined soon
    //Add total zombies killed at the end match screen.
    //THERE WILL BE IDLE ZOMBIES SCATTERED ABOUT THE MAP THAT WILL AGGRO WHEN BOTHERED. THIS SHOWCASES IDLE ANIMS
    //DO NOT FORGET TO ADJUST BOSS MONSTER SETTINGS

    [Header("Game Mode Settings")]
    public GameObject[] friendlySpawnPoints;
    public GameObject[] enemySpawnPoints;
    public GameObject[] zombieSpawnPoints;
    public GameObject[] ammoPickups;
    public GameObject[] enemies;
    public GameObject[] friendlies;
    public GameObject[] zombies;
    public GameObject player;
    public int zombieSpawnTime = 10; //how long before new zombie prefab is instantiated
    public int playerKillCount = 0;
    public int playerDeathCount = 0;
    public int friendliesLeft = 100; //How many friendlies are left before match over state is triggered
    public int enemyHumansLeft = 100;//How many enemies are left before match over state is triggered
    public int friendlyCount = 0; //max friendlies to spawn/have in game
    public int enemyCount = 0; //max enemies to spawn/have in game
    public bool matchOver;
    [Space]

    [Header("UI Settings")]
    public Text friendliesLeftText;
    public Text enemiesLeftText;
    public Text playerKillText;
    public Text playerDeathCounter;
    public Text deathText;
    public Text matchEnding;
    public Text losingStatus;
    public Text winningStatus;
    public Text tiedStatus;
    public Slider healthBar;


    private PlayerController playerController;
    private float time = 0;

    void Start()
    {
        matchOver = false;
        playerController = player.GetComponentInChildren<PlayerController>();
        playerController.isSurvival = false;
        playerController.isTDM = false;
        playerController.isChaos = true; //Assigning unique gamemode settings to player controller
        healthBar.maxValue = playerController.maxHealth;// Assigning value to the healthbar

        //spawning all NPCs at start of the game
        for (int i = 0; i < enemyCount; i++)
        {
            int enemyIndex = Random.Range(0, enemies.Length);
            int spawnIndex = Random.Range(0, enemySpawnPoints.Length);
            Instantiate(enemies[enemyIndex], enemySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
            enemies[enemyIndex].GetComponent<HumanController>().isAlerted = true;
            enemies[enemyIndex].GetComponent<HumanController>().isChaos = true;
        }

        for (int i = 0; i < friendlyCount; i++)
        {
            int friendlyIndex = Random.Range(0, friendlies.Length);
            int spawnIndex = Random.Range(0, friendlySpawnPoints.Length);
            Instantiate(friendlies[friendlyIndex], friendlySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
            friendlies[friendlyIndex].GetComponent<SurvivorController>().isChaos = true;
        }
    }

    void Update()
    {
        //This will keep track of all data for HUD
        healthBar.value = playerController.currentHealth;
        friendliesLeftText.text = friendliesLeft + " SURVIVORS ALIVE";
        enemiesLeftText.text = enemyHumansLeft + " ENEMIES ALIVE";
        playerKillText.text = playerKillCount + " KILLS!";
        playerDeathCounter.text = playerDeathCount + " DEATHS!";
        UIStatus(); //UI status switcher

        if (Time.time >= time) //spawning zombies at the set spawnzombie interval
        {
            SpawnZombie();
            time = Time.time + zombieSpawnTime;
        }

        if (!matchOver) //continues gamemode logic until game is over
        {

            //Check for player death.
            if (playerController.isDead == true)
            {
                deathText.enabled = true;
                return;
            }

            //Ends match based on team scores
            if (friendliesLeft <= 0|| enemyHumansLeft <= 0)
            {
                EndMatch();
            }
        }
    }

    void EndMatch()
    {
        matchOver = true;
        if (enemyHumansLeft > friendliesLeft)
        {
            matchEnding.text = "ENEMIES HAVE WON! " + enemyHumansLeft + " have survived the night";
        }
        else if (enemyHumansLeft < friendliesLeft)
        {
            matchEnding.text = "SURVIVORS WIN! " + friendliesLeft + " have survived the night.";
        }

        else
        {
            matchEnding.text = "Everyone has succumbed to the madess.";
        }

        Time.timeScale = 0.1f;

    }

    public void SpawnNext(int isEnemy)
    {
        if (isEnemy == 1)
        {
            int enemyIndex = Random.Range(0, enemies.Length);
            int spawnIndex = Random.Range(0, enemySpawnPoints.Length);
            Instantiate(enemies[enemyIndex], enemySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
            enemies[enemyIndex].GetComponent<HumanController>().isAlerted = true;
            enemies[enemyIndex].GetComponent<HumanController>().isTDM = true;
            Debug.Log("BADDIE HAS SPAWNED");
        }

        else if (isEnemy == 2)
        {
            int friendlyIndex = Random.Range(0, friendlies.Length);
            int spawnIndex = Random.Range(0, friendlySpawnPoints.Length);
            Instantiate(friendlies[friendlyIndex], friendlySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
            Debug.Log("GOOD BOI HAS SPAWNED");
        }
    }

    public void SpawnZombie()
    {
        int zombieIndex = Random.Range(0, zombies.Length);
        int spawnIndex = Random.Range(0, zombieSpawnPoints.Length);
        Instantiate(zombies[zombieIndex], zombieSpawnPoints[spawnIndex].transform.position, Quaternion.identity);
    }

    void UIStatus() //This switches the UI text indicating if player's team is winning, losing, or tied, while match is active
    {
        if (!matchOver)
        {
            if (enemyHumansLeft > friendliesLeft)
            {
                losingStatus.enabled = true;
                winningStatus.enabled = false;
                tiedStatus.enabled = false;
            }
            else if (enemyHumansLeft < friendliesLeft)
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

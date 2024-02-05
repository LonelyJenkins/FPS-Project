using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CHAOSManager : MonoBehaviour
{
    [Header("Game Mode Settings")]
    public GameObject[] friendlySpawnPoints;
    public GameObject[] enemySpawnPoints;
    public GameObject[] zombieSpawnPoints;
    public GameObject[] ammoPickups;
    public GameObject[] enemies;
    public GameObject[] friendlies;
    public GameObject[] zombies;
    public GameObject bossMonster;
    public GameObject player;
    public int zombieSpawnTime = 10; //how long before new zombie prefab is instantiated
    public int bossSpawnChance = 5;//A random integer between 0 and bossSpawnChance is utilized to create variable probability of bossMonster spawns
    public int playerKillCount = 0;
    public int playerDeathCount = 0;
    public int friendliesLeft = 100; //How many friendlies are left before match over state is triggered
    public int enemyHumansLeft = 100;//How many enemies are left before match over state is triggered
    public int friendlyCount = 0; //max friendlies to spawn/have in game
    public int enemyCount = 0; //max enemies to spawn/have in game
    public int zombieKillCounter = 0; //Value stored to show how many zombies were killed  during the match
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
        healthBar.maxValue = playerController.maxHealth;// Assigning value to the healthbar

        //spawning all NPCs at start of the game
        for (int i = 0; i < enemyCount; i++)
        {
            int enemyIndex = Random.Range(0, enemies.Length);
            int spawnIndex = Random.Range(0, enemySpawnPoints.Length);
            Instantiate(enemies[enemyIndex], enemySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
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
        healthBar.value = playerController.currentHealth;
        friendliesLeftText.text = friendliesLeft + " SURVIVORS ALIVE";
        enemiesLeftText.text = enemyHumansLeft + " ENEMIES ALIVE";
        playerKillText.text = playerKillCount + " KILLS!";
        playerDeathCounter.text = playerDeathCount + " DEATHS!";
        UIStatus(); //UI status switcher

        if (Time.time >= time) //spawning zombies at the set spawnzombie interval. Includes random chance of boss monster spawning
        {
            int spawnChance = Random.Range(0, bossSpawnChance+1);
            if (spawnChance == bossSpawnChance)
            {
                SpawnBoss();
            }

            else
            {
                SpawnZombie();
            }

            time = Time.time + zombieSpawnTime;
        }

        if (!matchOver) //continues gamemode logic until game is over
        {

            //Check for player death.
            if (playerController.isDead == true)
            {
                deathText.enabled = true;
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
            matchEnding.text = "ENEMIES HAVE WON! " + enemyHumansLeft + " have survived the night. AND " + zombieKillCounter + " ZEDS WERE KILLED";
        }
        else if (enemyHumansLeft < friendliesLeft)
        {
            matchEnding.text = "SURVIVORS WIN! " + friendliesLeft + " have survived the night. AND " + zombieKillCounter + " ZEDS WERE KILLED";
        }

        else
        {
            matchEnding.text = "Everyone has succumbed to the madness.";
        }

        Time.timeScale = 0.1f;

    }

    public void SpawnNext(int isEnemy)
    {
        if (isEnemy == 1)
        {
            if (enemyHumansLeft <= enemyCount-1) //stops spawning enemies when the humansLeft value is smaller than the max amount allowed on the map. 
            {
                return;
            }

            int enemyIndex = Random.Range(0, enemies.Length);
            int spawnIndex = Random.Range(0, enemySpawnPoints.Length);
            Instantiate(enemies[enemyIndex], enemySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
            Debug.Log("BADDIE HAS SPAWNED");
        }

        else if (isEnemy == 2)
        {
            if (friendliesLeft <= friendlyCount-1) //stops spawning friendlies if the friendliesLeft value is smaller than max amount of friendlies allowed on the map. This intentionally does not account for player.
            {
                return;
            }

            int friendlyIndex = Random.Range(0, friendlies.Length);
            int spawnIndex = Random.Range(0, friendlySpawnPoints.Length);
            Instantiate(friendlies[friendlyIndex], friendlySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
            Debug.Log("GOOD BOI HAS SPAWNED");
        }
    }

    void SpawnZombie()
    {
        int zombieIndex = Random.Range(0, zombies.Length);
        int spawnIndex = Random.Range(0, zombieSpawnPoints.Length);
        Instantiate(zombies[zombieIndex], zombieSpawnPoints[spawnIndex].transform.position, Quaternion.identity);
        ZombieController zombieSettings = zombies[zombieIndex].GetComponent<ZombieController>();
        zombieSettings.isAlerted = true;
        Debug.Log("A ZOMBIE HAS ARRIVED ON SITE");

    }

    void SpawnBoss()
    {
        int spawnIndex = Random.Range(0, zombieSpawnPoints.Length);
        Instantiate(bossMonster, zombieSpawnPoints[spawnIndex].transform.position, Quaternion.identity);
        BossController bossSettings = bossMonster.GetComponent<BossController>();
        bossSettings.isAlerted = true;
        Debug.Log("A BOSS HAS ENTERED THE MATCH");
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

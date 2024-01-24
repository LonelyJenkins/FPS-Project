using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    // AKA SURVIVAL GAME MODE MANAGER

    [Header("Game Mode Settings")]
    public GameObject[] friendlySpawnPoints;
    public GameObject[] ammoSpawnPoints;
    public GameObject[] enemySpawnPoints;
    public GameObject[] ammoPickups;
    public GameObject[] enemies;
    public GameObject[] friendlies;
    public GameObject player;
    public GameObject boss;
    public int playerKillCount = 0;
    public int enemyCount = 0;
    public int bossCount = 0;
    public int startingFriendlyCount;
    public int friendlyCount = 0;
    public int waveNumber = 0;
    public int doorsRemaining = 3;
    public int bossWaveInterval = 4;
    public int roundIntervalTime = 8;
    [Space]

    [Header("UI Settings")]
    public Text waveDisplay;
    public Text enemyCounter;
    public Text doorCounter;
    public Text friendlyCounter;
    public Text playerKillText;
    public Slider healthBar;

    private int lastBossWave;
    private int lastEnemyCount;
    private int lastBossCount;
    private bool isBossWave;
    private bool isEnemyWave;
    private bool allDoorsDestroyed = false;
    private bool roundOver;
    private PlayerController playerController;
    // Start is called before the first frame update
    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        healthBar.maxValue = playerController.maxHealth;//Assigning value to the health bar
        playerController.isTDM = false;
        playerController.isSurvival = true;
        //Assigning unique gamemode settings to player controller
        friendlyCount += startingFriendlyCount;
        //Begins rounds with predetermined amount of friendly NPCs
        waveNumber++;
        //Begins wave count
        lastBossWave = 0;
        //Data used to calculate Boss wave intervals
        NextWave();
    }

    // Update is called once per frame
    void Update()
    {
        //This will keep track of all data for HUD
        healthBar.value = playerController.currentHealth;
        enemyCounter.text = ("Enemies Left: " + enemyCount);
        doorCounter.text = ("Cabins Remaining: " + doorsRemaining);
        friendlyCounter.text = ("Survivors Alive: " + friendlyCount);

        //Check for player death.
        if (playerController.isDead == true)
        {
            waveDisplay.enabled = true;
            waveDisplay.text = ("YOU'VE BEEN KILLED");
            return;
        }

        //Checks if all cabin doors have been destroyed. This will determine if any ammo/friendlies continue to spawn in
        if (doorsRemaining <= 0)
        {
            allDoorsDestroyed = true;
        }

        //Ends round once all enemies have been eliminated
        if (isEnemyWave && enemyCount <= 0 && !roundOver)
        {
            roundOver = true;
            isEnemyWave = false;
            SpawnAmmo();
            SpawnFriendly();
            waveNumber++;
            waveDisplay.enabled = true;
            waveDisplay.text = ("WAVE " + waveNumber + " IS COMING");
            StartCoroutine(RoundInterval());
        }

        //Ends round for Boss Wave if all bosses are elminated
        if (isBossWave && bossCount <= 0 && !roundOver)
        {
            roundOver = true;
            isBossWave = false;
            SpawnAmmo();
            SpawnFriendly();
            waveNumber++;
            waveDisplay.enabled = true;
            waveDisplay.text = ("WAVE " + waveNumber + " IS COMING");
            StartCoroutine(RoundInterval());
        }
    }

    void NextWave()
    {
        //Calculates the amount of enemies to spawn in for current wave
        roundOver = false;
        enemyCount = lastEnemyCount;
        enemyCount += 4;
        lastEnemyCount = enemyCount;
        isEnemyWave = true;

        //Spawns randomly chosen zombie prefabs at randomly chosen spawn points and triggers their isAlerted bool on spawn
        for (int i = 0; i < enemyCount; i++)
        {
            int enemyIndex = Random.Range(0, enemies.Length);
            int spawnIndex = Random.Range(0, enemySpawnPoints.Length);
            Instantiate(enemies[enemyIndex], enemySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
            enemies[enemyIndex].GetComponent<ZombieController>().isAlerted = true;
        }

    }

    void BossWave()
    {
        //Takes data from previous bossWave to determine amount of BossMonsters to spawn in. Also registers the current bossWave for future usage
        roundOver = false;
        lastBossWave = waveNumber;
        bossCount = lastBossCount;
        bossCount += 2;
        lastBossCount = bossCount;
        isBossWave = true;

        //Spawns Boss monster at randomly chosen spawn points and triggers their isAlerted bool
        for (int i = 0; i < bossCount; i++)
        {
            int spawnIndex = Random.Range(0, enemySpawnPoints.Length);
            Instantiate(boss, enemySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
            boss.GetComponent<BossController>().isAlerted = true;
        }
    }

    public void SpawnFriendly()
    {
        //If not all cabin doors are destroyed, begin spawning friendlies
        if (!allDoorsDestroyed)
        {
            foreach (GameObject friendlySpawn in friendlySpawnPoints)
            {
                if(friendlySpawn.activeInHierarchy != false)
                {//this checks if cabin door has been destroyed before attempting to spawn in friendly
                    int dropChance = Random.Range(0, 2);
                    if (dropChance == 1)
                    {
                        friendlyCount++; //updates the friendly NPC count in the HUD
                        int friendlyIndex = Random.Range(0, friendlies.Length); //chooses random friendly prefab
                        Instantiate(friendlies[friendlyIndex], friendlySpawn.transform.position, Quaternion.identity);
                        friendlies[friendlyIndex].GetComponent<SurvivorController>().isSurvival = true;
                    }

                }
            }
        }
    }

    void SpawnAmmo()
    {
        //Spawns ammunition for player in similar fashion to friendlies
        if (!allDoorsDestroyed)
        {
            Debug.Log("AMMO HAS SPAWNED! COLLECT WHILE YOU CAN");
            foreach(GameObject ammoSpawn in ammoSpawnPoints)
            {
                if (ammoSpawn.activeInHierarchy != false)
                {
                    int ammoIndex = Random.Range(0, ammoPickups.Length);
                    Instantiate(ammoPickups[ammoIndex], ammoSpawn.transform.position, Quaternion.identity);
                }
            }
        }

    }

    IEnumerator RoundInterval()
    {
        //Gives player short period to prepare for incoming wave. This also determines if incoming wave should be Normal or Bosswave
        yield return new WaitForSeconds(roundIntervalTime);

        waveDisplay.enabled = false;

        if (waveNumber - bossWaveInterval == lastBossWave)
        {
            BossWave();
        }
        else
        {
            NextWave();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    // AKA SURVIVAL GAME MODE MANAGER

    public GameObject[] friendlySpawnPoints;
    public GameObject[] ammoSpawnPoints;
    public GameObject[] enemySpawnPoints;
    public GameObject[] ammoPickups;
    public GameObject[] enemies;
    public GameObject[] friendlies;
    public GameObject player;
    public GameObject boss;
    public int enemyCount = 0;
    public int bossCount = 0;
    public int startingFriendlyCount;
    public int friendlyCount = 0;
    public int waveNumber = 0;
    public int doorsRemaining = 3;
    public int bossWaveInterval = 4;
    public int roundIntervalTime = 8;
    public Text waveDisplay;
    public Text enemyCounter;
    public Text doorCounter;
    public Text friendlyCounter;

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
        friendlyCount += startingFriendlyCount;
        waveNumber++;
        lastBossWave = 0;
        NextWave();
    }

    // Update is called once per frame
    void Update()
    {
        enemyCounter.text = ("Enemies Left: " + enemyCount);
        doorCounter.text = ("Cabins Remaining: " + doorsRemaining);
        friendlyCounter.text = ("Survivors Alive: " + friendlyCount);

        if (playerController.isDead == true)
        {
            waveDisplay.enabled = true;
            waveDisplay.text = ("YOU'VE BEEN KILLED");
            return;
        }

        if (doorsRemaining <= 0)
        {
            allDoorsDestroyed = true;
        }

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
        roundOver = false;
        enemyCount = lastEnemyCount;
        enemyCount += 4;
        lastEnemyCount = enemyCount;
        isEnemyWave = true;

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
        roundOver = false;
        lastBossWave = waveNumber;
        bossCount = lastBossCount;
        bossCount += 2;
        lastBossCount = bossCount;
        isBossWave = true;

        for (int i = 0; i < bossCount; i++)
        {
            int spawnIndex = Random.Range(0, enemySpawnPoints.Length);
            Instantiate(boss, enemySpawnPoints[spawnIndex].transform.position, Quaternion.identity);
            boss.GetComponent<BossController>().isAlerted = true;
        }
    }

    public void SpawnFriendly()
    {
        if (!allDoorsDestroyed)
        {
            foreach (GameObject friendlySpawn in friendlySpawnPoints)
            {
                if(friendlySpawn.activeInHierarchy != false)
                {
                    int dropChance = Random.Range(0, 2);
                    if (dropChance == 1)
                    {
                        friendlyCount++;
                        int friendlyIndex = Random.Range(0, friendlies.Length);
                        Instantiate(friendlies[friendlyIndex], friendlySpawn.transform.position, Quaternion.identity);
                    }

                }
            }
        }
    }

    void SpawnAmmo()
    {
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

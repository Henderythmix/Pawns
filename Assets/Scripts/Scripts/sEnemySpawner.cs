using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sEnemySpawner : MonoBehaviour
{
    public static sEnemySpawner instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public Transform[] spawnerPositions;
    public Transform[] cachePlaces;
    public float timeBetweenSpawns = 2.5f;
    public float timeInBetweenRounds = 5f;
    public GameObject enemyPrefab;
    public AI[] typesOfAiToSpawn;

    public int minIncreaseRate = 3;
    public int maxIncreaseRate = 6;

    private int min = 4;
    private int max = 6;
    private int spawnCount = 1;

    private int rounds = 1;

    private bool waiting = false;

    private void Start()
    {
        spawnCount = Random.Range(min,max+1);
    }

    private void Update()
    {
        if (!waiting)
        {
            SpawnEnemy();
            spawnCount--;
            if (spawnCount > 0)
                StartCoroutine(WaitSometime(timeBetweenSpawns, false));
            else
            {
                PrepareNextRound();
                StartCoroutine("DisplayTime");
                StartCoroutine(WaitSometime(timeInBetweenRounds, true));
            }
        }
    }

    void SpawnEnemy()
    {
        if (spawnerPositions.Length > 0)
        {
            int randomLocation = Random.Range(0, spawnerPositions.Length);
            //Debug.Log(randomLocation);
            int randomType = Random.Range(0, typesOfAiToSpawn.Length);
            sAiController enemy = Instantiate(enemyPrefab).GetComponent<sAiController>();
            if (typesOfAiToSpawn.Length > 0)
            {
                enemy.aiType = typesOfAiToSpawn[randomType];
            }
            //Debug.Log(spawnerPositions[randomLocation].position);
            // Unity NavMeshAgent issues, cause factoring new positions is to hard.
            enemy.aiAgent.enabled = false;
            enemy.transform.position = spawnerPositions[randomLocation].position;
            enemy.aiAgent.enabled = true;

            int cacheOrPlayer = Random.Range(0, 2);
            if (cacheOrPlayer == 1) 
                enemy.destination = FindClosestTarget(spawnerPositions[randomLocation].position);
            else if (cachePlaces.Length > 0)
                enemy.destination = cachePlaces[Random.Range(0, cachePlaces.Length)];
            else
                Debug.LogError("There isn't any cache locations setup for sEnemySpawner");
        }
        else
            Debug.LogError("There isn't any spawner positions set on sEnemySpawner.");
    }

    public Transform FindClosestTarget(Vector3 position)
    {
        if (LevelManager.instance.playerCharactersSpawned[0] == null) LevelManager.instance.playerCharactersSpawned.Remove(LevelManager.instance.playerCharactersSpawned[0]);
        if (LevelManager.instance.playerCharactersSpawned.Count <= 0)
        {
            LevelManager.instance.LoseGame();
            return null;
        }

        Transform closestTarget = LevelManager.instance.playerCharactersSpawned[0].transform;
        if (LevelManager.instance.playerCharactersSpawned.Count > 0)
        {
            for (int i = 0; i < LevelManager.instance.playerCharactersSpawned.Count; i++)
            {

                if (LevelManager.instance.playerCharactersSpawned.Count == 0) break;
                else if (LevelManager.instance.playerCharactersSpawned[i] == null && LevelManager.instance.playerCharactersSpawned.Count - 1 != i) i++;
                else break;
                // If we've found a playable character closer to the spawn location chosen, we'll go to there instead
                if (Vector3.Distance(LevelManager.instance.playerCharactersSpawned[i].transform.position, position) < Vector3.Distance(closestTarget.position, position))
                    closestTarget = LevelManager.instance.playerCharactersSpawned[i].transform;
            }
        }
        if (LevelManager.instance.currentCache.Count > 0)
        {
            for (int i = 0; i < LevelManager.instance.currentCache.Count; i++)
            {
                if (LevelManager.instance.currentCache[i] != null)
                {
                    Transform cachePos = LevelManager.instance.currentCache[i].transform;
                    if (Vector3.Distance(cachePos.position, position) < Vector3.Distance(closestTarget.position, position))
                        closestTarget = cachePos;
                }
            }
        }
        else LevelManager.instance.LoseGame();

        return closestTarget;
    }
    
    void PrepareNextRound()
    {
        min += minIncreaseRate;
        max += maxIncreaseRate;
        spawnCount = Random.Range(min, max);
    }

    IEnumerator DisplayTime()
    {
        float timer = timeInBetweenRounds;
        cHudManager.instance.endOfWaveTimerText.gameObject.SetActive(true);
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            float displayTimer = Mathf.Round(timer * 10) / 10;
            cHudManager.instance.endOfWaveTimerText.text = "End of Wave Time: " + displayTimer;
            yield return new WaitForEndOfFrame();
        }
        cHudManager.instance.endOfWaveTimerText.gameObject.SetActive(false);
    }

    IEnumerator WaitSometime(float time, bool newRound)
    {
        waiting = true;
        yield return new WaitForSeconds(time);
        if (newRound) { 
            rounds++; 
            cHudManager.instance.roundLevelText.text = "Wave: " + rounds.ToString(); 
        }
        waiting = false;
    }
}

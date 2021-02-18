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
                StartCoroutine(WaitSometime(timeInBetweenRounds, true));
            }
        }
    }

    void SpawnEnemy()
    {
        if (spawnerPositions.Length > 0)
        {
            int randomLocation = Random.Range(0, spawnerPositions.Length);
            int randomType = Random.Range(0, typesOfAiToSpawn.Length);
            sAiController enemy = Instantiate(enemyPrefab).GetComponent<sAiController>();
            if (typesOfAiToSpawn.Length > 0)
            {
                enemy.aiType = typesOfAiToSpawn[randomType];
            }
            
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

    public Transform FindClosestTarget(Vector3 spawnerPos)
    {
        Transform closestEnemy = LevelManager.instance.playerCharactersAlive[0].transform;
        if (LevelManager.instance.playerCharactersAlive.Count > 0)
        {
            for (int i = 0; i < LevelManager.instance.playerCharactersAlive.Count; i++)
            {
                // If we've found a playable character closer to the spawn location chosen, we'll go to there instead
                if (Vector3.Distance(LevelManager.instance.playerCharactersAlive[i].transform.position, spawnerPos) < Vector3.Distance(closestEnemy.position, spawnerPos))
                    closestEnemy = LevelManager.instance.playerCharactersAlive[i].transform;
            }
        }
        return closestEnemy;
    }
    
    void PrepareNextRound()
    {
        min += minIncreaseRate;
        max += maxIncreaseRate;
        spawnCount = Random.Range(min, max);
    }

    IEnumerator WaitSometime(float time, bool newRound)
    {
        waiting = true;
        yield return new WaitForSeconds(time);
        if (newRound) { 
            rounds++; 
            cHudManager.instance.roundLevelText.text = "Round: " + rounds.ToString(); 
        }
        waiting = false;
    }
}

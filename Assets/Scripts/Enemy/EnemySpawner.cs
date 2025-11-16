using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] Transform waypointParent;
    [SerializeField] Transform enemiesParent;
    [SerializeField] GameObject[] enemies;
    [SerializeField] int totalEnemies = 100;        // should be multiple of 10 and at least 10 (敵の総数は10の倍数にして、少なくないようにする必要があります)

    private List<Transform> waypoints = new();
    private int obstacleAvoidancePriority = 1;      // setting priority for obstacle avoidance different for each enemy (敵ごとに障害物回避の優先順位を設定する)

    void Awake()
    {
        if (waypointParent == null || enemies.Length == 0)
        {
            Debug.LogError("Waypoints or enemies not assigned!", this);
            enabled = false;
            return;
        }

        // Cache all waypoints once
        // waypointsを一度にキャッシュ
        foreach (Transform child in waypointParent) waypoints.Add(child);
    }

    void Start()
    {
        // overriding total no. of enemies as per difficulty
        // 難易度に応じて敵の総数を上書き
        DifficultySettings settings = DifficultyManager.Instance.CurrentSettings;
        totalEnemies = settings.totalEnemies;

        // spawning NPCs in batches
        // NPCをベッチで生成
        StartCoroutine(SpawnInBatch());
    }

    void SpawnEnemies(int enemiesToSpawn)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject randomEnemy = GetRandomEnemy();
            Vector3 spawnPos;

            EnemyController enemyController = randomEnemy.GetComponent<EnemyController>();
            GameObject enemy;

            if (enemyController.CurrentEnemyType == EnemyType.Patrollable)
            {
                // getting few random waypoints
                List<Transform> shuffled = GetRandomWaypoints(5);
                spawnPos = shuffled[0].position;

                enemy = Instantiate(randomEnemy, spawnPos, Quaternion.identity, enemiesParent);
                enemy.GetComponent<EnemyPatrol>().SetWaypoints(shuffled.ToArray());
            }
            else
            {
                spawnPos = waypoints[Random.Range(0, waypoints.Count)].position;
                enemy = Instantiate(randomEnemy, spawnPos, Quaternion.identity, enemiesParent);
            }

            // setting obstacle avoidance priority
            enemy.GetComponent<NavMeshAgent>().avoidancePriority = obstacleAvoidancePriority++;
        }
    }

    // get only a few random waypoints to increase performance
    // パフォーマンスを向上させるために、いくつかのランドムなwaypointsを取得
    List<Transform> GetRandomWaypoints(int noOfWaypoints)
    {
        List<Transform> randomWaypoints = new(noOfWaypoints);

        for (int i = 0; i < noOfWaypoints; i++)
        {
            var random = waypoints[Random.Range(0, waypoints.Count)];

            // skipping duplicate waypoints (重複するwaypointsをスキップ)
            if (randomWaypoints.Contains(random)) i--;
            else randomWaypoints.Add(random);
        }

        return randomWaypoints;
    }

    GameObject GetRandomEnemy() => enemies[Random.Range(0, enemies.Length)];

    /*
    * spawning enemies in batches
    * wait for some seconds after each batch
    * for better performance
    * 敵をベッチで生成する
    * 各ベッチの間に少し待つ
    * パフォーマンスを向上させる
    */
    IEnumerator SpawnInBatch()
    {
        int no_of_batches = 10;
        int enemies_per_batch = totalEnemies / no_of_batches;

        for (int i = 0; i < no_of_batches; i++)
        {
            SpawnEnemies(enemies_per_batch);
            yield return new WaitForSeconds(5f);
        }

        yield return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 波次生成器：按"波"生成敌人。
/// 每一波生成 N 只敌人，同一波内每隔 spawnInterval 秒出一只；
/// 波与波之间等待 waveInterval 秒（留给玩家布防）。
/// </summary>
public class WaveSpawner : MonoBehaviour
{
    [Header("敌人预制体（把 Project 里的 Enemy 预制体拖进来）")]
    public EnemyMove enemyPrefab;

    [Header("路径点（和敌人共用同一串路点）")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("每波敌人数量")]
    public int enemiesPerWave = 5;

    [Header("同一波内两只敌人的间隔（秒）")]
    public float spawnInterval = 1f;

    [Header("波与波之间的布防时间（秒）")]
    public float waveInterval = 5f;

    [Header("总共几波")]
    public int totalWaves = 3;

    // Start 在场景开始后调用一次；这里启动协程
    private void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    // 协程：可以"暂停等待"再继续，而不卡住整个游戏
    private IEnumerator SpawnWaves()
    {
        for (int wave = 0; wave < totalWaves; wave++)
        {
            Debug.Log($"第 {wave + 1} 波开始");

            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnOne();
                // 等 spawnInterval 秒再出下一只（游戏照常运行，不卡顿）
                yield return new WaitForSeconds(spawnInterval);
            }

            // 一波出完，等布防时间再开下一波
            yield return new WaitForSeconds(waveInterval);
        }

        Debug.Log("所有波次生成完毕");

        // 等场上敌人全部清空，才算守住
        while (FindObjectsOfType<EnemyHealth>().Length > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Win();
        }
    }

    private void SpawnOne()
    {
        if (enemyPrefab == null || waypoints.Count == 0) return;

        // 在起点实例化一个敌人
        EnemyMove enemy = Instantiate(enemyPrefab, waypoints[0].position, Quaternion.identity);
        // 把路径交给它，它就知道怎么走了
        enemy.waypoints = waypoints;
    }
}

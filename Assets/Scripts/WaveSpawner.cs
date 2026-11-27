using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 波次生成器：按波次生成不同类型的敌人。
/// 敌人种类和波次编排都由下面的代码定义（不序列化），改完直接 Play 生效。
/// </summary>
public class WaveSpawner : MonoBehaviour
{
    [Header("敌人预制体（Project 里的 Enemy 预制体）")]
    public EnemyMove enemyPrefab;

    [Header("路径点（和敌人共用同一串路点）")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("同一波内两只敌人的间隔（秒）")]
    public float spawnInterval = 0.9f;

    [Header("波与波之间的布防时间（秒）")]
    public float waveInterval = 6f;

    private EnemySpec[] specs;
    private int[][] waves;

    private void Start()
    {
        specs = BuildSpecs();
        waves = BuildWaves();
        StartCoroutine(SpawnWaves());
    }

    /// <summary>三种敌人。索引 0/1/2 对应下面的波次编排。</summary>
    private static EnemySpec[] BuildSpecs()
    {
        return new EnemySpec[]
        {
            // 0 蠕虫：标准单位，什么都会一点
            new EnemySpec
            {
                displayName = "Worm",
                health = 100f,
                shield = 0f,
                speed = 2f,
                reward = 10,
                color = new Color(0.9f, 0.25f, 0.25f),
                size = 1f
            },

            // 1 疾行体：血少速度快，容易溜过去，成群出现
            new EnemySpec
            {
                displayName = "Runner",
                health = 55f,
                shield = 0f,
                speed = 3.6f,
                reward = 12,
                color = new Color(1f, 0.6f, 0.15f),
                size = 0.85f
            },

            // 2 护盾体：血厚 + 护盾，护盾未破时免疫减速
            new EnemySpec
            {
                displayName = "Shielded",
                health = 80f,
                shield = 60f,
                speed = 1.5f,
                reward = 20,
                color = new Color(0.55f, 0.45f, 0.95f),
                size = 1.15f
            }
        };
    }

    /// <summary>
    /// 波次编排：每个数组是一波，里面的数字 = 敌人种类索引。
    /// 例如 {0,0,1,2} 表示依次出 2 只蠕虫、1 只疾行体、1 只护盾体。
    /// </summary>
    private static int[][] BuildWaves()
    {
        return new int[][]
        {
            new int[] { 0, 0, 0, 0, 0 },                   // 第 1 波：纯蠕虫，教学
            new int[] { 0, 0, 0, 1, 1 },                   // 第 2 波：加入疾行体
            new int[] { 0, 0, 1, 1, 1, 2 },                // 第 3 波：首次出现护盾体
            new int[] { 1, 1, 1, 1, 0, 0, 2 },             // 第 4 波：疾行体成群
            new int[] { 1, 1, 2, 2, 0, 0, 0 },             // 第 5 波：护盾体加压
            new int[] { 1, 1, 2, 2, 2, 0, 0, 0 }           // 第 6 波：最终波
        };
    }

    private IEnumerator SpawnWaves()
    {
        for (int w = 0; w < waves.Length; w++)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetWave(w + 1, waves.Length);
            }

            Debug.Log("第 " + (w + 1) + " 波开始");

            foreach (int typeIndex in waves[w])
            {
                SpawnOne(typeIndex);
                yield return new WaitForSeconds(spawnInterval);
            }

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

    private void SpawnOne(int typeIndex)
    {
        if (enemyPrefab == null || waypoints.Count == 0) return;

        EnemyMove enemy = Instantiate(enemyPrefab, waypoints[0].position, Quaternion.identity);
        enemy.waypoints = waypoints;

        EnemySpec spec = specs[Mathf.Clamp(typeIndex, 0, specs.Length - 1)];
        enemy.speed = spec.speed;

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.ApplySpec(spec);
        }
    }
}
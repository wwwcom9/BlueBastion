using UnityEngine;

/// <summary>
/// 一种敌人的配置数据。和 TowerSpec 一样是纯数据类：
/// 三种敌人共用同一套脚本，差异全在这些参数里。
/// </summary>
[System.Serializable]
public class EnemySpec
{
    public string displayName = "敌人";

    [Header("生命")]
    public float health = 100f;

    [Header("护盾：先扣护盾再扣血，护盾未破时免疫减速")]
    public float shield = 0f;

    [Header("移动速度")]
    public float speed = 2f;

    [Header("击杀奖励能量")]
    public int reward = 10;

    [Header("治疗：给周围友军回血（0 = 不治疗）")]
    public float healAmount = 0f;

    public float healInterval = 2f;
    public float healRadius = 2.5f;

    [Header("身体颜色")]
    public Color color = Color.red;

    [Header("身体大小")]
    public float size = 1f;
}
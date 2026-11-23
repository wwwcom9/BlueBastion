using UnityEngine;

/// <summary>
/// 一种防御设施的配置数据（纯数据类，不继承 MonoBehaviour）。
/// 三种塔的行为差异全部由这里的参数决定，代码只有一份。
/// </summary>
[System.Serializable]
public class TowerSpec
{
    public string displayName = "防御塔";

    [Header("造价")]
    public int cost = 50;

    [Header("1 级属性")]
    public float damage = 20f;
    public float range = 3.5f;
    public float fireRate = 3f;

    [Header("范围伤害半径（0 = 只有单体伤害）")]
    public float splashRadius = 0f;

    [Range(0f, 1f)]
    public float splashDamageRatio = 0.6f;

    [Header("减速比例与持续时间（0 = 不减速）")]
    [Range(0f, 0.9f)]
    public float slowPercent = 0f;

    public float slowDuration = 0f;

    [Header("范围减速半径（0 = 只减速单体目标）")]
    public float slowRadius = 0f;

    [Header("三个等级的颜色")]
    public Color color1 = Color.blue;
    public Color color2 = Color.cyan;
    public Color color3 = Color.yellow;
}
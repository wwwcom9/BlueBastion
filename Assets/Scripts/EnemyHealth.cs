using UnityEngine;

/// <summary>
/// 敌人的生命值 + 护盾。只负责"受伤、死亡、血条"这一件事。
/// 护盾是特殊机制：先扣护盾，护盾未破时免疫减速。
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("最大生命值")]
    public float maxHealth = 100f;

    [Header("护盾值（0 = 没有护盾）")]
    public float maxShield = 0f;

    [Header("击杀后给玩家的能量")]
    public int rewardEnergy = 10;

    [Header("是否显示头顶血条")]
    public bool showHealthBar = true;

    private float currentHealth;
    private float currentShield;
    private EnemyHealthBar bar;

    public bool IsDead => currentHealth <= 0f;

    /// <summary>护盾还在时免疫减速。</summary>
    public bool HasShield => currentShield > 0f;

    /// <summary>当前血量比例 0~1。</summary>
    public float HealthRatio => maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

    /// <summary>当前护盾比例 0~1。</summary>
    public float ShieldRatio => maxShield > 0f ? Mathf.Clamp01(currentShield / maxShield) : 0f;

    private void Start()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;

        if (showHealthBar)
        {
            bar = EnemyHealthBar.Attach(transform, 0.9f, 0.13f, 0.62f);
            if (bar != null) bar.Bind(this);
        }
    }

    /// <summary>生成时由波次生成器调用，把这只敌人变成指定的种类。</summary>
    public void ApplySpec(EnemySpec spec)
    {
        if (spec == null) return;

        maxHealth = spec.health;
        maxShield = spec.shield;
        rewardEnergy = spec.reward;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = spec.color;

        transform.localScale = Vector3.one * spec.size;
    }

    /// <summary>受到伤害。先扣护盾，溢出部分才扣血。</summary>
    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        float remain = amount;

        if (currentShield > 0f)
        {
            float absorbed = Mathf.Min(currentShield, remain);
            currentShield -= absorbed;
            remain -= absorbed;
        }

        if (remain <= 0f) return;

        currentHealth -= remain;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEnergy(rewardEnergy);
        }

        Destroy(gameObject);
    }
}
using UnityEngine;

/// <summary>
/// 敌人的生命值。只负责"受伤、死亡、血条"这一件事。
/// 移动由 EnemyMove 负责，两个组件互不干扰。
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("最大生命值")]
    public float maxHealth = 100f;

    [Header("击杀后给玩家的能量")]
    public int rewardEnergy = 10;

    [Header("是否显示头顶血条")]
    public bool showHealthBar = true;

    private float currentHealth;
    private EnemyHealthBar bar;

    // 只读属性：外部可以查询，但不能直接改
    public bool IsDead => currentHealth <= 0f;

    /// <summary>当前血量比例 0~1，血条用它来画长度。</summary>
    public float HealthRatio => maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

    private void Start()
    {
        currentHealth = maxHealth;

        if (showHealthBar)
        {
            bar = EnemyHealthBar.Attach(transform, 0.9f, 0.13f, 0.62f);
            if (bar != null) bar.Bind(this);
        }
    }

    /// <summary>受到伤害。amount 为伤害值。</summary>
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth -= amount;

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
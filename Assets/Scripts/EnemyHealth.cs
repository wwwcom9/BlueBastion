using UnityEngine;

/// <summary>
/// 敌人的生命值。只负责"受伤、死亡"这一件事。
/// 移动由 EnemyMove 负责，两个组件互不干扰。
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("最大生命值")]
    public float maxHealth = 100f;

    [Header("击杀后给玩家的能量")]
    public int rewardEnergy = 10;

    private float currentHealth;

    // 只读属性：外部可以查询，但不能直接改
    public bool IsDead => currentHealth <= 0f;

    private void Start()
    {
        currentHealth = maxHealth;
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
        // 给玩家发击杀奖励（死亡音效、特效后面再加）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEnergy(rewardEnergy);
        }

        Destroy(gameObject);
    }
}
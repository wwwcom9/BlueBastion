using UnityEngine;

/// <summary>
/// 子弹：朝目标飞行，到达后造成伤害并销毁。
/// 目标中途死亡（被销毁）时，子弹自己也消失，避免报空引用。
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("飞行速度")]
    public float speed = 10f;

    [Header("伤害")]
    public float damage = 20f;

    private Transform target;
    private EnemyHealth targetHealth;

    /// <summary>由塔在生成子弹时调用，告诉它打谁、打多少。</summary>
    public void SetTarget(Transform t, float dmg)
    {
        target = t;
        damage = dmg;
        targetHealth = t != null ? t.GetComponent<EnemyHealth>() : null;
    }

    private void Update()
    {
        // 目标已经被销毁（比如被别的塔打死了），子弹就没必要存在了
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 朝目标飞行
        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime);

        // 到达目标
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
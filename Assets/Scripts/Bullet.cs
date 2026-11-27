using UnityEngine;

/// <summary>
/// 子弹：朝目标飞行，到达后造成伤害并销毁。
/// 两种互不相关的附加效果，由塔配置（0 表示没有）：
///   范围伤害 —— 命中点周围 splashRadius 内的敌人也受到一部分伤害
///   范围减速 —— 命中点周围 slowRadius 内的敌人全部变慢
/// 两者可以同时存在，也可以只有其中一个。
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("飞行速度")]
    public float speed = 10f;

    [Header("伤害")]
    public float damage = 20f;

    [Header("范围伤害半径（0 = 只有单体）")]
    public float splashRadius = 0f;

    [Range(0f, 1f)]
    public float splashDamageRatio = 0.6f;

    [Header("减速")]
    [Range(0f, 0.9f)]
    public float slowPercent = 0f;

    public float slowDuration = 0f;

    [Header("范围减速半径（0 = 只减速单体）")]
    public float slowRadius = 0f;

    // 特效颜色 = 发射这座塔的颜色
    private Color effectColor = Color.white;

    private Transform target;
    private EnemyHealth targetHealth;

    /// <summary>由塔在生成子弹时调用，告诉它打谁、打多少。</summary>
    public void SetTarget(Transform t, float dmg)
    {
        target = t;
        damage = dmg;
        targetHealth = t != null ? t.GetComponent<EnemyHealth>() : null;
    }

    /// <summary>由塔调用，告诉子弹的范围伤害、减速参数和特效用色。</summary>
    public void ConfigureEffects(float splashR, float splashRatio,
                                 float slowP, float slowD, float slowR, Color color)
    {
        splashRadius = splashR;
        splashDamageRatio = splashRatio;
        slowPercent = slowP;
        slowDuration = slowD;
        slowRadius = slowR;
        effectColor = color;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            Hit(target.position);
            Destroy(gameObject);
        }
    }

    private void Hit(Vector2 impactPoint)
    {
        // 1. 主目标：伤害 + 减速
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
            ApplySlowTo(targetHealth.transform);
        }

        // 2. 范围伤害：命中点周围的敌人受到一部分伤害
        if (splashRadius > 0f)
        {
            // 限流：0.35 秒内同色光圈只出现一次，避免高频塔把画面刷满
            SplashEffect.Spawn(impactPoint, splashRadius, effectColor, 0.28f, 0.8f, 0.35f);

            float splashDamage = damage * splashDamageRatio;

            foreach (EnemyHealth e in FindObjectsOfType<EnemyHealth>())
            {
                if (e == null || e == targetHealth) continue;

                if (Vector2.Distance(e.transform.position, impactPoint) <= splashRadius)
                {
                    e.TakeDamage(splashDamage);
                }
            }
        }

        // 3. 范围减速：命中点周围的敌人全部变慢（不改伤害）
        if (slowRadius > 0f)
        {
            // 没有范围伤害时，用这一圈来显示"减速力场"的作用范围
            if (splashRadius <= 0f)
            {
                SplashEffect.Spawn(impactPoint, slowRadius, effectColor, 0.22f, 0.35f, 0.35f);
            }

            foreach (EnemyHealth e in FindObjectsOfType<EnemyHealth>())
            {
                if (e == null) continue;

                if (Vector2.Distance(e.transform.position, impactPoint) <= slowRadius)
                {
                    ApplySlowTo(e.transform);
                }
            }
        }
    }

    private void ApplySlowTo(Transform t)
    {
        if (slowPercent <= 0f || t == null) return;

        // 护盾是特殊机制：护盾没破之前免疫减速
        EnemyHealth health = t.GetComponent<EnemyHealth>();
        if (health != null && health.HasShield) return;

        EnemyMove move = t.GetComponent<EnemyMove>();
        if (move != null)
        {
            move.ApplySlow(slowPercent, slowDuration);
        }
    }
}
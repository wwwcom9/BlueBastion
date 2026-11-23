using UnityEngine;

/// <summary>
/// 防御塔：每隔一段时间，在攻击范围内找最近的敌人，发射一颗子弹。
/// </summary>
public class Tower : MonoBehaviour
{
    [Header("攻击范围（单位）")]
    public float range = 3.5f;

    [Header("每发子弹伤害")]
    public float damage = 20f;

    [Header("每秒攻击次数")]
    public float fireRate = 1f;

    [Header("子弹预制体")]
    public Bullet bulletPrefab;

    // 距离下次可以开火还剩多少秒
    private float fireCooldown = 0f;

    private void Update()
    {
        // 倒计时，没到 0 就什么都不做
        fireCooldown -= Time.deltaTime;
        if (fireCooldown > 0f) return;

        EnemyHealth target = FindTarget();
        if (target == null) return;   // 范围内没敌人，等下一帧再看

        Fire(target);

        // 重置冷却：每秒 fireRate 次 → 每次间隔 1/fireRate 秒
        fireCooldown = 1f / Mathf.Max(0.01f, fireRate);
    }

    /// <summary>找出范围内距离自己最近的敌人。</summary>
    private EnemyHealth FindTarget()
    {
        EnemyHealth[] all = FindObjectsOfType<EnemyHealth>();

        EnemyHealth best = null;
        float bestDistance = range;

        foreach (EnemyHealth e in all)
        {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d <= bestDistance)
            {
                bestDistance = d;
                best = e;
            }
        }

        return best;
    }

    private void Fire(EnemyHealth target)
    {
        if (bulletPrefab == null) return;

        Bullet b = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        b.SetTarget(target.transform, damage);
    }

    /// <summary>在 Scene 视图里画出攻击范围，方便调数值（只在选中塔时显示）。</summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
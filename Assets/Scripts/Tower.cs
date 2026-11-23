using UnityEngine;

/// <summary>
/// 防御塔：在攻击范围内找最近的敌人并发射子弹。
/// 三种塔共用这一个脚本，区别只在数据（TowerSpec）：
///   脉冲炮 = 单体高频、电磁风暴 = 范围溅射、冻结力场 = 减速。
/// 支持 3 级升级，鼠标悬停时预览下一级的颜色。
/// </summary>
public class Tower : MonoBehaviour
{
    [Header("1 级时的攻击范围")]
    public float range = 3.5f;

    [Header("1 级时每发子弹伤害")]
    public float damage = 20f;

    [Header("1 级时每秒攻击次数")]
    public float fireRate = 1f;

    [Header("子弹预制体")]
    public Bullet bulletPrefab;

    [Header("范围伤害（0 = 单体攻击）")]
    public float splashRadius = 0f;

    [Range(0f, 1f)]
    public float splashDamageRatio = 0.6f;

    [Header("减速（0 = 不减速）")]
    [Range(0f, 0.9f)]
    public float slowPercent = 0f;

    public float slowDuration = 0f;

    [Header("范围减速半径（0 = 只减速单体）")]
    public float slowRadius = 0f;

    [Header("最高等级")]
    public int maxLevel = 3;

    [Header("每升一级的属性倍率")]
    public float damageGrowth = 1.5f;
    public float rangeGrowth = 1.15f;
    public float fireRateGrowth = 1.2f;

    [Header("每升一级放大的比例（外观变化）")]
    public float scaleGrowth = 1.12f;

    [Header("各等级颜色：1 级 / 2 级 / 3 级")]
    public Color[] levelColors = new Color[]
    {
        new Color(0.25f, 0.55f, 1f),
        new Color(0.20f, 0.90f, 0.90f),
        new Color(1f, 0.85f, 0.20f)
    };

    [Header("悬停预览时朝白色提亮的程度（0~1）")]
    [Range(0f, 1f)] public float previewLighten = 0.45f;

    public int Level { get; private set; } = 1;
    public bool CanUpgrade => Level < maxLevel;

    private float fireCooldown = 0f;
    private SpriteRenderer sr;
    private bool previewing = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        ApplyVisual();
    }

    /// <summary>建造时由建造点调用，用配置数据决定这座塔是什么类型。</summary>
    public void ApplySpec(TowerSpec spec)
    {
        if (spec == null) return;

        damage = spec.damage;
        range = spec.range;
        fireRate = spec.fireRate;
        splashRadius = spec.splashRadius;
        splashDamageRatio = spec.splashDamageRatio;
        slowPercent = spec.slowPercent;
        slowDuration = spec.slowDuration;
        slowRadius = spec.slowRadius;

        levelColors = new Color[] { spec.color1, spec.color2, spec.color3 };

        Level = 1;
        ApplyVisual();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        fireCooldown -= Time.deltaTime;
        if (fireCooldown > 0f) return;

        EnemyHealth target = FindTarget();
        if (target == null) return;

        Fire(target);
        fireCooldown = 1f / Mathf.Max(0.01f, fireRate);
    }

    /// <summary>升一级。成功返回 true，已满级返回 false。</summary>
    public bool Upgrade()
    {
        if (!CanUpgrade) return false;

        Level++;

        damage *= damageGrowth;
        range *= rangeGrowth;
        fireRate *= fireRateGrowth;

        ApplyVisual();
        return true;
    }

    /// <summary>鼠标悬停预览：可升级就把颜色提亮成下一级的浅色。</summary>
    public void SetUpgradePreview(bool on)
    {
        previewing = on;
        ApplyVisual();
    }

    private Color GetLevelColor()
    {
        if (levelColors == null || levelColors.Length == 0) return Color.white;
        int i = Mathf.Clamp(Level - 1, 0, levelColors.Length - 1);
        return levelColors[i];
    }

    private Color GetPreviewColor()
    {
        if (levelColors == null || levelColors.Length == 0) return Color.white;

        int next = Mathf.Clamp(Level, 0, levelColors.Length - 1);
        return Color.Lerp(levelColors[next], Color.white, previewLighten);
    }

    private void ApplyVisual()
    {
        if (sr != null)
        {
            sr.color = (previewing && CanUpgrade) ? GetPreviewColor() : GetLevelColor();
        }

        float s = Mathf.Pow(scaleGrowth, Level - 1);
        transform.localScale = new Vector3(s, s, 1f);
    }

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
        // 特效颜色跟随塔的等级色，并稍微提亮，暗背景下更明显
        Color effectTint = Color.Lerp(GetLevelColor(), Color.white, 0.25f);
        b.ConfigureEffects(splashRadius, splashDamageRatio, slowPercent, slowDuration, slowRadius, effectTint);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);

        // 溅射范围用红色圈表示，方便调参
        if (splashRadius > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, splashRadius);
        }
    }
}
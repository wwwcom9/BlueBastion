using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 让物体沿着一串路点依次移动，走到最后一个点后抵达基地。
/// 支持"被减速"：冻结力场命中后会变慢一段时间，并且身体染成冰蓝色。
/// </summary>
public class EnemyMove : MonoBehaviour
{
    [Header("路径点：按顺序把空物体拖进来")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("移动速度（单位/秒）")]
    public float speed = 2f;

    [Header("被减速时的染色")]
    public Color slowTint = new Color(0.45f, 0.75f, 1f, 1f);

    // 当前正走向第几个路点
    private int currentIndex = 0;

    // 减速状态：slowFactor = 1 表示正常速度
    private float slowFactor = 1f;
    private float slowTimer = 0f;

    private SpriteRenderer sr;
    private Color baseColor = Color.red;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // 在 Start 里记录本色：ApplySpec 在 Instantiate 之后才设颜色，
        // 如果写在 Awake 会记到预制体的默认红色。
        if (sr != null) baseColor = sr.color;
    }

    /// <summary>
    /// 施加减速。percent = 0.5 表示速度降到 50%。
    /// 多次命中时取"更强的减速"，并刷新持续时间。
    /// </summary>
    public void ApplySlow(float percent, float duration)
    {
        if (duration <= 0f) return;

        float factor = 1f - Mathf.Clamp01(percent);

        // 已有的减速更强就保留它，但时间照常刷新
        if (slowTimer <= 0f || factor < slowFactor)
        {
            slowFactor = factor;
        }

        slowTimer = Mathf.Max(slowTimer, duration);

        if (sr != null) sr.color = slowTint;
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        // 减速倒计时
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;

            if (slowTimer <= 0f)
            {
                slowFactor = 1f;
                if (sr != null) sr.color = baseColor;
            }
        }

        Transform target = waypoints[currentIndex];
        if (target == null) return;

        float currentSpeed = speed * (slowTimer > 0f ? slowFactor : 1f);

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            currentSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.05f)
        {
            currentIndex++;

            if (currentIndex >= waypoints.Count)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.EnemyLeaked();
                }

                Destroy(gameObject);
                return;
            }
        }
    }
}
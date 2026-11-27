using UnityEngine;

/// <summary>
/// 修复体：脚下有常驻的环绕光圈表示治疗范围，
/// 每隔一段时间给范围内的敌人回血，回血时光圈会亮一下。
/// 只挂在"修复体"这一种敌人身上，由 WaveSpawner 按需添加。
/// </summary>
public class EnemyHealer : MonoBehaviour
{
    [Header("每隔几秒治疗一次")]
    public float healInterval = 2f;

    [Header("每次治疗量")]
    public float healAmount = 12f;

    [Header("治疗半径（只治疗这个范围内的敌人，包括自己）")]
    public float healRadius = 2.5f;

    [Header("光环颜色")]
    public Color effectColor = new Color(0.3f, 1f, 0.5f);

    private float timer;
    private AuraRing aura;

    private void Start()
    {
        aura = AuraRing.Create(transform, healRadius, effectColor, 0.3f);
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        timer += Time.deltaTime;
        if (timer < healInterval) return;

        timer = 0f;
        DoHeal();
    }

    private void DoHeal()
    {
        Vector2 center = transform.position;
        bool healedAny = false;

        foreach (EnemyHealth e in FindObjectsOfType<EnemyHealth>())
        {
            if (e == null || e.IsDead) continue;

            if (Vector2.Distance(e.transform.position, center) <= healRadius)
            {
                if (e.Heal(healAmount)) healedAny = true;
            }
        }

        // 只有真的治到了才亮，避免满血时也闪
        if (healedAny && aura != null) aura.Pulse();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRadius);
    }
}
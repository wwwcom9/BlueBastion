using UnityEngine;

/// <summary>
/// 命中时的范围提示特效：一个从中心向外扩散并淡出的圆环。
/// 贴图由代码实时生成，不需要任何美术素材。
/// 用法：SplashEffect.Spawn(位置, 半径, 颜色);
/// </summary>
public class SplashEffect : MonoBehaviour
{
    // 所有特效共用同一张圆环贴图，避免每次生成都新建
    private static Sprite ringSprite;

    private SpriteRenderer sr;
    private float duration = 0.28f;
    private float radius = 1f;
    private Color tint = Color.white;
    private float elapsed;

    public static void Spawn(Vector2 position, float radius, Color color, float duration = 0.28f)
    {
        GameObject go = new GameObject("SplashEffect");
        go.transform.position = position;

        SpriteRenderer r = go.AddComponent<SpriteRenderer>();
        r.sprite = GetRingSprite();
        r.color = color;
        r.sortingOrder = 5; // 盖在敌人上面

        SplashEffect e = go.AddComponent<SplashEffect>();
        e.sr = r;
        e.radius = radius;
        e.tint = color;
        e.duration = duration;
        e.Init();
    }

    private void Init()
    {
        transform.localScale = Vector3.zero;
        if (sr != null) sr.color = new Color(tint.r, tint.g, tint.b, 0.9f);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float k = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));

        // 从中心扩散到目标直径（sprite 默认 1 世界单位，所以直径 = 2 × radius）
        float scale = Mathf.Lerp(0.15f, radius * 2f, k);
        transform.localScale = new Vector3(scale, scale, 1f);

        if (sr != null)
        {
            float alpha = Mathf.Lerp(0.9f, 0f, k);
            sr.color = new Color(tint.r, tint.g, tint.b, alpha);
        }

        if (k >= 1f) Destroy(gameObject);
    }

    /// <summary>生成一张"边缘亮、中间透明"的圆环贴图。</summary>
    private static Sprite GetRingSprite()
    {
        if (ringSprite != null) return ringSprite;

        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        float center = (size - 1) * 0.5f;
        float maxR = center;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / maxR; // 0 中心 → 1 边缘

                // 在 d≈0.82 处最亮，向内外两侧衰减，形成一圈光环
                float ring = 1f - Mathf.Clamp01(Mathf.Abs(d - 0.82f) / 0.18f);
                float alpha = ring * ring; // 平方让边缘更柔和

                // 圆外和圆心都完全透明
                if (d > 1f) alpha = 0f;

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();

        ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return ringSprite;
    }
}
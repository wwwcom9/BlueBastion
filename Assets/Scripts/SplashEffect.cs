using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 命中时的范围提示特效：一个从中心向外扩散并淡出的圆环。
/// 贴图由代码实时生成，不需要任何美术素材。
///
/// 为了防止高频塔把画面刷满，支持"同一颜色限流"：
/// minInterval 秒内同样的颜色只会出现一次。
/// </summary>
public class SplashEffect : MonoBehaviour
{
    // 所有特效共用同一张圆环贴图
    private static Sprite ringSprite;

    // 每种颜色上次生成特效的时间，用于限流
    private static readonly Dictionary<int, float> lastSpawnTime = new Dictionary<int, float>();

    private SpriteRenderer sr;
    private float duration = 0.28f;
    private float radius = 1f;
    private Color tint = Color.white;
    private float startAlpha = 0.9f;
    private float elapsed;

    /// <summary>
    /// 生成一个扩散光圈。
    /// minInterval &gt; 0 时，相同颜色的特效在这么短时间内不会重复生成。
    /// </summary>
    public static void Spawn(Vector2 position, float radius, Color color,
                             float duration = 0.28f, float alpha = 0.9f, float minInterval = 0f)
    {
        int key = color.GetHashCode();

        if (minInterval > 0f)
        {
            if (lastSpawnTime.TryGetValue(key, out float last))
            {
                if (Time.time - last < minInterval) return;
            }
            lastSpawnTime[key] = Time.time;
        }

        GameObject go = new GameObject("SplashEffect");
        go.transform.position = position;

        SpriteRenderer r = go.AddComponent<SpriteRenderer>();
        r.sprite = GetRingSprite();
        r.color = color;
        r.sortingOrder = 5;

        SplashEffect e = go.AddComponent<SplashEffect>();
        e.sr = r;
        e.radius = radius;
        e.tint = color;
        e.duration = duration;
        e.startAlpha = alpha;
        e.Init();
    }

    private void Init()
    {
        transform.localScale = Vector3.zero;
        if (sr != null) sr.color = new Color(tint.r, tint.g, tint.b, startAlpha);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float k = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));

        float scale = Mathf.Lerp(0.15f, radius * 2f, k);
        transform.localScale = new Vector3(scale, scale, 1f);

        if (sr != null)
        {
            sr.color = new Color(tint.r, tint.g, tint.b, Mathf.Lerp(startAlpha, 0f, k));
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
                float d = Mathf.Sqrt(dx * dx + dy * dy) / maxR;

                float ring = 1f - Mathf.Clamp01(Mathf.Abs(d - 0.82f) / 0.18f);
                float a = ring * ring;

                if (d > 1f) a = 0f;

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply();
        ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return ringSprite;
    }
}
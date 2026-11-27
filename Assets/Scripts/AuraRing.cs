using UnityEngine;

/// <summary>
/// 常驻环绕光圈：挂在修复体脚下，一直存在并缓慢旋转。
/// 每次治疗时会"脉冲"一下（变亮），让玩家看到它在回血。
/// 贴图同样是代码生成的虚线圈，不需要素材。
/// </summary>
public class AuraRing : MonoBehaviour
{
    private static Sprite auraSprite;

    private SpriteRenderer sr;
    private float baseAlpha = 0.25f;
    private float pulse = 0f;

    /// <summary>在 parent 脚下创建一个半径 radius 的常驻光圈。</summary>
    public static AuraRing Create(Transform parent, float radius, Color color, float baseAlpha = 0.25f)
    {
        GameObject go = new GameObject("HealAura");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one * (radius * 2f);

        SpriteRenderer r = go.AddComponent<SpriteRenderer>();
        r.sprite = GetAuraSprite();
        r.color = new Color(color.r, color.g, color.b, baseAlpha);
        r.sortingOrder = -1; // 画在敌人下面，不遮挡

        AuraRing a = go.AddComponent<AuraRing>();
        a.sr = r;
        a.baseAlpha = baseAlpha;
        return a;
    }

    /// <summary>治疗生效时调用，让光圈亮一下。</summary>
    public void Pulse()
    {
        pulse = 1f;
    }

    private void Update()
    {
        // 缓慢旋转，让"环绕"看得出来
        transform.Rotate(0f, 0f, 40f * Time.deltaTime);

        if (pulse > 0f)
        {
            pulse = Mathf.Max(0f, pulse - Time.deltaTime * 2f);
        }

        if (sr == null) return;

        // 基础呼吸 + 治疗脉冲
        float breathe = 0.75f + 0.25f * Mathf.Sin(Time.time * 2.2f);
        float alpha = baseAlpha * breathe + pulse * 0.55f;

        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(alpha));
    }

    /// <summary>生成一张"虚线圆环"贴图（8 段弧），旋转时能看出在转。</summary>
    private static Sprite GetAuraSprite()
    {
        if (auraSprite != null) return auraSprite;

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

                // 环的径向范围：0.78 ~ 1.0
                float inner = Mathf.Clamp01((d - 0.78f) / 0.22f);
                float outer = Mathf.Clamp01((1f - d) / 0.10f);

                // 8 段虚线
                float ang = Mathf.Atan2(dy, dx) / (2f * Mathf.PI) + 0.5f;
                float phase = Mathf.Repeat(ang * 8f, 1f);
                float dashA = phase < 0.55f ? 1f : 0f;

                float a = dashA * inner * outer;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply();
        auraSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return auraSprite;
    }
}
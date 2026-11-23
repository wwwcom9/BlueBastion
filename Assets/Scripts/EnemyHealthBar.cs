using UnityEngine;

/// <summary>
/// 敌人头顶的血条。背景 + 填充两块，全部用代码生成的白色贴图拼出来，
/// 不需要导入任何素材，也不需要手动摆 UI。
/// 由 EnemyHealth 在 Start 时自动挂上去。
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    private static Sprite whiteSprite;

    private EnemyHealth owner;
    private SpriteRenderer fill;
    private float width = 1f;
    private float fillHeight = 0.09f;

    /// <summary>在敌人身上创建一个血条，返回它以便绑定数据。</summary>
    public static EnemyHealthBar Attach(Transform parent, float width, float height, float yOffset)
    {
        GameObject root = new GameObject("HealthBar");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0f, yOffset, 0f);

        Sprite sprite = GetWhiteSprite();

        // 底：深色背景
        GameObject bgGo = new GameObject("BG");
        bgGo.transform.SetParent(root.transform, false);
        SpriteRenderer bg = bgGo.AddComponent<SpriteRenderer>();
        bg.sprite = sprite;
        bg.color = new Color(0f, 0f, 0f, 0.7f);
        bg.sortingOrder = 20;
        bgGo.transform.localScale = new Vector3(width, height, 1f);

        // 填充：会随血量缩短
        GameObject fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(root.transform, false);
        SpriteRenderer fr = fillGo.AddComponent<SpriteRenderer>();
        fr.sprite = sprite;
        fr.color = Color.green;
        fr.sortingOrder = 21;
        fillGo.transform.localScale = new Vector3(width, height * 0.72f, 1f);

        EnemyHealthBar bar = root.AddComponent<EnemyHealthBar>();
        bar.fill = fr;
        bar.width = width;
        bar.fillHeight = height * 0.72f;
        return bar;
    }

    public void Bind(EnemyHealth e)
    {
        owner = e;
    }

    private void LateUpdate()
    {
        if (owner == null || fill == null) return;

        float ratio = owner.HealthRatio;
        float w = Mathf.Max(0.0001f, width * ratio);

        // 从左边开始缩短：中心点要跟着左边缘走
        fill.transform.localScale = new Vector3(w, fillHeight, 1f);
        fill.transform.localPosition = new Vector3(-width * 0.5f + w * 0.5f, 0f, 0f);

        // 满血绿色，残血红色
        fill.color = Color.Lerp(Color.red, Color.green, ratio);
    }

    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite != null) return whiteSprite;

        const int size = 4;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, Color.white);
            }
        }

        tex.Apply();
        whiteSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return whiteSprite;
    }
}
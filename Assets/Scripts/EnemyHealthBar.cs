using UnityEngine;

/// <summary>
/// 敌人头顶的血条 + 护盾条。全部用代码生成的白色贴图拼出来，
/// 不需要导入素材，也不需要建 UI。
/// 护盾条只在敌人有护盾时显示，位置在血条正上方。
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    private static Sprite whiteSprite;

    private EnemyHealth owner;
    private SpriteRenderer hpFill;
    private SpriteRenderer shieldFill;

    private float width = 1f;
    private float hpHeight = 0.09f;
    private float shieldY = 0.06f;

    public static EnemyHealthBar Attach(Transform parent, float width, float height, float yOffset)
    {
        GameObject root = new GameObject("HealthBar");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0f, yOffset, 0f);

        Sprite sprite = GetWhiteSprite();

        // 生命条底
        GameObject bgGo = new GameObject("BG");
        bgGo.transform.SetParent(root.transform, false);
        SpriteRenderer bg = bgGo.AddComponent<SpriteRenderer>();
        bg.sprite = sprite;
        bg.color = new Color(0f, 0f, 0f, 0.7f);
        bg.sortingOrder = 20;
        bgGo.transform.localScale = new Vector3(width, height, 1f);

        // 生命填充
        GameObject hpGo = new GameObject("HP");
        hpGo.transform.SetParent(root.transform, false);
        SpriteRenderer hp = hpGo.AddComponent<SpriteRenderer>();
        hp.sprite = sprite;
        hp.color = Color.green;
        hp.sortingOrder = 21;
        hpGo.transform.localScale = new Vector3(width, height * 0.72f, 1f);

        // 护盾填充（在血条上方，更细）
        GameObject shGo = new GameObject("Shield");
        shGo.transform.SetParent(root.transform, false);
        SpriteRenderer sh = shGo.AddComponent<SpriteRenderer>();
        sh.sprite = sprite;
        sh.color = new Color(0.35f, 0.75f, 1f);
        sh.sortingOrder = 21;
        shGo.transform.localScale = new Vector3(width, height * 0.42f, 1f);
        shGo.transform.localPosition = new Vector3(0f, height * 0.72f, 0f);

        EnemyHealthBar bar = root.AddComponent<EnemyHealthBar>();
        bar.hpFill = hp;
        bar.shieldFill = sh;
        bar.width = width;
        bar.hpHeight = height * 0.72f;
        bar.shieldY = height * 0.72f;
        return bar;
    }

    public void Bind(EnemyHealth e)
    {
        owner = e;
    }

    private void LateUpdate()
    {
        if (owner == null) return;

        // 生命条：从左侧缩短
        if (hpFill != null)
        {
            float r = owner.HealthRatio;
            ApplyFill(hpFill, r, hpHeight, 0f);
            hpFill.color = Color.Lerp(Color.red, Color.green, r);
        }

        // 护盾条：没有护盾的敌人不显示
        if (shieldFill != null)
        {
            if (owner.maxShield <= 0f)
            {
                shieldFill.enabled = false;
            }
            else
            {
                float r = owner.ShieldRatio;
                shieldFill.enabled = r > 0.001f;
                ApplyFill(shieldFill, r, hpHeight * 0.6f, shieldY);
            }
        }
    }

    private void ApplyFill(SpriteRenderer fill, float ratio, float height, float y)
    {
        float w = Mathf.Max(0.0001f, width * Mathf.Clamp01(ratio));
        fill.transform.localScale = new Vector3(w, height, 1f);
        fill.transform.localPosition = new Vector3(-width * 0.5f + w * 0.5f, y, 0f);
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
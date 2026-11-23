using UnityEngine;

/// <summary>
/// 建造总控：保存三种防御设施的配置，处理 1/2/3 键切换。
/// 三种塔共用 1 个预制体，区别只在下面的数据。
///
/// 注意：specs 由代码在 Awake 里生成，不参与场景序列化。
/// 这样改脚本里的数值后，重新 Play 立刻生效，不需要在 Inspector 里重置。
/// </summary>
public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    [System.NonSerialized]
    public TowerSpec[] specs;

    public int SelectedIndex { get; private set; }

    public TowerSpec CurrentSpec
    {
        get
        {
            if (specs == null || specs.Length == 0) return null;
            return specs[Mathf.Clamp(SelectedIndex, 0, specs.Length - 1)];
        }
    }

    private GUIStyle style;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        specs = BuildDefaultSpecs();
    }

    /// <summary>三种塔的完整参数表。要调平衡就改这里。</summary>
    private static TowerSpec[] BuildDefaultSpecs()
    {
        return new TowerSpec[]
        {
            // 1 号：脉冲炮 —— 单体高频，最稳定的输出
            new TowerSpec
            {
                displayName = "Pulse Cannon",
                cost = 50,
                damage = 20f,
                range = 3.5f,
                fireRate = 3f,
                splashRadius = 0f,
                slowPercent = 0f,
                slowRadius = 0f,
                color1 = new Color(0.25f, 0.55f, 1f),
                color2 = new Color(0.20f, 0.90f, 0.90f),
                color3 = new Color(1f, 0.85f, 0.20f)
            },

            // 2 号：电磁风暴 —— 范围爆发，打密集队形
            new TowerSpec
            {
                displayName = "Storm Tower",
                cost = 80,
                damage = 12f,
                range = 3.0f,
                fireRate = 1.2f,
                splashRadius = 1.6f,
                splashDamageRatio = 0.7f,
                slowPercent = 0f,
                slowRadius = 0f,
                color1 = new Color(0.60f, 0.30f, 1f),
                color2 = new Color(0.85f, 0.50f, 1f),
                color3 = new Color(1f, 0.60f, 0.95f)
            },

            // 3 号：冻结力场 —— 高频范围小额伤害 + 范围减速
            new TowerSpec
            {
                displayName = "Frost Field",
                cost = 60,
                damage = 1f,
                range = 4.16f,
                fireRate = 7f,
                splashRadius = 1.8f,
                splashDamageRatio = 1f,
                slowPercent = 0.5f,
                slowDuration = 1.5f,
                slowRadius = 1.8f,
                color1 = new Color(0.30f, 0.85f, 0.85f),
                color2 = new Color(0.60f, 0.95f, 1f),
                color3 = new Color(0.95f, 1f, 1f)
            }
        };
    }

    private void Update()
    {
        if (specs == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectedIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2) && specs.Length > 1) SelectedIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3) && specs.Length > 2) SelectedIndex = 2;
    }

    // 临时调试 HUD：显示当前选中的塔型。正式 UI 做好后删掉。
    private void OnGUI()
    {
        if (specs == null || specs.Length == 0) return;

        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = 18;
        }

        string line = "";
        for (int i = 0; i < specs.Length; i++)
        {
            string mark = (i == SelectedIndex) ? ">> " : "      ";
            line += $"{mark}[{i + 1}] {specs[i].displayName} ({specs[i].cost})     ";
        }

        GUI.Label(new Rect(12f, 78f, 1300f, 28f), line, style);
    }
}
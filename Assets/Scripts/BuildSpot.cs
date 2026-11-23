using System.Collections;
using UnityEngine;

/// <summary>
/// 建造点：地图上可以放塔的位置。
/// 鼠标点一下 → 检查能量 → 够就扣能量并生成塔；不够就闪红提示。
/// </summary>
public class BuildSpot : MonoBehaviour
{
    [Header("建造这座塔要花多少能量")]
    public int buildCost = 50;

    [Header("要生成的塔预制体")]
    public Tower towerPrefab;

    [Header("空位颜色")]
    public Color emptyColor = new Color(0.35f, 0.35f, 0.35f, 0.55f);

    [Header("鼠标悬停颜色")]
    public Color hoverColor = new Color(0.25f, 0.9f, 0.45f, 0.75f);

    [Header("已有塔时的颜色")]
    public Color occupiedColor = new Color(0.15f, 0.15f, 0.15f, 0.45f);

    [Header("能量不足时的闪烁颜色")]
    public Color errorColor = new Color(0.95f, 0.2f, 0.2f, 0.85f);

    private SpriteRenderer sr;
    private Tower currentTower;

    public bool HasTower => currentTower != null;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = emptyColor;
    }

    // 鼠标进入 / 离开：换色，让玩家知道这里可点
    private void OnMouseEnter()
    {
        if (sr != null && !HasTower) sr.color = hoverColor;
    }

    private void OnMouseExit()
    {
        if (sr != null && !HasTower) sr.color = emptyColor;
    }

    // 鼠标点击：核心逻辑
    private void OnMouseDown()
    {
        // 已经有塔了：这里以后接"选中 / 升级"
        if (HasTower)
        {
            Debug.Log("这个位置已经有塔了");
            return;
        }

        if (GameManager.Instance == null || towerPrefab == null) return;

        // TrySpendEnergy 内部会判断够不够，够了才扣
        if (GameManager.Instance.TrySpendEnergy(buildCost))
        {
            currentTower = Instantiate(towerPrefab, transform.position, Quaternion.identity);
            if (sr != null) sr.color = occupiedColor;
            Debug.Log($"建造成功，花费 {buildCost} 能量，剩余 {GameManager.Instance.Energy}");
        }
        else
        {
            // 能量不足：闪一下红色 + 控制台提示
            StartCoroutine(FlashError());
            Debug.Log($"能量不足！需要 {buildCost}，当前只有 {GameManager.Instance.Energy}");
        }
    }

    private IEnumerator FlashError()
    {
        if (sr == null) yield break;

        sr.color = errorColor;
        yield return new WaitForSeconds(0.3f);
        sr.color = emptyColor;
    }
}
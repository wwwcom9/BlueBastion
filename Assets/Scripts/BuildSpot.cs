using System.Collections;
using UnityEngine;

/// <summary>
/// 建造点：地图上可以放塔的位置。
/// 空格子点击 → 花能量按"当前选中的塔型"建造；已有塔点击 → 花能量升级。
/// </summary>
public class BuildSpot : MonoBehaviour
{
    [Header("建造这座塔要花多少能量（没有 BuildManager 时才用）")]
    public int buildCost = 50;

    [Header("要生成的塔预制体")]
    public Tower towerPrefab;

    [Header("升级基础费用")]
    public int upgradeBaseCost = 30;

    [Header("每升一级，费用增长倍率")]
    public float upgradeCostGrowth = 1.6f;

    [Header("空位颜色")]
    public Color emptyColor = new Color(0.35f, 0.35f, 0.35f, 0.55f);

    [Header("空位悬停颜色")]
    public Color hoverColor = new Color(0.25f, 0.9f, 0.45f, 0.75f);

    [Header("已有塔时的颜色")]
    public Color occupiedColor = new Color(0.15f, 0.15f, 0.15f, 0.45f);

    [Header("已有塔时悬停颜色")]
    public Color occupiedHoverColor = new Color(0.9f, 0.75f, 0.2f, 0.7f);

    [Header("能量不足时的闪烁颜色")]
    public Color errorColor = new Color(0.95f, 0.2f, 0.2f, 0.85f);

    private SpriteRenderer sr;
    private Tower currentTower;
    private bool isHovering;

    public bool HasTower => currentTower != null;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = emptyColor;
    }

    private void OnMouseEnter()
    {
        isHovering = true;

        if (HasTower)
        {
            if (currentTower != null) currentTower.SetUpgradePreview(true);
            if (sr != null) sr.color = occupiedHoverColor;
            return;
        }

        if (sr != null) sr.color = hoverColor;
    }

    private void OnMouseExit()
    {
        isHovering = false;

        if (HasTower)
        {
            if (currentTower != null) currentTower.SetUpgradePreview(false);
            if (sr != null) sr.color = occupiedColor;
            return;
        }

        if (sr != null) sr.color = emptyColor;
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance == null) return;

        if (HasTower)
        {
            TryUpgrade();
            return;
        }

        TryBuild();
    }

    /// <summary>当前等级的升级费用。</summary>
    public int GetUpgradeCost()
    {
        if (currentTower == null) return 0;
        return Mathf.RoundToInt(upgradeBaseCost * Mathf.Pow(upgradeCostGrowth, currentTower.Level - 1));
    }

    private void TryBuild()
    {
        if (towerPrefab == null) return;

        // 从建造总控取"当前选中的塔型"；没有 BuildManager 时退回自身配置
        TowerSpec spec = BuildManager.Instance != null ? BuildManager.Instance.CurrentSpec : null;
        int cost = spec != null ? spec.cost : buildCost;

        if (!GameManager.Instance.TrySpendEnergy(cost))
        {
            StartCoroutine(FlashError());
            Debug.Log("能量不足！建造需要 " + cost + "，当前 " + GameManager.Instance.Energy);
            return;
        }

        currentTower = Instantiate(towerPrefab, transform.position, Quaternion.identity);

        if (spec != null)
        {
            currentTower.ApplySpec(spec);
        }

        if (sr != null) sr.color = occupiedColor;

        string towerName = spec != null ? spec.displayName : "Tower";
        Debug.Log("建造 " + towerName + "，花费 " + cost + "，剩余 " + GameManager.Instance.Energy);
    }

    private void TryUpgrade()
    {
        if (currentTower == null) return;

        if (!currentTower.CanUpgrade)
        {
            Debug.Log("已经是最高等级 " + currentTower.Level + " 级");
            return;
        }

        int cost = GetUpgradeCost();

        if (!GameManager.Instance.TrySpendEnergy(cost))
        {
            StartCoroutine(FlashError());
            Debug.Log("能量不足！升级需要 " + cost + "，当前 " + GameManager.Instance.Energy);
            return;
        }

        currentTower.Upgrade();

        // 鼠标还停在格子上时，立刻按新等级重新预览
        if (isHovering)
        {
            currentTower.SetUpgradePreview(true);
            if (sr != null) sr.color = occupiedHoverColor;
        }

        Debug.Log("升级到 " + currentTower.Level + " 级，花费 " + cost + "，剩余 " + GameManager.Instance.Energy);
    }

    private IEnumerator FlashError()
    {
        if (sr == null) yield break;

        Color before = HasTower ? occupiedColor : emptyColor;
        sr.color = errorColor;
        yield return new WaitForSeconds(0.3f);
        sr.color = before;
    }
}
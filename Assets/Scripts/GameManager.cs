using UnityEngine;

/// <summary>
/// 全局游戏状态管理：能量、基地生命、胜负。
/// 单例（Singleton）：整个场景只有一份，任何脚本都能通过 GameManager.Instance 访问。
/// </summary>
public class GameManager : MonoBehaviour
{
    // 静态属性：外部用 GameManager.Instance 拿到唯一实例
    public static GameManager Instance { get; private set; }

    [Header("开局能量")]
    public int startEnergy = 100;

    [Header("基地最大生命")]
    // 注意：必须小于"全场敌人数"，否则永远输不了，游戏就没有压力
    public int maxBaseHealth = 10;

    [Header("敌人抵达终点扣多少血")]
    public int leakDamage = 1;

    public int Energy { get; private set; }
    public int BaseHealth { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsWin { get; private set; }

    private GUIStyle hudStyle;

    private void Awake()
    {
        // 保证只有一个 GameManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 每次开始都恢复正常时间流速，避免上次失败留下 timeScale = 0
        Time.timeScale = 1f;

        Energy = startEnergy;
        BaseHealth = maxBaseHealth;
    }

    /// <summary>击杀敌人、通关奖励时加能量。</summary>
    public void AddEnergy(int amount)
    {
        if (IsGameOver) return;
        Energy += amount;
    }

    /// <summary>尝试花能量。够就扣掉返回 true，不够返回 false。</summary>
    public bool TrySpendEnergy(int amount)
    {
        if (Energy < amount) return false;
        Energy -= amount;
        return true;
    }

    /// <summary>有敌人抵达终点时调用。</summary>
    public void EnemyLeaked()
    {
        if (IsGameOver) return;

        BaseHealth -= leakDamage;
        Debug.Log($"敌人突破防线！基地剩余 {BaseHealth}");

        if (BaseHealth <= 0)
        {
            BaseHealth = 0;
            IsGameOver = true;
            Debug.Log("基地被摧毁，游戏失败");
            StopGameplay();
        }
    }

    /// <summary>全部波次清空后调用。</summary>
    public void Win()
    {
        if (IsGameOver || IsWin) return;
        IsWin = true;
        Debug.Log("防守成功，游戏胜利");
        StopGameplay();
    }

    /// <summary>
    /// 停止玩法：把时间流速设为 0，所有 Time.deltaTime 变成 0，
    /// 敌人不再移动、塔不再开火、协程也暂停。
    /// 后面做了正式的结算界面后，这里会改成弹面板 + 暂停。
    /// </summary>
    private void StopGameplay()
    {
        Time.timeScale = 0f;
    }

    // 临时调试面板：正式 UI 做好后会替换掉。
    // 注意用英文，因为 Unity 默认字体不含中文字形。
    private void OnGUI()
    {
        if (hudStyle == null)
        {
            hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 22 };
        }

        int alive = FindObjectsOfType<EnemyHealth>().Length;
        GUI.Label(new Rect(12, 10, 800, 30),
            $"HP {BaseHealth}/{maxBaseHealth}    Energy {Energy}    Enemies {alive}", hudStyle);

        if (IsGameOver) GUI.Label(new Rect(12, 46, 800, 30), "GAME OVER", hudStyle);
        if (IsWin) GUI.Label(new Rect(12, 46, 800, 30), "VICTORY", hudStyle);
    }
}
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 让物体沿着一串路点依次移动，走到最后一个点后回到起点循环。
/// 这是塔防游戏里"敌人沿路线前进"的最小实现。
/// </summary>
public class EnemyMove : MonoBehaviour
{
    [Header("路径点：按顺序把空物体拖进来")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("移动速度（单位/秒）")]
    public float speed = 2f;

    // 当前正走向第几个路点
    private int currentIndex = 0;

    // Unity 每帧会调用一次 Update
    private void Update()
    {
        // 没有配置路径就直接返回，避免空引用报错
        if (waypoints == null || waypoints.Count == 0) return;

        Transform target = waypoints[currentIndex];
        if (target == null) return;

        // 朝目标点移动这一步：长度 = 速度 × 本帧耗时
        // 用 MoveTowards 而不是直接赋值，保证不会越过目标点
        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime);

        // 距离足够近就认为到达，切换到下一个路点
        // 用 0.05 的容差，避免因为浮点误差永远到不了
        if (Vector2.Distance(transform.position, target.position) < 0.05f)
        {
            currentIndex++;

            // 走完最后一个点 = 抵达终点（基地）
            if (currentIndex >= waypoints.Count)
            {
                // 通知 GameManager 扣基地血量，然后销毁自己
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.EnemyLeaked();
                }

                Destroy(gameObject);
                return;
            }
        }
    }
}

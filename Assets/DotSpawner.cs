using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DotSpawner : MonoBehaviour
{
    [Header("生成参数")]
    public GameObject dotPrefab; // 点的预制体
    public int maxDots = 5; // 最大同时存在的点数量
    public float spawnInterval = 1f; // 生成间隔（秒）
    public float dotSpeedMin = 100f; // 点的最小移动速度
    public float dotSpeedMax = 300f; // 点的最大移动速度
    public float dotLifeMin = 5f; // 点的最小存活时间
    public float dotLifeMax = 10f; // 点的最大存活时间

    [Header("控制开关")]
    public bool isSpawnActive = true; // 是否允许生成红点（控制生成开关）

    private RectTransform backgroundRect;
    private float spawnTimer; // 生成计时器
    private int currentDotCount; // 当前点数量
    private List<GameObject> spawnedDots = new List<GameObject>(); // 存储所有生成的红点引用

    void Start()
    {
        backgroundRect = GetComponent<RectTransform>();
        spawnTimer = 0;
        currentDotCount = 0;
        isSpawnActive = true; // 初始允许生成
    }

    void Update()
    {
        if (!isSpawnActive) return;

        // 计时生成点
        spawnTimer += Time.deltaTime;
        // 关键修复：生成前先清理列表中的空引用，确保计数准确
        CleanupNullDots();
        if (spawnTimer >= spawnInterval && currentDotCount < maxDots)
        {
            SpawnDot();
            spawnTimer = 0;
        }
    }

    // 新增：清理列表中的空引用，并同步更新计数
    private void CleanupNullDots()
    {
        // 反向遍历删除空引用，避免遍历过程中列表长度变化导致的错误
        for (int i = spawnedDots.Count - 1; i >= 0; i--)
        {
            if (spawnedDots[i] == null)
            {
                spawnedDots.RemoveAt(i);
                // 确保计数和实际存在的Dot数量一致
                if (currentDotCount > 0)
                {
                    currentDotCount--;
                }
            }
        }
    }

    // 随机生成点
    void SpawnDot()
    {
        // 计算背景Image的局部坐标范围（随机生成位置）
        Rect rect = backgroundRect.rect;
        float randomX = Random.Range(-rect.width / 2, rect.width / 2);
        float randomY = Random.Range(-rect.height / 2, rect.height / 2);
        Vector2 spawnPos = new Vector2(randomX, randomY);

        // 实例化点，父物体设为BackgroundImage（确保在背景范围内）
        GameObject dot = Instantiate(dotPrefab, backgroundRect);
        dot.GetComponent<RectTransform>().anchoredPosition = spawnPos;

        // 随机设置点的移动速度和存活时间
        DotMovement movement = dot.GetComponent<DotMovement>();
        if (movement == null)
        {
            Debug.LogError("Dot预制体缺少DotMovement组件！");
            Destroy(dot);
            return;
        }
        movement.moveSpeed = Random.Range(dotSpeedMin, dotSpeedMax);
        movement.lifeTime = Random.Range(dotLifeMin, dotLifeMax);

        spawnedDots.Add(dot);
        currentDotCount++;

        // 优化：移除原来的onDestroy回调，改为通过CleanupNullDots统一管理
        // 避免回调延迟导致的计数不同步
    }

    public void ClearAllDotsAndStopSpawn()
    {
        // 1. 停止生成新红点
        isSpawnActive = false;

        // 2. 立即清空计数（关键修复）
        currentDotCount = 0;

        // 3. 遍历销毁所有已生成的红点
        foreach (GameObject dot in spawnedDots)
        {
            if (dot != null) // 避免销毁已不存在的物体
            {
                Destroy(dot);
            }
        }
        // 清空列表
        spawnedDots.Clear();
        spawnTimer = 0;

        
    }

    public void RestartDotSpawn()
    {
        // 1. 先清理残留的空引用（关键修复）
        CleanupNullDots();

        // 2. 重置所有状态变量
        spawnTimer = 0;      // 重置生成计时器
        isSpawnActive = true;// 恢复生成开关

        // 3. 生成红点时，计算当前实际存在的数量，只补到maxDots（关键修复）
        int dotsToSpawn = maxDots - currentDotCount;
        for (int i = 0; i < dotsToSpawn; i++)
        {
            SpawnDot();
        }

        
    }
}
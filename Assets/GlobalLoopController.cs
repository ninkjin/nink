using UnityEngine;

public class GlobalLoopController : MonoBehaviour
{
    public static GlobalLoopController Instance;

    [Header("循环配置")]
    public float loopInterval = 2f; // 循环间隔（秒）
    public bool isLoopActive = true; // 是否启用循环

    private float loopTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!isLoopActive) return;

        // 循环计时器
        loopTimer += Time.deltaTime;
        if (loopTimer >= loopInterval)
        {
            // 执行你要循环的逻辑（替换为自己的代码）
            ExecuteGlobalLoopLogic();

            loopTimer = 0; // 重置计时器，循环执行
        }
    }

    // 自定义：你要循环执行的全局逻辑
    private void ExecuteGlobalLoopLogic()
    {
        
    }

    // 外部调用：控制循环
    public void StartLoop() => isLoopActive = true;
    public void StopLoop() => isLoopActive = false;
    public void SetLoopInterval(float newInterval) => loopInterval = newInterval;
}
using UnityEngine;
using TMPro;

public class UIBaseCountdown : MonoBehaviour
{
    [Header("初始倒计时（秒）")]
    public int initialTime = 60;
    private int currentTime;
    public int bestTime; 

    // 标记是否正在倒计时
    private bool isCounting = false;
    // 计时用的累加器（用于每秒减少时间）
    private float timerAccumulator = 0f;

    private TMP_Text countdownText;

    void Start()
    {
        countdownText = GetComponent<TMP_Text>();
        // 初始化时间，但不开始倒计时
        currentTime = initialTime;
        UpdateUI();

        // 继续监听外部减少时间的事件
        CountdownEvents.OnReduceTime += ReduceTime;
    }

    void Update()
    {

        

        // 只有在倒计时启动后，才进行自动计时（如果需要每秒自动减少的话）
        if (isCounting && currentTime > 0)
        {
            timerAccumulator += Time.deltaTime;
            // 每满1秒，减少1秒倒计时
            if (timerAccumulator >= 1f)
            {
                timerAccumulator -= 1f;
                currentTime--;
                UpdateUI();

                // 检查是否结束
                if (currentTime <= 0)
                {
                    EndCountdown();
                }
            }
        }
    }

    public int Bestime(int bestime)
    {
        bestime = initialTime - currentTime;
        return bestime;
    }

    public void ResetCountTime()
    {
        currentTime = initialTime;
            UpdateUI();
    }

    // 外部调用此方法启动倒计时
    public void StartCountdown()
    {
        if (!isCounting) // 防止重复启动
        {
            isCounting = true;
            timerAccumulator = 0f; // 重置计时器
           
        }
    }

    // 停止倒计时（可选，供外部调用）
    public void StopCountdown()
    {
        isCounting = false;
    }

    // 倒计时结束处理
    private void EndCountdown()
    {
        isCounting = false;
        Debug.Log("倒计时结束,游戏失败");
        // 在这里添加游戏结束逻辑（如加载失败界面）
    }

    // 外部事件触发减少时间（如预制体碰撞）
    private void ReduceTime(int amount)
    {
        // 只有在倒计时启动后，才响应减少事件
        if (isCounting)
        {
            currentTime = Mathf.Max(0, currentTime - amount);
            UpdateUI();

            if (currentTime <= 0)
            {
                EndCountdown();
            }
        }
    }

    private void UpdateUI()
    {
        countdownText.text = $"Time: {currentTime}";
    }

    void OnDestroy()
    {
        CountdownEvents.OnReduceTime -= ReduceTime;
    }
}
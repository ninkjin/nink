using UnityEngine;
using TMPro;
using Unity.VisualScripting;

// 得分变更的事件定义（和你倒计时的事件逻辑保持一致）
public static class ScoreEvents
{
    // 定义得分变更事件（参数：增减的分数值）
    public static event System.Action<int> OnChangeScore;
    // 外部调用此方法触发得分变更
    public static void TriggerChangeScore(int amount)
    {
        OnChangeScore?.Invoke(amount);
    }
}

public class UIBaseScore : MonoBehaviour
{
    [Header("初始分数")]
    public int initialScore = 0;
    [Header("分数下限（默认0，防止负分）")]
    public int minScore = 0;
    [Header("分数上限（0表示无上限）")]
    public int maxScore = 60;

    //记录最小时间
    private int recordmintime = 10000;

    // 当前分数
    private int currentScore;
    // 分数显示的Text组件
    private TMP_Text scoreText;

    //打开一个游戏结束画面UI
    public GameObject gameoverui;

    public DotSpawner dotSpawner;

    public UIBaseCountdown uIBaseCountdown;

    public TMP_Text bestTime;

    void Start()
    {
        // 获取UI上的Text组件（确保新UI上有TMP_Text组件）
        scoreText = GetComponent<TMP_Text>();
        // 初始化分数
        currentScore = initialScore;
        // 更新初始UI显示
        UpdateScoreUI();

        // 监听外部的得分变更事件（和倒计时的事件逻辑对齐）
        ScoreEvents.OnChangeScore += ChangeScore;
    }

    private void opengameoverui()
    {
        if (gameoverui != null)
        {
            gameoverui.SetActive(true);

            return;
        }
    }


    public void SetScore(int newScore)
    {
        // 限制分数在上下限之间
        currentScore = ClampScore(newScore);
        UpdateScoreUI();
    }

    /// <summary>
    /// 得分变更核心逻辑（响应事件/外部调用）
    /// </summary>
    /// <param name="amount">加分传正数，减分传负数</param>
    private void ChangeScore(int amount)
    {
        // 计算新分数并限制上下限
        int newScore = currentScore + amount;
        currentScore = ClampScore(newScore);

        // 更新UI显示
        UpdateScoreUI();

        // 可选：分数变更后的回调（比如满分提示、分数过低提示）
        OnScoreChanged(amount);
    }

    /// <summary>
    /// 限制分数在上下限范围内
    /// </summary>
    private int ClampScore(int score)
    {
        if (maxScore > 0) // 0表示无上限
        {
            return Mathf.Clamp(score, minScore, maxScore);
        }
        else
        {
            return Mathf.Max(score, minScore);
        }
    }

    /// <summary>
    /// 更新UI上的分数显示
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"GOAL:{currentScore}";
            // 可选：自定义显示格式，比如补零（Score: 00100）
            // scoreText.text = $"Score: {currentScore:D5}";
        }
        else
        {
            Debug.LogWarning("未找到TMP_Text组件，请检查UI挂载！");
        }
    }

    /// <summary>
    /// 分数变更后的自定义逻辑（可扩展）
    /// </summary>
    /// <param name="changeAmount">本次变更的分数</param>
    private void OnScoreChanged(int changeAmount)
    {
        if (changeAmount > 0)
        {
            
            
        }
        else if (changeAmount < 0)
        {
            Debug.Log($"减分{Mathf.Abs(changeAmount)}，当前分数：{currentScore}");
           
        }

        // 示例：分数达到上限的提示
        if (maxScore > 0 && currentScore == maxScore)
        {
            //弹出GAMEOVER的画面
            
            opengameoverui();
            dotSpawner.ClearAllDotsAndStopSpawn();
            uIBaseCountdown.StopCountdown();
            int bestrecordtime= uIBaseCountdown.Bestime(0);
            recordmintime=Mathf.Min(bestrecordtime,recordmintime);
            bestTime.text = $"Time: {recordmintime}";
            currentScore = 0;
            UpdateScoreUI();

        }

// 示例：分数低于下限的提示
if (currentScore == minScore)
        {
            
        }
    }

    /// <summary>
    /// 外部调用：重置分数为初始值
    /// </summary>
    public void ResetScore()
    {
        currentScore = initialScore;
        UpdateScoreUI();
        Debug.Log("分数已重置！");
    }

    // 防止内存泄漏，销毁时移除事件监听
    void OnDestroy()
    {
        ScoreEvents.OnChangeScore -= ChangeScore;
    }
}

using UnityEngine.Events;

// 定义一个事件类型，参数为减少的秒数（可选）
public static class CountdownEvents
{
    // 当需要减少倒计时时触发的事件（参数：减少的秒数）
    public static UnityAction<int> OnReduceTime;
}
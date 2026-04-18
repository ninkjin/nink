using UnityEngine;
using UnityEngine.UI;

public class DotMovement : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 200f; // 移动速度（UI单位/秒）
    public float lifeTime = 3f; // 存活时间（秒）

    [Header("覆盖消除参数（新增）")]
    public float coverDestroyTime = 0.5f; // 被方框覆盖后消除所需时间（可调整）
    public int scoreAdd = 10; // 消除该红点加的分数（可调整）

    [Header("覆盖防抖参数（修复反复跳）")]
    public float coverStableTime = 0.05f; // 防抖时间（0.1秒即可）
    private float unstableTimer; // 防抖计时器

    private RectTransform rectTransform;
    private Vector2 moveDirection; // 移动方向
    private Rect parentRect; // 背景Image的矩形范围（碰壁边界）
    private float timer; // 生命周期计时器
    private float coverTimer; // 覆盖消除计时器
    private bool isCovered = false; // 是否被方框覆盖
    private bool isDestroyed = false; // 是否已被消除（防止重复处理）
    private bool isCoverDestroy = false;
    public System.Action onDestroy;

    void OnDestroy()
    {
        onDestroy?.Invoke();
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // 获取背景Image的矩形范围（作为活动边界）
        parentRect = transform.parent.GetComponent<RectTransform>().rect;

        // 随机初始移动方向（上下左右随机）
        float angle = Random.Range(0f, Mathf.PI * 2f); // 0~360度随机角度
        moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

        // 初始化计时器
        timer = 0;
        coverTimer = 0;

        isCovered = false;
        unstableTimer = 0;
        isDestroyed = false;
        isCoverDestroy = false; // 初始化覆盖消除标记
    }

    void Update()
    {
        // 已消除则不再执行任何逻辑
        if (isDestroyed) return; 

        // 移动点
        rectTransform.anchoredPosition += moveDirection * moveSpeed * Time.deltaTime;

        // 检测是否碰到背景边界（碰壁反弹）
        CheckWallCollision();

        // 生命周期计时，到时间删除
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            isCoverDestroy = false; // 自然消失不加分
            DestroySelf();
            return;
        }

        // 处理被覆盖后的消除逻辑
        if (isCovered)
        {
            coverTimer += Time.deltaTime;
            //Debug.Log($"【红点{gameObject.name}（ID：{gameObject.GetInstanceID()}）】当前coverTimer：{coverTimer:F2} 秒（目标：{coverDestroyTime}秒）");
            if (coverTimer >= coverDestroyTime)
            {
                isCoverDestroy = true; // 标记为覆盖消除，需要加分
                DestroySelf(); // 覆盖时间达标，消除红点
                coverTimer = 0f;
            }
        }
        else
        {
            coverTimer = 0f; // 未被覆盖时重置覆盖计时器（新增）
            
        }
    }

    // 检测碰壁并反弹
    void CheckWallCollision()
    {
        // 获取点在父物体（背景Image）中的锚点位置（局部坐标）
        Vector2 pos = rectTransform.anchoredPosition;
        float dotHalfWidth = rectTransform.rect.width / 2;
        float dotHalfHeight = rectTransform.rect.height / 2;

        // 左边界：x <= -背景宽/2 + 点半宽
        if (pos.x <= -parentRect.width / 2 + dotHalfWidth)
        {
            pos.x = -parentRect.width / 2 + dotHalfWidth; // 防止超出边界
            moveDirection.x = -moveDirection.x; // x方向反弹
        }
        // 右边界：x >= 背景宽/2 - 点半宽
        else if (pos.x >= parentRect.width / 2 - dotHalfWidth)
        {
            pos.x = parentRect.width / 2 - dotHalfWidth;
            moveDirection.x = -moveDirection.x;
        }

        // 下边界：y <= -背景高/2 + 点半高
        if (pos.y <= -parentRect.height / 2 + dotHalfHeight)
        {
            pos.y = -parentRect.height / 2 + dotHalfHeight;
            moveDirection.y = -moveDirection.y; // y方向反弹
        }
        // 上边界：y >= 背景高/2 - 点半高
        else if (pos.y >= parentRect.height / 2 - dotHalfHeight)
        {
            pos.y = parentRect.height / 2 - dotHalfHeight;
            moveDirection.y = -moveDirection.y;
        }

        // 更新位置
        rectTransform.anchoredPosition = pos;




    }

    private void DestroySelf()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (isCoverDestroy)
        {
            ScoreEvents.TriggerChangeScore(scoreAdd);
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDotCoverDestroySFX();
            }
        }
        

        Destroy(gameObject);
    }

    // 新增：外部调用——标记红点是否被覆盖
    public void SetCovered(bool covered)
    {
        if (isDestroyed) return;

        if (isCovered == covered)
        {
            unstableTimer = 0;
            return;
        }

        if (covered)
        {
            // 覆盖时：直接设为true，重置防抖计时器
            isCovered = true;
            unstableTimer = 0;
            
        }
        else
        {
            isCovered = false;
            // 未覆盖时：防抖计时，达标后切为false（仅过滤1帧）
            unstableTimer += Time.deltaTime;
            if (unstableTimer >= coverStableTime)
            {
                isCovered = false;
                unstableTimer = 0;
                
            }
        }

    }
}
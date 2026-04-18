using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    private float moveSpeed; // 移动速度
    private float changeDirInterval; // 转向间隔
    private float timer; // 转向计时器
    private Vector2 moveDirection; // 当前移动方向
    private Vector2 boundsMin; // 摄像机左下限
    private Vector2 boundsMax; // 摄像机右上限

    // 初始化移动参数
    public void Initialize(float speed, float interval, Vector2 min, Vector2 max)
    {
        moveSpeed = speed;
        changeDirInterval = interval;
        boundsMin = min;
        boundsMax = max;

        // 随机初始方向
        RandomizeDirection();
    }

    void Update()
    {
        // 移动目标
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // 计时转向
        timer += Time.deltaTime;
        if (timer >= changeDirInterval)
        {
            timer = 0;
            RandomizeDirection(); // 随机改变方向
        }

        // 限制在摄像机范围内（防止移出视野）
        ClampToCameraBounds();
    }

    // 随机生成移动方向（2D平面内的任意方向）
    void RandomizeDirection()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f); // 0~360度随机角度
        moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
    }

    // 确保目标不超出摄像机视野
    void ClampToCameraBounds()
    {
        Vector3 currentPos = transform.position;
        // 限制X轴在摄像机左右边界内
        currentPos.x = Mathf.Clamp(currentPos.x, boundsMin.x, boundsMax.x);
        // 限制Y轴在摄像机上下边界内
        currentPos.y = Mathf.Clamp(currentPos.y, boundsMin.y, boundsMax.y);
        transform.position = currentPos;
    }
}